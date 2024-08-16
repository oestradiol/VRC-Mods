using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[assembly: AssemblyCopyright("Created by " + ToggleFullScreen.BuildInfo.Author)]
[assembly: MelonInfo(typeof(ToggleFullScreen.Main), ToggleFullScreen.BuildInfo.Name, ToggleFullScreen.BuildInfo.Version, ToggleFullScreen.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(System.ConsoleColor.DarkMagenta)]

namespace ToggleFullScreen
{
    public static class BuildInfo
    {
        public const string Name = "ToggleFullScreen";
        public const string Author = "Elaina";
        public const string Version = "1.0.1";
    }

    public class Main : MelonMod
    {
        private static readonly bool useHeadLook = false;
        private static Resolution Previous;
        private static Resolution MaxRes;
        private static bool PreviousState;
        private static Toggle toggle;

        public override void OnApplicationStart()
        {
            Previous = new()
            {
                width = Screen.width,
                height = Screen.height
            };

            bool Initial = Screen.fullScreen;
            Screen.fullScreen = false;
            MaxRes = Screen.currentResolution;
            Screen.fullScreen = Initial;

            static IEnumerator OnUiManagerInit()
            {
                while (VRCUiManager.prop_VRCUiManager_0 == null)
                    yield return null;

                VRChat_OnUiManagerInit();

                yield break;
            }
            MelonCoroutines.Start(OnUiManagerInit());

            MelonLogger.Msg("Successfully loaded!");
        }

        private static void VRChat_OnUiManagerInit()
        {
            // Rescales and repositions Options Panel
            Transform Settings = GameObject.Find("UserInterface/MenuContent/Screens/Settings").transform;
            Transform OtherOptions = Settings.Find("OtherOptionsPanel");
            OtherOptions.position += (Settings.Find("VoiceOptionsPanel").position - OtherOptions.position) / 12;
            OtherOptions.localScale = new Vector3(1, 1.1f, 1);

            // Repositions Ui Toggles
            Vector3 proportion = (OtherOptions.Find("TooltipsToggle").transform.position - OtherOptions.Find("3PRotationToggle").transform.position) / 7;
            List<Transform> Children = new();
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

            toggle = ToggleButton.GetComponent<Toggle>();
            toggle.isOn = Screen.fullScreen;
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((isOn) => { Screen.fullScreen = isOn; }));
        }

        public override void OnUpdate()
        {
            if (PreviousState != Screen.fullScreen)
            {
                if (Screen.fullScreen)
                {
                    Previous = new()
                    {
                        width = Screen.width,
                        height = Screen.height
                    };
                    Screen.SetResolution(MaxRes.width, MaxRes.height, true);
                }
                else
                {
                    Screen.SetResolution(Previous.width, Previous.height, false);
                }
                if ((toggle != null) && (toggle.isOn != Screen.fullScreen)) toggle.isOn = Screen.fullScreen;
                PreviousState = Screen.fullScreen;
            }
        }
    }
}