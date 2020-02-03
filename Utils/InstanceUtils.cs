using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VRCDesktopCamera {
    public class InstanceUtils {

        public static MethodInfo timerMethod;
        public static MethodInfo setModeMethod;
        public static MethodInfo getMovementBehaviourMethod;
        public static MethodInfo getSpaceMethod;
        public static MethodInfo getCurrentPinMethod;

        public static FieldInfo worldCameraVectorField;
        public static FieldInfo worldCameraQuaternionField;

        public static void Init() {
            timerMethod = typeof(UserCameraController).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(x =>
            (x.GetParameters().Length == 1) &&
            (x.GetParameters()[0].ParameterType == typeof(int)) &&
            (x.ReturnType == typeof(IEnumerator))
            ).ToArray()[0];

            setModeMethod = typeof(UserCameraController).GetMethod("set_mode");
            getMovementBehaviourMethod = typeof(UserCameraController).GetMethod("get_movementBehaviour");
            getSpaceMethod = typeof(UserCameraController).GetMethod("get_space");
            getCurrentPinMethod = typeof(UserCameraController).GetMethod("get_currentPin");

            worldCameraVectorField = typeof(UserCameraController).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x =>
            x.FieldType == typeof(Vector3)
            ).OrderBy(x => x.Name).ToArray()[1];

            worldCameraQuaternionField = typeof(UserCameraController).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x =>
            x.FieldType == typeof(Quaternion)
            ).OrderBy(x => x.Name).ToArray()[1];
        }

        private static Transform singleButtonTemplate;
        public static Transform SingleButtonTemplate() {
            if (singleButtonTemplate == null) singleButtonTemplate = VRCToolsUtils.GetQuickMenuInstance().transform.Find("ShortcutMenu/WorldsButton");
            return singleButtonTemplate;
        }

        private static Transform menuTemplate;
        public static Transform MenuTemplate() {
            if (menuTemplate == null) menuTemplate = VRCToolsUtils.GetQuickMenuInstance().transform.Find("CameraMenu");
            return menuTemplate;
        }

        public static Camera GetMainCamera() {
            return VRCVrCamera.GetInstance().screenCamera;
        }

        // Thanks janni watermelon uwu
        public static int GetInstigatorId() {
            return VRC.PlayerManager.GetPlayer(VRC.Core.APIUser.CurrentUser.id).GetInstigatorId().Value;
        }
    }
}
