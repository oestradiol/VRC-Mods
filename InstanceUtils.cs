using UnityEngine;
using VRCTools;

namespace VRCDesktopCamera {
    public class InstanceUtils {
        private static Transform singleButtonTemplate;
        public static Transform SingleButtonTemplate() {
            if (singleButtonTemplate == null) singleButtonTemplate = QuickMenuUtils.GetQuickMenuInstance().transform.Find("ShortcutMenu/WorldsButton");
            return singleButtonTemplate;
        }

        private static Transform menuTemplate;
        public static Transform MenuTemplate() {
            if (menuTemplate == null) menuTemplate = QuickMenuUtils.GetQuickMenuInstance().transform.Find("CameraMenu");
            return menuTemplate;
        }
    }
}
