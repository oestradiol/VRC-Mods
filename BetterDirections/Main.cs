using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using MelonLoader;
using VRC.Animation;
using BuildInfo = BetterDirections.BuildInfo;
using Main = BetterDirections.Main;

[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: MelonInfo(typeof(Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace BetterDirections
{
    public static class BuildInfo
    {
        public const string Name = "BetterDirections";
        public const string Author = "Elaina & AxisAngle";
        public const string Version = "1.0.1";
    }

    internal static class UIXManager { public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit; }

    public class Main : MelonMod
    {
        private static HarmonyLib.Harmony _hInstance;
        private static MelonLogger.Instance _logger;

        // Wait for Ui Init so XRDevice.isPresent is defined
        public override void OnApplicationStart()
        {
            _logger = LoggerInstance;
            _hInstance = HarmonyInstance;

            WaitForUiInit();

            _logger.Msg("Successfully loaded!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                typeof(UIXManager).GetMethod("OnApplicationStart")!.Invoke(null, null);
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

        // Apply the patch
        public static void VRChat_OnUiManagerInit()
        {
            if (XRDevice.isPresent)
            {
                _logger.Msg("XRDevice detected. Initializing...");
                try
                {
                    foreach (var info in typeof(VRCMotionState).GetMethods().Where(method =>
                        method.Name.Contains("Method_Public_Void_Vector3_Single_") && !method.Name.Contains("PDM")))
                        _hInstance.Patch(info, new HarmonyMethod(typeof(Main).GetMethod(nameof(Prefix))));
                    _logger.Msg("Successfully loaded!");
                }
                catch (Exception e)
                {
                    _logger.Warning("Failed to initialize mod!");
                    _logger.Error(e);
                }
            }
            else
                _logger.Warning("Mod is VR-Only.");
        }

        // Substitute the direction from the original method with our own
        public static void Prefix(ref Vector3 __0) { __0 = CalculateDirection(__0); }

        // Gets the center of the eye (camera)
        private static GameObject _cameraObj;
        private static GameObject CameraObj =>
            _cameraObj ??= Resources.FindObjectsOfTypeAll<NeckMouseRotator>()[0]
                                .transform.Find(Environment.CurrentDirectory.Contains("vrchat-vrchat") ? "CenterEyeAnchor" : "Camera (eye)").gameObject;

        // Fixes the game's original direction to match the preferred one
        private static Vector3 CalculateDirection(Vector3 rawVelo)
        {
            var zInput = CameraObj.transform.forward;
            var badRot = Quaternion.LookRotation(
                            Vector3.Cross(Vector3.Cross(Vector3.up, zInput),Vector3.up));
            var inputDirection = Quaternion.Inverse(badRot) * rawVelo;
            return Quaternion.FromToRotation(CameraObj.transform.up, Vector3.up) *
                       CameraObj.transform.rotation *
                       inputDirection;
        }
    }
}