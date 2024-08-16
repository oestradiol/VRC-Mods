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
                CustomSubMenu.AddButton("Forward", () => Move(transform.right), Assets.Forward); //X+
                CustomSubMenu.AddButton("Backward", () => Move(-transform.right), Assets.Backward); //X-
                CustomSubMenu.AddButton("Tilt Left", () => Move(transform.forward), Assets.TLeft); //Z+
                CustomSubMenu.AddButton("Tilt Right", () => Move(-transform.forward), Assets.TRight); //Z-
                CustomSubMenu.AddButton("Left", () => Move(-transform.up), Assets.Left); //Y-
                CustomSubMenu.AddButton("Right", () => Move(transform.up), Assets.Right); //Y+

                CustomSubMenu.AddSubMenu("Other", () =>
                {
                    CustomSubMenu.AddButton("Reset", () => cameraTransform.localRotation = originalRotation, Assets.Reset);
                    CustomSubMenu.AddToggle("High precision", highPrecision, b => highPrecision = b, Assets.HP);
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
        private static AssetBundle Bundle;
        public static Texture2D Main, Forward, Backward, TLeft, TRight, Left, Right, Other, Reset, HP;

        public static void OnApplicationStart() { MelonCoroutines.Start(LoadAssets()); }

        private static IEnumerator LoadAssets()
        {
            try
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrackingRotator.trackingrotator");
                using var memoryStream = new MemoryStream((int)stream.Length);
                stream.CopyTo(memoryStream);
                Bundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
                Bundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                try { Main = LoadTexture("Main.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: Main.png"); }
                try { Forward = LoadTexture("Forward.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: Forward.png"); }
                try { Backward = LoadTexture("Backward.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: Backward.png"); }
                try { TLeft = LoadTexture("TLeft.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: TLeft.png"); }
                try { TRight = LoadTexture("TRight.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: TRight.png"); }
                try { Left = LoadTexture("Left.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: Left.png"); }
                try { Right = LoadTexture("Right.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: Right.png"); }
                try { Other = LoadTexture("Other.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: Other.png"); }
                try { Reset = LoadTexture("Reset.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: Reset.png"); }
                try { HP = LoadTexture("HP.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle: HP.png"); }
            } catch { MelonLogger.Warning("Failed to load AssetBundle! ActionMenuApi will have its icons completely broken."); }
            yield break;
        }

        private static Texture2D LoadTexture(string Texture)
        {
            Texture2D Texture2 = Bundle.LoadAsset_Internal(Texture, Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            Texture2.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            Texture2.hideFlags = HideFlags.HideAndDontSave;
            return Texture2;
        }
    }
}