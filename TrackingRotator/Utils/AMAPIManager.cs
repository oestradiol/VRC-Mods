using UnityEngine;
using ActionMenuApi.Api;
using UnhollowerRuntimeLib;
using System.Collections;
using MelonLoader;
using System.IO;
using System.Reflection;
using static TrackingRotator.Main;

namespace TrackingRotator.Utils
{
    internal static class AMAPIManager
    {
        public static void ActionMenuIntegration()
        {
            VRCActionMenuPage.AddSubMenu(ActionMenuPage.Main, "<color=#00a2ff>Tracking Rotator</color>", () =>
            {
                CustomSubMenu.AddButton("Forward", () => Move(Main.Transform.right), Assets.Forward); //X+
                CustomSubMenu.AddButton("Backward", () => Move(-Main.Transform.right), Assets.Backward); //X-
                CustomSubMenu.AddButton("Tilt Left", () => Move(Main.Transform.forward), Assets.TLeft); //Z+
                CustomSubMenu.AddButton("Tilt Right", () => Move(-Main.Transform.forward), Assets.TRight); //Z-
                CustomSubMenu.AddButton("Left", () => Move(-Main.Transform.up), Assets.Left); //Y-
                CustomSubMenu.AddButton("Right", () => Move(Main.Transform.up), Assets.Right); //Y+

                CustomSubMenu.AddSubMenu("Other", () =>
                {
                    CustomSubMenu.AddButton("Reset", () => CameraTransform.localRotation = OriginalRotation, Assets.Reset);
                    CustomSubMenu.AddToggle("High precision", HighPrecision, b => HighPrecision = b, Assets.Hp);
                }, Assets.Other);
            }, Assets.Main);
        }
    }

    // The code below was based on Lily's...
        // https://github.com/KortyBoi/VRChat-TeleporterVR/blob/main/Utils/ResourceManager.cs
    // And also knah's!
        // https://github.com/knah/VRCMods/blob/master/UIExpansionKit
    internal static class Assets
    {
        private static AssetBundle _bundle;
        public static Texture2D Main, Forward, Backward, TLeft, TRight, Left, Right, Other, Reset, Hp;

        public static void OnApplicationStart() { MelonCoroutines.Start(LoadAssets()); }

        private static IEnumerator LoadAssets()
        {
            try
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrackingRotator.trackingrotator");
                if (stream != null)
                {
                    var memoryStream = new MemoryStream((int)stream.Length);
                    stream.CopyTo(memoryStream);
                    _bundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
                    _bundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }
                try { Main = LoadTexture("Main.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: Main.png"); }
                try { Forward = LoadTexture("Forward.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: Forward.png"); }
                try { Backward = LoadTexture("Backward.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: Backward.png"); }
                try { TLeft = LoadTexture("TLeft.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: TLeft.png"); }
                try { TRight = LoadTexture("TRight.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: TRight.png"); }
                try { Left = LoadTexture("Left.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: Left.png"); }
                try { Right = LoadTexture("Right.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: Right.png"); }
                try { Other = LoadTexture("Other.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: Other.png"); }
                try { Reset = LoadTexture("Reset.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: Reset.png"); }
                try { Hp = LoadTexture("HP.png"); } catch { TrackingRotator.Main.Logger.Error("Failed to load image from asset bundle: HP.png"); }
            } catch { TrackingRotator.Main.Logger.Warning("Failed to load AssetBundle! ActionMenuApi will have its icons completely broken."); }
            yield break;
        }

        private static Texture2D LoadTexture(string texture)
        {
            var texture2 = _bundle.LoadAsset_Internal(texture, Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            texture2.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
            return texture2;
        }
    }
}