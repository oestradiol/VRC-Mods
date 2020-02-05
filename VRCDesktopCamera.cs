using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRCModLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using VRCDesktopCamera.Buttons;
using VRCDesktopCamera.Utils;
using Newtonsoft.Json;

namespace VRCDesktopCamera {
    [VRCModInfo("VRCDesktopCamera", "1.0.2", "nitro.", "https://github.com/nitrog0d/VRCDesktopCamera/releases/download/v1.0.2/VRCDesktopCamera.1.0.2.dll")]
    public class VRCDesktopCamera : VRCMod {

        private bool initialized = false;

        private void OnApplicationStart() {
            VRCModLogger.Log("[VRCDesktopCamera] Mod loaded.");
        }

        private void OnLevelWasLoaded(int level) {
            if (!initialized) {
                initialized = true;
                ModManager.StartCoroutine(Setup());
            }
        }

        private SingleButton cameraMovementButton;

        // This is a mess please don't look
        // and also pull request to improve it thx
        private void OnUpdate() {
            if (Settings.cameraEnabled && Settings.arrowKeysEnabled) {
                // Location
                if (Input.GetKey(KeyCode.LeftArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f);
                        else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f, 0f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.RightArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.up, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f);
                        else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f, 0f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.UpArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f, 0f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f);
                        else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f);
                            else CameraUtils.worldCameraVector += new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f, 0f);
                        }
                    } else {
                        if (Settings.rotateAroundUserCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(InstanceUtils.GetMainCamera().transform.position, InstanceUtils.GetMainCamera().transform.right, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f);
                        else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.PageUp)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraVector += new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f, 0f, 0f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f);
                    }
                }
                if (Input.GetKey(KeyCode.PageDown)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraVector += new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f, 0f, 0f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f);
                    }
                }

                // Rotation
                if (Input.GetKey(KeyCode.Keypad8)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? -2f : -1f, 0f, 0f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad2)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 2f : 1f, 0f, 0f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad4)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 2f : 1f, 0f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad6)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? -2f : -1f, 0f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad7)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, 0f, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 2f : 1f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad9)) {
                    if (Settings.moveCamera) {
                        if (Settings.allowCameraMovement) CameraUtils.worldCameraQuaternion *= Quaternion.Euler(0f, 0f, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? -2f : -1f);
                    } else {
                        UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f, 0f));
                    }
                }

                // Reset
                if (Input.GetKey(KeyCode.Keypad3)) {
                    if (Settings.cameraEnabled) CameraUtils.ResetCamera();
                }

                // Look at player
                if (Input.GetKey(KeyCode.Keypad1)) {
                    if (Settings.cameraEnabled) {
                        UserCameraController.Instance.viewFinder.transform.LookAt(InstanceUtils.GetMainCamera().transform);
                        UserCameraController.Instance.viewFinder.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
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
                            cameraMovementButton.setText("Camera\nMovement\n<color=yellow>" + (Settings.moveCamera ? "Camera" : "Viewer") + "</color>");
                            VRCToolsUtils.GetUiManagerInstance().QueueHudMessage("Camera Movement set to " + (Settings.moveCamera ? "Camera" : "Viewer"));
                        }
                    }
                }
            }
        }

        public class VersionCheckResponse {
            public string result { get; set; }
            public string latest { get; set; }
        }

        private IEnumerator Setup() {
            yield return VRCToolsUtils.WaitForUiManagerInit();
            // Let VRCTools make their cool changes
            yield return new WaitForSeconds(3f);

            bool updated = true;
            string latest = "";
            using (var request = UnityWebRequest.Put("https://vrcmods.nitro.moe/mods/versioncheck", Encoding.UTF8.GetBytes("{\"name\":\"" + Name + "\",\"version\":\"" + Version + "\"}"))) {
                request.method = UnityWebRequest.kHttpVerbPOST;
                yield return request.SendWebRequest();
                if (!request.isNetworkError && !request.isHttpError) {
                    try {
                        var response = JsonConvert.DeserializeObject<VersionCheckResponse>(request.downloadHandler.text);
                        if (response.result == "OUTDATED") {
                            updated = false;
                            latest = response.latest;
                        }
                    } catch (Exception) { }
                }
            }

            try {
                if (!VRCTrackingManager.IsInVRMode()) {
                    InstanceUtils.Init();

                    var quickMenu = VRCToolsUtils.GetQuickMenuInstance();
                    var cameraMenu = quickMenu.transform.Find("CameraMenu");

                    var panoramaButton = cameraMenu.Find("Panorama");
                    panoramaButton.localPosition = SingleButton.getButtonPositionFor(-1, 0);

                    var vrChiveButton = cameraMenu.Find("VRChive");
                    vrChiveButton.localPosition = SingleButton.getButtonPositionFor(-1, 1);

                    var backButton = cameraMenu.Find("BackButton");
                    backButton.localPosition = SingleButton.getButtonPositionFor(4, 2);

                    var cameraButton = new SingleButton("Camera", "Camera\n<color=red>Off</color>", "Toggles the Camera", 0, 0, cameraMenu);
                    cameraButton.setAction(() => {
                        Settings.cameraEnabled = !Settings.cameraEnabled;
                        cameraButton.setText("Camera\n<color=" + (Settings.cameraEnabled ? "green>On" : "red>Off") + "</color>");
                        CameraUtils.SetCameraMode(Settings.cameraEnabled ? 1 : 0);
                    });

                    var movementBehaviourButton = new SingleButton("MovementBehaviour", "Movement\nBehaviour\n<color=yellow>None</color>", "Changes the Camera's movement behaviour", 1, 0, cameraMenu);
                    movementBehaviourButton.setAction(() => {
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
                            movementBehaviourButton.setText("Movement\nBehaviour\n<color=yellow>" + behaviour + "</color>");
                            UserCameraController.Instance.actionCycleMovementBehaviour(InstanceUtils.GetInstigatorId());
                        }
                    });

                    var movementSpaceButton = new SingleButton("MovementSpace", "Movement\nSpace\n<color=yellow>Attached</color>", "Changes the Camera's movement space", 2, 0, cameraMenu);
                    movementSpaceButton.setAction(() => {
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
                            movementSpaceButton.setText("Movement\nSpace\n<color=yellow>" + space + "</color>");
                            UserCameraController.Instance.actionCycleMovementSpace(InstanceUtils.GetInstigatorId());
                            if (CameraUtils.GetCameraSpace() == CameraUtils.CameraSpace.World) Settings.allowCameraMovement = true; else Settings.allowCameraMovement = false;
                        }
                    });

                    var pinMenuButton = new SingleButton("PinMenu", "Pin Menu\n<color=red>Off</color>", "Toggles the Pin menu (which is pretty useless)", 0, 1, cameraMenu);
                    pinMenuButton.setAction(() => {
                        if (Settings.cameraEnabled) {
                            UserCameraController.Instance.actionTogglePinMenu(InstanceUtils.GetInstigatorId());
                            pinMenuButton.setText("Pin Menu\n<color=" + (UserCameraController.Instance.pinsHolder.activeSelf ? "green>On" : "red>Off") + "</color>");
                        }
                    });

                    var switchPinButton = new SingleButton("SwitchPin", "Switch Pin\n<color=yellow>Pin 1</color>", "Switches between 3 pins (aka profiles)", 1, 1, cameraMenu);
                    switchPinButton.setAction(() => {
                        if (Settings.cameraEnabled) {
                            var currentPin = CameraUtils.GetCurrentPin();
                            string pin = "?";
                            int newPin = 0;
                            switch (currentPin) {
                                case CameraUtils.Pin.Pin1:
                                    newPin = 1;
                                    pin = "Pin 2";
                                    break;
                                case CameraUtils.Pin.Pin2:
                                    newPin = 2;
                                    pin = "Pin 3";
                                    break;
                                case CameraUtils.Pin.Pin3:
                                    newPin = 0;
                                    pin = "Pin 1";
                                    break;
                            }
                            switchPinButton.setText("Switch Pin\n<color=yellow>" + pin + "</color>");
                            UserCameraController.Instance.actionChangePin(newPin);
                        }
                    });

                    var timer1Button = new SingleButton("Timer1", "Timer\n<color=yellow>3 seconds</color>", "Takes a picture after 3 seconds", 3, 0, cameraMenu);
                    timer1Button.setAction(() => {
                        if (Settings.cameraEnabled) {
                            CameraUtils.TakePicture(3);
                        }
                    });

                    var timer2Button = new SingleButton("Timer2", "Timer\n<color=yellow>5 seconds</color>", "Takes a picture after 5 seconds", 3, 1, cameraMenu);
                    timer2Button.setAction(() => {
                        if (Settings.cameraEnabled) {
                            CameraUtils.TakePicture(5);
                        }
                    });

                    var timer3Button = new SingleButton("Timer3", "Timer\n<color=yellow>10 seconds</color>", "Takes a picture after 10 seconds", 3, 2, cameraMenu);
                    timer3Button.setAction(() => {
                        if (Settings.cameraEnabled) {
                            CameraUtils.TakePicture(10);
                        }
                    });

                    var cameraScaleButton = new SingleButton("CameraScale", "Camera\nScale\n<color=yellow>Normal</color>", "Changes the Camera's scale", 2, 1, cameraMenu);
                    cameraScaleButton.setAction(() => {
                        if (Settings.cameraEnabled) {
                            string scale = "?";
                            switch (Settings.cameraScale) {
                                case CameraUtils.CameraScale.Normal:
                                    scale = "Medium";
                                    UserCameraController.Instance.viewFinder.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
                                    Settings.cameraScale = CameraUtils.CameraScale.Medium;
                                    break;
                                case CameraUtils.CameraScale.Medium:
                                    scale = "Big";
                                    UserCameraController.Instance.viewFinder.transform.localScale = new Vector3(2f, 1f, 2f);
                                    Settings.cameraScale = CameraUtils.CameraScale.Big;
                                    break;
                                case CameraUtils.CameraScale.Big:
                                    scale = "Normal";
                                    UserCameraController.Instance.viewFinder.transform.localScale = new Vector3(1f, 1f, 1f);
                                    Settings.cameraScale = CameraUtils.CameraScale.Normal;
                                    break;
                            }
                            cameraScaleButton.setText("Camera\nScale\n<color=yellow>" + scale + "</color>");
                        }
                    });

                    var toggleArrowKeysButton = new SingleButton("ArrowKeys", "Arrow Keys\n<color=green>On</color>", "Allows you to change the camera position\nand rotation using arrow keys and numpad keys\n<color=orange>(for more info check the GitHub page)</color>", 0, 2, cameraMenu);
                    toggleArrowKeysButton.setAction(() => {
                        Settings.arrowKeysEnabled = !Settings.arrowKeysEnabled;
                        toggleArrowKeysButton.setText("Arrow Keys\n<color=" + (Settings.arrowKeysEnabled ? "green>On" : "red>Off") + "</color>");
                    });

                    var rotateAroundUserCameraButton = new SingleButton("RotateAroundUserCamera", "Rotate\nAround\nUser Camera\n<color=red>Off</color>", "Makes the camera rotate around the user's camera\ninstead of just saying bye bye\n<color=orange>(for more info check the GitHub page)</color>", 1, 2, cameraMenu);
                    rotateAroundUserCameraButton.setAction(() => {
                        Settings.rotateAroundUserCamera = !Settings.rotateAroundUserCamera;
                        rotateAroundUserCameraButton.setText("Rotate\nAround\nUser Camera\n<color=" + (Settings.rotateAroundUserCamera ? "green>On" : "red>Off") + "</color>");
                    });

                    var toggleExtenderButton = new SingleButton("ToggleExtender", "Extender\n<color=red>Off</color>", "Toggles the Extender (useless)", 4, -1, cameraMenu);
                    toggleExtenderButton.setAction(() => {
                        if (Settings.cameraEnabled) {
                            UserCameraController.Instance.actionExtender(InstanceUtils.GetInstigatorId());
                            toggleExtenderButton.setText("Extender\n<color=" + (UserCameraController.Instance.extender.activeSelf ? "green>On" : "red>Off") + "</color>");
                        }
                    });

                    var gitHubButton = new SingleButton("GitHubPage", "<color=orange>" + (updated ? "GitHub\nPage</color>" : "GitHub Page</color>\n<color=lime>Update\navailable!</color>"), "Opens the GitHub page of the mod\nVersion: " + Version + (updated ? "" : "\n<color=lime>New version found (" + latest + "), update using VRChatModInstaller (VRCModManager).</color>"), -1, -1, cameraMenu);
                    gitHubButton.setAction(() => {
                        Application.OpenURL("https://github.com/nitrog0d/VRCDesktopCamera");
                    });

                    var filtersMenu = UnityEngine.Object.Instantiate(InstanceUtils.MenuTemplate(), quickMenu.transform);
                    filtersMenu.name = "FiltersMenu";

                    foreach (Transform child in filtersMenu) {
                        if (child.name == "BackButton") {
                            child.localPosition = SingleButton.getButtonPositionFor(4, 2);
                            child.GetComponent<UiTooltip>().text = "Go Back to the Camera Menu";
                            child.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                            child.GetComponent<Button>().onClick.AddListener(() => {
                                quickMenu.SetMenuIndex(5);
                            });
                        } else {
                            UnityEngine.Object.Destroy(child.gameObject);
                        }
                    }

                    var filtersButton = new SingleButton("Filters", "Filters", "Opens the filter menu", 4, 0, cameraMenu);
                    filtersButton.setAction(() => {
                        VRCToolsUtils.ShowQuickmenuPage("FiltersMenu", "CameraMenu");
                    });

                    var filters = new Dictionary<string, int>()
                    {
                        { "None", 0 },
                        { "Blueprint", 10 },
                        { "Code", 4 },
                        { "Sparkles", 5 },
                        { "Green\nScreen", 7 },
                        { "Hypno", 6 },
                        { "Alpha\nTransparent", 8 },
                        { "Drawing", 9 },
                        { "Glitch", 3 },
                        { "Pixelate", 2 },
                        { "Old Timey", 1 },
                        { "Trippy", 11 }
                    };

                    int row = 0;
                    int position = 0;

                    foreach (var filter in filters) {
                        var button = new SingleButton("Filter" + filter.Value, filter.Key, "Sets the filter to " + filter.Key.Replace("\n", " "), position, row, filtersMenu);
                        button.setAction(() => {
                            if (Settings.cameraEnabled) {
                                UserCameraController.Instance.actionSetFilter(filter.Value);
                            }
                        });
                        position++;
                        if (position == 4) {
                            position = 0;
                            row++;
                        }
                    }

                    cameraMovementButton = new SingleButton("ToggleCameraMovement", "Camera\nMovement\n<color=yellow>Viewer</color>", "Toggles the arrow/numpad keys movement between the actual Camera and the Viewer\nViewer requires Movement Space to be \"World\" <color=orange>(for more info check the GitHub page)</color>", 2, 2, cameraMenu);
                    cameraMovementButton.setAction(() => {
                        if (Settings.cameraEnabled) {
                            Settings.moveCamera = !Settings.moveCamera;
                            cameraMovementButton.setText("Camera\nMovement\n<color=yellow>" + (Settings.moveCamera ? "Camera" : "Viewer") + "</color>");
                        }
                    });

                }   

                } catch (Exception e) {
                VRCModLogger.LogError("[VRCDesktopCamera] Error!\n" + e);
            }
        }
    }
}
