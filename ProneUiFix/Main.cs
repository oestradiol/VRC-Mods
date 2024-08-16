using MelonLoader;
using ProneUiFix.Utils;
using System.Collections;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.XR;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

[assembly: AssemblyCopyright("Created by " + ProneUiFix.BuildInfo.Author)]
[assembly: MelonInfo(typeof(ProneUiFix.Main), ProneUiFix.BuildInfo.Name, ProneUiFix.BuildInfo.Version, ProneUiFix.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(System.ConsoleColor.DarkMagenta)]

namespace ProneUiFix
{
    public static class BuildInfo
    {
        public const string Name = "ProneUiFix";
        public const string Author = "Elaina";
        public const string Version = "1.0.1";
    }

    public class Main : MelonMod
    {
        private static EnableDisableListener Listener;
        private static MethodInfo PlaceUiMethod;
        private static void SetPlaceUiMethod() //Got from https://github.com/M-oons/VRChat-Mods/blob/master/ComfyVRMenu/Main.cs, thank you M-oons!
        {
            MethodInfo _placeUi = null;
            try
            {
                var xrefs = XrefScanner.XrefScan(typeof(VRCUiManager).GetMethod(nameof(VRCUiManager.LateUpdate)));
                foreach (var x in xrefs)
                {
                    if (x.Type == XrefType.Method && x.TryResolve() != null &&
                        x.TryResolve().GetParameters().Length == 1 &&
                        x.TryResolve().GetParameters()[0].ParameterType == typeof(bool))
                    {
                        _placeUi = (MethodInfo)x.TryResolve();
                        break;
                    }
                };
            }
            catch { }
            PlaceUiMethod = _placeUi;
        }

        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(WaitForUIInit());
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonLogger.Msg("Successfully loaded!");
            SetPlaceUiMethod();
        }

        public static IEnumerator WaitForUIInit()
        {
            while (GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop") == null)
                yield return null;

            if (!XRDevice.isPresent)
            {
                Listener = GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop").AddComponent<EnableDisableListener>();
                Listener.OnEnabled += delegate { MelonCoroutines.Start(PlaceMenuAgain()); };
            }
        }

        private static IEnumerator PlaceMenuAgain()
        {
            yield return new WaitForSeconds(1);
            PlaceUiMethod.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[] { true });
        }
    }
}