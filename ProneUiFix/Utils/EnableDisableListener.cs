using System;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace ProneUiFix.Utils
{
    // Ty Knah once again for your superior knowledge <3
    // https://github.com/knah/VRCMods/blob/326b5f6d3d1c4bc3474b3518938d49efb918c1d8/UIExpansionKit/Components/EnableDisableListener.cs
    public class EnableDisableListener : MonoBehaviour
    {
        public EnableDisableListener(IntPtr obj0) : base(obj0) { }
        [method: HideFromIl2Cpp] public event Action OnEnabled;
        private void OnEnable() => OnEnabled?.Invoke();
    }
}