using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using BuildInfo = ToggleFullScreen.BuildInfo;
using Main = ToggleFullScreen.Main;

[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: MelonInfo(typeof(Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace ToggleFullScreen
{
    public static class BuildInfo
    {
        public const string Name = "ToggleFullScreen";
        public const string Author = "Elaina";
        public const string Version = "1.1.2";
    }

    internal static class UIXManager
    {
        public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit;
        public static void RegisterSettingAsStringEnum(string categoryName, string settingName, IList<(string SettingsValue, string DisplayName)> possibleValues) =>
            UIExpansionKit.API.ExpansionKitApi.RegisterSettingAsStringEnum(categoryName, settingName, possibleValues);
    }

    public class Main : MelonMod
    {
        #region Init
        private static MelonPreferences_Entry<string> _fsResolution;
        private static MelonLogger.Instance _logger; // To use Logger instead of MelonLogger

        private static Toggle _toggle;
        private const bool UseHeadLook = false; // This is here just in case, since idfk what that headlook button is
        private static bool _previousState, _isUsingUix;
        private static Resolution _previous, _cMaxRes, _cHighRes, _cMediumRes, _cLowRes, _cMinimumRes;
        private enum ResTypes { Minimum, Low, Medium, High, Maximum }

        public override void OnApplicationStart()
        {
            _logger = LoggerInstance;
            _isUsingUix = MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit"));

            // Set preferences
            MelonPreferences.CreateCategory("ToggleFullScreen", "Toggle FullScreen");
            _fsResolution = MelonPreferences.CreateEntry("ToggleFullScreen", "FSResolution", "Maximum", "FullScreen Resolution");
            _previous = new Resolution { width = Screen.width, height = Screen.height };
            CheckAndUpdateResolutions();

            WaitForUiInit();
            
            _logger.Msg("Successfully loaded!");
        }
        #endregion

        #region UI
        private static void WaitForUiInit()
        {
            if (_isUsingUix)
                typeof(UIXManager).GetMethod(nameof(UIXManager.OnApplicationStart))!.Invoke(null, null);
            else
            {
                _logger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
                _logger.Warning("This also means that the function to change fullscreen resolution will only be usable by changing MelonPrefs directly at the file.");
                _logger.Warning("Again, please consider installing UIX.");
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
            if (!UseHeadLook) children.Remove(children.Find(x => x.name.Contains("HeadLookToggle")));
            children.Sort((x, y) => y.position.y.CompareTo(x.position.y));

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                child.position += proportion * i;
                child.localScale = new Vector3(1, (float)(1 / 1.1), 1);
            }
            if (!UseHeadLook)
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

            _toggle = toggleButton.GetComponent<Toggle>();
            _toggle.isOn = Screen.fullScreen;
            _toggle.onValueChanged = new Toggle.ToggleEvent();
            _toggle.onValueChanged.AddListener(new Action<bool>(isOn => Screen.fullScreen = isOn));
        }
        private static void UpdateUixSwitch() =>
            typeof(UIXManager).GetMethod(nameof(UIXManager.RegisterSettingAsStringEnum))!
                .Invoke(null, new object[]{"ToggleFullScreen", "FSResolution",
                    new[] {
                        ("Maximum", $"{_cMaxRes.width}x{_cMaxRes.height}"),
                        ("High", $"{_cHighRes.width}x{_cHighRes.height}"),
                        ("Medium", $"{_cMediumRes.width}x{_cMediumRes.height}"),
                        ("Low", $"{_cLowRes.width}x{_cLowRes.height}"),
                        ("Minimum", $"{_cMinimumRes.width}x{_cMinimumRes.height}")
                    }});
        #endregion

        #region ResolutionProcessing
        private static Resolution GetCurrentResFor(ResTypes quality)
        {
            CheckAndUpdateResolutions();
            return quality switch
            {
                ResTypes.High => _cHighRes,
                ResTypes.Medium => _cMediumRes,
                ResTypes.Low => _cLowRes,
                ResTypes.Minimum => _cMinimumRes,
                _ => _cMaxRes
            };
        }
        private static void CheckAndUpdateResolutions()
        {
            var temp = GetCurrentMaxRes();
            if (_cMaxRes.width == temp.width && _cMaxRes.height == temp.height) return;
            _cMaxRes = temp;
            _cHighRes = CalculatePropRes(new Resolution { width = 1600, height = 900 });
            _cMediumRes = CalculatePropRes(new Resolution { width = 1366, height = 768 });
            _cLowRes = CalculatePropRes(new Resolution { width = 1280, height = 720 });
            _cMinimumRes = CalculatePropRes(new Resolution { width = 852, height = 480 });
            if (_isUsingUix) UpdateUixSwitch();
        }
        // Had to use Natives below because all C# AND Unity methods failed me so why not :)
        private static Resolution GetCurrentMaxRes()
        {
            var currentHdc = GetWindowDC(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            return new Resolution
                {
                    width = GetDeviceCaps(currentHdc, 8), // DeviceCap.HORZRES = 8
                    height = GetDeviceCaps(currentHdc, 10) // DeviceCap.VERTRES = 10
                };
        }
        [DllImport("User32.dll", CharSet = CharSet.Auto)] private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("gdi32.dll")] private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        private static Resolution CalculatePropRes(Resolution propTo) =>
            new()
            {
                // I didn't know which resolutions to use and I didn't wanna make a set for each monitor so I thought-
                // "Why not just make them so they all follow the most used ones?"
                // -and this came of it.
                width = (int)Math.Floor(propTo.width * _cMaxRes.width / 1920d),
                height = (int)Math.Floor(propTo.height * _cMaxRes.height / 1080d)
            };
        #endregion

        #region ResolutionUpdating
        // Updates the Resolution after saving prefs
        public override void OnPreferencesSaved()
        {
            var current = GetCurrentAppliedRes();
            if (!Screen.fullScreen) return;
            _logger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to {_fsResolution.Value} ({current.width}x{current.height}).");
            Screen.SetResolution(current.width, current.height, true);
        }
        // Checks for state changes
        public override void OnUpdate()
        {
            if (_previousState == Screen.fullScreen) return;
            if (Screen.fullScreen)
            {
                _previous = new Resolution
                {
                    width = Screen.width,
                    height = Screen.height
                };
                var current = GetCurrentAppliedRes();
                _logger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to {_fsResolution.Value} ({current.width}x{current.height}).");
                Screen.SetResolution(current.width, current.height, true);
            }
            else
                Screen.SetResolution(_previous.width, _previous.height, false);
            if (_toggle != null && _toggle.isOn != Screen.fullScreen) _toggle.isOn = Screen.fullScreen;
            _previousState = Screen.fullScreen;
        }
        private static Resolution GetCurrentAppliedRes() => 
            _fsResolution.Value switch
            {
                "High" => GetCurrentResFor(ResTypes.High),
                "Medium" => GetCurrentResFor(ResTypes.Medium),
                "Low" => GetCurrentResFor(ResTypes.Low),
                "Minimum" => GetCurrentResFor(ResTypes.Minimum),
                _ => GetCurrentResFor(ResTypes.Maximum)
            };
        #endregion
    }
}