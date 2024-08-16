using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[assembly: AssemblyCopyright("Created by " + ToggleFullScreen.BuildInfo.Author)]
[assembly: MelonInfo(typeof(ToggleFullScreen.Main), ToggleFullScreen.BuildInfo.Name, ToggleFullScreen.BuildInfo.Version, ToggleFullScreen.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace ToggleFullScreen
{
    public static class BuildInfo
    {
        public const string Name = "ToggleFullScreen";
        public const string Author = "Elaina";
        public const string Version = "1.1.0";
    }

    internal static class UIXManager 
    { 
        public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit;

        public static void RegisterSettingAsStringEnum(string categoryName, string settingName, IList<(string SettingsValue, string DisplayName)> possibleValues) => 
            GetRegisterSettingAsStringEnumDelegate(categoryName, settingName, possibleValues);
        private static RegisterSettingAsStringEnumDelegate GetRegisterSettingAsStringEnumDelegate =>
            registerSettingAsStringEnumDelegate ??= (RegisterSettingAsStringEnumDelegate)Delegate.CreateDelegate(typeof(RegisterSettingAsStringEnumDelegate), null, RegisterSettingAsStringEnumMethod);
        private static MethodInfo RegisterSettingAsStringEnumMethod =>
            registerSettingAsStringEnumMethod ??= typeof(UIExpansionKit.API.ExpansionKitApi).GetMethod(nameof(UIExpansionKit.API.ExpansionKitApi.RegisterSettingAsStringEnum));
        private delegate void RegisterSettingAsStringEnumDelegate(string categoryName, string settingName, IList<(string SettingsValue, string DisplayName)> possibleValues);
        private static RegisterSettingAsStringEnumDelegate registerSettingAsStringEnumDelegate;
        private static MethodInfo registerSettingAsStringEnumMethod;
    }

    public class Main : MelonMod
    {
        private static Resolution Previous, Current, MaxRes, FirstRes, SecondRes, ThirdRes, FourthRes;
        private static MelonPreferences_Entry<string> FSResolution;
        private const bool useHeadLook = false;
        private static bool PreviousState, IsUsingUIX;
        private static Toggle toggle;

        public override void OnApplicationStart()
        {
            IsUsingUIX = MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit"));
            Previous = new()
            {
                width = Screen.width,
                height = Screen.height
            };

            // Set preferences
            var temp = Screen.resolutions;
            MaxRes = temp[temp.Count - 1];
            ProcessResolutions();
            MelonPreferences.CreateCategory("ToggleFullScreen", "Toggle FullScreen");
            FSResolution = MelonPreferences.CreateEntry("ToggleFullScreen", "FSResolution", "Maximum", "FullScreen Resolution");
            if (IsUsingUIX)
                typeof(UIXManager).GetMethod(nameof(UIXManager.RegisterSettingAsStringEnum)).Invoke(null, 
                    new object[]{"ToggleFullScreen", "FSResolution",
                                    new[] {
                                        ("Maximum", $"{MaxRes.width}x{MaxRes.height}"),
                                        ("High", $"{FirstRes.width}x{FirstRes.height}"),
                                        ("Medium", $"{SecondRes.width}x{SecondRes.height}"),
                                        ("Low", $"{ThirdRes.width}x{ThirdRes.height}"),
                                        ("Minimum", $"{FourthRes.width}x{FourthRes.height}")
                                    }});
            OnPreferencesSaved();

            WaitForUiInit();
            MelonLogger.Msg("Successfully loaded!");
        }

        // Updates the Resolution after saving prefs
        public override void OnPreferencesSaved()
        {
            switch (FSResolution.Value)
            {
                case "High":
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to High ({FirstRes.width}x{FirstRes.height}).");
                    Current = FirstRes;
                    break;
                case "Medium":
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Medium ({SecondRes.width}x{SecondRes.height}).");
                    Current = SecondRes;
                    break;
                case "Low":
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Low ({ThirdRes.width}x{ThirdRes.height}).");
                    Current = ThirdRes;
                    break;
                case "Minimum":
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Minimum ({FourthRes.width}x{FourthRes.height}).");
                    Current = FourthRes;
                    break;
                default:
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Maximum ({MaxRes.width}x{MaxRes.height}).");
                    Current = MaxRes;
                    break;
            }
            if (Screen.fullScreen)
                Screen.SetResolution(Current.width, Current.height, true);
        }

        private static void WaitForUiInit()
        {
            if (IsUsingUIX)
                typeof(UIXManager).GetMethod(nameof(UIXManager.OnApplicationStart)).Invoke(null, null);
            else
            {
                MelonLogger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
                MelonLogger.Warning("This also means that the function to change fullscreen resolution will only be usable by changing MelonPrefs directly at the file.");
                MelonLogger.Warning("Again, please consider installing UIX.");
                static IEnumerator OnUiManagerInit()
                {
                    while (VRCUiManager.prop_VRCUiManager_0 == null)
                        yield return null;
                    VRChat_OnUiManagerInit();
                }
                MelonCoroutines.Start(OnUiManagerInit());
            }
        }
        public static void VRChat_OnUiManagerInit()
        {
            // Rescales and repositions Options Panel
            Transform Settings = GameObject.Find("UserInterface/MenuContent/Screens/Settings").transform;
            Transform OtherOptions = Settings.Find("OtherOptionsPanel");
            OtherOptions.position += (Settings.Find("VoiceOptionsPanel").position - OtherOptions.position) / 12;
            OtherOptions.localScale = new Vector3(1, 1.1f, 1);

            // Repositions Ui Toggles
            Vector3 proportion = (OtherOptions.Find("TooltipsToggle").transform.position - OtherOptions.Find("3PRotationToggle").transform.position) / 7;
            List<Transform> Children = new();
            for (int i = 0; i < OtherOptions.GetChildCount(); i++)
            {
                var child = OtherOptions.GetChild(i);
                if (!child.name.Contains("Panel_Header") && !child.name.Contains("TitleText")) Children.Add(child);
            }
            if (!useHeadLook) Children.Remove(Children.Find(x => x.name.Contains("HeadLookToggle")));
            Children.Sort((x, y) => y.position.y.CompareTo(x.position.y));

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.position += proportion * i;
                child.localScale = new Vector3(1, (float)(1 / 1.1), 1);
            }
            if (!useHeadLook)
            {
                Transform HeadLookToggle = OtherOptions.Find("HeadLookToggle");
                HeadLookToggle.position = Children.Find(x => x.name.Contains("AllowAvatarCopyingToggle")).position;
                HeadLookToggle.localScale = new Vector3(1, (float)(1 / 1.1), 1);
            }

            // Creates new Toggle
            Transform ToggleButton = Object.Instantiate(OtherOptions.Find("ShowCommunityLabsToggle").gameObject, OtherOptions).transform;
            Object.DestroyImmediate(ToggleButton.GetComponent<UiSettingConfig>());

            ToggleButton.name = "FullScreenToggle";
            ToggleButton.GetComponentInChildren<Text>().text = "TOGGLE FULLSCREEN";

            toggle = ToggleButton.GetComponent<Toggle>();
            toggle.isOn = Screen.fullScreen;
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((isOn) => { Screen.fullScreen = isOn; }));
        }

        // Checks for state changes
        public override void OnUpdate()
        {
            if (PreviousState != Screen.fullScreen)
            {
                if (Screen.fullScreen)
                {
                    Previous = new()
                    {
                        width = Screen.width,
                        height = Screen.height
                    };
                    Screen.SetResolution(Current.width, Current.height, true);
                }
                else
                {
                    Screen.SetResolution(Previous.width, Previous.height, false);
                }
                if ((toggle != null) && (toggle.isOn != Screen.fullScreen)) toggle.isOn = Screen.fullScreen;
                PreviousState = Screen.fullScreen;
            }
        }

        // I didn't know which resolutions to use and I didn't wanna make a set for each monitor so I thought-
        // "Why not just make them so they all follow the most used ones?"
        // -and this came of it.
        private static Resolution CalculatePropRes(Resolution propTo) =>
            new Resolution()
            {
                width = (int)Math.Floor((double)(propTo.width * MaxRes.width / 1920)),
                height = (int)Math.Floor((double)(propTo.height * MaxRes.height / 1080))
            };

        private static void ProcessResolutions()
        {
            MaxRes = CalculatePropRes(new Resolution() { width = 1920, height = 1080 });
            FirstRes = CalculatePropRes(new Resolution() { width = 1600, height = 900 });
            SecondRes = CalculatePropRes(new Resolution() { width = 1366, height = 768 });
            ThirdRes = CalculatePropRes(new Resolution() { width = 1280, height = 720 });
            FourthRes = CalculatePropRes(new Resolution() { width = 852, height = 480 });
        }
    }
}