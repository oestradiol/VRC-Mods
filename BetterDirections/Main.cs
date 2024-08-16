using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using MelonLoader;
using VRC.Animation;

[assembly: AssemblyCopyright("Created by " + BetterDirections.BuildInfo.Author)]
[assembly: MelonInfo(typeof(BetterDirections.Main), BetterDirections.BuildInfo.Name, BetterDirections.BuildInfo.Version, BetterDirections.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace BetterDirections
{
    public static class BuildInfo
    {
        public const string Name = "BetterDirections";
        public const string Author = "Elaina & AxisAngle";
        public const string Version = "1.0.0";
    }

    public class Main : MelonMod
    {
        // Wait for Ui Init so XRDevice.isPresent is defined
        public override void OnApplicationStart()
        {
            IEnumerator OnUiManagerInit()
            {
                while (VRCUiManager.prop_VRCUiManager_0 == null)
                    yield return null;
                VRChat_OnUiManagerInit();
            }
            MelonCoroutines.Start(OnUiManagerInit());
        }

        // Apply the patch
        private void VRChat_OnUiManagerInit()
        {
            if (XRDevice.isPresent)
            {
                MelonLogger.Msg("XRDevice detected. Initializing...");
                try
                {
                    foreach (var info in typeof(VRCMotionState).GetMethods().Where(method =>
                        method.Name.Contains("Method_Public_Void_Vector3_Single_") && !method.Name.Contains("PDM")))
                        HarmonyInstance.Patch(info, new HarmonyMethod(typeof(Main).GetMethod(nameof(Prefix))));
                    MelonLogger.Msg("Successfully loaded!");
                }
                catch (Exception e)
                {
                    MelonLogger.Warning("Failed to initialize mod!");
                    MelonLogger.Error(e);
                }
            }
            else
                MelonLogger.Warning("Mod is VR-Only.");
        }

        // Substitute the direction from the original method with our own
        public static void Prefix(ref Vector3 __0) { __0 = CalculateDirection(__0); }

        // Gets the center of the eye (camera)
        private static GameObject cameraObj;
        private static GameObject CameraObj
        {
            get
            {
                if (cameraObj == null) 
                    cameraObj = Resources.FindObjectsOfTypeAll<NeckMouseRotator>()[0]
                                    .transform.Find(
                                        Environment.CurrentDirectory.Contains("vrchat-vrchat") ? "CenterEyeAnchor" : "Camera (eye)")
                                    .gameObject;
                return cameraObj;
            }
        }

        // Fixes the game's original direction to match the preferred one
        private static Vector3 CalculateDirection(Vector3 rawVelo)
        {
            var zInput = CameraObj.transform.forward;
            var badRot = Quaternion.LookRotation(
                            Vector3.Cross(
                                Vector3.Cross(Vector3.up, zInput), 
                                Vector3.up));
            var inputDirection = Quaternion.Inverse(badRot) * rawVelo;
            return Quaternion.FromToRotation(CameraObj.transform.up, Vector3.up) *
                       CameraObj.transform.rotation *
                       inputDirection;
        }
    }
}