using System;
using System.Linq;
using EvenMoreGameModes;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;


[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.First)]
public class MainMenuManagerStartPatch
{
    public static SpriteRenderer EMGMLogo;
    private static void Postfix(MainMenuManager __instance)
    {
        var amongUsLogo = GameObject.Find("LOGO-AU");
        if (amongUsLogo)
        {
            amongUsLogo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("EvenMoreGameModes.Resources.EvenMoreGameModes-Logo.png", 140f);
        }

        var rightpanel = __instance.gameModeButtons.transform.parent;
        var logoObject = new GameObject("titleLogo_EMGM");
        var logoTransform = logoObject.transform;

        EMGMLogo = logoObject.AddComponent<SpriteRenderer>();
        logoTransform.parent = rightpanel;
        logoTransform.localPosition = new(-0.16f, 0f, 1f);
        logoTransform.localScale *= 1.2f;

        var ambience = (GameObject.Find("Ambience"));
        if (ambience != null)
        {
            ambience.SetActive(true);
        }

        SetButtonColor(__instance.playButton);
        SetButtonColor(__instance.inventoryButton);
        SetButtonColor(__instance.shopButton);
        SetButtonColor(__instance.newsButton);
        SetButtonColor(__instance.myAccountButton);
        SetButtonColor(__instance.settingsButton);
        SetButtonColor(__instance.creditsButton);
        SetButtonColor(__instance.quitButton);
    }

    public static void SetButtonColor(PassiveButton playButton)
    {
        if (playButton == null) return;
        playButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color32(22, 167, 31, byte.MaxValue);
        playButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color32(22, 167, 31, byte.MaxValue);
    }

}
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
class MainMenuManagerLateUpdatePatch
{
    private static void Postfix(MainMenuManager __instance)
    {
        if (!__instance || !__instance.finishStartup) return;
        var playOnlineButton = __instance.PlayOnlineButton;
        if (playOnlineButton != null)
        {
            MainMenuManagerStartPatch.SetButtonColor(__instance.PlayOnlineButton);
        }

        var playLocalButton = __instance.playLocalButton;
        if (playLocalButton != null)
        {
            MainMenuManagerStartPatch.SetButtonColor(__instance.playLocalButton);
        }
    }
}
[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatch
{
    [HarmonyPatch(nameof(MainMenuManager.Start)), HarmonyPostfix, HarmonyPriority(Priority.Normal)]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        Application.targetFrameRate = 165;
        GameObject rightPanel = __instance.mainMenuUI.FindChild<Transform>("RightPanel").gameObject;
        __instance.screenTint.enabled = false;
        GameObject maskedBlackScreen = rightPanel.FindChild<Transform>("MaskedBlackScreen").gameObject;
        maskedBlackScreen.GetComponent<SpriteRenderer>().enabled = false;
        var background = GameObject.Find("BackgroundTexture");
        if (background != null)
        {
            var render = background.GetComponent<SpriteRenderer>();
            render.flipY = true;
            render.color = new Color(0f, 255f, 0f, 1f);
        }
        var tint = GameObject.Find("MainUI").transform.GetChild(0).gameObject;
        if (tint != null)
        {
            tint.GetComponent<SpriteRenderer>().enabled = false;
        }

        GameObject leftPanel = __instance.mainMenuUI.FindChild<Transform>("LeftPanel").gameObject;
        leftPanel.GetComponentsInChildren<SpriteRenderer>(true).Where(r => r.name == "Shine").ToList().ForEach(r => r.enabled = false);
        leftPanel.gameObject.FindChild<SpriteRenderer>("Divider").enabled = false;

        var howToPlayButton = __instance.howToPlayButton;
        var freeplayButton = howToPlayButton.transform.parent.Find("FreePlayButton");

        if (freeplayButton != null) freeplayButton.gameObject.SetActive(false);

        howToPlayButton.transform.SetLocalX(0);
    }
    public static T FindChild<T>(this GameObject obj, string name) where T : Object
    {
        string name2 = name;
        return obj.GetComponentsInChildren<T>().First(c => c.name == name2);
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPostfix]
    public static void OpenMenu_Postfix()
    {
        if (MainMenuManagerStartPatch.EMGMLogo != null) MainMenuManagerStartPatch.EMGMLogo.gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(MainMenuManager.ResetScreen)), HarmonyPostfix]
    public static void ResetScreen_Postfix()
    {
        if (MainMenuManagerStartPatch.EMGMLogo != null) MainMenuManagerStartPatch.EMGMLogo.gameObject.SetActive(true);
    }
}