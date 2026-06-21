using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class BreakableDoorTilemapSetup
{
    [MenuItem("Tools/Setup Breakable Door Tilemap")]
    static void SetupBreakableDoorTilemap()
    {
        // 1. Find Grid in the active scene
        Grid grid = Object.FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            EditorUtility.DisplayDialog("Grid Not Found",
                "No Grid found in the current scene. Please open the scene first.", "OK");
            return;
        }

        // 2. Check if BreakableDoor tilemap already exists
        string doorName = "BreakableDoor";
        Transform existing = grid.transform.Find(doorName);
        if (existing != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Already Exists",
                $"'{doorName}' tilemap already exists under the Grid.\n\n" +
                "Delete and recreate it?",
                "Yes, recreate", "Cancel");
            if (!overwrite) return;
            Object.DestroyImmediate(existing.gameObject);
        }

        // 3. Create the BreakableDoor tilemap GameObject
        GameObject doorGO = new GameObject(doorName);
        doorGO.transform.SetParent(grid.transform, false);
        doorGO.transform.localPosition = Vector3.zero;

        // 4. Add components
        Tilemap tilemap = doorGO.AddComponent<Tilemap>();
        TilemapRenderer renderer = doorGO.AddComponent<TilemapRenderer>();

        TilemapCollider2D collider = doorGO.AddComponent<TilemapCollider2D>();
        collider.isTrigger = false; // 物理阻挡，不穿透

        doorGO.AddComponent<BreakableDoorTilemap>();

        // 5. Set rendering order (above ground/wall/spikes)
        renderer.sortingOrder = 2;

        // 6. Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log($"✅ BreakableDoor tilemap created under Grid.{doorName}");
        EditorUtility.DisplayDialog("Success",
            $"BreakableDoor tilemap '{doorName}' created successfully!\n\n" +
            "Now you can:\n" +
            "1. Paint door tiles on the 'BreakableDoor' tilemap layer\n" +
            "2. The TilemapCollider2D blocks the player physically\n" +
            "3. BreakableDoorTilemap.cs is attached and ready\n" +
            "4. Attack tiles 2 times to break them",
            "OK");
    }
}
