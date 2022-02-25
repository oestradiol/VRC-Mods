using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using DesktopCamera.Utils;
using UnityEngine.XR;
using BuildInfo = DesktopCamera.BuildInfo;
using Main = DesktopCamera.Main;
using CameraMenu = MonoBehaviour1PublicBuToBuGaTMBuGaBuGaBuUnique;

[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: MelonInfo(typeof(Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

// This mod was firstly developed by nitro. and I continued
namespace DesktopCamera
{
    public static class BuildInfo
    {
        public const string Name = "DesktopCamera";
        public const string Author = "Davi & nitro.";
        public const string Version = "1.2.1";
    }

    internal static class Settings
    {
        //public static bool arrowKeysEnabled = true;
        //public static bool rotateAroundUserCamera = false;
        //public static bool moveCamera = false;
        //public static bool allowCameraMovement = false;
        public static bool cameraEnabled = false;
        //public static CameraUtils.CameraScale cameraScale = CameraUtils.CameraScale.Normal;
    }

    internal static class UIXManager { public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit; }

    public class Main : MelonMod
    {
        //public static MelonPreferences_Entry<float> CameraSpeed;
        //public static MelonPreferences_Entry<float> CameraSpeedAlt;
        internal static SingleButton CameraMovementButton;

        public override void OnApplicationStart()
        {
            //MelonPreferences.CreateCategory("DesktopCamera", "DesktopCamera");
            //CameraSpeed = MelonPreferences.CreateEntry("DesktopCamera", nameof(CameraSpeed), 5f, "Basic camera speed");
            //CameraSpeedAlt = MelonPreferences.CreateEntry("DesktopCamera", nameof(CameraSpeedAlt), 20f, "Alt camera speed (ALT pressed)");

            WaitForUiInit();

            MelonLogger.Msg("Successfully loaded!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                typeof(UIXManager).GetMethod("OnApplicationStart").Invoke(null, null);
            else
            {
                MelonLogger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
                static IEnumerator OnUiManagerInit()
                {
                    while (VRCUiManager.prop_VRCUiManager_0 == null)
                        yield return null;
                    VRChat_OnUiManagerInit();
                }
                MelonCoroutines.Start(OnUiManagerInit());
            }
        }

        //public override void OnUpdate() => MovementManager.KeysListener();

        public static void VRChat_OnUiManagerInit()
        {
            // Thank you JanNyaa (Janni9009#1751) <3
            //var qmBoxCollider = QuickMenu.prop_QuickMenu_0.GetComponent<BoxCollider>();
            //if (qmBoxCollider.size.y < 3768) qmBoxCollider.size += new Vector3(0f, 840f, 0f);
            //QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements/_CONTEXT/QM_Context_ToolTip/_ToolTipPanel/Text").GetComponent<Text>().supportRichText = true;

            SetupCameraMenu();
        }

        private static void SetupCameraMenu()
        {
            
            if (!XRDevice.isPresent)
            {
                var photoCameraButton = Resources.FindObjectsOfTypeAll<CameraMenu>()[0].transform.Find("Scrollrect/Viewport/VerticalLayoutGroup/Buttons/Button_PhotoCamera");
                var desktopCameraEnableDisableButton = UnityEngine.Object.Instantiate(photoCameraButton, photoCameraButton.parent).gameObject;
                desktopCameraEnableDisableButton.SetActive(true);
                desktopCameraEnableDisableButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                desktopCameraEnableDisableButton.GetComponent<Button>().onClick.AddListener(new Action(delegate
                {
                    Settings.cameraEnabled = !Settings.cameraEnabled;
                    //MyCameraMenu.gameObject.SetActive(Settings.cameraEnabled);
                    CameraUtils.SetCameraMode(Settings.cameraEnabled ? CameraUtils.CameraMode.Photo : CameraUtils.CameraMode.Off);
                }));

                //Remove StreamMode
                VRCUtils.GetUserCameraController().gameObject.transform.Find("ViewFinder/PhotoControls/Primary /ControlGroup_Main/Scroll View/Viewport/Content/StreamToggle").gameObject.SetActive(false);
            }

            /*var CameraMenu = QuickMenu.prop_QuickMenu_0.transform.Find("CameraMenu");
            var MyCameraMenu = UnityEngine.Object.Instantiate(new GameObject("DesktopCameraMenu"), CameraMenu).transform;
            MyCameraMenu.name = "DesktopCameraMenu";

            SingleButton cameraButton;
            if (XRDevice.isPresent)
                cameraButton = new SingleButton("Camera Status", "Camera Status\n<color=red>Off</color>", "Camera is On?", 0, 0, CameraMenu);
            else
            {
                cameraButton = new SingleButton("Camera", "Camera\n<color=red>Off</color>", "Toggles the Camera", 0, 0, CameraMenu);
                cameraButton.SetAction((Action)(() => {
                    Settings.cameraEnabled = !Settings.cameraEnabled;
                    MyCameraMenu.gameObject.SetActive(Settings.cameraEnabled);
                    cameraButton.SetText("Camera\n<color=" + (Settings.cameraEnabled ? "#ff73fa>On" : "red>Off") + "</color>");
                    CameraUtils.SetCameraMode(Settings.cameraEnabled ? CameraUtils.CameraMode.Photo : CameraUtils.CameraMode.Off);
                }));
            }

            var movementBehaviourButton = new SingleButton("MovementBehaviour", "Movement\nBehaviour\n<color=#ff73fa>None</color>", "Cycles the Camera's movement behaviour", 1, 0, MyCameraMenu);
            movementBehaviourButton.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    var cameraBehaviour = CameraUtils.GetCameraBehaviour();
                    string behaviour = "?";
                    switch (cameraBehaviour)
                    {
                        case CameraUtils.CameraBehaviour.None:
                            behaviour = "Smooth";
                            break;
                        case CameraUtils.CameraBehaviour.Smooth:
                            behaviour = "Look At";
                            break;
                        case CameraUtils.CameraBehaviour.LookAt:
                            behaviour = "None";
                            break;
                    }
                    movementBehaviourButton.SetText("Movement\nBehaviour\n<color=#ff73fa>" + behaviour + "</color>");
                    CameraUtils.CycleCameraBehaviour();
                }
            }));

            var movementSpaceButton = new SingleButton("MovementSpace", "Movement\nSpace\n<color=#ff73fa>Attached</color>", "Cycles the Camera's movement space", 2, 0, MyCameraMenu);
            movementSpaceButton.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    var cameraSpace = CameraUtils.GetCameraSpace();
                    string space = "?";
                    switch (cameraSpace)
                    {
                        case CameraUtils.CameraSpace.Attached:
                            space = "Local";
                            break;
                        case CameraUtils.CameraSpace.Local:
                            space = "World";
                            break;
                        case CameraUtils.CameraSpace.World:
                            space = "Attached";
                            break;
                    }
                    movementSpaceButton.SetText("Movement\nSpace\n<color=#ff73fa>" + space + "</color>");
                    CameraUtils.CycleCameraSpace();
                    if (CameraUtils.GetCameraSpace() == CameraUtils.CameraSpace.World) Settings.allowCameraMovement = true; else Settings.allowCameraMovement = false;
                }
            }));

            var pinMenuButton = new SingleButton("PinMenu", "Pin Menu\n<color=red>Off</color>", "Toggles the Pin menu", 0, 1, MyCameraMenu);
            pinMenuButton.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    CameraUtils.TogglePinMenu();
                    pinMenuButton.SetText("Pin Menu\n<color=" + (CameraUtils.GetPinsHolder().activeSelf ? "#ff73fa>On" : "red>Off") + "</color>");
                }
            }));

            var switchPinButton = new SingleButton("CyclePin", "Cycle Pin\n<color=#ff73fa>Pin 1</color>", "Cycles between 3 pins (aka profiles)", 1, 1, MyCameraMenu);
            switchPinButton.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    var currentPin = CameraUtils.GetCurrentPin();
                    string pin = "?";
                    int newPin = 1;
                    switch (currentPin)
                    {
                        case CameraUtils.Pin.Pin1:
                            newPin = 2;
                            pin = "Pin 2";
                            break;
                        case CameraUtils.Pin.Pin2:
                            newPin = 3;
                            pin = "Pin 3";
                            break;
                        case CameraUtils.Pin.Pin3:
                            newPin = 1;
                            pin = "Pin 1";
                            break;
                    }
                    switchPinButton.SetText("Cycle Pin\n<color=#ff73fa>" + pin + "</color>");
                    // Needed to initialize the buttons apparently
                    CameraUtils.TogglePinMenu();
                    CameraUtils.TogglePinMenu();
                    CameraUtils.SetPin(newPin);
                }
            }));

            var timer1Button = new SingleButton("Timer1", "Timer\n<color=#ff73fa>3s</color>", "Takes a picture after 3 seconds", 3, 0, MyCameraMenu);
            timer1Button.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    CameraUtils.TakePicture(3);
                }
            }));

            var timer2Button = new SingleButton("Timer2", "Timer\n<color=#ff73fa>5s</color>", "Takes a picture after 5 seconds", 3, 1, MyCameraMenu);
            timer2Button.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    CameraUtils.TakePicture(5);
                }
            }));

            var timer3Button = new SingleButton("Timer3", "Timer\n<color=#ff73fa>10s</color>", "Takes a picture after 10 seconds", 3, 2, MyCameraMenu);
            timer3Button.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    CameraUtils.TakePicture(10);
                }
            }));

            var cameraScaleButton = new SingleButton("CameraScale", "Camera\nScale\n<color=#ff73fa>Normal</color>", "Changes the Camera's scale", 2, 1, MyCameraMenu);
            cameraScaleButton.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    string scale = "?";
                    switch (Settings.cameraScale)
                    {
                        case CameraUtils.CameraScale.Normal:
                            scale = "Medium";
                            CameraUtils.GetViewFinder().transform.localScale = new Vector3(1.5f, 1f, 1.5f);
                            Settings.cameraScale = CameraUtils.CameraScale.Medium;
                            break;
                        case CameraUtils.CameraScale.Medium:
                            scale = "Big";
                            CameraUtils.GetViewFinder().transform.localScale = new Vector3(2f, 1f, 2f);
                            Settings.cameraScale = CameraUtils.CameraScale.Big;
                            break;
                        case CameraUtils.CameraScale.Big:
                            scale = "Normal";
                            CameraUtils.GetViewFinder().transform.localScale = new Vector3(1f, 1f, 1f);
                            Settings.cameraScale = CameraUtils.CameraScale.Normal;
                            break;
                    }
                    cameraScaleButton.SetText("Camera\nScale\n<color=#ff73fa>" + scale + "</color>");
                }
            }));

            var toggleArrowKeysButton = new SingleButton("ArrowKeys", "Arrow Keys\n<color=#ff73fa>On</color>",
                "Allows you to change the camera position\nand rotation using arrow keys and numpad keys\n<color=orange>(for more info check the GitHub page)</color>", 0, 2, MyCameraMenu);
            toggleArrowKeysButton.SetAction((Action)(() => {
                Settings.arrowKeysEnabled = !Settings.arrowKeysEnabled;
                toggleArrowKeysButton.SetText("Arrow Keys\n<color=" + (Settings.arrowKeysEnabled ? "#ff73fa>On" : "red>Off") + "</color>");
            }));

            var rotateAroundUserCameraButton = new SingleButton("RotateAroundUserCamera", "Rotate\nAround\nUser Camera\n<color=red>Off</color>",
                "Makes the camera rotate around the user's camera\ninstead of just saying bye bye\n<color=orange>(for more info check the GitHub page)</color>", 1, 2, MyCameraMenu);
            rotateAroundUserCameraButton.SetAction((Action)(() => {
                Settings.rotateAroundUserCamera = !Settings.rotateAroundUserCamera;
                rotateAroundUserCameraButton.SetText("Rotate\nAround\nUser Camera\n<color=" + (Settings.rotateAroundUserCamera ? "#ff73fa>On" : "red>Off") + "</color>");
            }));

            var toggleLockButton = new SingleButton("ToggleLock", "Lock\n<color=red>Off</color>", "Toggles the Lock (Camera pickup)", 4, -1, MyCameraMenu);
            toggleLockButton.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    toggleLockButton.SetText("Lock\n<color=" + (CameraUtils.GetViewFinder().GetComponent<VRC_Pickup>().pickupable ? "#ff73fa>On" : "red>Off") + "</color>");
                    CameraUtils.ToggleLock();
                }
            }));

            var gitHubButton = new SingleButton("GitHubPage", "<color=#ff73fa>" + "GitHub\nPage</color>",
                "Opens the GitHub page of the mod\n<color=#ff73fa><size=20><b>Mod created by nitro., maintained by Davi ♥</b></size></color>\nVersion: " + BuildInfo.Version, -1, -1, CameraMenu);
            gitHubButton.SetAction((Action)(() => {
                Application.OpenURL("https://github.com/d-mageek/VRC-Mods");
            }));

            CameraMovementButton = new SingleButton("ToggleCameraMovement", "Camera\nMovement\n<color=#ff73fa>Viewer</color>",
                "Toggles the arrow/numpad keys movement between the actual Camera and the Viewer\nViewer requires Movement Space to be \"World\" " +
                "<color=orange>(for more info check the GitHub page)</color>", 2, 2, MyCameraMenu);
            CameraMovementButton.SetAction((Action)(() => {
                if (Settings.cameraEnabled)
                {
                    Settings.moveCamera = !Settings.moveCamera;
                    CameraMovementButton.SetText("Camera\nMovement\n<color=#ff73fa>" + (Settings.moveCamera ? "Camera" : "Viewer") + "</color>");
                }
            }));*/

            //SetupFiltersMenu(MyCameraMenu);

            //SetupDefaultUiButtons(MyCameraMenu, cameraButton);

            //MyCameraMenu.gameObject.SetActive(false);
        }

        /*
        private static void SetupFiltersMenu(Transform MyCameraMenu)
        {
            var filtersMenu = UnityEngine.Object.Instantiate(MyCameraMenu.parent, QuickMenu.prop_QuickMenu_0.transform);
            filtersMenu.name = "FiltersMenu";

            for (var i = 0; i < filtersMenu.transform.childCount; i++)
            {
                var child = filtersMenu.transform.GetChild(i);
                if (child.name == "BackButton")
                {
                    child.localPosition = SingleButton.GetButtonPositionFor(4, 2);
                    child.GetComponent<UiTooltip>().field_Public_String_0 = "Go Back to the Camera Menu";
                    child.GetComponent<Button>().onClick.RemoveAllListeners();
                    child.GetComponent<Button>().onClick.AddListener((Action)(() => {
                        VRCUtils.ShowQuickMenuPage(QuickMenu.prop_QuickMenu_0, MyCameraMenu.parent, "FiltersMenu");
                    }));
                }
                else UnityEngine.Object.Destroy(child.gameObject);
            }

            var filters = new Dictionary<string, string>()
            {
                { "<color=#ff73fa>None</color>", "button-NONE" },
                { "<color=#00a0ff>Blueprint</color>", "Button-Blueprint" },
                { "<color=#20c20e>Code</color>", "Button-Code" },
                { "<color=#ffee7b>Sparkles</color>", "Button-Sparkles" },
                { "<color=#00ff23>Green\nScreen</color>", "Button-GreenScreen" },
                { "<color=#ff0000>G</color><color=#00ff00>l</color><color=#0000ff>i</color><color=#ff0000>t</color><color=#00ff00>c</color><color=#0000ff>h</color>", "Button-Glitch" }, // Sorry
                { "<color=#cd853f>Old Timey</color>", "Button-OLD-TIMEY" },
                { "<color=#a9a9a9>Drawing</color>", "Button-Drawing" },
                { "<color=white><i>Trippy</i></color>", "Button-Trippy" },
                { "<color=#ffffffcc>Local\nAlpha</color>", "Button-LocalAlpha" },
                { "<color=#ffffffcc>Alpha\nTransparent</color>", "Button-ALPHA" },
                { "<color=white>Pixelate</color>", "Button-PIXELS" }
            };

            int row = 0;
            int position = 0;
            foreach (var filter in filters)
            {
                var button = new SingleButton("Filter" + filter.Value, filter.Key, "Sets the filter to " + filter.Key.Replace("\n", " "), position, row, filtersMenu);
                button.SetAction((Action)(() => {
                    if (Settings.cameraEnabled)
                    {
                        // Needed to initialize the buttons apparently
                        CameraUtils.ToggleFilterMenu();
                        CameraUtils.ToggleFilterMenu();
                        CameraUtils.SetFilter(filter.Value);
                    }
                }));
                position++;
                if (position == 4)
                {
                    position = 0;
                    row++;
                }
            }

            var filtersButton = new SingleButton("Filters", "Filters", "Opens the filter menu", 4, 0, MyCameraMenu);
            filtersButton.SetAction((Action)(() => {
                VRCUtils.ShowQuickMenuPage(QuickMenu.prop_QuickMenu_0, filtersMenu, MyCameraMenu.parent.name);
            }));
        }

        private static void SetupDefaultUiButtons(Transform MyCameraMenu, SingleButton cameraButton)
        {
            var panoramaButton = MyCameraMenu.parent.Find("Panorama");
            panoramaButton.localPosition = SingleButton.GetButtonPositionFor(-1, 0);

            var vrChiveButton = MyCameraMenu.parent.Find("VRChive");
            vrChiveButton.localPosition = SingleButton.GetButtonPositionFor(-1, 1);

            var smoothCameraButton = MyCameraMenu.parent.Find("SmoothFPVCamera");
            smoothCameraButton.localPosition = SingleButton.GetButtonPositionFor(0, 3);

            var photoModeButton = MyCameraMenu.parent.Find("PhotoMode");
            photoModeButton.localPosition = SingleButton.GetButtonPositionFor(1, 3);
            photoModeButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            photoModeButton.GetComponent<Button>().onClick.AddListener((Action)(() => {
                Settings.cameraEnabled = true;
                cameraButton.SetText("Camera Status\n<color=#ff73fa>On</color>");
                MyCameraMenu.gameObject.SetActive(true);
                CameraUtils.SetCameraMode(CameraUtils.CameraMode.Photo);
            }));

            var videoModeButton = MyCameraMenu.parent.Find("VideoMode");
            videoModeButton.localPosition = SingleButton.GetButtonPositionFor(2, 3);
            videoModeButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            videoModeButton.GetComponent<Button>().onClick.AddListener((Action)(() => {
                Settings.cameraEnabled = true;
                cameraButton.SetText("Camera Status\n<color=#ff73fa>On</color>");
                MyCameraMenu.gameObject.SetActive(true);
                CameraUtils.SetCameraMode(CameraUtils.CameraMode.Video);
            }));

            var disableCameraButton = MyCameraMenu.parent.Find("DisableCamera");
            disableCameraButton.localPosition = SingleButton.GetButtonPositionFor(3, 3);
            disableCameraButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            disableCameraButton.GetComponent<Button>().onClick.AddListener((Action)(() => {
                Settings.cameraEnabled = false;
                cameraButton.SetText("Camera Status\n<color=red>Off</color>");
                MyCameraMenu.gameObject.SetActive(false);
                CameraUtils.SetCameraMode(CameraUtils.CameraMode.Off);
            }));

            var screenshotButton = MyCameraMenu.parent.Find("Screenshot");
            screenshotButton.localPosition = SingleButton.GetButtonPositionFor(4, 1);

            var backButton = MyCameraMenu.parent.Find("BackButton");
            backButton.localPosition = SingleButton.GetButtonPositionFor(4, 2);

            // "Create A Developer Light" the fuck even is this
            var lightButton = MyCameraMenu.parent.Find("Light");
            lightButton.localPosition = SingleButton.GetButtonPositionFor(4, 3);
        }
        */
    }
}