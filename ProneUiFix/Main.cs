using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MelonLoader;
using ProneUiFix.Utils;
using UIExpansionKit.API;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.XR;
using BuildInfo = ProneUiFix.BuildInfo;
using Main = ProneUiFix.Main;

[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: MelonInfo(typeof(Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace ProneUiFix
{
    public static class BuildInfo
    {
        public const string Name = "ProneUiFix";
        public const string Author = "Elaina";
        public const string Version = "1.0.4";
    }

    internal static class UIXManager { public static void OnApplicationStart() => ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit; }

    public class Main : MelonMod
    {
        private static MelonLogger.Instance _logger;
        private static MethodInfo _placeUi;
        private static MethodInfo PlaceUiMethod =>
            _placeUi ??= typeof(VRCUiManager).GetMethods()
                     .Where(m => 
                         m.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !m.Name.Contains("PDM") &&
                         m.GetParameters().Count(p => p.RawDefaultValue.ToString().Contains("False")) == 2)
                     .OrderBy(UnhollowerSupport.GetIl2CppMethodCallerCount).First();

        public override void OnApplicationStart()
        {
            _logger = LoggerInstance;
            
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();

            WaitForUiInit();

            _logger.Msg("Successfully loaded!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                typeof(UIXManager).GetMethod("OnApplicationStart")!.Invoke(null, null);
            else
            {
                _logger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
                static IEnumerator OnUiManagerInit()
                {
                    while (VRCUiManager.prop_VRCUiManager_0 == null)
                        yield return null;
                    VRChat_OnUiManagerInit();
                }
                MelonCoroutines.Start(OnUiManagerInit());
            }
        }

        public static void VRChat_OnUiManagerInit()
        {
            if (!XRDevice.isPresent)
                GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop")
                    .AddComponent<EnableDisableListener>().OnEnabled += delegate { MelonCoroutines.Start(PlaceMenuAgain()); };
        }

        private static IEnumerator PlaceMenuAgain()
        {
            yield return new WaitForSeconds(1);
            PlaceUiMethod.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[] { false, false });
        }
    }
}