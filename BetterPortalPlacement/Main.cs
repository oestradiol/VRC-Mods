using MelonLoader;
using System;
using System.Reflection;
using Harmony;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRC.Core;
using BetterPortalPlacement.Utils;
using UnityEngine.XR;
using VRC;
using System.Linq;

[assembly: AssemblyCopyright("Created by " + BetterPortalPlacement.BuildInfo.Author)]
[assembly: MelonInfo(typeof(BetterPortalPlacement.Main), BetterPortalPlacement.BuildInfo.Name, BetterPortalPlacement.BuildInfo.Version, BetterPortalPlacement.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

// This mod was firstly proposed and pre-developed by gompo and I continued/finished it
namespace BetterPortalPlacement
{
    public static class BuildInfo
    {
        public const string Name = "BetterPortalPlacement";
        public const string Author = "Davi";
        public const string Version = "1.0.0";
    }

    public class Main : MelonMod
    {
        private static PortalPtr portalPtr;
        private static PortalInfo portalInfo;
        private static MelonMod Instance;
        private static MelonPreferences_Entry<bool> IsModOn;
        private static MelonPreferences_Entry<bool> IsOnlyOnError;
        public static HarmonyInstance HarmonyInstance => Instance.Harmony;

        public override void OnApplicationStart()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<PortalPtr>();
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonPreferences.CreateCategory("BetterPortalPlacement", "BetterPortalPlacement Settings");
            IsModOn = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(IsModOn), true, "Enable BetterPortalPlacement");
            IsOnlyOnError = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("BetterPortalPlacement", nameof(IsOnlyOnError), false, "Use only on error?");
            Utilities.ApplyPatches();
            MelonLogger.Msg("Successfully loaded!");
        }

        public override void OnUpdate() => VRUtils.OnUpdate();

        public override void VRChat_OnUiManagerInit()
        {
            portalPtr = Utilities.GetPtrObj().AddComponent<PortalPtr>();
            if (XRDevice.isPresent) VRUtils.VRChat_OnUiManagerInit();
            EnableDisableListener QMListener = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements").gameObject.AddComponent<EnableDisableListener>();
            QMListener.OnEnabled += delegate { if (portalPtr.enabled) DisablePointer(); };
            QMListener.OnDisabled += delegate { VRUtils.OnQMDisable(); };
            DisablePointer();
        }

        private static void EnablePointer()
        {
            portalPtr.enabled = true;
            try { VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_0(); } catch { }
        }

        private static void DisablePointer() => portalPtr.enabled = false;

        public static bool OnPortalCreated(ApiWorld __0, ApiWorldInstance __1, Vector3 __2, Vector3 __3, bool __4, MethodInfo __originalMethod)
        {
            if (IsModOn.Value && !portalPtr.enabled)
            {
                portalInfo = new PortalInfo(__0, __1, __2, __3, __4);
                if (!IsOnlyOnError.Value)
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_1(
                        "Portal Placement", "Manual placement activated.\nPress ok to place portal.", "Ok", new Action(delegate { EnablePointer(); }));
                    return false;
                }
            }
            return true;
        }

        public static bool ShowAlert(ref string __0, ref string __1)
        {
            if (IsModOn.Value && __0.Contains("Cannot Create Portal"))
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_1(
                    "Failed to create portal", "Error: " + __1 + "\nPress continue to try again.", "Continue", new Action(delegate { EnablePointer(); }));
                return false;
            }
            return true;
        }

        public static bool CanPlace() => 
            !((from p in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.ToArray()
                where p != null && Vector3.Distance(p.transform.position, portalPtr.position) <= 1.75f
                select p).Count() != 0 ||
              (from s in SpawnManager.field_Private_Static_SpawnManager_0.field_Private_List_1_Spawn_0.ToArray()
                where (portalPtr.position - s.transform.position).sqrMagnitude < 9
                select s).Count() != 0);

        public static void RecreatePortal()
        {
            if (!CanPlace())
            {
                portalPtr.audio.Play();
                return;
            }
            var forward = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.forward;
            Utilities.CreatePortal(
                portalInfo.ApiWorld,
                portalInfo.ApiWorldInstance,
                portalPtr.position + (XRDevice.isPresent ? Vector3.one / 2 : - forward * 2),
                XRDevice.isPresent ? VRUtils.GetControllerTransform().forward : forward,
                portalInfo.WithUIErrors
            );
            DisablePointer();
        }
    }
}