using System;
using UnityEngine;

namespace DesktopCamera.Utils
{
    internal static class MovementManager
    {
        public static void OnUpdate()
        {
            if (Settings.cameraEnabled && Settings.arrowKeysEnabled)
            {
                var cameraRotation = CameraUtils.WorldCameraQuaternion.ToEuler();
                var actualCameraSpeed = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    ? Main.CameraSpeedAlt.Value / 1000
                    : Main.CameraSpeed.Value / 1000;
                var defaultRotSpeed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 2f : 1f;
                var defaultSpeed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 0.01f : 0.005f;

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement)
                        {
                            if (Settings.rotateAroundUserCamera)
                                CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position,
                                    VRCUtils.GetMainCamera().transform.up,
                                    -defaultRotSpeed);
                            else
                                CameraUtils.WorldCameraVector -= new Vector3(
                                    (float)Math.Sin(cameraRotation.y) * actualCameraSpeed, 0f,
                                    (float)Math.Cos(cameraRotation.y) * actualCameraSpeed);
                        }
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.localPosition += new Vector3(0f, 0f, -defaultSpeed);
                    }
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement)
                        {
                            if (Settings.rotateAroundUserCamera)
                                CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position,
                                    VRCUtils.GetMainCamera().transform.up,
                                    defaultRotSpeed);
                            else
                                CameraUtils.WorldCameraVector += new Vector3(
                                    (float)Math.Sin(cameraRotation.y) * actualCameraSpeed, 0f,
                                    (float)Math.Cos(cameraRotation.y) * actualCameraSpeed);
                        }
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.localPosition += new Vector3(0f, 0f, defaultSpeed);
                    }
                }

                if (Input.GetKey(KeyCode.PageUp))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement)
                        {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, -defaultRotSpeed);
                            else CameraUtils.WorldCameraVector += new Vector3(0f, actualCameraSpeed, 0f);
                        }
                    }
                    else
                    {
                        if (Settings.rotateAroundUserCamera) CameraUtils.GetViewFinder().transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, -defaultRotSpeed);
                        else CameraUtils.GetViewFinder().transform.localPosition += new Vector3(0f, defaultSpeed, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.PageDown))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement)
                        {
                            if (Settings.rotateAroundUserCamera) CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, defaultRotSpeed);
                            else CameraUtils.WorldCameraVector += new Vector3(0f, -actualCameraSpeed, 0f);
                        }
                    }
                    else
                    {
                        if (Settings.rotateAroundUserCamera) CameraUtils.GetViewFinder().transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, defaultRotSpeed);
                        else CameraUtils.GetViewFinder().transform.localPosition += new Vector3(0f, -defaultSpeed);
                    }
                }

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraVector -= new Vector3(
                            (float)Math.Cos(cameraRotation.y) * actualCameraSpeed, 0f,
                            (float)-Math.Sin(cameraRotation.y) * actualCameraSpeed);
                    }
                    else
                    {
                        if (Settings.rotateAroundUserCamera) CameraUtils.GetViewFinder().transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.up, -defaultRotSpeed);
                        else CameraUtils.GetViewFinder().transform.localPosition += new Vector3(-defaultSpeed, 0f, 0f);
                    }
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraVector += new Vector3(
                            (float)Math.Cos(cameraRotation.y) * actualCameraSpeed, 0f,
                            (float)-Math.Sin(cameraRotation.y) * actualCameraSpeed);
                    }
                    else
                    {
                        if (Settings.rotateAroundUserCamera) CameraUtils.GetViewFinder().transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.up, defaultRotSpeed);
                        else CameraUtils.GetViewFinder().transform.localPosition += new Vector3(defaultSpeed, 0f, 0f);
                    }
                }

                // Rotation
                if (Input.GetKey(KeyCode.Keypad8))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(-defaultRotSpeed, 0f, 0f);
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.Rotate(new Vector3(-defaultRotSpeed, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad2))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(defaultRotSpeed, 0f, 0f);
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.Rotate(new Vector3(defaultRotSpeed, 0f));
                    }
                }

                if (Input.GetKey(KeyCode.Keypad4))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(0f, -defaultRotSpeed, 0f);
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.Rotate(new Vector3(0f, 0f, -defaultRotSpeed));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad6))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(0f, defaultRotSpeed, 0f);
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.Rotate(new Vector3(0f, 0f, defaultRotSpeed));
                    }
                }

                if (Input.GetKey(KeyCode.Keypad7))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(0f, 0f, defaultRotSpeed);
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.Rotate(new Vector3(0f, -defaultRotSpeed, 0f));
                    }
                }
                if (Input.GetKey(KeyCode.Keypad9))
                {
                    if (Settings.moveCamera)
                    {
                        if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(0f, 0f, -defaultRotSpeed);
                    }
                    else
                    {
                        CameraUtils.GetViewFinder().transform.Rotate(new Vector3(0f, defaultRotSpeed, 0f));
                    }
                }

                // Reset
                if (Input.GetKeyDown(KeyCode.Keypad3))
                {
                    if (Settings.cameraEnabled) CameraUtils.ResetCamera();
                }

                // Look at player
                if (Input.GetKeyDown(KeyCode.Keypad1))
                {
                    if (Settings.cameraEnabled)
                    {
                        CameraUtils.GetViewFinder().transform.LookAt(VRCUtils.GetMainCamera().transform);
                        CameraUtils.GetViewFinder().transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
                    }
                }

                // Take pic
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    if (Settings.cameraEnabled) CameraUtils.TakePicture(0);
                }

                // Toggle camera movement
                if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (Settings.cameraEnabled)
                    {
                        if (Main.CameraMovementButton != null)
                        {
                            Settings.moveCamera = !Settings.moveCamera;
                            Main.CameraMovementButton.SetText("Camera\nMovement\n<color=#ff73fa>" + (Settings.moveCamera ? "Camera" : "Viewer") + "</color>");
                            VRCUtils.QueueHudMessage("Camera Movement set to " + (Settings.moveCamera ? "Camera" : "Viewer"));
                        }
                    }
                }
            }
        }
    }
}
