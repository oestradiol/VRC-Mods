using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VRCDesktopCamera {
    // Copied from VRCTools, with the intention to not require it anymore
    public static class VRCToolsUtils {
        private static QuickMenu quickmenuInstance;
        private static VRCUiManager uiManagerInstance;
        private static FieldInfo currentPageGetter;
        private static FieldInfo quickmenuContextualDisplayGetter;

        public static QuickMenu GetQuickMenuInstance() {
            if (quickmenuInstance == null) {
                MethodInfo quickMenuInstanceGetter = typeof(QuickMenu).GetMethod("get_Instance", BindingFlags.Public | BindingFlags.Static);
                if (quickMenuInstanceGetter == null) return null;
                quickmenuInstance = ((QuickMenu)quickMenuInstanceGetter.Invoke(null, new object[] { }));
            }
            return quickmenuInstance;
        }

        // Partial reproduction of SetMenuIndex from QuickMenu
        internal static void ShowQuickmenuPage(string pagename, string currentMenu = "ShortcutMenu") {
            QuickMenu quickmenu = GetQuickMenuInstance();
            Transform pageTransform = quickmenu?.transform.Find(pagename);
            if (pageTransform == null) {
            }

            if (currentPageGetter == null) {
                GameObject menu = quickmenu.transform.Find(currentMenu).gameObject;
                FieldInfo[] fis = typeof(QuickMenu).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where((fi) => fi.FieldType == typeof(GameObject)).ToArray();
                int count = 0;
                foreach (FieldInfo fi in fis) {
                    GameObject value = fi.GetValue(quickmenu) as GameObject;
                    if (value == menu && ++count == 2) {
                        currentPageGetter = fi;
                        break;
                    }
                }
                if (currentPageGetter == null) {
                    return;
                }
            }

            ((GameObject)currentPageGetter.GetValue(quickmenu))?.SetActive(false);
            GetQuickMenuInstance().transform.Find("QuickMenu_NewElements/_InfoBar").gameObject.SetActive(false);

            if (quickmenuContextualDisplayGetter != null)
                quickmenuContextualDisplayGetter = typeof(QuickMenu).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault((fi) => fi.FieldType == typeof(QuickMenuContextualDisplay));
            QuickMenuContextualDisplay quickmenuContextualDisplay = quickmenuContextualDisplayGetter?.GetValue(quickmenu) as QuickMenuContextualDisplay;
            if (quickmenuContextualDisplay != null) {
                currentPageGetter.SetValue(quickmenu, pageTransform.gameObject);
                typeof(QuickMenuContextualDisplay).GetMethod("SetDefaultContext", BindingFlags.Public | BindingFlags.Instance).Invoke(quickmenuContextualDisplay, new object[] { 0, null, null }); // This is the only way to pass the unknown enum type value
            }

            currentPageGetter.SetValue(quickmenu, pageTransform.gameObject);
            typeof(QuickMenu).GetMethod("SetContext", BindingFlags.Public | BindingFlags.Instance).Invoke(quickmenu, new object[] { 1, null, null }); // This is the only way to pass the unknown enum type value
            pageTransform.gameObject.SetActive(true);
        }

        public static IEnumerator WaitForUiManagerInit() {
            if (uiManagerInstance == null) {
                FieldInfo[] nonpublicStaticFields = typeof(VRCUiManager).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
                if (nonpublicStaticFields.Length == 0) {
                    yield break;
                }
                FieldInfo uiManagerInstanceField = nonpublicStaticFields.First(field => field.FieldType == typeof(VRCUiManager));
                if (uiManagerInstanceField == null) {
                    yield break;
                }
                uiManagerInstance = uiManagerInstanceField.GetValue(null) as VRCUiManager;
                while (uiManagerInstance == null) {
                    uiManagerInstance = uiManagerInstanceField.GetValue(null) as VRCUiManager;
                    yield return null;
                }
            }
        }

        public static VRCUiManager GetUiManagerInstance() {
            if (uiManagerInstance == null) {
                MethodInfo uiManagerInstanceGetter = typeof(VRCUiManager).GetMethod("get_Instance", BindingFlags.Public | BindingFlags.Static);
                if (uiManagerInstanceGetter == null) return null;
                uiManagerInstance = ((VRCUiManager)uiManagerInstanceGetter.Invoke(null, new object[] { }));
            }
            return uiManagerInstance;
        }
    }
}