using System;
using System.Collections;
using System.Reflection;
using VRCModLoader;
using UnityEngine;
using VRCTools;
using VRCDesktopCamera.Buttons;

namespace VRCDesktopCamera {
    [VRCModInfo("VRCDesktopCamera", "1.0", "nitro.")]
    public class VRCDesktopCamera : VRCMod {

        private bool initialized = false;

        private void OnApplicationStart() {
            VRCModLogger.Log("[VRCDesktopCamera] Mod loaded.");
        }

        private void OnLevelWasLoaded(int level) {
            if (!initialized) {
                initialized = true;
                //ModManager.StartCoroutine(Setup());
            }
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F1)) ModManager.StartCoroutine(Setup());
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
            yield return VRCUiManagerUtils.WaitForUiManagerInit();
            try {
                if (!VRCTrackingManager.IsInVRMode()) {
                    QuickMenu quickMenu = QuickMenuUtils.GetQuickMenuInstance();
                    Transform cameraMenuTransform = quickMenu.transform.Find("CameraMenu");

                    Transform panoramaButton = cameraMenuTransform.Find("Panorama");
                    panoramaButton.localPosition = BaseButton.getButtonPositionFor(-1, -1);

                    Transform vrChiveButton = cameraMenuTransform.Find("VRChive");
                    vrChiveButton.localPosition = BaseButton.getButtonPositionFor(-1, 0);

                    SingleButton cameraButton = new SingleButton("Camera\n<color=red>Off</color>", "Enables/Disables the Camera", 0, 0, cameraMenuTransform);
                    cameraButton.setAction(() => {
                        cameraEnabled = !cameraEnabled;
                        cameraButton.setText("Camera\n<color=" + (cameraEnabled ? "green>On" : "red>Off") + "</color>");
                        int param = cameraEnabled ? 1 : 0;
                        typeof(UserCameraController).GetMethod("set_mode").Invoke(UserCameraController.Instance, new object[] { param });
                    });

                    SingleButton movementBehaviourButton = new SingleButton("Movement\nBehaviour\n<color=yellow>None</color>", "Changes the Camera's movement behaviour", 1, 0, cameraMenuTransform);
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
                            UserCameraController.Instance.actionCycleMovementBehaviour(1);
                        }
                    });

                    SingleButton movementSpaceButton = new SingleButton("Movement\nSpace\n<color=yellow>Attached</color>", "Changes the Camera's movement space", 2, 0, cameraMenuTransform);
                    movementSpaceButton.setAction(() => {
                        if (cameraEnabled) {
                            CameraSpace cameraSpace = (CameraSpace)typeof(UserCameraController).GetMethod("get_space").Invoke(UserCameraController.Instance, new object[] { });
                            string space = "Attached";
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
                            UserCameraController.Instance.actionCycleMovementSpace(1);
                        }
                    });

                    SingleButton pinMenuButton = new SingleButton("Pin Menu\n<color=red>Off</color>", "Toggles the Pin menu (which is pretty useless)", 0, 1, cameraMenuTransform);
                    pinMenuButton.setAction(() => {
                        if (cameraEnabled) {
                            UserCameraController.Instance.actionTogglePinMenu(1);
                            pinMenuButton.setText("Pin Menu\n<color=" + (UserCameraController.Instance.pinsHolder.activeSelf ? "green>On" : "red>Off") + "</color>");
                        }
                    });

                    SingleButton switchPinButton = new SingleButton("Switch Pin\n<color=yellow>Pin 1</color>", "Switches between 3 pins (aka profiles)", 1, 1, cameraMenuTransform);
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

                    SingleButton timer1Button = new SingleButton("Timer\n<color=yellow>3 seconds</color>", "Takes a picture after 3 seconds", 0, 2, cameraMenuTransform);
                    timer1Button.setAction(() => {
                        if (cameraEnabled) {
                            ModManager.StartCoroutine((IEnumerator)typeof(UserCameraController).GetMethod("OHJDHBMFDFA", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(UserCameraController.Instance, new object[] { 3 }));
                        }
                    });

                    SingleButton timer2Button = new SingleButton("Timer\n<color=yellow>5 seconds</color>", "Takes a picture after 5 seconds", 1, 2, cameraMenuTransform);
                    timer2Button.setAction(() => {
                        if (cameraEnabled) {
                            UserCameraController.Instance.actionTimer(1);
                        }
                    });

                    SingleButton timer3Button = new SingleButton("Timer\n<color=yellow>10 seconds</color>", "Takes a picture after 10 seconds", 2, 2, cameraMenuTransform);
                    timer3Button.setAction(() => {
                        if (cameraEnabled) {
                            ModManager.StartCoroutine((IEnumerator)typeof(UserCameraController).GetMethod("OHJDHBMFDFA", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(UserCameraController.Instance, new object[] { 10 }));
                        }
                    });

                    SingleButton cameraScaleButton = new SingleButton("Camera\nScale\n<color=yellow>Normal</color>", "Changes the Camera's scale", 2, 1, cameraMenuTransform);
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
                }

                } catch (Exception e) {
                VRCModLogger.LogError("[VRCDesktopCamera] Error!\n" + e);
            }
        }
    }
}
