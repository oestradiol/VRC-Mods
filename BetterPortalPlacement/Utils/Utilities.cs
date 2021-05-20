using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using MelonLoader;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.XR;
using VRC.Core;

namespace BetterPortalPlacement.Utils
{
    internal sealed class PortalInfo
    {
        public PortalInfo(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 position, Vector3 forward, bool withUIErrors)
        {
            ApiWorld = apiWorld;
            ApiWorldInstance = apiWorldInstance;
            Position = position;
            Forward = forward;
            WithUIErrors = withUIErrors;
        }
        public ApiWorld ApiWorld { get; }
        public ApiWorldInstance ApiWorldInstance { get; }
        public Vector3 Position { get; }
        public Vector3 Forward { get; }
        public bool WithUIErrors { get; }
    }

    // Almost this entire class came from Gompog :) // I'm going to bonk you one day - Gompo // Please don't? Love you x3 <3
    internal static class Utilities 
    {
        public static bool IsQMRightHanded => QuickMenu.prop_QuickMenu_0.prop_Boolean_1;
        
        private static CloseMenuDelegate GetCloseMenuDelegate
        {
            get
            {
                
                if (closeMenuDelegate != null) return closeMenuDelegate;
                MethodInfo closeMenuMethod = typeof(VRCUiManager).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !method.Name.Contains("_PDM_"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();
                closeMenuDelegate = (CloseMenuDelegate)Delegate.CreateDelegate(
                    typeof(CloseMenuDelegate),
                    VRCUiManager.prop_VRCUiManager_0,
                    closeMenuMethod);
                return closeMenuDelegate;
            }
        }
        public static void CloseMenu(bool __0, bool __1) => GetCloseMenuDelegate(__0, __1);
        private static CloseMenuDelegate closeMenuDelegate;
        private delegate void CloseMenuDelegate(bool __0, bool __1);
        
        private static CreatePortalDelegate GetCreatePortalDelegate
        {
            get
            {
                if (createPortalDelegate != null) return createPortalDelegate;
                createPortalDelegate = (CreatePortalDelegate)Delegate.CreateDelegate(
                    typeof(CreatePortalDelegate),
                    null,
                    CreatePortalMethod);
                return createPortalDelegate;
            }
        }
        public static bool CreatePortal(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 foward, bool someBool) => 
            GetCreatePortalDelegate(apiWorld, apiWorldInstance, pos, foward, someBool);
        private static CreatePortalDelegate createPortalDelegate;
        private delegate bool CreatePortalDelegate(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 foward, bool someBool);


        private static MethodInfo CreatePortalMethod
        {
            get
            {
                return typeof(PortalInternal).GetMethod(nameof(PortalInternal.Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_Vector3_Vector3_Boolean_0));
            }
        }
        
        public static void ApplyPatches()
        {
            Main.HarmonyInstance.Patch(CreatePortalMethod, new HarmonyMethod(typeof(Main).GetMethod(nameof(Main.OnPortalCreated))));
            Main.HarmonyInstance.Patch(typeof(VRCUiPopupManager).GetMethod(nameof(VRCUiPopupManager.Method_Public_Void_String_String_Single_0)), 
                new HarmonyMethod(typeof(Main).GetMethod(nameof(Main.ShowAlert))));
        }

        public static GameObject GetPtrObj() 
        {
            var TrackingManager = VRCTrackingManager.field_Private_Static_VRCTrackingManager_0;
            if (!XRDevice.isPresent)
            {
                    return TrackingManager.GetComponentInChildren<NeckMouseRotator>()
                        .transform.Find("Camera (head)/Camera (eye)").gameObject;
            }
            return TrackingManager.gameObject;
        }
    }

    //Got some good ideas from Lily for this one!, Lily 🤝 Me. https://github.com/KortyBoi/VRChat-TeleporterVR/blob/main/Utils/VRUtils.cs
    internal static class VRUtils 
    {
        public static bool active;
        public static Ray ray;
        private const string RightTrigger = "Oculus_CrossPlatform_SecondaryIndexTrigger";
        private const string LeftTrigger = "Oculus_CrossPlatform_PrimaryIndexTrigger";
        private static GameObject ControllerLeft, ControllerRight;
        private static bool a, b;
        public static Controller controller;
        private static bool? TriggerIsDown
        {
            get
            {
                if (Input.GetButtonDown(RightTrigger) || Input.GetAxisRaw(RightTrigger) != 0 || Input.GetAxis(RightTrigger) >= 0.75f) return true;
                else if (Input.GetButtonDown(LeftTrigger) || Input.GetAxisRaw(LeftTrigger) != 0 || Input.GetAxis(LeftTrigger) >= 0.75f) return false;
                else return null;
            }
        }
        public enum Controller
        {
            Right,
            Left
        }

        public static void VRChat_OnUiManagerInit()
        {
            if (Environment.CurrentDirectory.Contains("vrchat-vrchat"))
            {
                ControllerRight = GameObject.Find("/_Application/TrackingVolume/TrackingOculus(Clone)/OVRCameraRig/TrackingSpace/RightHandAnchor/PointerOrigin (1)");
                ControllerLeft = GameObject.Find("/_Application/TrackingVolume/TrackingOculus(Clone)/OVRCameraRig/TrackingSpace/LeftHandAnchor/PointerOrigin (1)");
                MelonLogger.Msg(ConsoleColor.Blue, "Binds set: Oculus");
            }
            else
            {
                ControllerRight = GameObject.Find("/_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (right)/PointerOrigin");
                ControllerLeft = GameObject.Find("/_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (left)/PointerOrigin");
            }
        }

        public static void OnUpdate()
        {
            if (!active) return;
            if (a && TriggerIsDown == true)
            {
                a = false;
                if (controller == Controller.Right) Main.RecreatePortal();
                else controller = Controller.Right;
            }
            else if (b && TriggerIsDown == false)
            {
                b = false;
                if (controller == Controller.Left) Main.RecreatePortal();
                else controller = Controller.Left;
            }
            else if (!a || !b && TriggerIsDown == null)
            {
                a = true;
                b = true;
            }
        }

        public static void OnQMDisable()
        {
            if (XRDevice.isPresent)
            {
                if (!Utilities.IsQMRightHanded) controller = Controller.Right;
                else controller = Controller.Left;
            }
        }

        public static Transform GetControllerTransform() => controller == Controller.Right ? ControllerRight.transform : ControllerLeft.transform;

        public static RaycastHit RaycastVR()
        {
            ray = new Ray(GetControllerTransform().position, GetControllerTransform().forward);
            Physics.Raycast(ray, out RaycastHit hit, PortalPtr.defaultLength);
            return hit;
        }
    }
}