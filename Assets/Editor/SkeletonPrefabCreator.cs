using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// 将场景中的 Skeleton 一键转换为 Prefab + EnemySpawner。
/// 使用方法：Tools → Create Enemy Prefabs
/// </summary>
public static class SkeletonPrefabCreator
{
    private const string PREFAB_PATH = "Assets/Prefabs/Enemies";

    [MenuItem("Tools/Create Enemy Prefabs")]
    static void CreateAllEnemyPrefabs()
    {
        // 在场景中找到所有 SkeletonEnemy 实例
        var skeletons = Object.FindObjectsByType<SkeletonEnemy>(FindObjectsSortMode.None);
        if (skeletons.Length == 0)
        {
            EditorUtility.DisplayDialog("No Skeletons Found",
                "No SkeletonEnemy found in the current scene.", "OK");
            return;
        }

        // 确保 Prefabs 目录存在
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "../", PREFAB_PATH));

        int count = 0;
        foreach (var skeleton in skeletons)
        {
            CreatePrefabAndSpawner(skeleton);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ Done! {count} skeleton(s) converted to prefabs + spawners.");
        EditorUtility.DisplayDialog("Success",
            $"Converted {count} skeleton(s) to prefabs + spawners.\n\n" +
            "Each Skeleton was replaced by an EnemySpawner at the same position.\n" +
            $"Prefab saved to: {PREFAB_PATH}/Skeleton.prefab",
            "OK");
    }

    private static void CreatePrefabAndSpawner(SkeletonEnemy skeleton)
    {
        GameObject sourceGO = skeleton.gameObject;
        string prefabPath = $"{PREFAB_PATH}/Skeleton.prefab";

        // ── 1. 创建 Prefab ──
        GameObject prefab;
        if (File.Exists(Path.Combine(Application.dataPath, "../", prefabPath)))
        {
            // Prefab 已存在 → 用场景中的实例替换它
            prefab = PrefabUtility.SaveAsPrefabAsset(sourceGO, prefabPath);
        }
        else
        {
            prefab = PrefabUtility.SaveAsPrefabAsset(sourceGO, prefabPath);
        }

        if (prefab == null)
        {
            Debug.LogError($"Failed to create prefab at {prefabPath}");
            return;
        }

        // ── 2. 记录原始位置和父节点 ──
        Vector3 position = sourceGO.transform.position;
        Quaternion rotation = sourceGO.transform.rotation;
        Transform parent = sourceGO.transform.parent;

        // ── 3. 销毁场景中的原始 Skeleton ──
        Object.DestroyImmediate(sourceGO);

        // ── 4. 在原地创建 EnemySpawner ──
        GameObject spawnerGO = new GameObject($"Spawner_{prefab.name}");
        spawnerGO.transform.SetParent(parent);
        spawnerGO.transform.SetPositionAndRotation(position, rotation);

        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();

        // ── 5. 设置 SerializedField 引用 ──
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty prefabProp = so.FindProperty("enemyPrefab");
        if (prefabProp != null)
            prefabProp.objectReferenceValue = prefab.GetComponent<Enemy>();
        so.ApplyModifiedProperties();

        // ── 6. 选中新创建的 Spawner ──
        Selection.activeGameObject = spawnerGO;
        SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log($"Created Spawner at {position} → {prefabPath}");
    }
}
