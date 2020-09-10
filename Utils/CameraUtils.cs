using UnityEngine;
using VRC.UserCamera;

namespace DesktopCamera.Utils {
    public class CameraUtils {

        public static void SetCameraMode(CameraMode mode) {
            VRCUtils.GetUserCameraController().prop_EnumPublicSealedvaOfPhVi4vUnique_0 = (EnumPublicSealedvaOfPhVi4vUnique)mode;
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
            camInstance.StartCoroutine(camInstance.Method_Private_IEnumerator_Int32_PDM_0(timer));
        }

        public static CameraBehaviour GetCameraBehaviour() {
            return (CameraBehaviour)VRCUtils.GetUserCameraController().prop_EnumPublicSealedvaNoSmLo4vUnique_0;
        }

        public static CameraSpace GetCameraSpace() {
            return (CameraSpace)VRCUtils.GetUserCameraController().prop_EnumPublicSealedvaAtLoWoCO5vUnique_0;
        }

        public static Pin GetCurrentPin() {
            return (Pin)VRCUtils.GetUserCameraController().prop_Int32_0;
        }

        public static void CycleCameraBehaviour() {
            VRCUtils.GetUserCameraController().viewFinder.transform.Find("PhotoControls/Left_CameraMode").GetComponent<CameraInteractable>().Interact();
        }

        public static void CycleCameraSpace() {
            VRCUtils.GetUserCameraController().viewFinder.transform.Find("PhotoControls/Left_Space").GetComponent<CameraInteractable>().Interact();
        }

        public static void SetPin(int pin) {
            VRCUtils.GetUserCameraController().pinsHolder.transform.Find("button-Pin-" + pin).GetComponent<CameraInteractable>().Interact();
        }

        public static void SetFilter(string filter) {
            VRCUtils.GetUserCameraController().filtersHolder.transform.Find(filter).GetComponentInChildren<CameraInteractable>().Interact();
        }

        public static void TogglePinMenu() {
            VRCUtils.GetUserCameraController().viewFinder.transform.Find("PhotoControls/Left_Pins").GetComponent<CameraInteractable>().Interact();
        }

        public static void ToggleLock() {
            VRCUtils.GetUserCameraController().viewFinder.transform.Find("PhotoControls/Right_Lock").GetComponent<CameraInteractable>().Interact();
        }

        public static void ToggleFilterMenu() {
            VRCUtils.GetUserCameraController().viewFinder.transform.Find("PhotoControls/Right_Filters").GetComponent<CameraInteractable>().Interact();
        }


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
                return VRCUtils.GetUserCameraController().field_Private_Vector3_0;
            }
            set {
                VRCUtils.GetUserCameraController().field_Private_Vector3_0 = value;
            }
        }

        public static Quaternion worldCameraQuaternion {
            get {
                return VRCUtils.GetUserCameraController().field_Private_Quaternion_0;
            }
            set {
                VRCUtils.GetUserCameraController().field_Private_Quaternion_0 = value;
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
    }
}