using Harmony;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
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
            PlayerIEnumerableSetup.Patch();
        }

        public static void CloseMenu(bool __0, bool __1) => GetCloseMenuDelegate(__0, __1);
        private delegate void CloseMenuDelegate(bool __0, bool __1);
        private static CloseMenuDelegate closeMenuDelegate;
        private static CloseMenuDelegate GetCloseMenuDelegate
        {
            get
            {
                if (closeMenuDelegate != null) return closeMenuDelegate;
                MethodInfo closeMenuMethod = typeof(VRCUiManager).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Void_Boolean_Boolean_"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();
                closeMenuDelegate = (CloseMenuDelegate)Delegate.CreateDelegate(typeof(CloseMenuDelegate), VRCUiManager.prop_VRCUiManager_0, closeMenuMethod);
                return closeMenuDelegate;
            }
        }

        public static bool CreatePortal(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 foward, bool someBool) =>
            GetCreatePortalDelegate(apiWorld, apiWorldInstance, pos, foward, someBool);
        private delegate bool CreatePortalDelegate(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 foward, bool someBool);
        private static CreatePortalDelegate createPortalDelegate;
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

        public static void PopupV2(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null) => 
            GetPopupV2Delegate(title, innertxt, buttontxt, buttonOk, action);
        private delegate void PopupV2Delegate(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<VRCUiPopup> action = null);
        private static PopupV2Delegate popupV2Delegate;
        private static PopupV2Delegate GetPopupV2Delegate
        {
            get
            {
                if (popupV2Delegate != null) return popupV2Delegate;
                MethodInfo PopupV2Method = typeof(VRCUiPopupManager).GetMethods()
                    .First(methodBase => methodBase.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") &&
                    !methodBase.Name.Contains("PDM") &&
                    Utilities.ContainsStr(methodBase, "UserInterface/MenuContent/Popups/StandardPopupV2") &&
                    Utilities.WasUsedBy(methodBase, "OpenSaveSearchPopup"));
                popupV2Delegate = (PopupV2Delegate)Delegate.CreateDelegate(typeof(PopupV2Delegate), VRCUiPopupManager.prop_VRCUiPopupManager_0, PopupV2Method);
                return popupV2Delegate;
            }
        }

        public static bool OnPortalCreated(ApiWorld __0, ApiWorldInstance __1, Vector3 __2, Vector3 __3, bool __4, MethodInfo __originalMethod)
        {
            if (Main.IsModOn.Value && !Main.PtrIsOn())
            {
                CurrentInfo = new PortalInfo(__0, __1, __2, __3, __4);
                if (!Main.IsOnlyOnError.Value)
                {
                    PopupV2("Portal Placement", "Manual placement activated.\nPress ok to place portal.", "Ok", new Action(delegate { Main.EnablePointer(); }));
                    return false;
                }
            }
            return true;
        }

        public static bool ShowAlert(ref string __0, ref string __1)
        {
            var PortalButton = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo/WorldButtons/PortalButton").GetComponent<Button>();
            if (Main.IsModOn.Value && __0.Contains("Cannot Create Portal") && PortalButton.interactable)
            {
                PopupV2("Failed to create portal", "Error: " + __1 + "\nPress continue to try again.", "Continue", new Action(delegate { Main.EnablePointer(); }));
                return false;
            }
            return true;
        }
    }

    // Ty for the help here Knah! <3
    internal static class PlayerIEnumerableSetup
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr PlayerIEnumerableSetupDelegate(Vector3 position, float radius, IntPtr something1, IntPtr something2, IntPtr nativeMethodInfo);
        private static PlayerIEnumerableSetupDelegate playerIEnumerableSetupDelegate;
        public static void Patch()
        {
            unsafe
            {
                var setupMethod = typeof(PlayerManager).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Static_IEnumerable_1_Player_Vector3_Single_Nullable_1_Int32_Func_2_Player_Boolean_"))
                    .OrderBy(method => UnhollowerSupport.GetIl2CppMethodCallerCount(method)).Last();

                var originalMethod = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(setupMethod).GetValue(null);

                MelonUtils.NativeHookAttach((IntPtr)(&originalMethod), typeof(PlayerIEnumerableSetup).GetMethod(nameof(IEnumerableSetup),
                    BindingFlags.Static | BindingFlags.Public)!.MethodHandle.GetFunctionPointer());

                playerIEnumerableSetupDelegate = Marshal.GetDelegateForFunctionPointer<PlayerIEnumerableSetupDelegate>(originalMethod);
            }
        }

        public static bool IsUp;
        public static IntPtr IEnumerableSetup(Vector3 position, float radius, IntPtr something1, IntPtr something2, IntPtr nativeMethodInfo)
        {
            if (IsUp)
            { 
                try
                {
                    var myFunc = DelegateSupport.ConvertDelegate<Il2CppSystem.Func<Player, bool>>
                        (new Func<Player, bool>((player) => player.field_Private_APIUser_0.id == Player.prop_Player_0.field_Private_APIUser_0.id));
                    return playerIEnumerableSetupDelegate(position, radius, something1, myFunc.Pointer, nativeMethodInfo);
                }
                catch (Exception e)
                {
                    MelonLogger.Msg(ConsoleColor.Yellow, "Something went wrong in PlayerIEnumerableSetup Patch, please tell Davi:");
                    MelonLogger.Error($"{e}");
                }
            }
            return playerIEnumerableSetupDelegate(position, radius, something1, something2, nativeMethodInfo);
        }
    }
}