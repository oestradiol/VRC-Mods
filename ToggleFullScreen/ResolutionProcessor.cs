using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ResTypes = ToggleFullScreen.Main.ResTypes;

namespace ToggleFullScreen;

internal static class ResolutionProcessor
{
    internal static readonly Dictionary<ResTypes, Resolution> ResCache = new();

    internal static Resolution GetCurrentResFor(ResTypes quality)
    {
        CheckAndUpdateCache();
        return ResCache[quality];
    }

    internal static void CheckAndUpdateCache()
    {
        var temp = GetCurrentMaxRes();
        var maxRes = ResCache[ResTypes.Maximum];
        if (maxRes.width == temp.width && maxRes.height == temp.height) return;
        ResCache[ResTypes.Maximum] = temp;
        ResCache[ResTypes.High] = CalculatePropRes(new Resolution { width = 1600, height = 900 });
        ResCache[ResTypes.Medium] = CalculatePropRes(new Resolution { width = 1366, height = 768 });
        ResCache[ResTypes.Low] = CalculatePropRes(new Resolution { width = 1280, height = 720 });
        ResCache[ResTypes.Minimum] = CalculatePropRes(new Resolution { width = 852, height = 480 });
        if (Main.IsUsingUix) UiManager.UpdateUixSwitch();
    }
    
    // Had to use Natives below because all C# AND Unity methods failed me so why not :)
    private static Resolution GetCurrentMaxRes()
    {
        var currentHdc = GetWindowDC(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
        return new Resolution
            {
                width = GetDeviceCaps(currentHdc, 8), // DeviceCap.HORZRES = 8
                height = GetDeviceCaps(currentHdc, 10) // DeviceCap.VERTRES = 10
            };
    }
    
    [DllImport("User32.dll", CharSet = CharSet.Auto)] private static extern IntPtr GetWindowDC(IntPtr hWnd);
    
    [DllImport("gdi32.dll")] private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    private static Resolution CalculatePropRes(Resolution propTo)
    {
        var maxRes = ResCache[ResTypes.Maximum];
        return new Resolution
        {
                // I didn't know which resolutions to use and I didn't wanna make a set for each monitor so I thought:
                // "Why not just make them so they all follow the most used ones?"
                // -and this came of it.
                width = (int)Math.Floor(propTo.width * maxRes.width / 1920d),
                height = (int)Math.Floor(propTo.height * maxRes.height / 1080d)
        };
    }
}