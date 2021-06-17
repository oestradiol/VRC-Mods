using System;
using UnityEngine;

namespace DesktopCamera.Utils
{
    internal static class MovementManager
    {
        private static Vector3 cameraRotation;
        private static float DefaultRotSpeed, DefaultSpeed, ActualCameraSpeed;

        public static void KeysListener()
        {
            if (Settings.cameraEnabled && Settings.arrowKeysEnabled)
            {
                cameraRotation = CameraUtils.WorldCameraQuaternion.ToEuler();
                DefaultRotSpeed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 2f : 1f;
                DefaultSpeed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 0.01f : 0.005f;
                ActualCameraSpeed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? Main.CameraSpeedAlt.Value / 1000 : Main.CameraSpeed.Value / 1000;

                // Position
                if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) ArrowsX(false);
                else if (!Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow)) ArrowsX(true);

                if (Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)) ArrowsY(false);
                else if (!Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow)) ArrowsY(true);

                if (Input.GetKey(KeyCode.PageUp) && !Input.GetKey(KeyCode.PageDown)) PageBtn(false);
                else if (!Input.GetKey(KeyCode.PageUp) && Input.GetKey(KeyCode.PageDown)) PageBtn(true);

                // Rotation
                if (Input.GetKey(KeyCode.Keypad6) && !Input.GetKey(KeyCode.Keypad4)) KeypadArrowsX(false);
                else if (!Input.GetKey(KeyCode.Keypad6) && Input.GetKey(KeyCode.Keypad4)) KeypadArrowsX(true);

                if (Input.GetKey(KeyCode.Keypad8) && !Input.GetKey(KeyCode.Keypad2)) KeypadArrowsY(false);
                else if (!Input.GetKey(KeyCode.Keypad8) && Input.GetKey(KeyCode.Keypad2)) KeypadArrowsY(true);

                if (Input.GetKey(KeyCode.Keypad9) && !Input.GetKey(KeyCode.Keypad7)) KeypadUpper(false);
                else if (!Input.GetKey(KeyCode.Keypad9) && Input.GetKey(KeyCode.Keypad7)) KeypadUpper(true);

                // Reset
                if (Input.GetKeyDown(KeyCode.Keypad3) && Settings.cameraEnabled) CameraUtils.ResetCamera();

                // Look at player
                if (Input.GetKeyDown(KeyCode.Keypad1) && Settings.cameraEnabled)
                {
                    CameraUtils.GetViewFinder().transform.LookAt(VRCUtils.GetMainCamera().transform);
                    CameraUtils.GetViewFinder().transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
                }

                // Take pic
                if (Input.GetKeyDown(KeyCode.KeypadPlus) && Settings.cameraEnabled) CameraUtils.TakePicture(0);

                // Toggle camera movement
                if (Input.GetKeyDown(KeyCode.KeypadMinus) && Settings.cameraEnabled && Main.CameraMovementButton != null)
                {
                    Settings.moveCamera = !Settings.moveCamera;
                    Main.CameraMovementButton.SetText("Camera\nMovement\n<color=#ff73fa>" + (Settings.moveCamera ? "Camera" : "Viewer") + "</color>");
                    VRCUtils.QueueHudMessage("Camera Movement set to " + (Settings.moveCamera ? "Camera" : "Viewer"));
                }
            }
        }

        private static void ArrowsX(bool isNeg)
        {
            if (Settings.moveCamera)
            {
                if (Settings.allowCameraMovement) CameraUtils.WorldCameraVector += new Vector3(
                    (float)Math.Cos(cameraRotation.y) * (isNeg ? -1 : 1) * ActualCameraSpeed, 0f,
                    (float)-Math.Sin(cameraRotation.y) * (isNeg ? -1 : 1) * ActualCameraSpeed);
            }
            else
            {
                if (Settings.rotateAroundUserCamera)
                    CameraUtils.GetViewFinder().transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.up, (isNeg ? -1 : 1) * DefaultRotSpeed);
                else CameraUtils.GetViewFinder().transform.localPosition += new Vector3((isNeg ? -1 : 1) * DefaultSpeed, 0f, 0f);
            }
        }

        private static void ArrowsY(bool isNeg)
        {
            if (Settings.moveCamera)
            {
                if (Settings.allowCameraMovement)
                {
                    if (Settings.rotateAroundUserCamera)
                        CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position,
                            VRCUtils.GetMainCamera().transform.up,
                            (isNeg ? -1 : 1) * DefaultRotSpeed);
                    else
                        CameraUtils.WorldCameraVector += new Vector3(
                            (float)Math.Sin(cameraRotation.y) * (isNeg ? -1 : 1) * ActualCameraSpeed, 0f,
                            (float)Math.Cos(cameraRotation.y) * (isNeg ? -1 : 1) * ActualCameraSpeed);
                }
            }
            else CameraUtils.GetViewFinder().transform.localPosition += new Vector3(0f, 0f, (isNeg ? -1 : 1) * DefaultSpeed);
        }

        private static void PageBtn(bool isNeg)
        {
            if (Settings.moveCamera)
            {
                if (Settings.allowCameraMovement)
                {
                    if (Settings.rotateAroundUserCamera)
                        CameraUtils.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, (!isNeg ? -1 : 1) * DefaultRotSpeed);
                    else CameraUtils.WorldCameraVector += new Vector3(0f, (isNeg ? -1 : 1) * ActualCameraSpeed, 0f);
                }
            }
            else
            {
                if (Settings.rotateAroundUserCamera)
                    CameraUtils.GetViewFinder().transform.RotateAround(VRCUtils.GetMainCamera().transform.position, VRCUtils.GetMainCamera().transform.right, (!isNeg ? -1 : 1) * DefaultRotSpeed);
                else CameraUtils.GetViewFinder().transform.localPosition += new Vector3(0f, (isNeg ? -1 : 1) * DefaultSpeed, 0f);
            }
        }

        private static void KeypadArrowsX(bool isNeg)
        {
            if (Settings.moveCamera)
            { if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(0f, (isNeg ? -1 : 1) * DefaultRotSpeed, 0f); }
            else CameraUtils.GetViewFinder().transform.Rotate(new Vector3(0f, 0f, (isNeg ? -1 : 1) * DefaultRotSpeed));
        }

        private static void KeypadArrowsY(bool isNeg)
        {
            if (Settings.moveCamera)
            { if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler((!isNeg ? -1 : 1) * DefaultRotSpeed, 0f, 0f); }
            else CameraUtils.GetViewFinder().transform.Rotate(new Vector3((!isNeg ? -1 : 1) * DefaultRotSpeed, 0f, 0f));
        }

        private static void KeypadUpper(bool isNeg)
        {
            if (Settings.moveCamera)
            { if (Settings.allowCameraMovement) CameraUtils.WorldCameraQuaternion *= Quaternion.Euler(0f, 0f, (!isNeg ? -1 : 1) * DefaultRotSpeed); }
            else CameraUtils.GetViewFinder().transform.Rotate(new Vector3(0f, (isNeg ? -1 : 1) * DefaultRotSpeed, 0f));
        }
    }
}