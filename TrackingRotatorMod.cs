using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;

namespace TrackingRotator {

    public static class ModBuildInfo {
        public const string Name = "TrackingRotator";
        public const string Author = "nitro.";
        public const string Version = "1.0.0";
        public const string DownloadLink = "https://github.com/nitrog0d/TrackingRotator/releases/latest/download/TrackingRotator.dll";
        public const string GameDeveloper = "VRChat";
        public const string Game = "VRChat";
    }

    public class TrackingRotatorMod : MelonMod {
        private const string ModCategory = "TrackingRotator";
        private const string RotationValuePref = "RotationValue";
        private const string HighPrecisionRotationValuePref = "HighPrecisionRotationValue";
        private const string ResetRotationOnSceneChangePref = "ResetRotationOnSceneChange";

        private static float rotationValue = 0f;
        private static float highPrecisionRotationValue = 0f;
        private static bool highPrecision = false;
        private static bool resetRotationOnSceneChange = false;

        private static Transform cameraTransform = null;
        private static Quaternion originalRotation;

        public override void OnApplicationStart() {
            MelonLogger.Msg("Mod loaded.");
            MelonPreferences.CreateCategory(ModCategory, "Tracking Rotator");
            MelonPreferences.CreateEntry(ModCategory, RotationValuePref, 22.5f, "Rotation value");
            MelonPreferences.CreateEntry(ModCategory, HighPrecisionRotationValuePref, 1f, "High precision rotation value");
            MelonPreferences.CreateEntry(ModCategory, ResetRotationOnSceneChangePref, false, "Reset rotation when a new world loads");
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Tracking rotation", ShowRotationMenu);
            OnPreferencesSaved();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            if (resetRotationOnSceneChange) {
                if (cameraTransform) {
                    cameraTransform.localRotation = originalRotation;
                }
            }
        }

        public override void OnPreferencesSaved() {
            rotationValue = MelonPreferences.GetEntryValue<float>(ModCategory, RotationValuePref);
            highPrecisionRotationValue = MelonPreferences.GetEntryValue<float>(ModCategory, HighPrecisionRotationValuePref);
            resetRotationOnSceneChange = MelonPreferences.GetEntryValue<bool>(ModCategory, ResetRotationOnSceneChangePref);
        }

        public override void VRChat_OnUiManagerInit() {
            cameraTransform = Object.FindObjectOfType<VRCVrCameraSteam>().field_Public_Transform_0;
            originalRotation = cameraTransform.localRotation;
        }

        private static ICustomShowableLayoutedMenu rotationMenu = null;

        // Based on knah's ViewPointTweaker mod, https://github.com/knah/VRCMods/blob/master/ViewPointTweaker
        private void ShowRotationMenu() {
            if (rotationMenu == null) {
                rotationMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

                void Move(Vector3 direction) {
                    cameraTransform.Rotate(direction, highPrecision ? highPrecisionRotationValue : rotationValue, Space.World);
                }

                var transform = Camera.main.transform;

                rotationMenu.AddSpacer();
                rotationMenu.AddSimpleButton("Forward", () => Move(transform.right));
                rotationMenu.AddSpacer();
                rotationMenu.AddSpacer();

                rotationMenu.AddSimpleButton("Tilt Left", () => Move(transform.forward));
                rotationMenu.AddSimpleButton("Reset", () => cameraTransform.localRotation = originalRotation);
                rotationMenu.AddSimpleButton("Tilt Right", () => Move(-transform.forward));
                rotationMenu.AddSpacer();

                rotationMenu.AddSpacer();
                rotationMenu.AddSimpleButton("Backward", () => Move(-transform.right));
                rotationMenu.AddSimpleButton("Left", () => Move(-transform.up));
                rotationMenu.AddSimpleButton("Right", () => Move(transform.up));

                rotationMenu.AddToggleButton("High precision", b => highPrecision = b, () => highPrecision);
                rotationMenu.AddSpacer();
                rotationMenu.AddSpacer();
                rotationMenu.AddSimpleButton("Back", rotationMenu.Hide);
            }

            rotationMenu.Show();
        }
    }
}
