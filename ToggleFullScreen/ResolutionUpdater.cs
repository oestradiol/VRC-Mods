using System;
using UnityEngine;
using ResTypes = ToggleFullScreen.Main.ResTypes;

namespace ToggleFullScreen;

internal static class ResolutionUpdater
{
    internal static Resolution Previous;
    internal static bool PreviousState;
    
    internal static void OnPrefsChanged()
    {
        var current = GetCurrentAppliedRes();
        Main.Logger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to {Main.FsResolution.Value} ({current.width}x{current.height}).");
        Screen.SetResolution(current.width, current.height, true);
    }
    
    internal static void UpdateResolution()
    {
        if (Screen.fullScreen)
        {
            Previous = new Resolution
            {
                width = Screen.width,
                height = Screen.height
            };
            var current = GetCurrentAppliedRes();
            Main.Logger.Msg(ConsoleColor.Green, $"Setting FullScreen resolution to {Main.FsResolution.Value} ({current.width}x{current.height}).");
            Screen.SetResolution(current.width, current.height, true);
        }
        else
            Screen.SetResolution(Previous.width, Previous.height, false);
            
        if (UiManager.Toggle != null) 
            UiManager.Toggle.isOn = Screen.fullScreen;
        PreviousState = Screen.fullScreen;
    }

    private static Resolution GetCurrentAppliedRes() => 
        ResolutionProcessor.GetCurrentResFor(
            Enum.TryParse<ResTypes>(Main.FsResolution.Value, out var result) ? result : ResTypes.Maximum
        );
}