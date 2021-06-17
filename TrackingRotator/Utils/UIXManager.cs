using UIExpansionKit.API;
using static TrackingRotator.Main;

namespace TrackingRotator.Utils
{
    internal static class UIXManager
    {
        public static void OnApplicationStart() => ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Tracking rotation", ShowRotationMenu);

        // Based on knah's ViewPointTweaker mod, https://github.com/knah/VRCMods/blob/master/ViewPointTweaker
        private static ICustomShowableLayoutedMenu rotationMenu = null;
        private static void ShowRotationMenu()
        {
            if (rotationMenu == null)
            {
                rotationMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

                rotationMenu.AddSpacer();
                rotationMenu.AddSimpleButton("Forward", () => Move(transform.right));
                rotationMenu.AddSpacer();
                rotationMenu.AddSpacer();

                rotationMenu.AddSimpleButton("Tilt Left", () => Move(transform.forward));
                rotationMenu.AddSimpleButton("Reset", () => cameraTransform.localRotation = originalRotation);
                rotationMenu.AddSimpleButton("Tilt Right", () => Move(-transform.forward));
                rotationMenu.AddSpacer();

                rotationMenu.AddSpacer();
                rotationMenu.AddSimpleButton("Backward", () => Move(-transform.right));
                rotationMenu.AddSimpleButton("Left", () => Move(-transform.up));
                rotationMenu.AddSimpleButton("Right", () => Move(transform.up));

                rotationMenu.AddToggleButton("High precision", b => highPrecision = b, () => highPrecision);
                rotationMenu.AddSpacer();
                rotationMenu.AddSpacer();
                rotationMenu.AddSimpleButton("Back", rotationMenu.Hide);
            }

            rotationMenu.Show();
        }
    }
}