using HarmonyLib;
using Steamworks;
using System;
using Virality.Behaviours;
using Virality.Helpers;

namespace Virality.Patches;

[HarmonyPatch(typeof(SteamLobbyHandler))]
[HarmonyPriority(Priority.First)]
internal static class SteamLobbyHandlerPatches
{
    public const int MaxConnectionRetry = 5;
    public static int CurrentConnectionAttempt = 0;

    /// <summary>
    ///     Prefix patch for the HostMatch method.
    ///     Overrides the max players value with the one from the Virality config.
    /// </summary>
    /// <param name="__instance"> Instance of the SteamLobbyHandler. </param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SteamLobbyHandler.HostMatch))]
    private static bool HostMatchPrefix(ref SteamLobbyHandler __instance, Action<ulong> _action, bool privateMatch)
    {
        __instance.m_MaxPlayers = Virality.MaxPlayers!.Value;
        SteamLobbyHelper.LobbyHandler = __instance;
        Virality.Logger?.LogDebug($"Max players set to {__instance.m_MaxPlayers}.");

        var voiceAppId = CustomPhotonInstanceHelper.GetCustomVoiceAppId();
        if (CustomPhotonInstanceHelper.CustomAppIdIsValid(Virality.CustomPhotonRealtimeAppId?.Value) && CustomPhotonInstanceHelper.CustomAppIdIsValid(voiceAppId) && CustomPhotonInstanceHelper.AppIsDefault())
        {
            // force photon to custom instance
            CustomPhotonInstanceHelper.SetAppId(Virality.CustomPhotonRealtimeAppId!.Value, voiceAppId!);
            CustomPhotonInstanceHelper.ForceReconnection();

            // this sucks and needs to be rewritten
            var instance = __instance;

            PhotonReconnectionBehaviour.Instance?.InvokeCallbackOnConnection(() =>
            {
                CurrentConnectionAttempt++;
                Virality.Logger.LogDebug("Reconnected....");
                CustomPhotonInstanceHelper.LogAppId();

                if (CurrentConnectionAttempt < MaxConnectionRetry)
                {
                    instance.HostMatch(_action, privateMatch);
                }
                else
                {
                    // TODO: Display in-ui
                    Virality.Logger.LogDebug("Reconnection FAILED!");
                }
            });

            // force return false so we can re-call host later
            return false;
        }

        ResetConnectionAttempts();
        return true;
    }

    /// <summary>
    ///     Postfix patch for the OnLobbyCreatedCallback method.
    ///     Updates Steam rich presence and photon appid syncing.
    /// </summary>
    /// <param name="__instance"> Instance of the SteamLobbyHandler. </param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SteamLobbyHandler.OnLobbyCreatedCallback))]
    private static void OnLobbyCreatedCallbackPostfix(ref SteamLobbyHandler __instance)
    {
        // network custom photon appid to allow smooth joining
        if (!CustomPhotonInstanceHelper.AppIsDefault())
        {
            // TODO : Get this appid value from the lobby settings itself
            SteamMatchmaking.SetLobbyData(SteamLobbyHelper.GetLobbyId(), CustomPhotonInstanceHelper.RealtimeAppIdKey, Virality.CustomPhotonRealtimeAppId!.Value);
            SteamMatchmaking.SetLobbyData(SteamLobbyHelper.GetLobbyId(), CustomPhotonInstanceHelper.VoiceAppIdKey, CustomPhotonInstanceHelper.GetCustomVoiceAppId());

            Virality.Logger?.LogDebug("Setting lobby appid data...");
        }

        if (!Virality.AllowFriendJoining!.Value)
            return;

        SteamMatchmaking.SetLobbyType(SteamLobbyHelper.GetLobbyId(), ELobbyType.k_ELobbyTypeFriendsOnly);
        SteamLobbyHelper.SetRichPresenceJoinable();
    }

    /// <summary>
    ///     Prefix patch for the JoinLobby method.
    ///     Switches appid if needed.
    /// </summary>
    /// <param name="__instance"> Instance of the SteamLobbyHandler. </param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SteamLobbyHandler.JoinLobby))]
    private static bool JoinLobbyPrefix(ref SteamLobbyHandler __instance, CSteamID lobbyID)
    {
        Virality.Logger?.LogDebug("Attempting to join. Lobby app IDs:");
        string? realtimeAppId = SteamMatchmaking.GetLobbyData(lobbyID, CustomPhotonInstanceHelper.RealtimeAppIdKey);
        string? voiceAppId = SteamMatchmaking.GetLobbyData(lobbyID, CustomPhotonInstanceHelper.VoiceAppIdKey);

        // set app ids to default if not valid
        if (!CustomPhotonInstanceHelper.CustomAppIdIsValid(realtimeAppId))
            realtimeAppId = CustomPhotonInstanceHelper.DefaultRealtimeAppId;
        if (!CustomPhotonInstanceHelper.CustomAppIdIsValid(voiceAppId))
            voiceAppId = CustomPhotonInstanceHelper.DefaultVoiceAppId;

        Virality.Logger?.LogDebug(realtimeAppId);
        Virality.Logger?.LogDebug(voiceAppId);

        // if we are not currently using the lobby's appid, switch!
        if (!CustomPhotonInstanceHelper.AppIdMatches(realtimeAppId, voiceAppId))
        {
            // force photon to custom instance
            CustomPhotonInstanceHelper.SetAppId(realtimeAppId!, voiceAppId!);
            CustomPhotonInstanceHelper.ForceReconnection();

            // this sucks and needs to be rewritten
            var instance = __instance;
            PhotonReconnectionBehaviour.Instance?.InvokeCallbackOnConnection(() =>
            {
                CurrentConnectionAttempt++;
                Virality.Logger.LogDebug("Reconnected....");
                CustomPhotonInstanceHelper.LogAppId();

                if (CurrentConnectionAttempt < MaxConnectionRetry)
                {
                    instance.JoinLobby(lobbyID);
                }
                else
                {
                    // TODO: Display in-ui
                    Virality.Logger.LogDebug("Reconnection FAILED!");
                }
            });

            // force return false so we can re-call host later
            return false;
        }

        ResetConnectionAttempts();
        return true;
    }

    private static void ResetConnectionAttempts()
    {
        CurrentConnectionAttempt = 0;
        PhotonReconnectionBehaviour.Instance?.InvokeCallbackOnConnection(null);
    }
}