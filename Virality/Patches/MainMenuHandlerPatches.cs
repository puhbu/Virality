using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Photon.Pun;
using Steamworks;
using Virality.Helpers;

namespace Virality.Patches;

[HarmonyPatch(typeof(MainMenuHandler))]
[HarmonyPriority(Priority.First)]
internal static class MainMenuHandlerPatches
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(MainMenuHandler.OnSteamHosted))]
    private static IEnumerable<CodeInstruction> OnSteamHostedTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .SearchForward(instruction => instruction.opcode == OpCodes.Ldc_I4_4)
            .ThrowIfInvalid("Could not find max players constant")
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, Virality.MaxPlayers!.Value))
            .InstructionEnumeration();
    }

    [HarmonyPrefix]
    [HarmonyPatch("Start")]
    private static void StartPatch()
    {
        // capture defaults if we haven't already
        CustomPhotonInstanceHelper.CaptureDefaults();

        // reset photon to default to avoid breaking normal matchmaking (if not host)
        if (!CustomPhotonInstanceHelper.AppIsDefault())
        {
            CustomPhotonInstanceHelper.SetToDefaultApp();
            CustomPhotonInstanceHelper.ForceReconnection();
        }

        CustomPhotonInstanceHelper.LogAppId();
        SteamLobbyHandlerPatches.CurrentConnectionAttempt = 0;
    }
}