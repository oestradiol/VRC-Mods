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
        public const string Author = "Davi & nitro."; // <3
        public const string Version = "1.0.2";
        public const string DownloadLink = "https://github.com/nitrog0d/TrackingRotator/releases/latest/download/TrackingRotator.dll";
        public const string GameDeveloper = "VRChat";
        public const string Game = "VRChat";
    }

    public class TrackingRotatorMod : MelonMod 
    {

        private const string ModCategory = "TrackingRotator";
        private const string RotationValuePref = "RotationValue";
        private const string HighPrecisionRotationValuePref = "HighPrecisionRotationValue";
        private const string ResetRotationOnSceneChangePref = "ResetRotationOnSceneChange";

        private static float rotationValue = 0f;
        private static float highPrecisionRotationValue = 0f;
        private static bool resetRotationOnSceneChange, IsUsingUIX, IsUsingAMAPI = false;

        public static bool highPrecision = false;

        public static Transform transform;
        public static Transform cameraTransform = null;
        public static Quaternion originalRotation;

        public override void OnApplicationStart() 
        {
            MelonLogger.Msg("Mod loaded.");
            MelonPreferences.CreateCategory(ModCategory, "Tracking Rotator");
            MelonPreferences.CreateEntry(ModCategory, RotationValuePref, 22.5f, "Rotation value");
            MelonPreferences.CreateEntry(ModCategory, HighPrecisionRotationValuePref, 1f, "High precision rotation value");
            MelonPreferences.CreateEntry(ModCategory, ResetRotationOnSceneChangePref, false, "Reset rotation when a new world loads");
            OnPreferencesSaved();

            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("ActionMenuApi")))
            {
                Assets.OnApplicationStart();
                IsUsingAMAPI = true;
            }
            else MelonLogger.Warning("For a better experience, please consider using ActionMenuApi.");
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
            {
                typeof(UIXManager).GetMethod("OnApplicationStart").Invoke(null, null);
                IsUsingUIX = true;
            } 
            else MelonLogger.Warning("For a better experience, please consider using UIExpansionKit.");

            if (!IsUsingAMAPI && !IsUsingUIX) MelonLogger.Error("Failed to load both UIExpansionKit and ActionMenuApi! The mod will not be loaded.");
            else MelonCoroutines.Start(WaitForUiInit());
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) 
        {
            if (resetRotationOnSceneChange && cameraTransform) cameraTransform.localRotation = originalRotation;
        }

        public override void OnPreferencesSaved() 
        {
            rotationValue = MelonPreferences.GetEntryValue<float>(ModCategory, RotationValuePref);
            highPrecisionRotationValue = MelonPreferences.GetEntryValue<float>(ModCategory, HighPrecisionRotationValuePref);
            resetRotationOnSceneChange = MelonPreferences.GetEntryValue<bool>(ModCategory, ResetRotationOnSceneChangePref);
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
    }
}