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

        if (Virality.CustomPhotonRealtimeAppId?.Value != null && CustomPhotonInstanceHelper.AppIsDefault())
        {
            // force photon to custom instance
            CustomPhotonInstanceHelper.SetAppId(Virality.CustomPhotonRealtimeAppId!.Value, Virality.CustomPhotonRealtimeAppId!.Value);
            CustomPhotonInstanceHelper.ForceReconnection();

            // this sucks and needs to be rewritten
            var instance = __instance;
            PhotonReconnectionBehaviour.Instance?.InvokeCallbackOnConnection(() =>
            {
                Virality.Logger.LogDebug("Reconnected....");
                instance.HostMatch(_action, privateMatch);
            });

            // force return false so we can re-call host later
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Postfix patch for the OnLobbyCreatedCallback method.
    ///     Updates Steam rich presence.
    /// </summary>
    /// <param name="__instance"> Instance of the SteamLobbyHandler. </param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SteamLobbyHandler.OnLobbyCreatedCallback))]
    private static void OnLobbyCreatedCallbackPostfix(ref SteamLobbyHandler __instance)
    {
        if (!Virality.AllowFriendJoining!.Value)
            return;

        SteamMatchmaking.SetLobbyType(SteamLobbyHelper.GetLobbyId(), ELobbyType.k_ELobbyTypeFriendsOnly);
        SteamLobbyHelper.SetRichPresenceJoinable();
    }
}