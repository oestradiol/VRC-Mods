using BetterPortalPlacement.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using UnhollowerRuntimeLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.XR;
using VRC;
using BuildInfo = BetterPortalPlacement.BuildInfo;
using Main = BetterPortalPlacement.Main;

[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: MelonInfo(typeof(Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

// This mod was firstly proposed and pre-developed by gompo and I continued/finished it
namespace BetterPortalPlacement
{
    public static class BuildInfo
    {
        public const string Name = "BetterPortalPlacement";
        public const string Author = "Davi";
        public const string Version = "1.0.8";
    }

    internal static class UIXManager { public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit; }

    public class Main : MelonMod
    {
        private static PortalPtr _portalPtr;
        public static MelonPreferences_Entry<bool> IsModOn;
        public static MelonPreferences_Entry<bool> UseConfirmationPopup;
        public static MelonPreferences_Entry<bool> IsOnlyOnError;
        public static HarmonyLib.Harmony HInstance;
        public static MelonLogger.Instance Logger;
        public static bool PtrIsOn() => _portalPtr.enabled;

        public override void OnApplicationStart()
        {
            Logger = LoggerInstance;
            HInstance = HarmonyInstance;
            
            ClassInjector.RegisterTypeInIl2Cpp<PortalPtr>();
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonPreferences.CreateCategory("BetterPortalPlacement", "BetterPortalPlacement Settings");
            IsModOn = MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(IsModOn), true, "Enable BetterPortalPlacement");
            UseConfirmationPopup = MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(UseConfirmationPopup), false, "Use confirmation popup when dropping portal?");
            IsOnlyOnError = MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(IsOnlyOnError), false, "Use only on error?");
            Patches.ApplyPatches();

            WaitForUiInit();

            Logger.Msg("Successfully loaded!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                typeof(UIXManager).GetMethod("OnApplicationStart")!.Invoke(null, null);
            else
            {
                Logger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
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
            _portalPtr = Utilities.GetPtrObj().AddComponent<PortalPtr>();
            if (XRDevice.isPresent) VRUtils.VRChat_OnUiManagerInit();
            var qmListener = Resources.FindObjectsOfTypeAll<VRC.UI.Elements.QuickMenu>()[0].gameObject.AddComponent<EnableDisableListener>();
            qmListener.OnEnabled += () => { if (_portalPtr.enabled) DisablePointer(); };
            qmListener.OnDisabled += VRUtils.OnQMDisable;
            DisablePointer();
        }

        public override void OnUpdate() => VRUtils.OnUpdate();

        public static void EnablePointer()
        {
            try { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); } 
            catch { /* ignored */ }
            _portalPtr.enabled = true;
        }

        private static void DisablePointer() => _portalPtr.enabled = false;

        public static bool CanPlace()
        {
            try
            {
                var distance = Vector3.Distance(Player.prop_Player_0.transform.position, _portalPtr.position);
                return !((from p in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.ToArray()
                           where p != null && p.prop_APIUser_0.id != Player.prop_Player_0.prop_APIUser_0.id && 
                           Vector3.Distance(p.transform.position, _portalPtr.position) <= 1.75f
                           select p).Count() != 0 ||
                          (distance <= 1.1f || distance >= 5.1) ||
                         (from s in SpawnManager.field_Private_Static_SpawnManager_0.field_Private_List_1_Spawn_0.ToArray()
                           where (_portalPtr.position - s.transform.position).sqrMagnitude < 9
                           select s).Count() != 0);
            }
            catch (Exception e)
            {
                Logger.Warning("Something went wrong in calculating CanPlace bool!");
                Logger.Error(e);
                return false;
            }
        }

        public static void RecreatePortal()
        {
            if (!CanPlace())
            {
                _portalPtr.audio.Play();
                return;
            }
            var forward = XRDevice.isPresent ? VRUtils.GetControllerTransform().forward : VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.forward;
            PlayerIEnumerableSetup.IsUp = true;
            Patches.CreatePortal(
                Patches.CurrentInfo.ApiWorld,
                Patches.CurrentInfo.ApiWorldInstance,
                _portalPtr.position - forward * 2,
                forward,
                Patches.CurrentInfo.WithUIErrors
            );
            PlayerIEnumerableSetup.IsUp = false;
            DisablePointer();
        }
    }
}