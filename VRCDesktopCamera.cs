using System;
using System.Collections;
using System.Reflection;
using VRCModLoader;
using VRCTools;
using UnityEngine;
using UnityEngine.UI;
using Harmony;

namespace VRCDesktopCamera {
    [VRCModInfo("VRCDesktopCamera", "1.0", "nitro.")]
    public class VRCDesktopCamera : VRCMod {

        private bool initialized = false;
        private CameraBehaviour cameraBehaviour;
        private CameraSpace cameraSpace;

        private void OnApplicationStart() {
            VRCModLogger.Log("[VRCDesktopCamera] Mod loaded.");
        }

        private void OnLevelWasLoaded(int level) {
            if (!initialized) {
                ModManager.StartCoroutine(Setup());
                initialized = true;
            }
        }

        private static bool QuickMenuCameraCheck() {
            return false;
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

        private IEnumerator Setup() {
            yield return VRCUiManagerUtils.WaitForUiManagerInit();
            try {
                // Maybe sometime get this method properly so it supports updates
                // well actually idc, il2cpp coming soon so fuck it
                MethodInfo check = typeof(QuickMenu).GetMethod("OEAKDHJMDOI", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo replacement = typeof(VRCDesktopCamera).GetMethod("QuickMenuCameraCheck", BindingFlags.NonPublic | BindingFlags.Static);

                HarmonyInstance harmonyInstance = HarmonyInstance.Create("VRCDesktopCamera");
                harmonyInstance.Patch(check, new HarmonyMethod(replacement));

                QuickMenu quickMenu = QuickMenuUtils.GetQuickMenuInstance();
                Transform cameraMenuTransform = quickMenu.transform.Find("CameraMenu");


                Transform panoramaButton = cameraMenuTransform.Find("Panorama");
                Vector3 panoramaButtonPos = panoramaButton.localPosition;
                panoramaButton.localPosition = new Vector3(panoramaButtonPos.x + -420f, panoramaButtonPos.y + 420f, 0f);

                Transform vrChiveButton = cameraMenuTransform.Find("VRChive");
                Vector3 vrChiveButtonPos = vrChiveButton.localPosition;
                vrChiveButton.localPosition = new Vector3(vrChiveButtonPos.x + -840f, vrChiveButtonPos.y, 0f);

                Transform photoModeButton = cameraMenuTransform.Find("PhotoMode");
                photoModeButton.gameObject.SetActive(true);
                Vector3 photoModeButtonPos = photoModeButton.localPosition;
                photoModeButton.localPosition = new Vector3(photoModeButtonPos.x, photoModeButtonPos.y + 420f, 0f);

                Transform videoModeButton = cameraMenuTransform.Find("VideoMode");
                videoModeButton.gameObject.SetActive(true);
                if (!VRCTrackingManager.IsInVRMode()) videoModeButton.GetComponent<Button>().interactable = false;
                Vector3 videoModeButtonPos = videoModeButton.localPosition;
                videoModeButton.localPosition = new Vector3(videoModeButtonPos.x, videoModeButtonPos.y + 420f, 0f);

                Transform disableCameraButton = cameraMenuTransform.Find("DisableCamera");
                disableCameraButton.gameObject.SetActive(true);
                Vector3 disableCameraButtonPos = disableCameraButton.localPosition;
                disableCameraButton.localPosition = new Vector3(disableCameraButtonPos.x, disableCameraButtonPos.y + 420f, 0f);

                Transform templateButton = cameraMenuTransform.GetChild(0);

                if (templateButton) {
                    Transform movementBehaviourButton = UnityEngine.Object.Instantiate(templateButton, cameraMenuTransform);
                    movementBehaviourButton.GetComponentInChildren<Text>().text = "Movement\nBehaviour\n<color=green>None</color>";
                    movementBehaviourButton.GetComponent<UiTooltip>().text = "Changes the Camera's movement behaviour.";
                    movementBehaviourButton.name = "CameraMovementBehaviour";
                    movementBehaviourButton.localPosition = new Vector3(-630f, 630f, 0f);
                    movementBehaviourButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    movementBehaviourButton.GetComponent<Button>().onClick.AddListener(() => {
                        switch (cameraBehaviour) {
                            case CameraBehaviour.None:
                                cameraBehaviour = CameraBehaviour.Smooth;
                                movementBehaviourButton.GetComponentInChildren<Text>().text = "Movement\nBehaviour\n<color=green>Smooth</color>";
                                break;
                            case CameraBehaviour.Smooth:
                                cameraBehaviour = CameraBehaviour.LookAt;
                                movementBehaviourButton.GetComponentInChildren<Text>().text = "Movement\nBehaviour\n<color=green>Look At</color>";
                                break;
                            case CameraBehaviour.LookAt:
                                cameraBehaviour = CameraBehaviour.None;
                                movementBehaviourButton.GetComponentInChildren<Text>().text = "Movement\nBehaviour\n<color=green>None</color>";
                                break;
                        }
                        
                        typeof(UserCameraController).GetMethod("set_movementBehaviour").Invoke(UserCameraController.Instance, new object[] { (int)cameraBehaviour });
                    });

                    Transform spaceButton = UnityEngine.Object.Instantiate(templateButton, cameraMenuTransform);
                    spaceButton.GetComponentInChildren<Text>().text = "Space\n<color=green>Attached</color>";
                    spaceButton.GetComponent<UiTooltip>().text = "Changes the Camera's movement space.";
                    spaceButton.name = "CameraSpace";
                    spaceButton.localPosition = new Vector3(-210f, 630f, 0f);
                    spaceButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    spaceButton.GetComponent<Button>().onClick.AddListener(() => {
                        switch (cameraSpace) {
                            case CameraSpace.Attached:
                                cameraSpace = CameraSpace.Local;
                                spaceButton.GetComponentInChildren<Text>().text = "Movement\nBehaviour\n<color=green>Local</color>";
                                break;
                            case CameraSpace.Local:
                                cameraSpace = CameraSpace.World;
                                spaceButton.GetComponentInChildren<Text>().text = "Movement\nBehaviour\n<color=green>World</color>";
                                break;
                            case CameraSpace.World:
                                cameraSpace = CameraSpace.Attached;
                                spaceButton.GetComponentInChildren<Text>().text = "Movement\nBehaviour\n<color=green>Attached</color>";
                                break;
                        }
                        typeof(UserCameraController).GetMethod("set_space").Invoke(UserCameraController.Instance, new object[] { (int)cameraSpace });
                    });
                }
            } catch (Exception e) {
                VRCModLogger.LogError("[VRCDesktopCamera] Error!\n" + e);
            }
        }
    }
}
