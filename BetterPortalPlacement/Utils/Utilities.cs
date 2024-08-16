using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.XR;
using VRC.Core;

namespace BetterPortalPlacement.Utils
{
    internal sealed class PortalInfo
    {
        public readonly ApiWorld ApiWorld;
        public readonly ApiWorldInstance ApiWorldInstance;
        public readonly Il2CppSystem.Action<string> WhateverThisIs;
        public PortalInfo(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Il2CppSystem.Action<string> whateverThisIs)
        {
            ApiWorld = apiWorld;
            ApiWorldInstance = apiWorldInstance;
            WhateverThisIs = whateverThisIs;
        }
    }

    internal static class Utilities
    {
        private static VRC.UI.Elements.QuickMenu _cachedQuickMenu;
        public static bool IsQmRightHanded =>
                (_cachedQuickMenu ??= Resources.FindObjectsOfTypeAll<VRC.UI.Elements.QuickMenu>()[0]).prop_Boolean_0;

        public static GameObject GetPtrObj() 
        {
            var trackingManager = VRCTrackingManager.field_Private_Static_VRCTrackingManager_0;
            if (!XRDevice.isPresent)
            {
                return trackingManager.GetComponentInChildren<NeckMouseRotator>()
                    .transform.Find(Environment.CurrentDirectory.Contains("vrchat-vrchat") ? "CenterEyeAnchor" : "Camera (head)/Camera (eye)").gameObject;
            }
            return trackingManager.gameObject;
        }

        // https://github.com/Psychloor/AdvancedInvites/blob/9675094a24fa9ceb33b07571abbbad6bd8e47ac0/AdvancedInvites/Utilities.cs#L405
        public static bool ContainsStr(MethodBase methodBase, string match)
        {
            try
            {
                return XrefScanner.XrefScan(methodBase)
                    .Any(instance => instance.Type == XrefType.Global && 
                         instance.ReadAsObject()?.ToString().IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0);
            } catch { return false; }
        }

        public static bool WasUsedBy(MethodBase methodBase, string methodName)
        {
            try
            {
                return XrefScanner.UsedBy(methodBase)
                    .Any(instance => instance.TryResolve() != null &&
                         instance.TryResolve().Name.Equals(methodName, StringComparison.Ordinal));
            } catch { return false; }
        }
    }

    //Got some good ideas from Lily for this one!, Lily 🤝 Me. https://github.com/KortyBoi/VRChat-TeleporterVR/blob/main/Utils/VRUtils.cs
    internal static class VRUtils 
    {
        public static bool Active;
        public static Ray Ray;
        private const string RightTrigger = "Oculus_CrossPlatform_SecondaryIndexTrigger";
        private const string LeftTrigger = "Oculus_CrossPlatform_PrimaryIndexTrigger";
        private static GameObject _controllerLeft, _controllerRight;
        private static bool _a, _b;
        private static Controller _controller;
        private static bool GetTriggerIsDown(string trigger) => Input.GetButtonDown(trigger) ||
                                                                Input.GetAxisRaw(trigger) != 0 ||
                                                                Input.GetAxis(trigger) >= 0.75f;
        private static bool? TriggerIsDown
        {
            get
            {
                if (GetTriggerIsDown(RightTrigger)) return true; 
                if (GetTriggerIsDown(LeftTrigger)) return false;
                return null;
            }
        }
        private enum Controller
        {
            Right,
            Left
        }

        public static void VRChat_OnUiManagerInit()
        {
            if (Environment.CurrentDirectory.Contains("vrchat-vrchat"))
            {
                _controllerRight = GameObject.Find("/_Application/TrackingVolume/TrackingOculus(Clone)/OVRCameraRig/TrackingSpace/RightHandAnchor/PointerOrigin (1)");
                _controllerLeft = GameObject.Find("/_Application/TrackingVolume/TrackingOculus(Clone)/OVRCameraRig/TrackingSpace/LeftHandAnchor/PointerOrigin (1)");
            }
            else
            {
                _controllerRight = GameObject.Find("/_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (right)/PointerOrigin");
                _controllerLeft = GameObject.Find("/_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (left)/PointerOrigin");
            }
        }

        public static void OnUpdate()
        {
            if (!Active) return;
            if (_a && TriggerIsDown == true)
            {
                _a = false;
                if (_controller == Controller.Right) Main.RecreatePortal();
                else _controller = Controller.Right;
            }
            else if (_b && TriggerIsDown == false)
            {
                _b = false;
                if (_controller == Controller.Left) Main.RecreatePortal();
                else _controller = Controller.Left;
            }
            else if ((!_a || !_b) && TriggerIsDown == null)
            {
                _a = true;
                _b = true;
            }
        }

        public static void OnQMDisable()
        {
            if (!XRDevice.isPresent) return;
            _controller = !Utilities.IsQmRightHanded ? Controller.Right : Controller.Left;
        }

        public static Transform GetControllerTransform() => _controller == Controller.Right ? _controllerRight.transform : _controllerLeft.transform;

        public static RaycastHit RaycastVR()
        {
            Ray = new Ray(GetControllerTransform().position, GetControllerTransform().forward);
            Physics.Raycast(Ray, out var hit, PortalPtr.DefaultLength);
            return hit;
        }
    }
}