using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using VRCModLoader;
using UnityEngine;
using VRCDesktopCamera.Buttons;
using UnityEngine.UI;

namespace VRCDesktopCamera {
    [VRCModInfo("VRCDesktopCamera", "1.0.0", "nitro.")]
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

        private bool arrowKeysEnabled = true;
        private bool rotateAroundCamera = false;

        // Thanks janni watermelon uwu
        private int GetInstigatorId() {
            return VRC.PlayerManager.GetPlayer(VRC.Core.APIUser.CurrentUser.id).GetInstigatorId().Value;
        }

        private void OnUpdate() {
            if (cameraEnabled && arrowKeysEnabled) {
                // Location

                if (Input.GetKey(KeyCode.LeftArrow)) {
                    if (rotateAroundCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.up, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f);
                    else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f, 0f, 0f);
                }
                if (Input.GetKey(KeyCode.RightArrow)) {
                    if (rotateAroundCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.up, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f);
                    else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f, 0f, 0f);
                }
                if (Input.GetKey(KeyCode.UpArrow)) {
                    if (rotateAroundCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.right, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f);
                    else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f, 0f);
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    if (rotateAroundCamera) UserCameraController.Instance.viewFinder.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.right, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f);
                    else UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f, 0f);
                }
                if (Input.GetKey(KeyCode.PageUp)) UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.01f : 0.005f);
                if (Input.GetKey(KeyCode.PageDown)) UserCameraController.Instance.viewFinder.transform.localPosition += new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -0.01f : -0.005f);

                // Rotation
                if (Input.GetKey(KeyCode.Keypad8)) UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f, 0f));
                if (Input.GetKey(KeyCode.Keypad2)) UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f, 0f));
                if (Input.GetKey(KeyCode.Keypad4)) UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f));
                if (Input.GetKey(KeyCode.Keypad6)) UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f));
                if (Input.GetKey(KeyCode.Keypad7)) UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -2f : -1f, 0f));
                if (Input.GetKey(KeyCode.Keypad9)) UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 2f : 1f, 0f));

                // Reset
                if (Input.GetKey(KeyCode.Keypad3)) {
                    MethodInfo toggleCam = typeof(UserCameraController).GetMethod("set_mode");
                    toggleCam.Invoke(UserCameraController.Instance, new object[] { 0 });
                    toggleCam.Invoke(UserCameraController.Instance, new object[] { 1 });
                }
                if (Input.GetKey(KeyCode.Keypad1)) {
                    UserCameraController.Instance.viewFinder.transform.LookAt(VRCPlayer.Instance.transform);
                    UserCameraController.Instance.viewFinder.transform.Rotate(new Vector3(30f, 0f));
                }

                // Take pic
                if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                    ModManager.StartCoroutine((IEnumerator)typeof(UserCameraController).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => (x.GetParameters().Length == 1) && (x.GetParameters()[0].ParameterType == typeof(int)) && (x.ReturnType == typeof(IEnumerator))).ToArray()[0].Invoke(UserCameraController.Instance, new object[] { 0 }));
                }
            }
        }

        private bool cameraEnabled = false;
        private CameraScale cameraScale = CameraScale.Normal;

        private enum CameraScale {
            Normal,
            Medium,
            Big
        }
        
        private enum CameraBehaviour {
            None,
            Smooth,
            LookAt
        }

        private enum CameraSpace {
            Attached,
            Local,
            World
        }

        private enum Pin {
            Pin1,
            Pin2,
            Pin3
        }

        private IEnumerator Setup() {
            yield return VRCTools.VRCUiManagerUtils.WaitForUiManagerInit();
            // Let VRCTools make their cool changes
            yield return new WaitForSeconds(3f);
            try {
                if (!VRCTrackingManager.IsInVRMode()) {
                    QuickMenu quickMenu = QuickMenuUtils.GetQuickMenuInstance();
                    Transform cameraMenu = quickMenu.transform.Find("CameraMenu");

                    Transform panoramaButton = cameraMenu.Find("Panorama");
                    panoramaButton.localPosition = SingleButton.getButtonPositionFor(-1, -1);

                    Transform vrChiveButton = cameraMenu.Find("VRChive");
                    vrChiveButton.localPosition = SingleButton.getButtonPositionFor(-1, 0);

                    Transform backButton = cameraMenu.Find("BackButton");
                    backButton.localPosition = SingleButton.getButtonPositionFor(4, 2);

                    SingleButton cameraButton = new SingleButton("Camera", "Camera\n<color=red>Off</color>", "Toggles the Camera", 0, 0, cameraMenu);
                    cameraButton.setAction(() => {
                        cameraEnabled = !cameraEnabled;
                        cameraButton.setText("Camera\n<color=" + (cameraEnabled ? "green>On" : "red>Off") + "</color>");
                        int param = cameraEnabled ? 1 : 0;
                        typeof(UserCameraController).GetMethod("set_mode").Invoke(UserCameraController.Instance, new object[] { param });
                    });

                    SingleButton movementBehaviourButton = new SingleButton("MovementBehaviour", "Movement\nBehaviour\n<color=yellow>None</color>", "Changes the Camera's movement behaviour", 1, 0, cameraMenu);
                    movementBehaviourButton.setAction(() => {
                        if (cameraEnabled) {
                            CameraBehaviour cameraBehaviour = (CameraBehaviour)typeof(UserCameraController).GetMethod("get_movementBehaviour").Invoke(UserCameraController.Instance, new object[] { });
                            string behaviour = "?";
                            switch (cameraBehaviour) {
                                case CameraBehaviour.None:
                                    behaviour = "Smooth";
                                    break;
                                case CameraBehaviour.Smooth:
                                    behaviour = "Look At";
                                    break;
                                case CameraBehaviour.LookAt:
                                    behaviour = "None";
                                    break;
                            }
                            movementBehaviourButton.setText("Movement\nBehaviour\n<color=yellow>" + behaviour + "</color>");
                            UserCameraController.Instance.actionCycleMovementBehaviour(GetInstigatorId());
                        }
                    });

                    SingleButton movementSpaceButton = new SingleButton("MovementSpace", "Movement\nSpace\n<color=yellow>Attached</color>", "Changes the Camera's movement space", 2, 0, cameraMenu);
                    movementSpaceButton.setAction(() => {
                        if (cameraEnabled) {
                            CameraSpace cameraSpace = (CameraSpace)typeof(UserCameraController).GetMethod("get_space").Invoke(UserCameraController.Instance, new object[] { });
                            string space = "?";
                            switch (cameraSpace) {
                                case CameraSpace.Attached:
                                    space = "Local";
                                    break;
                                case CameraSpace.Local:
                                    space = "World";
                                    break;
                                case CameraSpace.World:
                                    space = "Attached";
                                    break;
                            }
                            movementSpaceButton.setText("Movement\nSpace\n<color=yellow>" + space + "</color>");
                            UserCameraController.Instance.actionCycleMovementSpace(GetInstigatorId());
                        }
                    });

                    SingleButton pinMenuButton = new SingleButton("PinMenu", "Pin Menu\n<color=red>Off</color>", "Toggles the Pin menu (which is pretty useless)", 0, 1, cameraMenu);
                    pinMenuButton.setAction(() => {
                        if (cameraEnabled) {
                            UserCameraController.Instance.actionTogglePinMenu(GetInstigatorId());
                            pinMenuButton.setText("Pin Menu\n<color=" + (UserCameraController.Instance.pinsHolder.activeSelf ? "green>On" : "red>Off") + "</color>");
                        }
                    });

                    SingleButton switchPinButton = new SingleButton("SwitchPin", "Switch Pin\n<color=yellow>Pin 1</color>", "Switches between 3 pins (aka profiles)", 1, 1, cameraMenu);
                    switchPinButton.setAction(() => {
                        if (cameraEnabled) {
                            Pin currentPin = (Pin)typeof(UserCameraController).GetMethod("get_currentPin").Invoke(UserCameraController.Instance, new object[] { });
                            string pin = "?";
                            int newPin = 0;
                            switch (currentPin) {
                                case Pin.Pin1:
                                    newPin = 1;
                                    pin = "Pin 2";
                                    break;
                                case Pin.Pin2:
                                    newPin = 2;
                                    pin = "Pin 3";
                                    break;
                                case Pin.Pin3:
                                    newPin = 0;
                                    pin = "Pin 1";
                                    break;
                            }
                            switchPinButton.setText("Switch Pin\n<color=yellow>" + pin + "</color>");
                            UserCameraController.Instance.actionChangePin(newPin);
                        }
                    });

                    SingleButton timer1Button = new SingleButton("Timer1", "Timer\n<color=yellow>3 seconds</color>", "Takes a picture after 3 seconds", 3, 0, cameraMenu);
                    timer1Button.setAction(() => {
                        if (cameraEnabled) {
                            ModManager.StartCoroutine((IEnumerator)typeof(UserCameraController).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => (x.GetParameters().Length == 1) && (x.GetParameters()[0].ParameterType == typeof(int)) && (x.ReturnType == typeof(IEnumerator))).ToArray()[0].Invoke(UserCameraController.Instance, new object[] { 3 }));
                        }
                    });

                    SingleButton timer2Button = new SingleButton("Timer2", "Timer\n<color=yellow>5 seconds</color>", "Takes a picture after 5 seconds", 3, 1, cameraMenu);
                    timer2Button.setAction(() => {
                        if (cameraEnabled) {
                            UserCameraController.Instance.actionTimer(GetInstigatorId());
                        }
                    });

                    SingleButton timer3Button = new SingleButton("Timer3", "Timer\n<color=yellow>10 seconds</color>", "Takes a picture after 10 seconds", 3, 2, cameraMenu);
                    timer3Button.setAction(() => {
                        if (cameraEnabled) {
                            ModManager.StartCoroutine((IEnumerator)typeof(UserCameraController).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => (x.GetParameters().Length == 1) && (x.GetParameters()[0].ParameterType == typeof(int)) && (x.ReturnType == typeof(IEnumerator))).ToArray()[0].Invoke(UserCameraController.Instance, new object[] { 10 }));
                        }
                    });

                    SingleButton cameraScaleButton = new SingleButton("CameraScale", "Camera\nScale\n<color=yellow>Normal</color>", "Changes the Camera's scale", 2, 1, cameraMenu);
                    cameraScaleButton.setAction(() => {
                        if (cameraEnabled) {
                            string scale = "?";
                            switch (cameraScale) {
                                case CameraScale.Normal:
                                    scale = "Medium";
                                    UserCameraController.Instance.viewFinder.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
                                    cameraScale = CameraScale.Medium;
                                    break;
                                case CameraScale.Medium:
                                    scale = "Big";
                                    UserCameraController.Instance.viewFinder.transform.localScale = new Vector3(2f, 1f, 2f);
                                    cameraScale = CameraScale.Big;
                                    break;
                                case CameraScale.Big:
                                    scale = "Normal";
                                    UserCameraController.Instance.viewFinder.transform.localScale = new Vector3(1f, 1f, 1f);
                                    cameraScale = CameraScale.Normal;
                                    break;
                            }
                            cameraScaleButton.setText("Camera\nScale\n<color=yellow>" + scale + "</color>");
                        }
                    });

                    SingleButton toggleArrowKeysButton = new SingleButton("ArrowKeys", "Arrow Keys\n<color=green>On</color>", "Allows you to change the camera position\nand rotation using arrow keys and numpad keys\n(for more info check the GitHub page)", 0, 2, cameraMenu);
                    toggleArrowKeysButton.setAction(() => {
                        arrowKeysEnabled = !arrowKeysEnabled;
                        toggleArrowKeysButton.setText("Arrow Keys\n<color=" + (arrowKeysEnabled ? "green>On" : "red>Off") + "</color>");
                    });

                    SingleButton rotateAroundCameraButton = new SingleButton("RotateAroundCamera", "Rotate\nAround\nCamera\n<color=red>Off</color>", "Makes the camera rotate around the user's camera\ninstead of just saying bye bye\n(for more info check the GitHub page)", 1, 2, cameraMenu);
                    rotateAroundCameraButton.setAction(() => {
                        rotateAroundCamera = !rotateAroundCamera;
                        rotateAroundCameraButton.setText("Rotate\nAround\nCamera\n<color=" + (rotateAroundCamera ? "green>On" : "red>Off") + "</color>");
                    });

                    SingleButton toggleExtenderButton = new SingleButton("ToggleExtender", "Extender\n<color=red>Off</color>", "Toggles the Extender (why)", 4, -1, cameraMenu);
                    toggleExtenderButton.setAction(() => {
                        if (cameraEnabled) {
                            UserCameraController.Instance.actionExtender(GetInstigatorId());
                            toggleExtenderButton.setText("Extender\n<color=" + (UserCameraController.Instance.extender.activeSelf ? "green>On" : "red>Off") + "</color>");
                        }
                    });

                    SingleButton gitHubButton = new SingleButton("GitHubPage", "GitHub\nPage", "Opens the GitHub page of the mod\nVersion: 1.0.0", 4, 0, cameraMenu);
                    gitHubButton.setAction(() => {
                        Application.OpenURL("https://github.com/nitrog0d/VRCDesktopCamera");
                    });

                    Transform filtersMenu = UnityEngine.Object.Instantiate(InstanceUtils.MenuTemplate(), quickMenu.transform);
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

                    SingleButton filtersButton = new SingleButton("Filters", "Filters", "Opens the filter menu", 2, 2, cameraMenu);
                    filtersButton.setAction(() => {
                        QuickMenuUtils.ShowQuickmenuPage("FiltersMenu", "CameraMenu");
                    });

                    Dictionary<string, int> filters = new Dictionary<string, int>()
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

                    foreach (KeyValuePair<string, int> filter in filters) {
                        SingleButton button = new SingleButton("Filter" + filter.Value, filter.Key, "Sets the filter to " + filter.Key, position, row, filtersMenu);
                        button.setAction(() => {
                            if (cameraEnabled) {
                                UserCameraController.Instance.actionSetFilter(filter.Value);
                            }
                        });
                        position++;
                        if (position == 4) {
                            position = 0;
                            row++;
                        }
                    }

                }   

                } catch (Exception e) {
                VRCModLogger.LogError("[VRCDesktopCamera] Error!\n" + e);
            }
        }
    }
}
