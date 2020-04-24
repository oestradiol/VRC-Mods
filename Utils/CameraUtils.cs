using UnityEngine;

namespace DesktopCamera.Utils {
    public class CameraUtils {

        public enum CameraMode {
            Off,
            Photo,
            Video
        }

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
            World,
            // I have to check COUNT later, I suppose it's nothing important
            COUNT
        }

        public enum Pin {
            Pin1,
            Pin2,
            Pin3
        }

        public static Vector3 worldCameraVector {
            get {
                return VRCUtils.GetUserCameraController().field_Vector3_0;
            }
            set {
                VRCUtils.GetUserCameraController().field_Vector3_0 = value;
            }
        }

        public static Quaternion worldCameraQuaternion {
            get {
                return VRCUtils.GetUserCameraController().field_Quaternion_0;
            }
            set {
                VRCUtils.GetUserCameraController().field_Quaternion_0 = value;
            }
        }

        // https://answers.unity.com/questions/489350/rotatearound-without-transform.html
        public static void RotateAround(Vector3 center, Vector3 axis, float angle) {
            var pos = worldCameraVector;
            var rot = Quaternion.AngleAxis(angle, axis);
            var dir = pos - center;
            dir = rot * dir;
            worldCameraVector = center + dir;

            var myRot = worldCameraQuaternion;
            worldCameraQuaternion *= Quaternion.Inverse(myRot) * rot * myRot;
        }

        public static void SetCameraMode(CameraMode mode) {
            // This needs to be updated every VRChat has an update that changes the code
            VRCUtils.GetUserCameraController().prop_Type3571649751_0 = (Type3571649751)mode;
        }

        public static void ResetCamera() {
            SetCameraMode(CameraMode.Off);
            SetCameraMode(CameraMode.Photo);
            var camInstance = VRCUtils.GetUserCameraController();
            worldCameraVector = camInstance.viewFinder.transform.position;
            worldCameraQuaternion = camInstance.viewFinder.transform.rotation;
            worldCameraQuaternion *= Quaternion.Euler(90f, 0f, 180f);
            camInstance.photoCamera.transform.position = camInstance.viewFinder.transform.position;
            camInstance.photoCamera.transform.rotation = camInstance.viewFinder.transform.rotation;
        }

        public static void TakePicture(int timer) {
            var camInstance = VRCUtils.GetUserCameraController();
            camInstance.StartCoroutine(camInstance.Method_Private_Int32_0(timer));
        }

        public static CameraBehaviour GetCameraBehaviour() {
            // This needs to be updated every VRChat has an update that changes the code
            return (CameraBehaviour)VRCUtils.GetUserCameraController().prop_Type1241909174_0;
        }

        public static CameraSpace GetCameraSpace() {
            // This needs to be updated every VRChat has an update that changes the code
            return (CameraSpace)VRCUtils.GetUserCameraController().prop_Type2937985213_0;
        }

        public static Pin GetCurrentPin() {
            return (Pin)VRCUtils.GetUserCameraController().prop_Int32_0;
        }

    }
}