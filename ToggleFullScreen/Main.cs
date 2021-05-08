using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace ToggleFullScreen
{
    public static class BuildInfo
    {
        public const string Name = "ToggleFullScreen";
        public const string Author = "Davi";
        public const string Version = "1.0.0";
    }

    public class Main : MelonMod
    {
        private static readonly bool useHeadLook = false;

        private static Resolution Previous = new Resolution()
        {
            width = Screen.width,
            height = Screen.height
        };

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Successfully loaded!");
        }

        public override void VRChat_OnUiManagerInit()
        {
            // Rescales and repositions Options Panel
            Transform Settings = GameObject.Find("UserInterface/MenuContent/Screens/Settings").transform;
            Transform OtherOptions = Settings.Find("OtherOptionsPanel");
            OtherOptions.position += (Settings.Find("VoiceOptionsPanel").position - OtherOptions.position) / 12;
            OtherOptions.localScale = new Vector3(1, 1.1f, 1);

            // Repositions Ui Toggles
            Vector3 proportion = (OtherOptions.Find("TooltipsToggle").transform.position - OtherOptions.Find("3PRotationToggle").transform.position) / 7;
            List<Transform> Children = new List<Transform>();
            for (int i = 0; i < OtherOptions.GetChildCount(); i++)
            {
                var child = OtherOptions.GetChild(i);
                if (!child.name.Contains("Panel_Header") && !child.name.Contains("TitleText")) Children.Add(child);
            }
            if (!useHeadLook) Children.Remove(Children.Find(x => x.name.Contains("HeadLookToggle")));
            Children.Sort((x, y) => y.position.y.CompareTo(x.position.y));

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.position += proportion * i;
                child.localScale = new Vector3(1, (float)(1 / 1.1), 1);
            }
            if (!useHeadLook)
            {
                Transform HeadLookToggle = OtherOptions.Find("HeadLookToggle");
                HeadLookToggle.position = Children.Find(x => x.name.Contains("AllowAvatarCopyingToggle")).position;
                HeadLookToggle.localScale = new Vector3(1, (float)(1 / 1.1), 1);
            }

            // Creates new Toggle
            Transform ToggleButton = Object.Instantiate(OtherOptions.Find("ShowCommunityLabsToggle").gameObject, OtherOptions).transform;
            Object.DestroyImmediate(ToggleButton.GetComponent<UiSettingConfig>());

            ToggleButton.name = "FullScreenToggle";
            ToggleButton.GetComponentInChildren<Text>().text = "TOGGLE FULLSCREEN";

            Toggle toggle = ToggleButton.GetComponent<Toggle>();
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((isOn) => { OnToggle(isOn); }));
            toggle.isOn = Screen.fullScreen;
        }

        private static void OnToggle(bool isOn)
        {
            if (isOn)
            {
                Previous = new Resolution()
                {
                    width = Screen.width,
                    height = Screen.height
                };
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
            else
            {
                Screen.SetResolution(Previous.width, Previous.height, false);
            }
        }
    }
}