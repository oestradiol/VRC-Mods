using System;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.XR;

namespace BetterPortalPlacement.Utils
{
    internal class PortalPtr : MonoBehaviour
    {
        public static readonly float defaultLength = 3.0f;
        public Vector3 position = Vector3.zero;
        private GameObject previewObj;

        public PortalPtr(IntPtr obj0) : base(obj0)
        {
        }

        private void Awake()
        {
            previewObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DontDestroyOnLoad(previewObj);
            previewObj.GetComponent<Collider>().enabled = false;
            previewObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            previewObj.transform.position = position;
        }

        private void OnEnable()
        {
            if (XRDevice.isPresent) VRUtils.active = true;
            Utilities.CloseMenu(true, false);
            previewObj.SetActive(true);
        }

        private void Update()
        {
            var endPos = CalculateEndPoint();
            previewObj.transform.position = endPos;
            position = endPos;
            // SetPortalColor();
            if (Input.GetKeyUp(KeyCode.Mouse0)) Main.RecreatePortal();
        }

        private void OnDisable()
        {
            if (XRDevice.isPresent) VRUtils.active = false;
            previewObj.SetActive(false);
        }

        [HideFromIl2Cpp]
        private Vector3 CalculateEndPoint()
        {
            var hit = XRDevice.isPresent ? VRUtils.RaycastVR() : Raycast();
            return hit.collider ? hit.point : DefaultPos();
        }

        [HideFromIl2Cpp]
        private RaycastHit Raycast()
        {
            Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, defaultLength);
            return hit;
        }

        [HideFromIl2Cpp]
        private Vector3 DefaultPos()
        {
            return XRDevice.isPresent?
            VRUtils.ray.origin + VRUtils.ray.direction * defaultLength : transform.position + transform.forward * defaultLength;
        }
    }
}