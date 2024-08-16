using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Harmony;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine.Events;

[assembly: AssemblyCopyright("Created by " + BetterSteadycam.BuildInfo.Author)]
[assembly: MelonInfo(typeof(BetterSteadycam.Main), BetterSteadycam.BuildInfo.Name, BetterSteadycam.BuildInfo.Version, BetterSteadycam.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

// This mod was firstly developed by nitro. and I continued
namespace BetterSteadycam 
{

    public static class BuildInfo
    {
        public const string Name = "BetterSteadycam";
        public const string Author = "Elaina & nitro.";
        public const string Version = "1.0.0";
    }

    public class Main : MelonMod {
        private const string ModCategory = "BetterSteadycam";
        private const string FieldOfViewPref = "FieldOfView";
        private const string SmoothingPref = "Smoothing";
        private const string SmoothingValuePref = "SmoothingValue";
        private const string RenderUIPref = "RenderUI";
        private const string EnableButtonOnDesktopPref = "EnableButtonOnDesktop";

        private static float fieldOfView = 0f;
        private static bool smoothing = false;
        private static float smoothingValue = 0f;
        private static bool renderUi = false;
        private static bool enableButtonOnDesktop = false;

        private static bool vrMode = false;
        private static bool uiManagerExists = false;
        private static Camera actualFpvCamera = null;
        private static GameObject steadycamDesktopButton = null;
        private static int newCullingMask = 0;
        private static bool updateCullingMask = false;

        public override void OnApplicationStart() {
            MelonLogger.Msg("Mod loaded.");

            MelonPreferences.CreateCategory(ModCategory, "Better Steadycam");
            MelonPreferences.CreateEntry(ModCategory, FieldOfViewPref, 60f, "Field of view (FOV)");
            MelonPreferences.CreateEntry(ModCategory, SmoothingPref, true, "Smoothing");
            MelonPreferences.CreateEntry(ModCategory, SmoothingValuePref, 5f, "Smoothing value");
            MelonPreferences.CreateEntry(ModCategory, RenderUIPref, true, "Render UI");
            MelonPreferences.CreateEntry(ModCategory, EnableButtonOnDesktopPref, true, "Enable button on desktop");

            if (Environment.GetCommandLineArgs().Any(args => args.Equals("--no-vr", StringComparison.OrdinalIgnoreCase))) vrMode = false; else vrMode = true;

            LoadPreferences();

            var harmony = HarmonyInstance.Create("BetterSteadycam");
            harmony.Patch(typeof(FPVCameraController).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public), new HarmonyMethod(typeof(Main).GetMethod("FPVCameraControllerUpdatePatch", BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public override void OnPreferencesLoaded() => LoadPreferences();

        public override void OnPreferencesSaved() => LoadPreferences();

        public override void VRChat_OnUiManagerInit() {
            uiManagerExists = true;
            SetButtonEnabled(enableButtonOnDesktop);
        }

        public override void OnUpdate() {
            if (Input.GetKeyDown(KeyCode.F10)) {
                MelonPreferences.SetEntryValue(ModCategory, RenderUIPref, !renderUi);
                renderUi = !renderUi;
                updateCullingMask = true;
            }
        }

        private void LoadPreferences() {
            fieldOfView = MelonPreferences.GetEntryValue<float>(ModCategory, FieldOfViewPref);
            smoothing = MelonPreferences.GetEntryValue<bool>(ModCategory, SmoothingPref);
            smoothingValue = MelonPreferences.GetEntryValue<float>(ModCategory, SmoothingValuePref);
            renderUi = MelonPreferences.GetEntryValue<bool>(ModCategory, RenderUIPref);
            enableButtonOnDesktop = MelonPreferences.GetEntryValue<bool>(ModCategory, EnableButtonOnDesktopPref);

            updateCullingMask = true;

            if (uiManagerExists) SetButtonEnabled(enableButtonOnDesktop);
        }

        private static bool FPVCameraControllerUpdatePatch(FPVCameraController __instance) {
            if (__instance.field_Private_EnumNPublicSealedvaOfSm3vUnique_0 == FPVCameraController.EnumNPublicSealedvaOfSm3vUnique.Smooth) {
                Transform cameraToFollow;

                var videoCamera = __instance.field_Public_GameObject_1;

                if (videoCamera.activeSelf) cameraToFollow = videoCamera.transform; else cameraToFollow = __instance.field_Private_Transform_0;

                var fpvCamera = __instance.field_Public_GameObject_0;

                if (!actualFpvCamera) actualFpvCamera = fpvCamera.GetComponent<Camera>();

                actualFpvCamera.fieldOfView = fieldOfView;

                if (updateCullingMask) {
                    newCullingMask = renderUi ? Camera.main.cullingMask : Camera.main.cullingMask & ~(1 << LayerMask.NameToLayer("UI"));
                    updateCullingMask = false;
                }

                actualFpvCamera.cullingMask = newCullingMask;

                fpvCamera.transform.position = cameraToFollow.position;

                if (smoothing) fpvCamera.transform.rotation = Quaternion.Lerp(fpvCamera.transform.rotation, cameraToFollow.rotation, Time.deltaTime * smoothingValue); else fpvCamera.transform.rotation = cameraToFollow.rotation;
            }
            return false;
        }

        private void SetButtonEnabled(bool enabled) {
            var cameraMenu = QuickMenu.prop_QuickMenu_0.transform.Find("CameraMenu");
            // ffs VRChat team "Stabalize the Desktop Stream of Your View" what the fuck is stabalize
            cameraMenu.Find("SmoothFPVCamera").GetComponent<UiTooltip>().field_Public_String_0 = "Stabilize the Desktop view of your game";

            if (!vrMode) {
                if (enabled) {
                    if (!steadycamDesktopButton) {
                        steadycamDesktopButton = UnityEngine.Object.Instantiate(cameraMenu.Find("SmoothFPVCamera"), cameraMenu).gameObject;
                        steadycamDesktopButton.name = "nitroSmoothFPVCamera";
                        steadycamDesktopButton.SetActive(true);
                        steadycamDesktopButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                        steadycamDesktopButton.GetComponent<Button>().onClick.AddListener((UnityAction)(() => {
                            var on = steadycamDesktopButton.transform.Find("Toggle_States_StandingEnabled/ON").gameObject;
                            var off = steadycamDesktopButton.transform.Find("Toggle_States_StandingEnabled/OFF").gameObject;
                            if (on.gameObject.activeSelf) {
                                on.SetActive(false);
                                off.SetActive(true);
                                FPVCameraController.field_Public_Static_FPVCameraController_0.prop_EnumNPublicSealedvaOfSm3vUnique_0 = FPVCameraController.EnumNPublicSealedvaOfSm3vUnique.Off;
                            } else {
                                on.SetActive(true);
                                off.SetActive(false);
                                FPVCameraController.field_Public_Static_FPVCameraController_0.prop_EnumNPublicSealedvaOfSm3vUnique_0 = FPVCameraController.EnumNPublicSealedvaOfSm3vUnique.Smooth;
                            }
                        }));
                        steadycamDesktopButton.transform.localPosition = new Vector3(-630f + (0 * 420f), 1050f + (3 * -420f));
                    }
                } else {
                    if (steadycamDesktopButton) UnityEngine.Object.DestroyImmediate(steadycamDesktopButton);
                }
            }
        }
    }
}
