using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.XR;

namespace BetterPortalPlacement.Utils
{
    // AssetBundle stuff I got from Knah https://github.com/knah/VRCMods/blob/master/SparkleBeGone/SparkleBeGoneMod.cs
    internal class PortalPtr : MonoBehaviour
    {
        public PortalPtr(IntPtr obj0) : base(obj0) { }
        public static readonly float defaultLength = Single.PositiveInfinity;
        public Vector3 position = Vector3.zero;
        public AudioSource audio;
        private GameObject previewObj;
        private LineRenderer lineRenderer;
        private LineRenderer RightHandLR;
        private Texture2D myWhiteLaserTexture;

        private void Awake()
        {
            SetupPreviewObj();
            SetupAudioSource();
        }

        private void OnEnable() => ToggleOnOff(true);

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

        private void OnDisable() => ToggleOnOff(false);

        [HideFromIl2Cpp]
        private void ToggleOnOff(bool IsOn)
        {
            if (XRDevice.isPresent)
            {
                VRUtils.active = IsOn;
                lineRenderer.enabled = IsOn;
            }
            previewObj.SetActive(IsOn);
            if (IsOn) Patches.CloseMenu(true, false);
        }

        [HideFromIl2Cpp]
        private void SetupPreviewObj()
        {
            previewObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            previewObj.GetComponent<Collider>().enabled = false;
            previewObj.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            previewObj.transform.position = position;
            previewObj.name = "PortalPreview";
            DontDestroyOnLoad(previewObj);

            RightHandLR = Resources.FindObjectsOfTypeAll<LineRenderer>()
                .Where(lr => lr.gameObject.name.Contains("RightHandBeam")).First();

            previewObj.GetComponent<Renderer>().material = RightHandLR.GetMaterial();
            if (XRDevice.isPresent)
            {
                lineRenderer = previewObj.AddComponent<LineRenderer>();
                lineRenderer.material = RightHandLR.GetMaterial();
                lineRenderer.enabled = false;
            }

            LoadBundle();

            if (lineRenderer != null) lineRenderer.material.mainTexture = myWhiteLaserTexture;
            previewObj.GetComponent<Renderer>().material.mainTexture = myWhiteLaserTexture;
        }

        [HideFromIl2Cpp]
        private void LoadBundle()
        {
			using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BetterPortalPlacement.betterportalplacement");
			using var memStream = new MemoryStream((int)stream.Length);
			stream.CopyTo(memStream);
			var bundle = AssetBundle.LoadFromMemory_Internal(memStream.ToArray(), 0);
			myWhiteLaserTexture = bundle.LoadAsset_Internal("Assets/BetterPortalPlacement/sniper_beam_white.png", UnhollowerRuntimeLib.Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
			myWhiteLaserTexture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }

        [HideFromIl2Cpp]
        private void SetupAudioSource()
        {
            var audioManager = VRCAudioManager.field_Private_Static_VRCAudioManager_0;
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.field_Public_VRCUiPopupAlert_0.field_Public_AudioClip_0;
            audioSource.volume = 0.3f;
            audioSource.playOnAwake = false;
            audioSource.spatialize = false;
            audioSource.loop = false;
            audioSource.outputAudioMixerGroup = new[]
            {
                audioManager.field_Public_AudioMixerGroup_0,
                audioManager.field_Public_AudioMixerGroup_1,
                audioManager.field_Public_AudioMixerGroup_2
            }.Single(g => g.name == "UI");
            audio = audioSource;
        }

        [HideFromIl2Cpp]
        private void SetupColors(bool CanPlace = true)
        {
            Color color;
            if (CanPlace) color = Color.cyan;
            else color = Color.red;
            previewObj.GetComponent<Renderer>().material.SetColor("_TintColor", color);
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