using System.Collections;
using UnityEngine;
using VRCModLoader;

namespace VRCDesktopCamera.Utils {
    public class CameraUtils {

        public enum CameraScale {
            Normal,
            Medium,
            Big
        }

        public enum CameraBehaviour {
            None,
            Smooth,
            LookAt
        }

        public enum CameraSpace {
            Attached,
            Local,
            World
        }

        public enum Pin {
            Pin1,
            Pin2,
            Pin3
        }

        public static Vector3 worldCameraVector {
            get {
                return (Vector3)InstanceUtils.worldCameraVectorField.GetValue(UserCameraController.Instance);
            }
            set {
                InstanceUtils.worldCameraVectorField.SetValue(UserCameraController.Instance, value);
            }
        }

        public static Quaternion worldCameraQuaternion {
            get {
                return (Quaternion)InstanceUtils.worldCameraQuaternionField.GetValue(UserCameraController.Instance);
            }
            set {
                InstanceUtils.worldCameraQuaternionField.SetValue(UserCameraController.Instance, value);
            }
        }

        // https://answers.unity.com/questions/489350/rotatearound-without-transform.html
        public static void RotateAround(Vector3 center, Vector3 axis, float angle) {

            Vector3 pos = worldCameraVector;
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            Vector3 dir = pos - center;
            dir = rot * dir;
            worldCameraVector = center + dir;

            Quaternion myRot = worldCameraQuaternion;
            worldCameraQuaternion *= Quaternion.Inverse(myRot) * rot * myRot;
        }

        public static void SetCameraMode(int mode) {
            InstanceUtils.setModeMethod.Invoke(UserCameraController.Instance, new object[] { mode });
        }

        public static void ResetCamera() {
            SetCameraMode(0);
            SetCameraMode(1);
            worldCameraVector = UserCameraController.Instance.viewFinder.transform.position;
            worldCameraQuaternion = UserCameraController.Instance.viewFinder.transform.rotation;
            worldCameraQuaternion *= Quaternion.Euler(90f, 0f, 180f);
            UserCameraController.Instance.photoCamera.transform.position = UserCameraController.Instance.viewFinder.transform.position;
            UserCameraController.Instance.photoCamera.transform.rotation = UserCameraController.Instance.viewFinder.transform.rotation;
        }

        public static void TakePicture(int timer) {
            ModManager.StartCoroutine((IEnumerator)InstanceUtils.timerMethod.Invoke(UserCameraController.Instance, new object[] { timer }));
        }

        public static CameraBehaviour GetCameraBehaviour() {
            return (CameraBehaviour)InstanceUtils.getMovementBehaviourMethod.Invoke(UserCameraController.Instance, new object[] { });
        }

        public static CameraSpace GetCameraSpace() {
            return (CameraSpace)InstanceUtils.getSpaceMethod.Invoke(UserCameraController.Instance, new object[] { });
        }

        public static Pin GetCurrentPin() {
            return (Pin)InstanceUtils.getCurrentPinMethod.Invoke(UserCameraController.Instance, new object[] { });
        }

    }
}
