using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace DesktopCamera.Utils
{
    // Based on https://github.com/DubyaDude/RubyButtonAPI
    // Thanks DubyaDude and Emilia (yoshifan#9550) <3
    // I promise that one day I'll actually use the repo above, I just don't feel like rewriting the buttons codes in Main.cs right now :c
    internal class SingleButton
    {
        public Transform Button;

        public SingleButton(string name, string text, string tooltip, float x, float y, Transform childOf = null, UnityAction action = null)
        {
            Button = Object.Instantiate(QuickMenu.prop_QuickMenu_0.transform.Find("ShortcutMenu/WorldsButton"), childOf);
            Button.name = BuildInfo.Name + name;
            SetText(text);
            SetTooltip(tooltip);
            SetButtonPosition(x, y);
            SetAction(action);
        }

        public static Vector3 GetButtonPositionFor(float x, float y) => new(-630f + (x * 420f), 1050f + (y * -420f));

        public void SetButtonPosition(float x, float y)
        { Button.localPosition = GetButtonPositionFor(x, y); }

        public void SetTooltip(string tooltip)
        { Button.GetComponent<UiTooltip>().field_Public_String_0 = tooltip; }

        public void SetInteractable(bool interactable)
        { Button.gameObject.GetComponent<Button>().interactable = interactable; }

        public void SetText(string text)
        { Button.GetComponentInChildren<Text>().text = text; }

        public void SetAction(UnityAction action)
        {
            Button.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            Button.GetComponent<Button>().onClick.AddListener(action);
        }
    }
}