using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.XR;
using VRC.Core;
using NeckMouseRotator = MonoBehaviourPublicObSiBoSiVeBoQuVeBoSiUnique;

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

    internal static class Utilities
    {
        private static VRC.UI.Elements.QuickMenu cachedQuickMenu;
        public static bool IsQMRightHanded {
            get
            {
                cachedQuickMenu ??= Resources.FindObjectsOfTypeAll<VRC.UI.Elements.QuickMenu>()[0];
                return cachedQuickMenu.prop_Boolean_0;
            }
        }

        public static GameObject GetPtrObj() 
        {
            var TrackingManager = VRCTrackingManager.field_Private_Static_VRCTrackingManager_0;
            if (!XRDevice.isPresent)
            {
                return TrackingManager.GetComponentInChildren<NeckMouseRotator>()
                    .transform.Find(Environment.CurrentDirectory.Contains("vrchat-vrchat") ? "CenterEyeAnchor" : "Camera (head)/Camera (eye)").gameObject;
            }
            return TrackingManager.gameObject;
        }

        // https://github.com/Psychloor/AdvancedInvites/blob/9675094a24fa9ceb33b07571abbbad6bd8e47ac0/AdvancedInvites/Utilities.cs#L405
        public static bool ContainsStr(MethodBase methodBase, string match)
        {
            try
            {
                return XrefScanner.XrefScan(methodBase)
                    .Any(instance => instance.Type == XrefType.Global && 
                         instance.ReadAsObject()?.ToString().IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0);
            } catch { }
            return false;
        }

        public static bool WasUsedBy(MethodBase methodBase, string methodName)
        {
            try
            {
                return XrefScanner.UsedBy(methodBase)
                    .Any(instance => instance.TryResolve() != null &&
                         instance.TryResolve().Name.Equals(methodName, StringComparison.Ordinal));
            } catch { }
            return false;
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
            if (a && (TriggerIsDown == true))
            {
                a = false;
                if (controller == Controller.Right) Main.RecreatePortal();
                else controller = Controller.Right;
            }
            else if (b && (TriggerIsDown == false))
            {
                b = false;
                if (controller == Controller.Left) Main.RecreatePortal();
                else controller = Controller.Left;
            }
            else if ((!a || !b) && (TriggerIsDown == null))
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