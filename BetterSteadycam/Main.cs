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
        public const string Author = "Elaina & nitro.";
        public const string Version = "1.0.1";
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
                typeof(UIXManager).GetMethod("OnApplicationStart").Invoke(null, null);
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
            var cameraMenu = QuickMenu.prop_QuickMenu_0.transform.Find("CameraMenu");
            // ffs VRChat team "Stabalize the Desktop Stream of Your View" what the fuck is stabalize
            cameraMenu.Find("SmoothFPVCamera").GetComponent<UiTooltip>().field_Public_String_0 = "Stabilize the Desktop view of your game";

            if (!XRDevice.isPresent)
            {
                if (enabled && !steadycamDesktopButton)
                {
                    steadycamDesktopButton = UnityEngine.Object.Instantiate(cameraMenu.Find("SmoothFPVCamera"), cameraMenu).gameObject;
                    steadycamDesktopButton.SetActive(true);
                    steadycamDesktopButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    steadycamDesktopButton.GetComponent<Button>().onClick.AddListener((UnityAction)(() =>
                    {
                        var on = steadycamDesktopButton.transform.Find("Toggle_States_StandingEnabled/ON").gameObject;
                        var off = steadycamDesktopButton.transform.Find("Toggle_States_StandingEnabled/OFF").gameObject;
                        var Flag = on.gameObject.activeSelf;
                        on.SetActive(!Flag);
                        off.SetActive(Flag);
                        FPVCameraController.field_Public_Static_FPVCameraController_0.prop_EnumNPublicSealedvaOfSm3vUnique_0 =
                            Flag ? FPVCameraController.EnumNPublicSealedvaOfSm3vUnique.Off : FPVCameraController.EnumNPublicSealedvaOfSm3vUnique.Smooth;
                    }));
                    steadycamDesktopButton.transform.localPosition = new Vector3(-630f + (0 * 420f), 1050f + (3 * -420f));
                }
                else if (steadycamDesktopButton) UnityEngine.Object.DestroyImmediate(steadycamDesktopButton);
            }
        }

        private static bool FPVCameraControllerUpdatePatch(FPVCameraController __instance)
        {
            if (__instance.field_Private_EnumNPublicSealedvaOfSm3vUnique_0 == FPVCameraController.EnumNPublicSealedvaOfSm3vUnique.Smooth)
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
