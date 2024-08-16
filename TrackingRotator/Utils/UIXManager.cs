using UIExpansionKit.API;
using static TrackingRotator.Main;

namespace TrackingRotator.Utils
{
    internal static class UIXManager
    {
        public static void OnApplicationStart()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Tracking Rotator", ShowRotationMenu);
            ExpansionKitApi.OnUiManagerInit += VRChat_OnUiManagerInit;
        }

        // Based on knah's ViewPointTweaker mod, https://github.com/knah/VRCMods/blob/master/ViewPointTweaker
        private static ICustomShowableLayoutedMenu _rotationMenu;
        private static void ShowRotationMenu()
        {
            if (_rotationMenu == null)
            {
                _rotationMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

                _rotationMenu.AddSpacer();
                _rotationMenu.AddSimpleButton("Forward", () => Move(Transform.right));
                _rotationMenu.AddSpacer();
                _rotationMenu.AddSpacer();

                _rotationMenu.AddSimpleButton("Tilt Left", () => Move(Transform.forward));
                _rotationMenu.AddSimpleButton("Reset", () => CameraTransform.localRotation = OriginalRotation);
                _rotationMenu.AddSimpleButton("Tilt Right", () => Move(-Transform.forward));
                _rotationMenu.AddSpacer();

                _rotationMenu.AddSpacer();
                _rotationMenu.AddSimpleButton("Backward", () => Move(-Transform.right));
                _rotationMenu.AddSimpleButton("Left", () => Move(-Transform.up));
                _rotationMenu.AddSimpleButton("Right", () => Move(Transform.up));

                _rotationMenu.AddToggleButton("High precision", b => HighPrecision = b, () => HighPrecision);
                _rotationMenu.AddSpacer();
                _rotationMenu.AddSpacer();
                _rotationMenu.AddSimpleButton("Back", _rotationMenu.Hide);
            }
            _rotationMenu.Show();
        }
    }
}