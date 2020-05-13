using Il2CppSystem.Reflection;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;
using UnhollowerRuntimeLib;
using VRC.UserCamera;

namespace DesktopCamera.Utils {

    public static class VRCUtils {

        private static FieldInfo currentPageGetter;
        public static void ShowQuickMenuPage(QuickMenu quickMenu, Transform pageTransform, string currentMenu = "ShortcutMenu") {
            if (currentPageGetter == null) {
                var menu = quickMenu.transform.Find(currentMenu).gameObject;
                var fis = Il2CppTypeOf<QuickMenu>.Type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType == Il2CppTypeOf<GameObject>.Type).ToArray();
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
            var quickMenuContextualDisplay = quickMenu.field_Private_QuickMenuContextualDisplay_0;
            quickMenuContextualDisplay.Method_Public_Void_EnumNPublicSealedvaUnNoToUs7vUsNoUnique_0(QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique.NoSelection);
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
            return VRCVrCamera.field_Private_Static_VRCVrCamera_0.screenCamera;
        }

        public static UserCameraController GetUserCameraController() {
            return UserCameraController.field_Internal_Static_UserCameraController_0;
        }

        public static Player GetPlayer() {
            // This *probably* needs to be updated every VRChat has an update that changes the code
            return PlayerManager.Method_Public_Static_Player_String_1(APIUser.CurrentUser.id);
        }

        public static VRCUiManager GetVRCUiManager() {
            return VRCUiManager.prop_VRCUiManager_0;
        }

        public static void QueueHudMessage(string message) {
            GetVRCUiManager().Method_Public_Void_String_3(message);
        }
    }
}