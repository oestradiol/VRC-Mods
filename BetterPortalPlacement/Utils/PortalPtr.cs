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
        public const float DefaultLength = float.PositiveInfinity;
        public Vector3 position = Vector3.zero;
        public AudioSource audio;
        private GameObject _previewObj;
        private LineRenderer _lineRenderer;
        private LineRenderer _rightHandLr;
        private Texture2D _myWhiteLaserTexture;
        private static readonly int TintColor = Shader.PropertyToID("_TintColor");

        private void Awake()
        {
            SetupPreviewObj();
            SetupAudioSource();
        }

        private void OnEnable() => ToggleOnOff(true);

        private void Update()
        {
            var endPos = CalculateEndPoint();
            _previewObj.transform.position = endPos;
            position = endPos;
            if (_lineRenderer != null)
            {
                if (Math.Abs(_lineRenderer.startWidth - _rightHandLr.startWidth) > 0.001f) _lineRenderer.SetWidth(_rightHandLr.startWidth, _rightHandLr.endWidth);
                _lineRenderer.SetPosition(0, endPos);
                _lineRenderer.SetPosition(1, VRUtils.GetControllerTransform().position);
            }
            SetupColors(Main.CanPlace());
            if (Input.GetKeyUp(KeyCode.Mouse0)) Main.RecreatePortal();
        }

        private void OnDisable() => ToggleOnOff(false);

        [HideFromIl2Cpp]
        private void ToggleOnOff(bool isOn)
        {
            if (XRDevice.isPresent)
            {
                VRUtils.Active = isOn;
                _lineRenderer.enabled = isOn;
            }
            _previewObj.SetActive(isOn);
            if (isOn) Patches.CloseMenu(true, false);
        }

        [HideFromIl2Cpp]
        private void SetupPreviewObj()
        {
            _previewObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _previewObj.GetComponent<Collider>().enabled = false;
            _previewObj.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            _previewObj.transform.position = position;
            _previewObj.name = "PortalPreview";
            DontDestroyOnLoad(_previewObj);

            _rightHandLr = Resources.FindObjectsOfTypeAll<LineRenderer>()
                .First(lr => lr.gameObject.name.Contains("RightHandBeam"));

            _previewObj.GetComponent<Renderer>().material = _rightHandLr.GetMaterial();
            if (XRDevice.isPresent)
            {
                _lineRenderer = _previewObj.AddComponent<LineRenderer>();
                _lineRenderer.material = _rightHandLr.GetMaterial();
                _lineRenderer.enabled = false;
            }

            LoadBundle();

            if (_lineRenderer != null) _lineRenderer.material.mainTexture = _myWhiteLaserTexture;
            _previewObj.GetComponent<Renderer>().material.mainTexture = _myWhiteLaserTexture;
        }

        [HideFromIl2Cpp]
        private void LoadBundle()
        { 
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BetterPortalPlacement.betterportalplacement");
            if (stream == null) return;
            var memStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memStream);
            var bundle = AssetBundle.LoadFromMemory_Internal(memStream.ToArray(), 0);
            _myWhiteLaserTexture = bundle.LoadAsset_Internal("Assets/BetterPortalPlacement/sniper_beam_white.png", UnhollowerRuntimeLib.Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            _myWhiteLaserTexture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
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
        private void SetupColors(bool canPlace = true)
        {
            var color = canPlace ? Color.cyan : Color.red;
            _previewObj.GetComponent<Renderer>().material.SetColor(TintColor, color);
            if (_lineRenderer != null) _lineRenderer.SetColors(color, color);
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
            var transform1 = transform;
            Physics.Raycast(new Ray(transform1.position, transform1.forward), out RaycastHit hit, DefaultLength);
            return hit;
        }

        [HideFromIl2Cpp]
        private Vector3 DefaultPos()
        {
            var transform1 = transform;
            return XRDevice.isPresent?
            VRUtils.Ray.origin + VRUtils.Ray.direction * DefaultLength : transform1.position + transform1.forward * DefaultLength;
        }
    }
}