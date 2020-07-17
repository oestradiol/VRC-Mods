using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DesktopCamera.Buttons;
using DesktopCamera.Utils;
using MelonLoader;
using UnityEngine.Networking;
using Il2CppSystem.Text;
using Newtonsoft.Json;
using System.Collections;
using VRC.SDKBase;

namespace DesktopCamera {

    public static class ModBuildInfo {
        public const string Name = "DesktopCamera";
        public const string Author = "nitro.";
        public const string Company = null;
        public const string Version = "1.0.9";
        public const string DownloadLink = "https://github.com/nitrog0d/DesktopCamera/releases/latest/download/DesktopCamera.dll";
        public const string GameDeveloper = "VRChat";
        public const string Game = "VRChat";
    }

    public class VersionCheckResponse {
        public string result { get; set; }
        public string latest { get; set; }
    }

    public class Main : MelonMod {

        public override void OnApplicationStart() {
            MelonModLogger.Log("Mod loaded.");
        }

        public override void VRChat_OnUiManagerInit() {
            MelonCoroutines.Start(Setup());
        }

        private SingleButton cameraMovementButton = null;

        private IEnumerator Setup() {
            var request = new UnityWebRequest("https://vrchat.nitro.moe/mods/versioncheck", "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{\"name\":\"" + ModBuildInfo.Name + "\",\"version\":\"" + ModBuildInfo.Version + "\"}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            //yield return request.SendWebRequest();

            var asyncOperation = request.SendWebRequest();

            // yield return doesn't work for now, so I had to change it to this.
            while (!asyncOperation.isDone) {
                yield return new WaitForEndOfFrame();
            }

            bool updated = true;
            string latest = "";

            if (!request.isNetworkError && !request.isHttpError) {
                try {
                    var response = JsonConvert.DeserializeObject<VersionCheckResponse>(request.downloadHandler.text);
                    if (response.result == "OUTDATED") {
                        updated = false;
                        latest = response.latest;
                    }
                } catch (Exception) { }
            }

            var quickMenu = VRCUtils.GetQuickMenu();
            if (!updated) quickMenu.transform.Find("ShortcutMenu/CameraButton").GetComponentInChildren<Text>().text = "Camera\n<color=lime>Update\navailable!</color>";
            var cameraMenu = quickMenu.transform.Find("CameraMenu");

            var filtersMenu = UnityEngine.Object.Instantiate(cameraMenu, quickMenu.transform);
            filtersMenu.name = "FiltersMenu";

            var panoramaButton = cameraMenu.Find("Panorama");
            panoramaButton.localPosition = SingleButton.getButtonPositionFor(-1, 0);

            var vrChiveButton = cameraMenu.Find("VRChive");
            vrChiveButton.localPosition = SingleButton.getButtonPositionFor(-1, 1);

            var backButton = cameraMenu.Find("BackButton");
            backButton.localPosition = SingleButton.getButtonPositionFor(4, 2);

            var screenshotButton = cameraMenu.Find("Screenshot");
            screenshotButton.localPosition = SingleButton.getButtonPositionFor(4, 1);

            // Thank you Janni9009#1751 <3
            quickMenu.GetComponent<BoxCollider>().size += new Vector3(0f, 840f, 0f);

            var photoModeButton = cameraMenu.Find("PhotoMode");
            photoModeButton.localPosition = SingleButton.getButtonPositionFor(0, 3);

            var videoModeButton = cameraMenu.Find("VideoMode");
            videoModeButton.localPosition = SingleButton.getButtonPositionFor(1, 3);

            var disableCameraButton = cameraMenu.Find("DisableCamera");
            disableCameraButton.localPosition = SingleButton.getButtonPositionFor(2, 3);


            var cameraButton = new SingleButton("Camera", "Camera\n<color=red>Off</color>", "Toggles the Camera", 0, 0, cameraMenu);
            cameraButton.setAction((Action)(() => {
                Settings.cameraEnabled = !Settings.cameraEnabled;
                cameraButton.setText("Camera\n<color=" + (Settings.cameraEnabled ? "#845bff>On" : "red>Off") + "</color>");
                CameraUtils.SetCameraMode(Settings.cameraEnabled ? CameraUtils.CameraMode.Photo : CameraUtils.CameraMode.Off);
            }));

            var movementBehaviourButton = new SingleButton("MovementBehaviour", "Movement\nBehaviour\n<color=#845bff>None</color>", "Cycles the Camera's movement behaviour", 1, 0, cameraMenu);
            movementBehaviourButton.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    var cameraBehaviour = CameraUtils.GetCameraBehaviour();
                    string behaviour = "?";
                    switch (cameraBehaviour) {
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
                    movementBehaviourButton.setText("Movement\nBehaviour\n<color=#845bff>" + behaviour + "</color>");
                    CameraUtils.CycleCameraBehaviour();
                }
            }));

            var movementSpaceButton = new SingleButton("MovementSpace", "Movement\nSpace\n<color=#845bff>Attached</color>", "Cycles the Camera's movement space", 2, 0, cameraMenu);
            movementSpaceButton.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    var cameraSpace = CameraUtils.GetCameraSpace();
                    string space = "?";
                    switch (cameraSpace) {
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
                    movementSpaceButton.setText("Movement\nSpace\n<color=#845bff>" + space + "</color>");
                    CameraUtils.CycleCameraSpace();
                    if (CameraUtils.GetCameraSpace() == CameraUtils.CameraSpace.World) Settings.allowCameraMovement = true; else Settings.allowCameraMovement = false;
                }
            }));

            var pinMenuButton = new SingleButton("PinMenu", "Pin Menu\n<color=red>Off</color>", "Toggles the Pin menu", 0, 1, cameraMenu);
            pinMenuButton.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    CameraUtils.TogglePinMenu();
                    pinMenuButton.setText("Pin Menu\n<color=" + (VRCUtils.GetUserCameraController().pinsHolder.activeSelf ? "#845bff>On" : "red>Off") + "</color>");
                }
            }));

            var switchPinButton = new SingleButton("CyclePin", "Cycle Pin\n<color=#845bff>Pin 1</color>", "Cycles between 3 pins (aka profiles)", 1, 1, cameraMenu);
            switchPinButton.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    var currentPin = CameraUtils.GetCurrentPin();
                    string pin = "?";
                    int newPin = 1;
                    switch (currentPin) {
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
                    switchPinButton.setText("Cycle Pin\n<color=#845bff>" + pin + "</color>");
                    // Needed to initialize the buttons apparently
                    CameraUtils.TogglePinMenu();
                    CameraUtils.TogglePinMenu();
                    CameraUtils.SetPin(newPin);
                }
            }));

            var timer1Button = new SingleButton("Timer1", "Timer\n<color=#845bff>3 seconds</color>", "Takes a picture after 3 seconds", 3, 0, cameraMenu);
            timer1Button.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    CameraUtils.TakePicture(3);
                }
            }));

            var timer2Button = new SingleButton("Timer2", "Timer\n<color=#845bff>5 seconds</color>", "Takes a picture after 5 seconds", 3, 1, cameraMenu);
            timer2Button.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    CameraUtils.TakePicture(5);
                }
            }));

            var timer3Button = new SingleButton("Timer3", "Timer\n<color=#845bff>10 seconds</color>", "Takes a picture after 10 seconds", 3, 2, cameraMenu);
            timer3Button.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    CameraUtils.TakePicture(10);
                }
            }));

            var cameraScaleButton = new SingleButton("CameraScale", "Camera\nScale\n<color=#845bff>Normal</color>", "Changes the Camera's scale", 2, 1, cameraMenu);
            cameraScaleButton.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    string scale = "?";
                    switch (Settings.cameraScale) {
                        case CameraUtils.CameraScale.Normal:
                            scale = "Medium";
                            VRCUtils.GetUserCameraController().viewFinder.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
                            Settings.cameraScale = CameraUtils.CameraScale.Medium;
                            break;
                        case CameraUtils.CameraScale.Medium:
                            scale = "Big";
                            VRCUtils.GetUserCameraController().viewFinder.transform.localScale = new Vector3(2f, 1f, 2f);
                            Settings.cameraScale = CameraUtils.CameraScale.Big;
                            break;
                        case CameraUtils.CameraScale.Big:
                            scale = "Normal";
                            VRCUtils.GetUserCameraController().viewFinder.transform.localScale = new Vector3(1f, 1f, 1f);
                            Settings.cameraScale = CameraUtils.CameraScale.Normal;
                            break;
                    }
                    cameraScaleButton.setText("Camera\nScale\n<color=#845bff>" + scale + "</color>");
                }
            }));

            var toggleArrowKeysButton = new SingleButton("ArrowKeys", "Arrow Keys\n<color=#845bff>On</color>", "Allows you to change the camera position\nand rotation using arrow keys and numpad keys\n<color=orange>(for more info check the GitHub page)</color>", 0, 2, cameraMenu);
            toggleArrowKeysButton.setAction((Action)(() => {
                Settings.arrowKeysEnabled = !Settings.arrowKeysEnabled;
                toggleArrowKeysButton.setText("Arrow Keys\n<color=" + (Settings.arrowKeysEnabled ? "#845bff>On" : "red>Off") + "</color>");
            }));

            var rotateAroundUserCameraButton = new SingleButton("RotateAroundUserCamera", "Rotate\nAround\nUser Camera\n<color=red>Off</color>", "Makes the camera rotate around the user's camera\ninstead of just saying bye bye\n<color=orange>(for more info check the GitHub page)</color>", 1, 2, cameraMenu);
            rotateAroundUserCameraButton.setAction((Action)(() => {
                Settings.rotateAroundUserCamera = !Settings.rotateAroundUserCamera;
                rotateAroundUserCameraButton.setText("Rotate\nAround\nUser Camera\n<color=" + (Settings.rotateAroundUserCamera ? "#845bff>On" : "red>Off") + "</color>");
            }));

            var toggleLockButton = new SingleButton("ToggleLock", "Lock\n<color=red>Off</color>", "Toggles the Lock (Camera pickup)", 4, -1, cameraMenu);
            toggleLockButton.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    toggleLockButton.setText("Lock\n<color=" + (VRCUtils.GetUserCameraController().viewFinder.GetComponent<VRC_Pickup>().pickupable ? "#845bff>On" : "red>Off") + "</color>");
                    CameraUtils.ToggleLock();
                }
            }));

            var gitHubButton = new SingleButton("GitHubPage", "<color=orange>" + (updated ? "GitHub\nPage</color>" : "GitHub Page</color>\n<color=lime>Update\navailable!</color>"), "Opens the GitHub page of the mod\nMod created by nitro.#0007\nVersion: " + ModBuildInfo.Version + (updated ? "" : "\n<color=lime>New version found (" + latest + "), update it in the GitHub page.</color>"), -1, -1, cameraMenu);
            gitHubButton.setAction((Action)(() => {
                Application.OpenURL(updated ? "https://github.com/nitrog0d/DesktopCamera" : "https://github.com/nitrog0d/DesktopCamera/releases");
            }));

            var childCount = filtersMenu.transform.childCount;
            for (var i = 0; i < childCount; i++) {
                var child = filtersMenu.transform.GetChild(i);
                if (child.name == "BackButton") {
                    child.localPosition = SingleButton.getButtonPositionFor(4, 2);
                    child.GetComponent<UiTooltip>().text = "Go Back to the Camera Menu";
                    child.GetComponent<Button>().onClick.RemoveAllListeners();
                    child.GetComponent<Button>().onClick.AddListener((Action)(() => {
                        VRCUtils.ShowQuickMenuPage(quickMenu, cameraMenu, "FiltersMenu");
                    }));
                } else {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }

            var filters = new Dictionary<string, string>()
            {
                { "None", "button-NONE" },
                { "Blueprint", "Button-Blueprint" },
                { "Code", "Button-Code" },
                { "Sparkles", "Button-Sparkles" },
                { "Green\nScreen", "Button-GreenScreen" },
                { "Hypno", "Button-Hypno" },
                { "Alpha\nTransparent", "Button-ALPHA" },
                { "Drawing", "Button-Drawing" },
                { "Glitch", "Button-Glitch" },
                { "Pixelate", "Button-PIXELS" },
                { "Old Timey", "Button-OLD-TIMEY" },
                { "Trippy", "Button-Trippy" }
            };

            int row = 0;
            int position = 0;

            foreach (var filter in filters) {
                var button = new SingleButton("Filter" + filter.Value, filter.Key, "Sets the filter to " + filter.Key.Replace("\n", " "), position, row, filtersMenu);
                button.setAction((Action)(() => {
                    if (Settings.cameraEnabled) {
                        // Needed to initialize the buttons apparently
                        CameraUtils.ToggleFilterMenu();
                        CameraUtils.ToggleFilterMenu();
                        CameraUtils.SetFilter(filter.Value);
                    }
                }));
                position++;
                if (position == 4) {
                    position = 0;
                    row++;
                }
            }

            var filtersButton = new SingleButton("Filters", "Filters", "Opens the filter menu", 4, 0, cameraMenu);
            filtersButton.setAction((Action)(() => {
                VRCUtils.ShowQuickMenuPage(quickMenu, filtersMenu, cameraMenu.name);
            }));

            cameraMovementButton = new SingleButton("ToggleCameraMovement", "Camera\nMovement\n<color=#845bff>Viewer</color>", "Toggles the arrow/numpad keys movement between the actual Camera and the Viewer\nViewer requires Movement Space to be \"World\" <color=orange>(for more info check the GitHub page)</color>", 2, 2, cameraMenu);
            cameraMovementButton.setAction((Action)(() => {
                if (Settings.cameraEnabled) {
                    Settings.moveCamera = !Settings.moveCamera;
                    cameraMovementButton.setText("Camera\nMovement\n<color=#845bff>" + (Settings.moveCamera ? "Camera" : "Viewer") + "</color>");
                }
            }));
        }

        // This is a mess please don't look
        // and also pull request to improve it thx
        public override void OnUpdate() {
            // Testing
            //if (Input.GetKeyDown(KeyCode.F1)) {
            //    var quickMenu = VRCUtils.GetQuickMenu();
            //    var cameraMenu = quickMenu.transform.Find("CameraMenu");
            //    VRCUtils.ShowQuickMenuPage(quickMenu, cameraMenu);
            //}
            if (Settings.cameraEnabled && Settings.arrowKeysEnabled) {
                if (Input.GetKey(KeyCode.LeftArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -2f : -1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 0.01f : 0.005f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) VRCUtils.GetUserCameraController().viewFinder.transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -2f : -1f);
                        else VRCUtils.GetUserCameraController().viewFinder.transform.localPosition += new Vector3((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -0.01f : -0.005f, 0f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.RightArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 2f : 1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -0.01f : -0.005f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) VRCUtils.GetUserCameraController().viewFinder.transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 2f : 1f);
                        else VRCUtils.GetUserCameraController().viewFinder.transform.localPosition += new Vector3((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 0.01f : 0.005f, 0f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.UpArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -2f : -1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 0.01f : 0.005f, 0f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) VRCUtils.GetUserCameraController().viewFinder.transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -2f : -1f);
                        else VRCUtils.GetUserCameraController().viewFinder.transform.localPosition += new Vector3(0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 0.01f : 0.005f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 2f : 1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -0.01f : -0.005f, 0f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) VRCUtils.GetUserCameraController().viewFinder.transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 2f : 1f);
                        else VRCUtils.GetUserCameraController().viewFinder.transform.localPosition += new Vector3(0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -0.01f : -0.005f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.PageUp)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraVector += new Vector3((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 0.01f : 0.005f, 0f, 0f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.localPosition += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 0.01f : 0.005f);
                    }
                }
                if (Input.GetKey(KeyCode.PageDown)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraVector += new Vector3((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -0.01f : -0.005f, 0f, 0f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.localPosition += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -0.01f : -0.005f);
                    }
                }

                // Rotation
                if (Input.GetKey(KeyCode.Keypad8)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? -2f : -1f, 0f, 0f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.Rotate(new Vector3((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -2f : -1f, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad2)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 2f : 1f, 0f, 0f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.Rotate(new Vector3((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 2f : 1f, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad4)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 2f : 1f, 0f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.Rotate(new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -2f : -1f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad6)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? -2f : -1f, 0f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.Rotate(new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 2f : 1f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad7)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, 0f, Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 2f : 1f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.Rotate(new Vector3(0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? -2f : -1f, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad9)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, 0f, Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? -2f : -1f);
                    } else {
                        VRCUtils.GetUserCameraController().viewFinder.transform.Rotate(new Vector3(0f, (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? 2f : 1f, 0f));
                    }
                }

                // Reset
                if (Input.GetKeyDown(KeyCode.Keypad3)) {
                    if (Settings.cameraEnabled) CameraUtils.ResetCamera();
                }

                // Look at player
                if (Input.GetKeyDown(KeyCode.Keypad1)) {
                    if (Settings.cameraEnabled) {
                        VRCUtils.GetUserCameraController().viewFinder.transform.LookAt(VRCUtils.GetMainCamera().transform);
                        VRCUtils.GetUserCameraController().viewFinder.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
                    }
                }

                // Take pic
                if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                    if (Settings.cameraEnabled) CameraUtils.TakePicture(0);
                }

                // Toggle camera movement
                if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                    if (Settings.cameraEnabled) {
                        if (cameraMovementButton != null) {
                            Settings.moveCamera = !Settings.moveCamera;
                            cameraMovementButton.setText("Camera\nMovement\n<color=#845bff>" + (Settings.moveCamera ? "Camera" : "Viewer") + "</color>");
                            VRCUtils.QueueHudMessage("Camera Movement set to " + (Settings.moveCamera ? "Camera" : "Viewer"));
                        }
                    }
                }
            }
        }
    }
}
