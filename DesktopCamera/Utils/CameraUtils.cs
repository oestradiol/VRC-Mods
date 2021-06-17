using UnityEngine;
using VRC.UserCamera;

namespace DesktopCamera.Utils
{
    public class CameraUtils
    {

        private static GameObject viewFinder = null;
        private static GameObject photoCamera = null;
        private static GameObject pinsHolder = null;

        public static GameObject GetViewFinder()
        {
            if (!viewFinder) viewFinder = VRCUtils.GetUserCameraController().transform.Find("ViewFinder").gameObject;
            return viewFinder;
        }

        public static GameObject GetPhotoCamera()
        {
            if (!photoCamera) photoCamera = VRCUtils.GetUserCameraController().transform.Find("PhotoCamera").gameObject;
            return photoCamera;
        }

        public static GameObject GetPinsHolder()
        {
            if (!pinsHolder) pinsHolder = GetViewFinder().transform.Find("UI_Pin-TRAY").gameObject;
            return pinsHolder;
        }

        public static void SetCameraMode(CameraMode mode)
        { VRCUtils.GetUserCameraController().prop_UserCameraMode_0 = (UserCameraMode)mode; }

        public static void ResetCamera()
        {
            SetCameraMode(CameraMode.Off);
            SetCameraMode(CameraMode.Photo);
            var viewFinder = GetViewFinder();
            var photoCamera = GetPhotoCamera();
            WorldCameraVector = viewFinder.transform.position;
            WorldCameraQuaternion = viewFinder.transform.rotation;
            WorldCameraQuaternion *= Quaternion.Euler(90f, 0f, 180f);
            photoCamera.transform.position = viewFinder.transform.position;
            photoCamera.transform.rotation = viewFinder.transform.rotation;
        }

        public static void TakePicture(int timer)
        {
            var camInstance = VRCUtils.GetUserCameraController();
            camInstance.StartCoroutine(camInstance.Method_Private_IEnumerator_Int32_PDM_0(timer));
        }

        // This used to be an obfuscated enum but I'll leave it like this anyway
        public static CameraBehaviour GetCameraBehaviour() => (CameraBehaviour)VRCUtils.GetUserCameraController().prop_UserCameraMovementBehaviour_0;

        // This used to be an obfuscated enum but I'll leave it like this anyway
        public static CameraSpace GetCameraSpace() => (CameraSpace)VRCUtils.GetUserCameraController().prop_UserCameraSpace_0;

        public static Pin GetCurrentPin() => (Pin)VRCUtils.GetUserCameraController().prop_Int32_0;

        public static void CycleCameraBehaviour() => GetViewFinder().transform.Find("PhotoControls/Left_CameraMode").GetComponent<CameraInteractable>().Interact();
        
        public static void CycleCameraSpace() => GetViewFinder().transform.Find("PhotoControls/Left_Space").GetComponent<CameraInteractable>().Interact();
        
        public static void SetPin(int pin) => GetPinsHolder().transform.Find("button-Pin-" + pin).GetComponent<CameraInteractable>().Interact();
        
        public static void SetFilter(string filter) =>  GetViewFinder().transform.Find("Filters/" + filter).GetComponent<CameraInteractable>().Interact();
        
        public static void TogglePinMenu() => GetViewFinder().transform.Find("PhotoControls/Left_Pins").GetComponent<CameraInteractable>().Interact();
        
        public static void ToggleLock() => GetViewFinder().transform.Find("PhotoControls/Right_Lock").GetComponent<CameraInteractable>().Interact();
        
        public static void ToggleFilterMenu() => GetViewFinder().transform.Find("PhotoControls/Right_Filters").GetComponent<CameraInteractable>().Interact();
        
        public enum CameraMode
        {
            Off,
            Photo,
            Video
        }

        public enum CameraScale
        {
            Normal,
            Medium,
            Big
        }

        public enum CameraBehaviour
        {
            None,
            Smooth,
            LookAt
        }

        public enum CameraSpace
        {
            Attached,
            Local,
            World,
            // I have to check COUNT later, I suppose it's nothing important
            COUNT
        }

        public enum Pin
        {
            Pin1,
            Pin2,
            Pin3
        }

        public static Vector3 WorldCameraVector
        {
            get => VRCUtils.GetUserCameraController().field_Private_Vector3_0;
            set { VRCUtils.GetUserCameraController().field_Private_Vector3_0 = value; }
        }

        public static Quaternion WorldCameraQuaternion
        {
            get => VRCUtils.GetUserCameraController().field_Private_Quaternion_0;
            set { VRCUtils.GetUserCameraController().field_Private_Quaternion_0 = value; }
        }

        // https://answers.unity.com/questions/489350/rotatearound-without-transform.html
        public static void RotateAround(Vector3 center, Vector3 axis, float angle)
        {
            var pos = WorldCameraVector;
            var rot = Quaternion.AngleAxis(angle, axis);
            var dir = pos - center;
            dir = rot * dir;
            WorldCameraVector = center + dir;

            var myRot = WorldCameraQuaternion;
            WorldCameraQuaternion *= Quaternion.Inverse(myRot) * rot * myRot;
        }
    }
}