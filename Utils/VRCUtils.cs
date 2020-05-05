using Il2CppSystem.Reflection;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;

namespace DesktopCamera.Utils {

    public static class VRCUtils {

        private static FieldInfo currentPageGetter;
        public static void ShowQuickMenuPage(QuickMenu quickMenu, Transform pageTransform, string currentMenu = "ShortcutMenu") {
            if (currentPageGetter == null) {
                var menu = quickMenu.transform.Find(currentMenu).gameObject;
                var fis = QuickMenu.Il2CppType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType == GameObject.Il2CppType).ToArray();
                int count = 0;
                foreach (FieldInfo fi in fis) {
                    var value = fi.GetValue(quickMenu)?.TryCast<GameObject>();
                    if (value == menu && ++count == 2) {
                        currentPageGetter = fi;
                        break;
                    }
                }
                if (currentPageGetter == null) return;
            }

            currentPageGetter.GetValue(quickMenu).TryCast<GameObject>().SetActive(false);
            quickMenu.transform.Find("QuickMenu_NewElements/_InfoBar").gameObject.SetActive(false);
            var quickMenuContextualDisplay = quickMenu.field_QuickMenuContextualDisplay_0;
            quickMenuContextualDisplay.Method_Public_Nested0_0(QuickMenuContextualDisplay.Nested0.NoSelection);
            pageTransform.gameObject.SetActive(true);
            currentPageGetter.SetValue(quickMenu, pageTransform.gameObject);
        }

        public static QuickMenu GetQuickMenu() {
            return QuickMenu.prop_QuickMenu_0;
        }

        public static Transform SingleButtonTemplate() {
            return GetQuickMenu().transform.Find("ShortcutMenu/WorldsButton");
        }

        public static Transform MenuTemplate() {
            return GetQuickMenu().transform.Find("CameraMenu");
        }

        public static Camera GetMainCamera() {
            return VRCVrCamera.field_VRCVrCamera_0.screenCamera;
        }

        public static UserCameraController GetUserCameraController() {
            return UserCameraController.field_UserCameraController_0;
        }

        public static Player GetPlayer() {
            // This *probably* needs to be updated every VRChat has an update that changes the code
            return PlayerManager.Method_Public_String_1(APIUser.CurrentUser.id);
        }

        public static VRCUiManager GetVRCUiManager() {
            return VRCUiManager.field_VRCUiManager_0;
        }

        public static void QueueHudMessage(string message) {
            // Not working, probably another method
            GetVRCUiManager().Method_Private_String_0(message);
        }
    }
}