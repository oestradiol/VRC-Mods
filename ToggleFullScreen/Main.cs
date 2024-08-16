using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using BuildInfo = ToggleFullScreen.BuildInfo;
using Main = ToggleFullScreen.Main;

[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: MelonInfo(typeof(Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace ToggleFullScreen
{
    public static class BuildInfo
    {
        public const string Name = "ToggleFullScreen";
        public const string Author = "Elaina";
        public const string Version = "1.1.3";
    }

    public class Main : MelonMod
    {
        public static MelonPreferences_Entry<string> FsResolution;
        public static MelonLogger.Instance Logger;

        internal const bool UseHeadLook = false; // This is here just in case, since idfk what that headlook button is
        internal static bool IsUsingUix;

        internal enum ResTypes { Minimum, Low, Medium, High, Maximum }

        public override void OnApplicationStart()
        {
            Logger = LoggerInstance;
            IsUsingUix = MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit"));

            // Set preferences
            MelonPreferences.CreateCategory("ToggleFullScreen", "Toggle FullScreen");
            FsResolution = MelonPreferences.CreateEntry("ToggleFullScreen", "FSResolution", "Maximum", "FullScreen Resolution");
            ResolutionUpdater.Previous = new Resolution { width = Screen.width, height = Screen.height };
            ResolutionProcessor.CheckAndUpdateCache();

            UiManager.WaitForUiInit();
            
            Logger.Msg("Successfully loaded!");
        }

        public override void OnPreferencesSaved()
        {
            if (!Screen.fullScreen) return;
            ResolutionUpdater.OnPrefsChanged();
        }
        
        public override void OnUpdate()
        {
            if (ResolutionUpdater.PreviousState == Screen.fullScreen) return;
            ResolutionUpdater.UpdateResolution();
        }
    }
}