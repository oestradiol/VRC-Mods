using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

[assembly: AssemblyCopyright("Created by " + BetterSteadycam.BuildInfo.Author)]
[assembly: MelonInfo(typeof(BetterSteadycam.Main), BetterSteadycam.BuildInfo.Name, BetterSteadycam.BuildInfo.Version, BetterSteadycam.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

// This mod was firstly developed by nitro. and I continued
namespace BetterSteadycam
{
    public static class BuildInfo
    {
        public const string Name = "BetterSteadycam";
        public const string Author = "Davi & nitro.";
        public const string Version = "1.0.4";
    }

    internal static class UIXManager { public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit; }

    public class Main : MelonMod
    {
        private static MelonPreferences_Entry<float> _smoothingValue, _fieldOfView;
        private static MelonPreferences_Entry<bool> _smoothing, _renderUi, _enableButtonOnDesktop;
        private static int _newCullingMask;
        private static bool _updateCullingMask, _uiManagerExists;
        private static Camera _actualFpvCamera;
        private static GameObject _steadycamDesktopButton;
        private static HarmonyLib.Harmony _hInstance;
        private static MelonLogger.Instance _logger;

        public override void OnApplicationStart()
        {
            _logger = LoggerInstance;
            _hInstance = HarmonyInstance;
            MelonPreferences.CreateCategory("BetterSteadycam", "BetterSteadycam Settings");
            _fieldOfView = MelonPreferences.CreateEntry("BetterSteadycam", nameof(_fieldOfView), 60f, "Field of view (FOV)");
            _smoothing = MelonPreferences.CreateEntry("BetterSteadycam", nameof(_smoothing), true, "Smoothing");
            _smoothingValue = MelonPreferences.CreateEntry("BetterSteadycam", nameof(_smoothingValue), 5f, "Smoothing value");
            _renderUi = MelonPreferences.CreateEntry("BetterSteadycam", nameof(_renderUi), true, "Render UI");
            _enableButtonOnDesktop = MelonPreferences.CreateEntry("BetterSteadycam", nameof(_enableButtonOnDesktop), true, "Enable button on desktop");
            LoadPreferences();

            _hInstance.Patch(typeof(FPVCameraController).GetMethod(nameof(FPVCameraController.Update)),
                new HarmonyMethod(typeof(Main).GetMethod(nameof(FpvCameraControllerUpdatePatch), BindingFlags.NonPublic | BindingFlags.Static)));

            WaitForUiInit();

            _logger.Msg("Successfully loaded!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                typeof(UIXManager).GetMethod(nameof(UIXManager.OnApplicationStart))!.Invoke(null, null);
            else
            {
                _logger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
                static IEnumerator OnUiManagerInit()
                {
                    while (VRCUiManager.prop_VRCUiManager_0 == null)
                        yield return null;
                    VRChat_OnUiManagerInit();
                }
                MelonCoroutines.Start(OnUiManagerInit());
            }
        }

        public override void OnPreferencesLoaded() => LoadPreferences();

        public override void OnPreferencesSaved() => LoadPreferences();

        private static void LoadPreferences()
        {
            _updateCullingMask = true;
            if (_uiManagerExists) SetButtonEnabled(_enableButtonOnDesktop.Value);
        }

        public static void VRChat_OnUiManagerInit()
        {
            _uiManagerExists = true;
            SetButtonEnabled(_enableButtonOnDesktop.Value);
        }

        public override void OnUpdate()
        {
            if (!Input.GetKeyDown(KeyCode.F10)) return;
            MelonPreferences.SetEntryValue("BetterSteadycam", nameof(_renderUi), !_renderUi.Value);
            _updateCullingMask = true;
        }

        private static void SetButtonEnabled(bool enabled)
        {
            var steadyCamButton = GameObject.Find("UserInterface").transform // This is like this because GameObject.Find can't find inactive objects.
                                    .Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Camera/Scrollrect/Viewport/VerticalLayoutGroup/Buttons/Button_Steadycam").gameObject;
            // ffs VRChat team "Stabalize the Desktop Stream of Your View" what the fuck is stabalize - 9th of November, 2021 Update: they fixed it 🙏 
            // cameraMenu.Find("Scrollrect/Viewport/VerticalLayoutGroup/Buttons/Button_Steadycam").GetComponent<UiTooltip>().field_Public_String_0 = "Stabilize the Desktop view of your game";

            if (XRDevice.isPresent) return;
            switch (enabled)
            {
                case true when !_steadycamDesktopButton:
                    _steadycamDesktopButton = UnityEngine.Object.Instantiate(steadyCamButton, steadyCamButton.transform.parent);
                    _steadycamDesktopButton.SetActive(true);
                    _steadycamDesktopButton.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
                    _steadycamDesktopButton.GetComponent<Toggle>().onValueChanged.AddListener(new Action<bool>(isOn =>
                    {
                        var on = _steadycamDesktopButton.transform.Find("Icon_On").GetComponent<Image>();
                        var off = _steadycamDesktopButton.transform.Find("Icon_Off").GetComponent<Image>();
                        on.color = new Color(on.color.r, on.color.g, on.color.b, isOn ? 1 : 0.1f);
                        off.color = new Color(off.color.r, off.color.g, off.color.b, !isOn ? 1 : 0.1f);
                        FPVCameraController.field_Public_Static_FPVCameraController_0.prop_FPVCameraMode_0 =
                            isOn ? FPVCameraController.FPVCameraMode.Smooth : FPVCameraController.FPVCameraMode.Off;
                    }));
                    break;
                case false when _steadycamDesktopButton:
                    UnityEngine.Object.DestroyImmediate(_steadycamDesktopButton);
                    break;
            }
        }

        private static bool FpvCameraControllerUpdatePatch(FPVCameraController __instance)
        {
            if (__instance.field_Private_FPVCameraMode_0 != FPVCameraController.FPVCameraMode.Smooth) return false;
            var videoCamera = __instance.field_Public_GameObject_1;

            var cameraToFollow = videoCamera.activeSelf ? videoCamera.transform : __instance.field_Private_Transform_0;

            var fpvCamera = __instance.field_Public_GameObject_0;

            if (!_actualFpvCamera) _actualFpvCamera = fpvCamera.GetComponent<Camera>();

            _actualFpvCamera.fieldOfView = _fieldOfView.Value;

            if (_updateCullingMask)
            {
                _newCullingMask = _renderUi.Value ? Camera.main!.cullingMask : Camera.main!.cullingMask & ~(1 << LayerMask.NameToLayer("UI"));
                _updateCullingMask = false;
            }

            _actualFpvCamera.cullingMask = _newCullingMask;

            fpvCamera.transform.position = cameraToFollow.position;

            fpvCamera.transform.rotation = _smoothing.Value ? 
                Quaternion.Lerp(fpvCamera.transform.rotation, cameraToFollow.rotation, Time.deltaTime * _smoothingValue.Value) : 
                fpvCamera.transform.rotation = cameraToFollow.rotation;
            return false;
        }
    }
}