using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using MelonLoader;
using MonoMod.Utils;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace BetterPortalPlacement.Utils
{
    // Almost this entire class came from Gompog :) // I'm going to bonk you one day - Gompo // Please don't? Love you~
    internal static class Patches
    {
        public static PortalInfo CurrentInfo;

        public static void ApplyPatches()
        {
            Main.HInstance.Patch(CreatePortalMethod, new HarmonyMethod(typeof(Patches).GetMethod(nameof(OnPortalCreated))));
            Main.HInstance.Patch(typeof(VRCUiPopupManager).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Void_String_String_Single_"))
                    .OrderBy(UnhollowerSupport.GetIl2CppMethodCallerCount).Last(),
                new HarmonyMethod(typeof(Patches).GetMethod(nameof(ShowAlert))));
            PlayerIEnumerableSetup.Patch();
        }

        public static void CloseMenu(bool __0, bool __1) => GetCloseMenuDelegate(__0, __1);
        private delegate void CloseMenuDelegate(bool __0, bool __1);
        private static CloseMenuDelegate _closeMenuDelegate;
        private static CloseMenuDelegate GetCloseMenuDelegate => 
            _closeMenuDelegate ??= typeof(VRCUiManager).GetMethods()
                .Where(method => method.Name.StartsWith("Method_Public_Void_Boolean_Boolean_"))
                .OrderBy(UnhollowerSupport.GetIl2CppMethodCallerCount).Last()
                .CreateDelegate<CloseMenuDelegate>(VRCUiManager.prop_VRCUiManager_0);

        public static bool CreatePortal(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 forward, Il2CppSystem.Action<string> someStrAction = null) =>
            (_createPortalDelegate ??= CreatePortalMethod.CreateDelegate<CreatePortalDelegate>())(apiWorld, apiWorldInstance, pos, forward, someStrAction);
        private delegate bool CreatePortalDelegate(ApiWorld apiWorld, ApiWorldInstance apiWorldInstance, Vector3 pos, Vector3 forward, Il2CppSystem.Action<string> someStrAction = null);
        private static CreatePortalDelegate _createPortalDelegate;
        private static MethodInfo _createPortalMethod;
        private static MethodInfo CreatePortalMethod => _createPortalMethod ??= typeof(PortalInternal).GetMethods()
                .First(method => method.Name.StartsWith("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_Vector3_Vector3_Action_1_String_") && 
                                 Utilities.ContainsStr(method, "admin_dont_allow_portal"));

        private static void PopupV2(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<string> action = null) => 
            GetPopupV2Delegate(title, innertxt, buttontxt, buttonOk, action);
        private delegate void PopupV2Delegate(string title, string innertxt, string buttontxt, Il2CppSystem.Action buttonOk, Il2CppSystem.Action<string> action = null);
        private static PopupV2Delegate _popupV2Delegate;
        private static PopupV2Delegate GetPopupV2Delegate =>
            _popupV2Delegate ??= typeof(VRCUiPopupManager).GetMethods()
                .First(methodBase => methodBase.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") &&
                                     !methodBase.Name.Contains("PDM") &&
                                     Utilities.ContainsStr(methodBase, "UserInterface/MenuContent/Popups/StandardPopupV2") &&
                                     Utilities.WasUsedBy(methodBase, "OpenSaveSearchPopup"))
                .CreateDelegate<PopupV2Delegate>(VRCUiPopupManager.prop_VRCUiPopupManager_0);

        public static bool OnPortalCreated(ApiWorld __0, ApiWorldInstance __1, Vector3 __2, Vector3 __3, Il2CppSystem.Action<string> __4)
        {
            if (!Main.IsModOn.Value || Main.PtrIsOn()) return true;
            CurrentInfo = new PortalInfo(__0, __1, __4);
            if (Main.IsOnlyOnError.Value) return true;
            if (Main.UseConfirmationPopup.Value) PopupV2("Portal Placement", "Manual placement activated.\nPress ok to place portal.", "Ok", 
                new Action(Main.EnablePointer));
            else Main.EnablePointer();
            return false;
        }

        public static bool ShowAlert(ref string __0, ref string __1)
        {
            if (!Main.IsModOn.Value || !__0.Contains("Cannot Create Portal") || !GameObject
                    .Find("UserInterface/MenuContent/Screens/WorldInfo/WorldButtons/PortalButton")
                    .GetComponent<Button>().interactable) return true;
            PopupV2("Failed to create portal", "Error: " + __1 + "\nPress continue to try again.", "Continue", new Action(Main.EnablePointer));
            return false;
        }
    }

    // Ty for the help here Knah! <3
    internal static class PlayerIEnumerableSetup
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr PlayerIEnumerableSetupDelegate(Vector3 position, float radius, IntPtr something1, IntPtr something2, IntPtr nativeMethodInfo);
        private static PlayerIEnumerableSetupDelegate _playerIEnumerableSetupDelegate;
        public static void Patch()
        {
            unsafe
            {
                var setupMethod = typeof(PlayerManager).GetMethods()
                    .Where(method => method.Name.StartsWith("Method_Public_Static_IEnumerable_1_Player_Vector3_Single_Nullable_1_Int32_Func_2_Player_Boolean_"))
                    .OrderBy(UnhollowerSupport.GetIl2CppMethodCallerCount).Last();

                var originalMethod = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(setupMethod).GetValue(null);

                MelonUtils.NativeHookAttach((IntPtr)(&originalMethod), typeof(PlayerIEnumerableSetup).GetMethod(nameof(EnumerableSetup),
                    BindingFlags.Static | BindingFlags.Public)!.MethodHandle.GetFunctionPointer());

                _playerIEnumerableSetupDelegate = Marshal.GetDelegateForFunctionPointer<PlayerIEnumerableSetupDelegate>(originalMethod);
            }
        }

        public static bool IsUp;
        public static IntPtr EnumerableSetup(Vector3 position, float radius, IntPtr something1, IntPtr something2, IntPtr nativeMethodInfo)
        {
            if (!IsUp)
                return _playerIEnumerableSetupDelegate(position, radius, something1, something2, nativeMethodInfo);
            try
            {
                var myFunc = DelegateSupport.ConvertDelegate<Il2CppSystem.Func<Player, bool>>
                    (new Func<Player, bool>(player => player.prop_APIUser_0.id == Player.prop_Player_0.prop_APIUser_0.id));
                return _playerIEnumerableSetupDelegate(position, radius, something1, myFunc.Pointer, nativeMethodInfo);
            }
            catch (Exception e)
            {
                Main.Logger.Msg(ConsoleColor.Yellow, "Something went wrong in PlayerIEnumerableSetup Patch, please tell Davi:");
                Main.Logger.Error($"{e}");
            }
            return _playerIEnumerableSetupDelegate(position, radius, something1, something2, nativeMethodInfo);
        }
    }
}