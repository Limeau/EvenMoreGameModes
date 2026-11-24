using EvenMoreGameModes;
using HarmonyLib;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
class VersionShowerStartPatch
{
    private static void Postfix(VersionShower __instance)
    {
        var credentialsText = $"<size=100%><b><color=#00ff00>Even More Game Modes</color> <color=#ff0000>{Main.CurrentVersion}</color></b></size>";

        var credentials = Object.Instantiate(__instance.text);
        credentials.text = credentialsText;
        credentials.alignment = TextAlignmentOptions.Center;
        credentials.transform.position = new Vector3(0f, 2.67f, -2f);
        credentials.fontSize = credentials.fontSizeMax = credentials.fontSizeMin = 2f;
    }
}