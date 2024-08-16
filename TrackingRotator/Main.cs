using System;
using System.Linq;
using System.Collections;
using Il2CppSystem.Reflection;
using UnhollowerRuntimeLib;
using MelonLoader;
using UnityEngine;
using TrackingRotator.Utils;
using AssemblyCopyright = System.Reflection.AssemblyCopyrightAttribute;
using Object = UnityEngine.Object;

[assembly: AssemblyCopyright("Created by " + TrackingRotator.BuildInfo.Author)]
[assembly: MelonInfo(typeof(TrackingRotator.Main), TrackingRotator.BuildInfo.Name, TrackingRotator.BuildInfo.Version, TrackingRotator.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

// This mod was firstly developed by nitro. and I continued it
namespace TrackingRotator
{
    public static class BuildInfo
    {
        public const string Name = "TrackingRotator";
        public const string Author = "Elaina & nitro.";
        public const string Version = "1.0.2";
    }

    public class Main : MelonMod
    {
        private static MelonPreferences_Entry<float> HighPrecisionRotationValue, RotationValue;
        private static MelonPreferences_Entry<bool> UIXIntegration, AMAPIIntegration, ResetRotationOnSceneChange;
        private static bool IsUsingUIX, IsUsingAMAPI;

        public static bool highPrecision;
        public static Transform transform, cameraTransform;
        public static Quaternion originalRotation;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("TrackingRotator", "Tracking Rotator");
            UIXIntegration = MelonPreferences.CreateEntry("TrackingRotator", nameof(UIXIntegration), true, "Integrate with UiExpansionKit?");
            AMAPIIntegration = MelonPreferences.CreateEntry("TrackingRotator", nameof(AMAPIIntegration), true, "Integrate with Action Menu?");
            RotationValue = MelonPreferences.CreateEntry("TrackingRotator", nameof(RotationValue), 22.5f, "Rotation value");
            HighPrecisionRotationValue = MelonPreferences.CreateEntry("TrackingRotator", nameof(HighPrecisionRotationValue), 1f, "High precision rotation value");
            ResetRotationOnSceneChange = MelonPreferences.CreateEntry("TrackingRotator", nameof(ResetRotationOnSceneChange), false, "Reset rotation when a new world loads");

            Integrations();

            MelonLogger.Msg("Successfully loaded!");
        }

        private static void Integrations()
        {
            if (AMAPIIntegration.Value)
            {
                if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("ActionMenuApi")))
                {
                    Assets.OnApplicationStart();
                    IsUsingAMAPI = true;
                }
                else MelonLogger.Warning("For a better experience, please consider using ActionMenuApi.");
            }
            else MelonLogger.Warning("Integration with ActionMenuApi has been deactivated on Settings.");

            if (UIXIntegration.Value)
            {
                if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                {
                    typeof(UIXManager).GetMethod("OnApplicationStart").Invoke(null, null);
                    IsUsingUIX = true;
                }
                else MelonLogger.Warning("For a better experience, please consider using UIExpansionKit.");
            }
            else MelonLogger.Warning("Integration with UIExpansionKit has been deactivated on Settings.");

            if (!AMAPIIntegration.Value && !UIXIntegration.Value)
                MelonLogger.Warning("Both integrations (Action Menu and UiExpansionKit) have been deactivated. " +
                    "The mod cannot run without those, therefore, expect it to fail. If this was not intended, " +
                    "please consider activating at least one of the integrations on Settings.");

            if (!IsUsingAMAPI && !IsUsingUIX)
            {
                MelonLogger.Error("Failed to load both integrations with UIExpansionKit and ActionMenuApi! The mod will not be loaded.");
            }
            else
            {
                static IEnumerator OnUiManagerInit()
                {
                    while (VRCUiManager.prop_VRCUiManager_0 == null)
                        yield return null;
                    VRChat_OnUiManagerInit();
                }
                MelonCoroutines.Start(OnUiManagerInit());
            }
        }

        private static void VRChat_OnUiManagerInit()
        {
            var camera = Object.FindObjectOfType<VRCVrCamera>();
            var Transform = camera.GetIl2CppType().GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => f.FieldType == Il2CppType.Of<Transform>()).ToArray()[0];
            cameraTransform = Transform.GetValue(camera).Cast<Transform>();
            originalRotation = cameraTransform.localRotation;
            transform = Camera.main.transform;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) 
        { if (ResetRotationOnSceneChange.Value && cameraTransform) cameraTransform.localRotation = originalRotation; }

        public static void Move(Vector3 direction) => 
            cameraTransform.Rotate(direction, highPrecision ? HighPrecisionRotationValue.Value : RotationValue.Value, Space.World);
    }
}