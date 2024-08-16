using ProneUiFix.Utils;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnhollowerRuntimeLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.XR;

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
        public const string Version = "1.0.2";
    }

    public class Main : MelonMod
    {
        private static EnableDisableListener Listener;
        private static MethodInfo placeUi;
        private static MethodInfo PlaceUiMethod
        {
            get
            {
                if (placeUi == null) placeUi = typeof(VRCUiManager).GetMethods()
                     .Where(m => 
                         m.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !m.Name.Contains("PDM") &&
                         m.GetParameters().Where(p => p.RawDefaultValue.ToString().Contains("False")).Count() == 2)
                     .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).First();
                return placeUi;
            }
        }

        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(WaitForUIInit());
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonLogger.Msg("Successfully loaded!");
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
            PlaceUiMethod.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[] { false, false });
        }
    }
}