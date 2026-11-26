using System;
using UnityEditor;
using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.Combat.Data;
using Game.Features.Combat.Systems;

namespace Game.Features.Combat.Editor
{
    /// <summary>
    /// Tools > Combat: makes placeholder enemies (coloured shapes, no art needed) and places them in a scene.
    /// Replace the sprite on the prefabs whenever real art exists.
    /// </summary>
    public static class CombatPlaceholderMenu
    {
        private const string Root = "Assets/Features/Combat";
        private const string DataFolder = Root + "/Data";
        private const string PrefabFolder = Root + "/Prefabs";
        private const string SlimeDataPath = DataFolder + "/Enemy_Slime.asset";
        private const string BossDataPath = DataFolder + "/Enemy_Boss.asset";
        private const string SlimePrefabPath = PrefabFolder + "/Slime.prefab";
        private const string BossPrefabPath = PrefabFolder + "/Boss.prefab";

        [MenuItem("Tools/Combat/1. Create Placeholder Enemies")]
        public static void CreatePlaceholders()
        {
            EnsureFolder(Root, "Data");
            EnsureFolder(Root, "Prefabs");

            EnemyData slimeData = CreateData(SlimeDataPath, so =>
            {
                Set(so, "_enemyId", "slime");
                Set(so, "_displayName", "Slime");
                Set(so, "_maxHealth", 30f);
                Set(so, "_moveSpeed", 2f);
                Set(so, "_attackDamage", 8f);
            });

            EnemyData bossData = CreateData(BossDataPath, so =>
            {
                Set(so, "_enemyId", "boss");
                Set(so, "_displayName", "Boss");
                Set(so, "_maxHealth", 300f);
                Set(so, "_moveSpeed", 2.2f);
                Set(so, "_detectRange", 8f);
                Set(so, "_loseRange", 14f);
                Set(so, "_attackDamage", 15f);
                Set(so, "_attackRange", 2.2f);
                Set(so, "_attackRadius", 1.4f);
                Set(so, "_windupSeconds", 0.8f);
                Set(so, "_recoverSeconds", 0.9f);
                Set(so, "_attackKnockback", 9f);
                Set(so, "_goldMin", 80);
                Set(so, "_goldMax", 120);
                Set(so, "_exp", 100);
            });

            CreatePrefab(SlimePrefabPath, "Slime", slimeData, new Color(0.35f, 0.85f, 0.4f), 1f, false);
            CreatePrefab(BossPrefabPath, "Boss", bossData, new Color(0.7f, 0.25f, 0.85f), 3f, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Combat placeholders ready in " + Root);
        }

        [MenuItem("Tools/Combat/2. Place Slime Spawner In Scene")]
        public static void PlaceSpawner()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<EnemyController>(SlimePrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Combat", "Run '1. Create Placeholder Enemies' first.", "OK");
                return;
            }

            Vector3 center = SceneCenter();
            var spawner = new GameObject("EnemySpawner");
            Undo.RegisterCreatedObjectUndo(spawner, "Place spawner");
            spawner.transform.position = center;

            for (int i = 0; i < 3; i++)
            {
                var point = new GameObject("Point" + (i + 1));
                point.transform.SetParent(spawner.transform, false);
                point.transform.position = center + new Vector3(i * 4f, i % 2 == 0 ? 0f : 2f, 0f);
            }

            var component = spawner.AddComponent<EnemySpawner>();
            var so = new SerializedObject(component);
            so.FindProperty("_enemyPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = spawner;
        }

        [MenuItem("Tools/Combat/3. Place Boss In Scene")]
        public static void PlaceBoss()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Combat", "Run '1. Create Placeholder Enemies' first.", "OK");
                return;
            }

            var boss = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(boss, "Place boss");
            boss.transform.position = SceneCenter();

            // Every boss in a scene needs its own save id.
            var persistence = boss.GetComponent<BossPersistence>();
            var so = new SerializedObject(persistence);
            so.FindProperty("_id").stringValue = Guid.NewGuid().ToString();
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = boss;
        }

        private static Vector3 SceneCenter()
        {
            SceneView view = SceneView.lastActiveSceneView;
            Vector3 position = view != null ? view.pivot : Vector3.zero;
            position.z = 0f;
            return position;
        }

        private static void CreatePrefab(string path, string name, EnemyData data, Color color, float size, bool isBoss)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            var instance = new GameObject(name);

            var sprite = instance.AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sprite.color = color;
            sprite.sortingLayerName = "Characters";
            instance.transform.localScale = Vector3.one * size;

            var body = instance.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;

            var collider = instance.AddComponent<CircleCollider2D>();
            collider.radius = 0.35f;

            instance.AddComponent<YSort>();
            instance.AddComponent<Health>();
            instance.AddComponent<SpriteFlasher>();

            EnemyController controller = isBoss
                ? instance.AddComponent<BossController>()
                : instance.AddComponent<EnemyController>();

            var so = new SerializedObject(controller);
            so.FindProperty("_data").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (isBoss)
                instance.AddComponent<BossPersistence>();

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
        }

        private static EnemyData CreateData(string path, Action<SerializedObject> fill)
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (existing != null)
                return existing;

            var data = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(data, path);

            var so = new SerializedObject(data);
            fill(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);

            return data;
        }

        private static void Set(SerializedObject so, string field, float value)
        {
            so.FindProperty(field).floatValue = value;
        }

        private static void Set(SerializedObject so, string field, int value)
        {
            so.FindProperty(field).intValue = value;
        }

        private static void Set(SerializedObject so, string field, string value)
        {
            so.FindProperty(field).stringValue = value;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
