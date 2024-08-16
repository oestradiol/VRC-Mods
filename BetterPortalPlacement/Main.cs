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

[assembly: AssemblyCopyright("Created by " + BetterPortalPlacement.BuildInfo.Author)]
[assembly: MelonInfo(typeof(BetterPortalPlacement.Main), BetterPortalPlacement.BuildInfo.Name, BetterPortalPlacement.BuildInfo.Version, BetterPortalPlacement.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

// This mod was firstly proposed and pre-developed by gompo and I continued/finished it
namespace BetterPortalPlacement
{
    public static class BuildInfo
    {
        public const string Name = "BetterPortalPlacement";
        public const string Author = "Elaina";
        public const string Version = "1.0.3";
    }

    internal static class UIXManager { public static void OnApplicationStart() => UIExpansionKit.API.ExpansionKitApi.OnUiManagerInit += Main.VRChat_OnUiManagerInit; }

    public class Main : MelonMod
    {
        private static MelonMod Instance;
        private static PortalPtr portalPtr;
        public static MelonPreferences_Entry<bool> IsModOn;
        public static MelonPreferences_Entry<bool> UseConfirmationPopup;
        public static MelonPreferences_Entry<bool> IsOnlyOnError;
        public static HarmonyLib.Harmony HInstance => Instance.HarmonyInstance;
        public static bool PtrIsOn() => portalPtr.enabled;

        public override void OnApplicationStart()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<PortalPtr>();
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonPreferences.CreateCategory("BetterPortalPlacement", "BetterPortalPlacement Settings");
            IsModOn = MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(IsModOn), true, "Enable BetterPortalPlacement");
            UseConfirmationPopup = MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(UseConfirmationPopup), false, "Use confirmation popup when dropping portal?");
            IsOnlyOnError = MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(IsOnlyOnError), false, "Use only on error?");
            Patches.ApplyPatches();

            WaitForUiInit();

            MelonLogger.Msg("Successfully loaded!");
        }

        private static void WaitForUiInit()
        {
            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")))
                typeof(UIXManager).GetMethod("OnApplicationStart").Invoke(null, null);
            else
            {
                MelonLogger.Warning("UiExpansionKit (UIX) was not detected. Using coroutine to wait for UiInit. Please consider installing UIX.");
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
            portalPtr = Utilities.GetPtrObj().AddComponent<PortalPtr>();
            if (XRDevice.isPresent) VRUtils.VRChat_OnUiManagerInit();
            EnableDisableListener QMListener = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements").gameObject.AddComponent<EnableDisableListener>();
            QMListener.OnEnabled += delegate { if (portalPtr.enabled) DisablePointer(); };
            QMListener.OnDisabled += delegate { VRUtils.OnQMDisable(); };
            DisablePointer();
        }

        public override void OnUpdate() => VRUtils.OnUpdate();

        public static void EnablePointer()
        {
            try { VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP"); } catch { }
            portalPtr.enabled = true;
        }

        public static void DisablePointer() => portalPtr.enabled = false;

        public static bool CanPlace()
        {
            try
            {
                var distance = Vector3.Distance(Player.prop_Player_0.transform.position, portalPtr.position);
                return !((from p in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.ToArray()
                           where p != null && p.prop_APIUser_0.id != Player.prop_Player_0.prop_APIUser_0.id && 
                           Vector3.Distance(p.transform.position, portalPtr.position) <= 1.75f
                           select p).Count() != 0 ||
                          (distance <= 1.1f || distance >= 5.1) ||
                         (from s in SpawnManager.field_Private_Static_SpawnManager_0.field_Private_List_1_Spawn_0.ToArray()
                           where (portalPtr.position - s.transform.position).sqrMagnitude < 9
                           select s).Count() != 0);
            }
            catch (Exception e)
            {
                MelonLogger.Warning("Something went wrong in calculating CanPlace bool!");
                MelonLogger.Error(e);
                return false;
            }
        }

        public static void RecreatePortal()
        {
            if (!CanPlace())
            {
                portalPtr.audio.Play();
                return;
            }
            var forward = XRDevice.isPresent ? VRUtils.GetControllerTransform().forward : VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.forward;
            PlayerIEnumerableSetup.IsUp = true;
            Patches.CreatePortal(
                Patches.CurrentInfo.ApiWorld,
                Patches.CurrentInfo.ApiWorldInstance,
                portalPtr.position - forward * 2,
                forward,
                Patches.CurrentInfo.WithUIErrors
            );
            PlayerIEnumerableSetup.IsUp = false;
            DisablePointer();
        }
    }
}