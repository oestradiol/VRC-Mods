using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace VRCDesktopCamera.Buttons {
    class SingleButton : BaseButton {
        public SingleButton(string text, string tooltip, int x, int y, Transform childOf = null, UnityAction action = null) {
            button = Object.Instantiate(InstanceUtils.SingleButtonTemplate(), childOf);
            button.name = "SingleButton";
            setText(text);
            setTooltip(tooltip);
            setButtonPosition(x, y);
            setAction(action);
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
