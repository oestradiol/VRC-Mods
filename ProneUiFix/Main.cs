using MelonLoader;
using ProneUiFix.Utils;
using System.Collections;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.XR;

namespace ProneUiFix
{
    public static class BuildInfo
    {
        public const string Name = "ProneUiFix";
        public const string Author = "Davi";
        public const string Version = "1.0.0";
    }

    public class Main : MelonMod
    {
        private static EnableDisableListener Listener;

        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonLogger.Msg("Successfully loaded!");
        }

        public override void VRChat_OnUiManagerInit()
        {
            if (!XRDevice.isPresent)
            {
                Listener = GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop").AddComponent<EnableDisableListener>();
                Listener.OnEnabled += delegate { MelonCoroutines.Start(PlaceMenuAgain()); };
            }
        }

        private static IEnumerator PlaceMenuAgain()
        {
            float i = 0;
            while (i < 1) // Wait for 1s
            {
                i += Time.deltaTime;
                yield return null;
            }
            VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_Boolean_1(true);
        }
    }
}