using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Features.Character.EditorTools
{
    /// <summary>Creates the 12 sprite-swap clips (Idle/Walk x 6 directions). Existing clips are kept.</summary>
    internal static class PlayerClipBuilder
    {
        public static bool BuildAll()
        {
            bool allOk = true;

            foreach (string kind in PlayerAnimationAssets.Kinds)
            {
                foreach (PlayerDirection direction in PlayerAnimationAssets.Directions)
                    allOk &= BuildClip(kind, direction);
            }

            return allOk;
        }

        private static bool BuildClip(string kind, PlayerDirection direction)
        {
            string clipPath = PlayerAnimationAssets.ClipPath(kind, direction.ClipSuffix);

            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            {
                Debug.Log($"Clip already exists, kept: {clipPath}");
                return true;
            }

            string texturePath = PlayerAnimationAssets.TexturePath(kind, direction);
            Sprite[] sprites = LoadSprites(texturePath);

            if (sprites.Length != PlayerAnimationAssets.FrameCount)
            {
                Debug.LogError($"Expected {PlayerAnimationAssets.FrameCount} sprites but found {sprites.Length}: {texturePath}");
                return false;
            }

            AssetDatabase.CreateAsset(CreateClip(sprites), clipPath);
            return true;
        }

        private static AnimationClip CreateClip(Sprite[] sprites)
        {
            var clip = new AnimationClip { frameRate = PlayerAnimationAssets.FramesPerSecond };

            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / PlayerAnimationAssets.FramesPerSecond,
                    value = sprites[i],
                };
            }

            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Stop time after the last frame so every frame lasts the same time when looping.
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            settings.startTime = 0f;
            settings.stopTime = sprites.Length / PlayerAnimationAssets.FramesPerSecond;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            return clip;
        }

        private static Sprite[] LoadSprites(string texturePath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .OrderBy(sprite => FrameIndex(sprite.name))
                .ToArray();
        }

        // Sprite names are "<file>_<index>"; sort by the number, not alphabetically.
        private static int FrameIndex(string spriteName)
        {
            string suffix = spriteName.Substring(spriteName.LastIndexOf('_') + 1);
            return int.TryParse(suffix, out int index) ? index : int.MaxValue;
        }
    }
}
