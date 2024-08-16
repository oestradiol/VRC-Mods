﻿using System.Linq;
using Il2CppSystem.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRC.UserCamera;

namespace DesktopCamera.Utils
{
    // Thanks Emilia (yoshifan#9550) <3
    internal static class VRCUtils
    {
        private static FieldInfo currentPageGetter;

        public static void ShowQuickMenuPage(QuickMenu quickMenu, Transform pageTransform, string currentMenu = "ShortcutMenu")
        {
            if (currentPageGetter == null)
            {
                var menu = quickMenu.transform.Find(currentMenu).gameObject;
                var fis = Il2CppType.Of<QuickMenu>().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType == Il2CppType.Of<GameObject>()).ToArray();
                int count = 0;
                foreach (FieldInfo fi in fis)
                {
                    var value = fi.GetValue(quickMenu)?.TryCast<GameObject>();
                    if (value == menu && ++count == 2)
                    {
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

        public static Camera GetMainCamera() => VRCVrCamera.field_Private_Static_VRCVrCamera_0.field_Public_Camera_0;

        public static UserCameraController GetUserCameraController() => UserCameraController.field_Internal_Static_UserCameraController_0;
        
        public static void QueueHudMessage(string message) => VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_PDM_0(message);
    }
}