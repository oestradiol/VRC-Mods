using UIExpansionKit.API;
using static TrackingRotator.Main;

namespace TrackingRotator.Utils
{
    internal static class UIXManager
    {
        public static void OnApplicationStart()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Tracking rotation", ShowRotationMenu);
            ExpansionKitApi.OnUiManagerInit += VRChat_OnUiManagerInit;
        }

        // Based on knah's ViewPointTweaker mod, https://github.com/knah/VRCMods/blob/master/ViewPointTweaker
        private static ICustomShowableLayoutedMenu RotationMenu = null;
        private static void ShowRotationMenu()
        {
            if (RotationMenu == null)
            {
                RotationMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

                RotationMenu.AddSpacer();
                RotationMenu.AddSimpleButton("Forward", () => Move(transform.right));
                RotationMenu.AddSpacer();
                RotationMenu.AddSpacer();

                RotationMenu.AddSimpleButton("Tilt Left", () => Move(transform.forward));
                RotationMenu.AddSimpleButton("Reset", () => cameraTransform.localRotation = originalRotation);
                RotationMenu.AddSimpleButton("Tilt Right", () => Move(-transform.forward));
                RotationMenu.AddSpacer();

                RotationMenu.AddSpacer();
                RotationMenu.AddSimpleButton("Backward", () => Move(-transform.right));
                RotationMenu.AddSimpleButton("Left", () => Move(-transform.up));
                RotationMenu.AddSimpleButton("Right", () => Move(transform.up));

                RotationMenu.AddToggleButton("High precision", b => highPrecision = b, () => highPrecision);
                RotationMenu.AddSpacer();
                RotationMenu.AddSpacer();
                RotationMenu.AddSimpleButton("Back", RotationMenu.Hide);
            }

            RotationMenu.Show();
        }
    }
}