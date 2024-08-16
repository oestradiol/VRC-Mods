using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using UnityEngine;
using VRC;
using VRC.Core;

namespace BetterPortalPlacement.Utils
{
    // Almost this entire class came from Gompog :) // I'm going to bonk you one day - Gompo // Please don't? Love you x3 <3
    internal static class Patches
    {
        public static PortalInfo CurrentInfo;

        public static void ApplyPatches()
        {
            Main.HarmonyInstance.Patch(CreatePortalMethod, new HarmonyMethod(typeof(Patches).GetMethod(nameof(OnPortalCreated))));
            Main.HarmonyInstance.Patch(typeof(VRCUiPopupManager).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Void_String_String_Single_"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last(),
                new HarmonyMethod(typeof(Patches).GetMethod(nameof(ShowAlert))));
            // PlayerIEnumerableSetup.Patch(); // Still have to make this work
        }

        public static void CloseMenu(bool __0, bool __1) => GetCloseMenuDelegate(__0, __1);
        private static CloseMenuDelegate closeMenuDelegate;
        private delegate void CloseMenuDelegate(bool __0, bool __1);
        private static CloseMenuDelegate GetCloseMenuDelegate
        {
            get
            {
                if (closeMenuDelegate != null) return closeMenuDelegate;
                MethodInfo closeMenuMethod = typeof(VRCUiManager).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !method.Name.Contains("_PDM_"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();
                closeMenuDelegate = (CloseMenuDelegate)Delegate.CreateDelegate(typeof(CloseMenuDelegate), VRCUiManager.prop_VRCUiManager_0, closeMenuMethod);
                return closeMenuDelegate;
            }
        }

        public static bool CreatePortal(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 foward, bool someBool) =>
            GetCreatePortalDelegate(apiWorld, apiWorldInstance, pos, foward, someBool);
        private static CreatePortalDelegate createPortalDelegate;
        private delegate bool CreatePortalDelegate(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 foward, bool someBool);
        private static CreatePortalDelegate GetCreatePortalDelegate
        {
            get
            {
                if (createPortalDelegate != null) return createPortalDelegate;
                createPortalDelegate = (CreatePortalDelegate)Delegate.CreateDelegate(typeof(CreatePortalDelegate), null, CreatePortalMethod);
                return createPortalDelegate;
            }
        }

        private static MethodInfo CreatePortalMethod => typeof(PortalInternal).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_Vector3_Vector3_Boolean_"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();


        public static bool OnPortalCreated(ApiWorld __0, ApiWorldInstance __1, Vector3 __2, Vector3 __3, bool __4, MethodInfo __originalMethod)
        {
            if (Main.IsModOn.Value && !Main.PtrIsOn())
            {
                CurrentInfo = new PortalInfo(__0, __1, __2, __3, __4);
                if (!Main.IsOnlyOnError.Value)
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_1(
                        "Portal Placement", "Manual placement activated.\nPress ok to place portal.", "Ok", new Action(delegate { Main.EnablePointer(); }));
                    return false;
                }
            }
            return true;
        }

        public static bool ShowAlert(ref string __0, ref string __1)
        {
            if (Main.IsModOn.Value && __0.Contains("Cannot Create Portal") && !__1.Contains("You cannot create a portal to a private world created by another user."))
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_1(
                    "Failed to create portal", "Error: " + __1 + "\nPress continue to try again.", "Continue", new Action(delegate { Main.EnablePointer(); }));
                return false;
            }
            return true;
        }
    }

    //// Apparently Knah is a god :o
    //class PlayerIEnumerableSetup
    //{
    //    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //    private delegate IEnumerable<Player> PlayerIEnumerableSetupDelegate(IntPtr position, IntPtr radius, IntPtr something1, IntPtr something2, IntPtr nativeMethodInfo);

    //    private static PlayerIEnumerableSetupDelegate playerIEnumerableSetupDelegate;
    //    public static void Patch()
    //    {
    //        unsafe
    //        {
    //            var setupMethod = typeof(PlayerManager).GetMethods()
    //                .Where(method => method.Name.StartsWith("Method_Public_Static_IEnumerable_1_Player_Vector3_Single_Nullable_1_Int32_Func_2_Player_Boolean_"))
    //                .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();

    //            var originalMethod = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(setupMethod).GetValue(null);

    //            MelonUtils.NativeHookAttach((IntPtr)(&originalMethod), typeof(PlayerIEnumerableSetup).GetMethod(nameof(Prefix), 
    //                BindingFlags.Static | BindingFlags.Public)!.MethodHandle.GetFunctionPointer());

    //            playerIEnumerableSetupDelegate = Marshal.GetDelegateForFunctionPointer<PlayerIEnumerableSetupDelegate>(originalMethod);
    //        }
    //    }

    //    public static IEnumerable<Player> Prefix(IntPtr position, IntPtr radius, IntPtr something1, IntPtr something2, IntPtr nativeMethodInfo)
    //    {
    //        try
    //        {
    //            // While I can't loop over this IEnum, I guess I'm just gonna make my own list
    //            // IEnumerable<Player> OriginalIEnumerable = playerIEnumerableSetupDelegate(position, radius, something1, something2, nativeMethodInfo);
    //            return (IEnumerable<Player>)(from p in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.ToArray()
    //                                             where p != null && p.field_Private_APIUser_0.id != Player.prop_Player_0.field_Private_APIUser_0.id
    //                                             && Vector3.Distance(p.transform.position, Main.PtrCurrentPos()) <= 1.75f
    //                                             select p);
    //        }
    //        catch (Exception e)
    //        {
    //            MelonLogger.Msg(ConsoleColor.Yellow, "Something went wrong in PlayerIEnumerableSetup Patch, please tell Elaina:");
    //            MelonLogger.Error($"{e}");
    //            return null;
    //        }
    //    }
    //}

    // The reason I'm using Native Patching is the following:
    // // The one on the bottom works, but is less reliable (not very future-proof against versions, this could be dangerous according to gompo). The one above it, just simply breaks Unhollower *for some reason* ¯\_(ツ)_/¯

    //[HarmonyPatch(typeof(PlayerManager), "Method_Public_Static_IEnumerable_1_Player_Vector3_Single_Nullable_1_Int32_Func_2_Player_Boolean_0")]
    //class Patch0
    //{
    //    private static List<Player> Players() => PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;

    //    static bool Prefix(ref IEnumerable<Player> __result)
    //    {
    //        __result = (IEnumerable<Player>)(from p in Players().ToArray() // Invalid cast. I STILL HAVE TO FIGURE THAT OUT SOME DAY!!
    //                                         where p != null && p.field_Private_APIUser_0.id != Player.prop_Player_0.field_Private_APIUser_0.id
    //                                         && Vector3.Distance(p.transform.position, Main.PtrCurrentPos()) <= 1.75f
    //                                         select p);
    //        return false;
    //    }
    //}

    //[HarmonyPatch(typeof(PortalInternal), "Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_Vector3_Vector3_Boolean_0")]
    //class Patch1
    //{
    //    private static List<Player> Players() => PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;

    //    static void Prefix(out int __state)
    //    {
    //        __state = Players().IndexOf(Player.prop_Player_0);
    //        Players().RemoveAt(__state);
    //    }

    //    static void Postfix(int __state) => Players().Insert(__state, Player.prop_Player_0);
    //}
}