using UnityEngine;
using UnityEngine.UI;

namespace VRCDesktopCamera.Buttons {
    public class BaseButton {
        public Transform button;
        
        public static Vector3 getButtonPositionFor(int x, int y) {
            return new Vector3(-630f + (x * 420f), 1050f + (y * -420f));
        }

        public void setButtonPosition(int x, int y) {
            button.localPosition = getButtonPositionFor(x, y);
        }

        public void setTooltip(string tooltip) {
            button.GetComponent<UiTooltip>().text = tooltip;
            button.GetComponent<UiTooltip>().alternateText = tooltip;
        }

        public void setInteractable(bool interactable) {
            button.gameObject.GetComponent<Button>().interactable = interactable;
        }

    }
}
