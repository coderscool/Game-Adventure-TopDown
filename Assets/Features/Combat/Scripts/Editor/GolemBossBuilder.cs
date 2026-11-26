using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using Game.Features.Combat.Systems;

namespace Game.Features.Combat.Editor
{
    /// <summary>
    /// Tools > Combat > 4. Build Golem Boss: slices the golem sheet, makes one clip per animation,
    /// builds the Animator and puts the golem on the Boss prefab. Safe to run again.
    /// </summary>
    public static class GolemBossBuilder
    {
        private const string Folder = "Assets/Features/Combat";
        private const string SheetPath = Folder + "/Art/Golem/Golem_Sheet.png";
        private const string ClipFolder = Folder + "/Animations";
        private const string ControllerPath = ClipFolder + "/Golem.controller";
        private const string LaserPath = Folder + "/Art/Golem/Golem_Laser.png";
        private const string BossPrefabPath = Folder + "/Prefabs/Boss.prefab";

        private const int Columns = 10;
        private const int FrameSize = 100;
        private const int SheetSize = 1000;
        private const int FrameCount = 67;
        private const float FramesPerSecond = 10f;
        private const float BossScale = 3f;

        // Frame ranges (inclusive) in the sheet, taken from the tags in the Aseprite file.
        private static readonly (string name, int first, int last, bool loop, string trigger)[] Clips =
        {
            ("Idle", 0, 3, true, null),
            ("Glow", 4, 11, false, "Glow"),
            ("Shoot", 12, 20, false, "Shoot"),
            ("Immune", 21, 28, false, "Immune"),
            ("Melee", 29, 35, false, "Melee"),
            ("LaserCast", 36, 42, false, "Laser"),
            ("ShieldCast", 43, 52, false, "Shield"),
            ("Death", 53, 66, false, "Death"),
        };

        [MenuItem("Tools/Combat/4. Build Golem Boss")]
        public static void Build()
        {
            if (!SliceSheet())
                return;

            Sprite[] sprites = LoadSprites();
            if (sprites.Length != FrameCount)
            {
                Debug.LogError($"Expected {FrameCount} sprites but found {sprites.Length}: {SheetPath}");
                return;
            }

            Sprite[] laser = SliceLaser() ? LoadSprites(LaserPath) : new Sprite[0];

            AnimationClip[] clips = BuildClips(sprites);
            AnimatorController controller = BuildController(clips);
            ApplyToBossPrefab(sprites[0], controller, laser);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Golem boss ready. Boss prefab now uses the golem sprites.");
        }

        // ---------- Slicing ----------

        private static bool SliceSheet()
        {
            return SliceGrid(SheetPath, "Golem_", SheetSize, SheetSize, FrameSize, FrameSize, FrameCount, Columns, new Vector2(0.5f, 0.5f));
        }

        // The laser sheet is one column of 300x100 cells (15 rows); the beam starts about 52 px from the left edge.
        private static bool SliceLaser()
        {
            return SliceGrid(LaserPath, "GolemLaser_", 300, 1500, 300, 100, 15, 1, new Vector2(52f / 300f, 0.5f));
        }

        private static bool SliceGrid(string path, string namePrefix, int sheetWidth, int sheetHeight,
            int cellWidth, int cellHeight, int count, int columns, Vector2 pivot)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                Debug.LogError("Texture not found: " + path);
                return false;
            }

            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteImportMode != SpriteImportMode.Multiple) { importer.spriteImportMode = SpriteImportMode.Multiple; changed = true; }
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 100f)) { importer.spritePixelsPerUnit = 100f; changed = true; }
            if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }

            if (changed)
            {
                importer.SaveAndReimport();
                importer = (TextureImporter)AssetImporter.GetAtPath(path);
            }

            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            provider.GetDataProvider<ITextureDataProvider>().GetTextureActualWidthAndHeight(out int width, out int height);
            if (width != sheetWidth || height != sheetHeight)
            {
                Debug.LogError($"Unexpected size {width}x{height} (expected {sheetWidth}x{sheetHeight}): {path}");
                return false;
            }

            // Re-slicing would change sprite ids and break clips that already use them.
            if (provider.GetSpriteRects().Length == count)
                return true;

            var rects = new SpriteRect[count];
            for (int i = 0; i < count; i++)
            {
                int column = i % columns;
                int row = i / columns;

                var rect = new SpriteRect
                {
                    name = namePrefix + i,
                    rect = new Rect(column * cellWidth, sheetHeight - (row + 1) * cellHeight, cellWidth, cellHeight),
                    alignment = SpriteAlignment.Custom,
                    pivot = pivot,
                };

                PropertyInfo idProperty = typeof(SpriteRect).GetProperty(nameof(SpriteRect.spriteID));
                MethodInfo generate = idProperty.PropertyType.GetMethod("Generate", BindingFlags.Public | BindingFlags.Static);
                idProperty.SetValue(rect, generate.Invoke(null, null));

                rects[i] = rect;
            }

            provider.SetSpriteRects(rects);

            var nameFileIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameFileIds != null)
                nameFileIds.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());

            provider.Apply();
            importer.SaveAndReimport();
            return true;
        }

        private static Sprite[] LoadSprites()
        {
            return LoadSprites(SheetPath);
        }

        private static Sprite[] LoadSprites(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1)))
                .ToArray();
        }

        // ---------- Clips and animator ----------

        private static AnimationClip[] BuildClips(Sprite[] sprites)
        {
            var result = new AnimationClip[Clips.Length];

            for (int c = 0; c < Clips.Length; c++)
            {
                var info = Clips[c];
                string path = $"{ClipFolder}/Golem_{info.name}.anim";

                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null)
                {
                    clip = new AnimationClip { frameRate = FramesPerSecond };
                    AssetDatabase.CreateAsset(clip, path);
                }

                int count = info.last - info.first + 1;
                var keys = new ObjectReferenceKeyframe[count];
                for (int i = 0; i < count; i++)
                {
                    keys[i] = new ObjectReferenceKeyframe
                    {
                        time = i / FramesPerSecond,
                        value = sprites[info.first + i],
                    };
                }

                EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = info.loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                EditorUtility.SetDirty(clip);
                result[c] = clip;
            }

            return result;
        }

        private static AnimatorController BuildController(AnimationClip[] clips)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;

            AnimatorState idle = null;

            for (int c = 0; c < Clips.Length; c++)
            {
                var info = Clips[c];
                AnimatorState state = machine.AddState(info.name);
                state.motion = clips[c];

                if (info.trigger == null)
                {
                    idle = state;
                    machine.defaultState = state;
                    continue;
                }

                controller.AddParameter(info.trigger, AnimatorControllerParameterType.Trigger);

                AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
                enter.AddCondition(AnimatorConditionMode.If, 0f, info.trigger);
                enter.hasExitTime = false;
                enter.duration = 0f;
                enter.canTransitionToSelf = false;

                // Everything except death returns to idle when it finishes.
                if (info.name != "Death")
                {
                    AnimatorStateTransition back = state.AddTransition(idle);
                    back.hasExitTime = true;
                    back.exitTime = 1f;
                    back.duration = 0f;
                }
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void SetSprites(SerializedProperty list, Sprite[] sprites, int first, int last)
        {
            list.arraySize = last - first + 1;

            for (int i = 0; i < list.arraySize; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[first + i];
        }

        // ---------- Prefab ----------

        private static void ApplyToBossPrefab(Sprite firstSprite, AnimatorController controller, Sprite[] laser)
        {
            if (!File.Exists(BossPrefabPath))
            {
                Debug.LogError("Boss prefab not found. Run 'Tools > Combat > 1. Create Placeholder Enemies' first.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(BossPrefabPath);

            try
            {
                var renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = firstSprite;
                renderer.color = Color.white;

                var animator = root.GetComponent<Animator>();
                if (animator == null)
                    animator = root.AddComponent<Animator>();

                animator.runtimeAnimatorController = controller;
                root.transform.localScale = Vector3.one * BossScale;

                var body = root.GetComponent<CircleCollider2D>();
                if (body != null)
                {
                    body.radius = 0.2f;
                    body.offset = new Vector2(0f, -0.08f);
                }

                var boss = root.GetComponent<BossController>();
                if (boss != null)
                {
                    var serialized = new SerializedObject(boss);
                    serialized.FindProperty("_animator").objectReferenceValue = animator;

                    // Laser sheet rows: 0 is empty, 1-7 charge-up, 8-14 beam.
                    if (laser.Length == 15)
                    {
                        SetSprites(serialized.FindProperty("_laserChargeFrames"), laser, 1, 7);
                        SetSprites(serialized.FindProperty("_laserBeamFrames"), laser, 8, 14);
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, BossPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
