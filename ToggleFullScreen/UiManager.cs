using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using ResTypes = ToggleFullScreen.Main.ResTypes;

namespace ToggleFullScreen;

internal static class UixManager
{
    public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += UiManager.Init;
    public static void RegisterSettingAsStringEnum(string categoryName, string settingName, IList<(string SettingsValue, string DisplayName)> possibleValues) =>
        UIExpansionKit.API.ExpansionKitApi.RegisterSettingAsStringEnum(categoryName, settingName, possibleValues);
}

internal static class UiManager
{
    internal static Toggle Toggle;

    internal static void WaitForUiInit()
    {
        if (Main.IsUsingUix)
            typeof(UixManager).GetMethod(nameof(UixManager.OnApplicationStart))!.Invoke(null, null);
        else
        {
            Main.Logger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
            Main.Logger.Warning("This also means that the function to change fullscreen resolution will only be usable by changing MelonPrefs directly at the file.");
            Main.Logger.Warning("Again, please consider installing UIX.");
            static IEnumerator OnUiManagerInit()
            {
                while (VRCUiManager.prop_VRCUiManager_0 == null)
                    yield return null;
                Init();
            }
            MelonCoroutines.Start(OnUiManagerInit());
        }
    }

    internal static void Init()
    {
        // Rescales and repositions Options Panel
        var settings = GameObject.Find("UserInterface/MenuContent/Screens/Settings").transform;
        var otherOptions = settings.Find("OtherOptionsPanel");
        var position = otherOptions.position;
        position += (settings.Find("VoiceOptionsPanel").position - position) / 12;
        otherOptions.position = position;
        otherOptions.localScale = new Vector3(1, 1.1f, 1);

        // Repositions Ui Toggles
        var proportion = (otherOptions.Find("TooltipsToggle").transform.position - otherOptions.Find("3PRotationToggle").transform.position) / 7;
        List<Transform> children = new();
        for (var i = 0; i < otherOptions.GetChildCount(); i++)
        {
            var child = otherOptions.GetChild(i);
            if (!child.name.Contains("Panel_Header") && !child.name.Contains("TitleText")) children.Add(child);
        }
        if (!Main.UseHeadLook) children.Remove(children.Find(x => x.name.Contains("HeadLookToggle")));
        children.Sort((x, y) => y.position.y.CompareTo(x.position.y));

        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            child.position += proportion * i;
            child.localScale = new Vector3(1, (float)(1 / 1.1), 1);
        }
        if (!Main.UseHeadLook)
        {
            var headLookToggle = otherOptions.Find("HeadLookToggle");
            headLookToggle.position = children.Find(x => x.name.Contains("AllowAvatarCopyingToggle")).position;
            headLookToggle.localScale = new(1, (float)(1 / 1.1), 1);
        }

        // Creates new Toggle
        var toggleButton = Object.Instantiate(otherOptions.Find("ShowCommunityLabsToggle").gameObject, otherOptions).transform;
        Object.DestroyImmediate(toggleButton.GetComponent<UiSettingConfig>());

        toggleButton.name = "FullScreenToggle";
        toggleButton.GetComponentInChildren<Text>().text = "TOGGLE FULLSCREEN";

        Toggle = toggleButton.GetComponent<Toggle>();
        Toggle.isOn = Screen.fullScreen;
        Toggle.onValueChanged = new Toggle.ToggleEvent();
        Toggle.onValueChanged.AddListener(new Action<bool>(isOn => Screen.fullScreen = isOn));
    }

    internal static void UpdateUixSwitch()
    {
        var possibleValues = ((ResTypes[])Enum.GetValues(typeof(ResTypes))).ToList()
            .ConvertAll(i =>
            {
                var res = ResolutionProcessor.ResCache[i];
                return (i.ToString(), $"{res.width}x{res.height}");
            });
        typeof(UixManager).GetMethod(nameof(UixManager.RegisterSettingAsStringEnum))!
            .Invoke(null, new object[]{"ToggleFullScreen", "FSResolution", possibleValues});
    }
}