using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DesktopCamera.Utils;

namespace DesktopCamera.Buttons
{

    // Based on https://github.com/DubyaDude/RubyButtonAPI
    // Thanks DubyaDude and Emilia (yoshifan#9550) <3
    // I promise that one day I'll actually use the repo above, I just don't feel like rewriting the buttons codes in Main.cs right now :c

    class SingleButton
    {

        public Transform button;

        public SingleButton(string name, string text, string tooltip, int x, int y, Transform childOf = null, UnityAction action = null)
        {
            button = Object.Instantiate(VRCUtils.SingleButtonTemplate(), childOf);
            button.name = "nitro" + name;
            setText(text);
            setTooltip(tooltip);
            setButtonPosition(x, y);
            setAction(action);
        }

        public static Vector3 getButtonPositionFor(int x, int y)
        {
            return new Vector3(-630f + (x * 420f), 1050f + (y * -420f));
        }

        public void setButtonPosition(int x, int y)
        {
            button.localPosition = getButtonPositionFor(x, y);
        }

        public void setTooltip(string tooltip)
        {
            button.GetComponent<UiTooltip>().field_Public_String_0 = tooltip;
        }

        public void setInteractable(bool interactable)
        {
            button.gameObject.GetComponent<Button>().interactable = interactable;
        }

        public void setText(string text)
        {
            button.GetComponentInChildren<Text>().text = text;
        }

        public void setAction(UnityAction action)
        {
            button.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<Button>().onClick.AddListener(action);
        }
    }
}