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
        public const string Author = "Davi";
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
        private static Toggle toggle;
        private const bool useHeadLook = false;
        private static bool PreviousState, IsUsingUIX;
        private static MelonPreferences_Entry<string> FSResolution;

        #region Init
        public override void OnApplicationStart()
        {
            IsUsingUIX = MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit"));
            InitCachedVars();

            // Set preferences
            MelonPreferences.CreateCategory("ToggleFullScreen", "Toggle FullScreen");
            FSResolution = MelonPreferences.CreateEntry("ToggleFullScreen", "FSResolution", "Maximum", "FullScreen Resolution");
            if (IsUsingUIX) UpdateUIXSwitch();

            WaitForUiInit();
            MelonLogger.Msg("Successfully loaded!");
        }
        private static void InitCachedVars()
        {
            Previous = new()
            {
                width = Screen.width,
                height = Screen.height
            };
            c_MaxRes = GetCurrentMaxRes();
            c_HighRes = CalculatePropRes(new() { width = 1600, height = 900 });
            c_MediumRes = CalculatePropRes(new() { width = 1366, height = 768 });
            c_LowRes = CalculatePropRes(new() { width = 1280, height = 720 });
            c_MinimumRes = CalculatePropRes(new() { width = 852, height = 480 });
        }
        #endregion

        #region UI
        private static void UpdateUIXSwitch() =>
            typeof(UIXManager).GetMethod(nameof(UIXManager.RegisterSettingAsStringEnum))
                .Invoke(null, new object[]{"ToggleFullScreen", "FSResolution",
                    new[] {
                        ("Maximum", $"{c_MaxRes.width}x{c_MaxRes.height}"),
                        ("High", $"{c_HighRes.width}x{c_HighRes.height}"),
                        ("Medium", $"{c_MediumRes.width}x{c_MediumRes.height}"),
                        ("Low", $"{c_LowRes.width}x{c_LowRes.height}"),
                        ("Minimum", $"{c_MinimumRes.width}x{c_MinimumRes.height}")
                    }});
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
            OtherOptions.localScale = new(1, 1.1f, 1);

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
                child.localScale = new(1, (float)(1 / 1.1), 1);
            }
            if (!useHeadLook)
            {
                Transform HeadLookToggle = OtherOptions.Find("HeadLookToggle");
                HeadLookToggle.position = Children.Find(x => x.name.Contains("AllowAvatarCopyingToggle")).position;
                HeadLookToggle.localScale = new(1, (float)(1 / 1.1), 1);
            }

            // Creates new Toggle
            Transform ToggleButton = Object.Instantiate(OtherOptions.Find("ShowCommunityLabsToggle").gameObject, OtherOptions).transform;
            Object.DestroyImmediate(ToggleButton.GetComponent<UiSettingConfig>());

            ToggleButton.name = "FullScreenToggle";
            ToggleButton.GetComponentInChildren<Text>().text = "TOGGLE FULLSCREEN";

            toggle = ToggleButton.GetComponent<Toggle>();
            toggle.isOn = Screen.fullScreen;
            toggle.onValueChanged = new();
            toggle.onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((isOn) => { Screen.fullScreen = isOn; }));
        }
        #endregion

        #region ResolutionProcessing
        private static Resolution Previous, c_MaxRes, c_HighRes, c_MediumRes, c_LowRes, c_MinimumRes;
        private static Resolution MaxRes => GetCurrentMaxRes();
        private static Resolution HighRes => GetCurrentResFor("High");
        private static Resolution MediumRes => GetCurrentResFor("Medium");
        private static Resolution LowRes => GetCurrentResFor("Low");
        private static Resolution MinimumRes => GetCurrentResFor("Minimum");
        private static Resolution GetCurrentMaxRes() => Screen.resolutions[Screen.resolutions.Length - 1]; // NEEDS URGENT FIX.
                                                                                                           //var temp = Screen.fullScreen;
                                                                                                           //Screen.fullScreen = false; Resolution result = Screen.currentResolution; Screen.fullScreen = temp;
                                                                                                           //return result;
                                                                                                           //return new() { width = Display.main.systemWidth, height = Display.main.systemHeight };
        private static Resolution GetCurrentResFor(string Quality)
        {
            CheckAndUpdateResolutions();
            Resolution Current;
            switch (Quality)
            {
                case "Medium":
                    Current = c_MediumRes;
                    break;
                case "Low":
                    Current = c_LowRes;
                    break;
                case "Minimum":
                    Current = c_MinimumRes;
                    break;
                default:
                    Current = c_HighRes;
                    break;
            }
            return Current;
        }
        private static void CheckAndUpdateResolutions()
        {
            var temp = MaxRes;
            if (c_MaxRes.width != temp.width || c_MaxRes.height != temp.height)
            {
                c_MaxRes = temp;
                c_HighRes = CalculatePropRes(new() { width = 1600, height = 900 });
                c_MediumRes = CalculatePropRes(new() { width = 1366, height = 768 });
                c_LowRes = CalculatePropRes(new() { width = 1280, height = 720 });
                c_MinimumRes = CalculatePropRes(new() { width = 852, height = 480 });
                UpdateUIXSwitch();
            }
        }
        private static Resolution CalculatePropRes(Resolution propTo) =>
            new()
            {
                // I didn't know which resolutions to use and I didn't wanna make a set for each monitor so I thought-
                // "Why not just make them so they all follow the most used ones?"
                // -and this came of it.
                width = (int)Math.Floor((double)(propTo.width * c_MaxRes.width / 1920)),
                height = (int)Math.Floor((double)(propTo.height * c_MaxRes.height / 1080))
            };
        #endregion

        #region ResolutionUpdating
        // Updates the Resolution after saving prefs
        public override void OnPreferencesSaved()
        {
            Resolution Current = GetCurrentAppliedRes();
            if (Screen.fullScreen)
                Screen.SetResolution(Current.width, Current.height, true);
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
                    var Current = GetCurrentAppliedRes();
                    Screen.SetResolution(Current.width, Current.height, true);
                }
                else
                    Screen.SetResolution(Previous.width, Previous.height, false);
                if ((toggle != null) && (toggle.isOn != Screen.fullScreen)) toggle.isOn = Screen.fullScreen;
                PreviousState = Screen.fullScreen;
            }
        }
        private static Resolution GetCurrentAppliedRes()
        {
            Resolution Current;
            switch (FSResolution.Value)
            {
                case "High":
                    Current = HighRes;
#if DEBUG
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to High ({Current.width}x{Current.height}).");
#endif
                    break;
                case "Medium":
                    Current = MediumRes;
#if DEBUG
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Medium ({Current.width}x{Current.height}).");
#endif
                    break;
                case "Low":
                    Current = LowRes;
#if DEBUG
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Low ({Current.width}x{Current.height}).");
#endif
                    break;
                case "Minimum":
                    Current = MinimumRes;
#if DEBUG
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Minimum ({Current.width}x{Current.height}).");
#endif
                    break;
                default:
                    Current = MaxRes;
#if DEBUG
                    MelonLogger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to Maximum ({Current.width}x{Current.height}).");
#endif
                    break;
            }
            return Current;
        }
        #endregion
    }
}