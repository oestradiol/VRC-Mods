using System;
using System.Linq;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.XR;

namespace BetterPortalPlacement.Utils
{
    internal class PortalPtr : MonoBehaviour
    {
        public PortalPtr(IntPtr obj0) : base(obj0) { }
        public static readonly float defaultLength = 3.0f;
        public Vector3 position = Vector3.zero;
        private LineRenderer lineRenderer;
        private LineRenderer RightHandLR;
        private GameObject previewObj;

        private void Awake()
        {
            previewObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (XRDevice.isPresent)
            {
                RightHandLR = Resources.FindObjectsOfTypeAll<LineRenderer>()
                    .Where(lr => lr.gameObject.name.Contains("RightHandBeam")).First();
                lineRenderer = previewObj.AddComponent<LineRenderer>();
                lineRenderer.material = RightHandLR.GetMaterial();
                lineRenderer.enabled = false;
            }
            DontDestroyOnLoad(previewObj);
            previewObj.GetComponent<Collider>().enabled = false;
            //previewObj.GetComponent<Renderer>().material = RightHandLR.GetMaterial();
            previewObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            previewObj.transform.position = position;
            previewObj.name = "PortalPreview";
            SetupColors();
        }

        private void OnEnable()
        {
            if (XRDevice.isPresent)
            {
                VRUtils.active = true;
                lineRenderer.enabled = true;
            }
            Utilities.CloseMenu(true, false);
            previewObj.SetActive(true);
        }

        private void Update()
        {
            var endPos = CalculateEndPoint();
            previewObj.transform.position = endPos;
            position = endPos;
            if (lineRenderer != null)
            {
                if (lineRenderer.startWidth != RightHandLR.startWidth) lineRenderer.SetWidth(RightHandLR.startWidth, RightHandLR.endWidth);
                lineRenderer.SetPosition(0, endPos);
                lineRenderer.SetPosition(1, VRUtils.GetControllerTransform().position);
            }
            SetupColors(Main.CanPlace());
            if (Input.GetKeyUp(KeyCode.Mouse0)) Main.RecreatePortal();
        }

        private void OnDisable()
        {
            if (XRDevice.isPresent)
            {
                VRUtils.active = false;
                lineRenderer.enabled = false;
            }
            previewObj.SetActive(false);
        }

        [HideFromIl2Cpp]
        private void SetupColors(bool CanPlace = true)
        {
            Color color;
            if (CanPlace) color = Color.cyan;
            else color = Color.red;
            //previewObj.GetComponent<Renderer>().material.color = color; // This failed sadly
            if (lineRenderer != null) lineRenderer.SetColors(color, color);
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