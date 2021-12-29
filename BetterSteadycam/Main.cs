using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Events;

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
        public const string Version = "1.0.3";
    }

    internal static class UIXManager { public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit; }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        private static MelonPreferences_Entry<float> SmoothingValue, FieldOfView;
        private static MelonPreferences_Entry<bool> Smoothing, RenderUi, EnableButtonOnDesktop;
        private static int newCullingMask;
        private static bool updateCullingMask, uiManagerExists;
        private static Camera actualFpvCamera;
        private static GameObject steadycamDesktopButton;
        private static HarmonyLib.Harmony HInstance => Instance.HarmonyInstance;

        public override void OnApplicationStart()
        {
            Instance = this;
            MelonPreferences.CreateCategory("BetterSteadycam", "BetterSteadycam Settings");
            FieldOfView = MelonPreferences.CreateEntry("BetterSteadycam", nameof(FieldOfView), 60f, "Field of view (FOV)");
            Smoothing = MelonPreferences.CreateEntry("BetterSteadycam", nameof(Smoothing), true, "Smoothing");
            SmoothingValue = MelonPreferences.CreateEntry("BetterSteadycam", nameof(SmoothingValue), 5f, "Smoothing value");
            RenderUi = MelonPreferences.CreateEntry("BetterSteadycam", nameof(RenderUi), true, "Render UI");
            EnableButtonOnDesktop = MelonPreferences.CreateEntry("BetterSteadycam", nameof(EnableButtonOnDesktop), true, "Enable button on desktop");
            LoadPreferences();

            HInstance.Patch(typeof(FPVCameraController).GetMethod(nameof(FPVCameraController.Update)),
                new HarmonyMethod(typeof(Main).GetMethod(nameof(FPVCameraControllerUpdatePatch), BindingFlags.NonPublic | BindingFlags.Static)));

            WaitForUiInit();

            MelonLogger.Msg("Successfully loaded!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                typeof(UIXManager).GetMethod(nameof(UIXManager.OnApplicationStart)).Invoke(null, null);
            else
            {
                MelonLogger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
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
            updateCullingMask = true;
            if (uiManagerExists) SetButtonEnabled(EnableButtonOnDesktop.Value);
        }

        public static void VRChat_OnUiManagerInit()
        {
            uiManagerExists = true;
            SetButtonEnabled(EnableButtonOnDesktop.Value);
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                MelonPreferences.SetEntryValue("BetterSteadycam", nameof(RenderUi), !RenderUi.Value);
                updateCullingMask = true;
            }
        }

        private static void SetButtonEnabled(bool enabled)
        {
            var SteadyCamButton = GameObject.Find("UserInterface").transform // This is like this because GameObject.Find can't find inactive objects.
                                    .Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Camera/Scrollrect/Viewport/VerticalLayoutGroup/Buttons/Button_Steadycam").gameObject;
            // ffs VRChat team "Stabalize the Desktop Stream of Your View" what the fuck is stabalize - 9th of November, 2021 Update: they fixed it 🙏 
            // cameraMenu.Find("Scrollrect/Viewport/VerticalLayoutGroup/Buttons/Button_Steadycam").GetComponent<UiTooltip>().field_Public_String_0 = "Stabilize the Desktop view of your game";

            if (!XRDevice.isPresent)
            {
                if (enabled && !steadycamDesktopButton)
                {
                    steadycamDesktopButton = UnityEngine.Object.Instantiate(SteadyCamButton, SteadyCamButton.transform.parent);
                    steadycamDesktopButton.SetActive(true);
                    steadycamDesktopButton.GetComponent<Toggle>().onValueChanged = new();
                    steadycamDesktopButton.GetComponent<Toggle>().onValueChanged.AddListener(new Action<bool>(isOn =>
                    {
                        var on = steadycamDesktopButton.transform.Find("Icon_On").GetComponent<Image>();
                        var off = steadycamDesktopButton.transform.Find("Icon_Off").GetComponent<Image>();
                        on.color = new(on.color.r, on.color.g, on.color.b, isOn ? 1 : 0.1f);
                        off.color = new(off.color.r, off.color.g, off.color.b, !isOn ? 1 : 0.1f);
                        FPVCameraController.field_Public_Static_FPVCameraController_0.prop_FPVCameraMode_0 =
                            isOn ? FPVCameraController.FPVCameraMode.Smooth : FPVCameraController.FPVCameraMode.Off;
                    }));
                }
                else if (!enabled && steadycamDesktopButton) UnityEngine.Object.DestroyImmediate(steadycamDesktopButton);
            }
        }

        private static bool FPVCameraControllerUpdatePatch(FPVCameraController __instance)
        {
            if (__instance.field_Private_FPVCameraMode_0 == FPVCameraController.FPVCameraMode.Smooth)
            {
                Transform cameraToFollow;

                var videoCamera = __instance.field_Public_GameObject_1;

                cameraToFollow = videoCamera.activeSelf ? videoCamera.transform : __instance.field_Private_Transform_0;

                var fpvCamera = __instance.field_Public_GameObject_0;

                if (!actualFpvCamera) actualFpvCamera = fpvCamera.GetComponent<Camera>();

                actualFpvCamera.fieldOfView = FieldOfView.Value;

                if (updateCullingMask)
                {
                    newCullingMask = RenderUi.Value ? Camera.main.cullingMask : Camera.main.cullingMask & ~(1 << LayerMask.NameToLayer("UI"));
                    updateCullingMask = false;
                }

                actualFpvCamera.cullingMask = newCullingMask;

                fpvCamera.transform.position = cameraToFollow.position;

                fpvCamera.transform.rotation = Smoothing.Value ? 
                    Quaternion.Lerp(fpvCamera.transform.rotation, cameraToFollow.rotation, Time.deltaTime * SmoothingValue.Value) : 
                    fpvCamera.transform.rotation = cameraToFollow.rotation;
            }
            return false;
        }
    }
}