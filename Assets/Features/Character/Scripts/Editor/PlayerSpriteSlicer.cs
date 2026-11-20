using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.Features.Character.EditorTools
{
    /// <summary>Slices every player strip (8 frames of 48x64) and sets the import settings.</summary>
    internal static class PlayerSpriteSlicer
    {
        public static bool SliceAll()
        {
            bool allOk = true;

            foreach (string kind in PlayerAnimationAssets.Kinds)
            {
                foreach (PlayerDirection direction in PlayerAnimationAssets.Directions)
                    allOk &= SliceTexture(PlayerAnimationAssets.TexturePath(kind, direction));
            }

            return allOk;
        }

        private static bool SliceTexture(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                Debug.LogError($"Texture not found: {path}");
                return false;
            }

            // Reimport first so the sprite data provider sees Multiple mode.
            if (ApplyImportSettings(importer))
            {
                importer.SaveAndReimport();
                importer = (TextureImporter)AssetImporter.GetAtPath(path);
            }

            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            provider.GetDataProvider<ITextureDataProvider>().GetTextureActualWidthAndHeight(out int width, out int height);
            if (width != PlayerAnimationAssets.FrameCount * PlayerAnimationAssets.FrameWidth
                || height != PlayerAnimationAssets.FrameHeight)
            {
                Debug.LogError($"Unexpected texture size {width}x{height} (expected " +
                               $"{PlayerAnimationAssets.FrameCount * PlayerAnimationAssets.FrameWidth}x{PlayerAnimationAssets.FrameHeight}): {path}");
                return false;
            }

            string baseName = Path.GetFileNameWithoutExtension(path);

            // Re-slicing would change the sprite ids and break clips that already reference them.
            if (IsAlreadySliced(provider.GetSpriteRects(), baseName))
                return true;

            SpriteRect[] rects = CreateRects(baseName);
            provider.SetSpriteRects(rects);
            UpdateNameFileIdPairs(provider, rects);
            provider.Apply();
            importer.SaveAndReimport();
            return true;
        }

        /// <summary>Returns true when something changed and the importer must be reimported.</summary>
        private static bool ApplyImportSettings(TextureImporter importer)
        {
            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, PlayerAnimationAssets.PixelsPerUnit))
            {
                importer.spritePixelsPerUnit = PlayerAnimationAssets.PixelsPerUnit;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            return changed;
        }

        private static bool IsAlreadySliced(SpriteRect[] rects, string baseName)
        {
            if (rects.Length != PlayerAnimationAssets.FrameCount)
                return false;

            for (int i = 0; i < PlayerAnimationAssets.FrameCount; i++)
            {
                string expectedName = $"{baseName}_{i}";
                SpriteRect match = rects.FirstOrDefault(r => r.name == expectedName);

                bool sizeMatches = match != null
                    && Mathf.Approximately(match.rect.width, PlayerAnimationAssets.FrameWidth)
                    && Mathf.Approximately(match.rect.height, PlayerAnimationAssets.FrameHeight);

                if (!sizeMatches)
                    return false;
            }

            return true;
        }

        private static SpriteRect[] CreateRects(string baseName)
        {
            var rects = new SpriteRect[PlayerAnimationAssets.FrameCount];

            for (int i = 0; i < rects.Length; i++)
            {
                var spriteRect = new SpriteRect
                {
                    name = $"{baseName}_{i}",
                    rect = new Rect(
                        i * PlayerAnimationAssets.FrameWidth,
                        0f,
                        PlayerAnimationAssets.FrameWidth,
                        PlayerAnimationAssets.FrameHeight),
                    alignment = SpriteAlignment.Center,   // keep Center: the foot collider and YSort expect it
                    pivot = new Vector2(0.5f, 0.5f),
                };

                AssignNewSpriteId(spriteRect);
                rects[i] = spriteRect;
            }

            return rects;
        }

        private static void AssignNewSpriteId(SpriteRect spriteRect)
        {
            // The GUID type behind SpriteRect.spriteID differs between Unity versions, so it is created via reflection.
            PropertyInfo idProperty = typeof(SpriteRect).GetProperty(nameof(SpriteRect.spriteID));
            MethodInfo generate = idProperty.PropertyType.GetMethod("Generate", BindingFlags.Public | BindingFlags.Static);
            idProperty.SetValue(spriteRect, generate.Invoke(null, null));
        }

        private static void UpdateNameFileIdPairs(ISpriteEditorDataProvider provider, SpriteRect[] rects)
        {
            var nameFileIdProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameFileIdProvider == null)
                return;

            nameFileIdProvider.SetNameFileIdPairs(
                rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        }
    }
}
