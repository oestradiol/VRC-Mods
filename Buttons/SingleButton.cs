using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using VRCDesktopCamera.Utils;

namespace VRCDesktopCamera.Buttons {
    class SingleButton {

        public Transform button;

        public SingleButton(string name, string text, string tooltip, int x, int y, Transform childOf = null, UnityAction action = null) {
            button = Object.Instantiate(VRCUtils.SingleButtonTemplate(), childOf);
            button.name = "nitro" + name;
            setText(text);
            setTooltip(tooltip);
            setButtonPosition(x, y);
            setAction(action);
        }

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

        public void setText(string text) {
            button.GetComponentInChildren<Text>().text = text;
        }

        public void setAction(UnityAction action) {
            button.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<Button>().onClick.AddListener(action);
        }
    }
}