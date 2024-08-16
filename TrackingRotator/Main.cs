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
using BuildInfo = TrackingRotator.BuildInfo;
using Main = TrackingRotator.Main;

[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: MelonInfo(typeof(Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit", "ActionMenuApi")]

// This mod was firstly developed by nitro. and I continued
namespace TrackingRotator
{
    public static class BuildInfo
    {
        public const string Name = "TrackingRotator";
        public const string Author = "Elaina & nitro.";
        public const string Version = "1.0.3";
    }

    public class Main : MelonMod
    {
        public static MelonLogger.Instance Logger;
        private static MelonPreferences_Entry<float> _highPrecisionRotationValue, _rotationValue;
        private static MelonPreferences_Entry<bool> _uixIntegration, _amapiIntegration, _resetRotationOnSceneChange;
        private static bool _isUsingUix, _isUsingAmapi;
        public static bool HighPrecision;
        public static Transform Transform, CameraTransform;
        public static Quaternion OriginalRotation;

        public override void OnApplicationStart()
        {
            Logger = LoggerInstance;
            
            MelonPreferences.CreateCategory("TrackingRotator", "Tracking Rotator");
            _uixIntegration = MelonPreferences.CreateEntry("TrackingRotator", nameof(_uixIntegration), true, "Integrate with UiExpansionKit?");
            _amapiIntegration = MelonPreferences.CreateEntry("TrackingRotator", nameof(_amapiIntegration), true, "Integrate with Action Menu?");
            _rotationValue = MelonPreferences.CreateEntry("TrackingRotator", nameof(_rotationValue), 22.5f, "Rotation value");
            _highPrecisionRotationValue = MelonPreferences.CreateEntry("TrackingRotator", nameof(_highPrecisionRotationValue), 1f, "High precision rotation value");
            _resetRotationOnSceneChange = MelonPreferences.CreateEntry("TrackingRotator", nameof(_resetRotationOnSceneChange), false, "Reset rotation when a new world loads");
            
            Integrations();
            
            Logger.Msg("Successfully loaded!");
        }

        private static void Integrations()
        {
            if (_amapiIntegration.Value)
                if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("ActionMenuApi")))
                {
                    Assets.OnApplicationStart();
                    _isUsingAmapi = true;
                }
                else Logger.Warning("For a better experience, please consider installing ActionMenuApi.");
            else Logger.Warning("Integration with ActionMenuApi has been deactivated on Settings.");

            if (_uixIntegration.Value)
                if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                {
                    typeof(UIXManager).GetMethod("OnApplicationStart")!.Invoke(null, null);
                    _isUsingUix = true;
                }
                else Logger.Warning("For a better experience, please consider installing UIExpansionKit.");
            else Logger.Warning("Integration with UIExpansionKit has been deactivated on Settings.");

            if (!_amapiIntegration.Value && !_uixIntegration.Value)
                Logger.Warning("Both integrations (Action Menu and UiExpansionKit) have been deactivated. " +
                                "The mod cannot run without those, therefore, expect it to fail. If this was not intended, " +
                                "please consider activating at least one of the integrations on Settings.");

            if (!_isUsingAmapi && !_isUsingUix)
                Logger.Error("Failed to load both integrations with UIExpansionKit and ActionMenuApi! The mod will not be loaded.");
            else if (!_isUsingUix)
            {
                Logger.Warning("Using coroutine to wait for UiInit.");
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
            var camera = Object.FindObjectOfType<VRCVrCamera>();
            var transform = camera.GetIl2CppType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.FieldType == Il2CppType.Of<Transform>()).ToArray()[0];
            CameraTransform = transform.GetValue(camera).Cast<Transform>();
            OriginalRotation = CameraTransform.localRotation;
            Transform = Camera.main!.transform;
            if (_isUsingAmapi) typeof(AMAPIManager).GetMethod("ActionMenuIntegration")!.Invoke(null, null);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) 
        { if (_resetRotationOnSceneChange.Value && CameraTransform) CameraTransform.localRotation = OriginalRotation; }

        public static void Move(Vector3 direction) => 
            CameraTransform.Rotate(direction, HighPrecision ? _highPrecisionRotationValue.Value : _rotationValue.Value, Space.World);
    }
}