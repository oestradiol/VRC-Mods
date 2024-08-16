using MelonLoader;
using UnityEngine;
using System.Linq;
using UnhollowerRuntimeLib;
using System.Collections;
using TrackingRotator.Utils;
using Il2CppSystem.Reflection;

namespace TrackingRotator 
{
    public static class ModBuildInfo {
        public const string Name = "TrackingRotator";
        public const string Author = "Elaina & nitro."; // <3
        public const string Version = "1.0.2";
        public const string DownloadLink = "https://github.com/nitrog0d/TrackingRotator/releases/latest/download/TrackingRotator.dll";
        public const string GameDeveloper = "VRChat";
        public const string Game = "VRChat";
    }

    public class TrackingRotatorMod : MelonMod 
    {

        private const string ModCategory = "TrackingRotator";
        private const string UIXIntegration = "UIXIntegration";
        private const string AMAPIIntegration = "AMAPIIntegration";
        private const string RotationValuePref = "RotationValue";
        private const string HighPrecisionRotationValuePref = "HighPrecisionRotationValue";
        private const string ResetRotationOnSceneChangePref = "ResetRotationOnSceneChange";

        private static float rotationValue = 0f;
        private static float highPrecisionRotationValue = 0f;
        private static bool resetRotationOnSceneChange, IsUsingUIX, IsUsingAMAPI = false;
        private static bool UIXintegration, AMAPIintegration = true;

        public static bool highPrecision = false;
        public static Transform transform;
        public static Transform cameraTransform;
        public static Quaternion originalRotation;

        public override void OnApplicationStart() 
        {
            MelonPreferences.CreateCategory(ModCategory, "Tracking Rotator");
            MelonPreferences.CreateEntry(ModCategory, UIXIntegration, true, "Integrate with UiExpansionKit?");
            MelonPreferences.CreateEntry(ModCategory, AMAPIIntegration, true, "Integrate with Action Menu?");
            MelonPreferences.CreateEntry(ModCategory, RotationValuePref, 22.5f, "Rotation value");
            MelonPreferences.CreateEntry(ModCategory, HighPrecisionRotationValuePref, 1f, "High precision rotation value");
            MelonPreferences.CreateEntry(ModCategory, ResetRotationOnSceneChangePref, false, "Reset rotation when a new world loads");
            OnPreferencesSaved();

            Integrations();

            MelonLogger.Msg("Mod loaded.");
        }

        public override void OnPreferencesSaved() 
        {
            UIXintegration = MelonPreferences.GetEntryValue<bool>(ModCategory, UIXIntegration);
            AMAPIintegration = MelonPreferences.GetEntryValue<bool>(ModCategory, AMAPIIntegration);
            rotationValue = MelonPreferences.GetEntryValue<float>(ModCategory, RotationValuePref);
            highPrecisionRotationValue = MelonPreferences.GetEntryValue<float>(ModCategory, HighPrecisionRotationValuePref);
            resetRotationOnSceneChange = MelonPreferences.GetEntryValue<bool>(ModCategory, ResetRotationOnSceneChangePref);
        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName) 
        {
            if (resetRotationOnSceneChange && cameraTransform) cameraTransform.localRotation = originalRotation;
        }

        public static IEnumerator WaitForUiInit() 
        {
            while (Object.FindObjectOfType<VRCVrCamera>() == null)
                yield return null;

            var camera = Object.FindObjectOfType<VRCVrCamera>();
            var Transform = camera.GetIl2CppType().GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => f.FieldType == Il2CppType.Of<Transform>()).ToArray()[0];
            cameraTransform = Transform.GetValue(camera).Cast<Transform>();
            originalRotation = cameraTransform.localRotation;
            transform = Camera.main.transform;

            if (IsUsingAMAPI) typeof(AMAPIManager).GetMethod("ActionMenuIntegration").Invoke(null, null);
        }

        public static void Move(Vector3 direction)
        {
            cameraTransform.Rotate(direction, highPrecision ? highPrecisionRotationValue : rotationValue, Space.World);
        }

        private static void Integrations()
        {
            if (AMAPIintegration)
            {
                if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("ActionMenuApi")))
                {
                    Assets.OnApplicationStart();
                    IsUsingAMAPI = true;
                }
                else MelonLogger.Warning("For a better experience, please consider using ActionMenuApi.");
            }
            else MelonLogger.Warning("Integration with ActionMenuApi has been deactivated on Settings.");

            if (UIXintegration)
            {
                if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                {
                    typeof(UIXManager).GetMethod("OnApplicationStart").Invoke(null, null);
                    IsUsingUIX = true;
                }
                else MelonLogger.Warning("For a better experience, please consider using UIExpansionKit.");
            }
            else MelonLogger.Warning("Integration with UIExpansionKit has been deactivated on Settings.");

            if (!AMAPIintegration && !UIXintegration)
                MelonLogger.Warning("Both integrations (Action Menu and UiExpansionKit) have been deactivated. " +
                    "The mod cannot run without those, therefore, expect it to fail. If this was not intended, " +
                    "please consider activating at least one of the integrations on Settings.");

            if (!IsUsingAMAPI && !IsUsingUIX)
            {
                MelonLogger.Error("Failed to load both integrations with UIExpansionKit and ActionMenuApi! The mod will not be loaded.");
            }
            else MelonCoroutines.Start(WaitForUiInit());
        }
    }
}