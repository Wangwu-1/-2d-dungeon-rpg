using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SpikeTilemapSetup
{
    private const string SPIKE_LAYER_NAME = "Spike";
    private const int SPIKE_LAYER_INDEX = 8;

    [MenuItem("Tools/Setup Spike Tilemap")]
    static void SetupSpikeTilemap()
    {
        // 1. Validate Spike layer exists
        int spikeLayer = LayerMask.NameToLayer(SPIKE_LAYER_NAME);
        if (spikeLayer == -1)
        {
            EditorUtility.DisplayDialog(
                "Layer Not Found",
                $"Layer '{SPIKE_LAYER_NAME}' (index {SPIKE_LAYER_INDEX}) not found.\n\n" +
                "Make sure you've added the Spike layer in:\n" +
                "Edit → Project Settings → Tags and Layers → User Layer 8",
                "OK");
            return;
        }

        // 2. Find Grid in the active scene
        Grid grid = Object.FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            EditorUtility.DisplayDialog("Grid Not Found",
                "No Grid found in the current scene. Please open the scene first.", "OK");
            return;
        }

        // 3. Check if Spike tilemap already exists
        string spikeName = "Spikes";
        Transform existing = grid.transform.Find(spikeName);
        if (existing != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Already Exists",
                $"'{spikeName}' tilemap already exists under the Grid.\n\n" +
                "Delete and recreate it?",
                "Yes, recreate", "Cancel");
            if (!overwrite) return;
            Object.DestroyImmediate(existing.gameObject);
        }

        // 4. Create the Spike tilemap GameObject
        GameObject spikeGO = new GameObject(spikeName);
        spikeGO.layer = spikeLayer;
        spikeGO.transform.SetParent(grid.transform, false);
        spikeGO.transform.localPosition = Vector3.zero;

        // 5. Add components
        Tilemap tilemap = spikeGO.AddComponent<Tilemap>();
        spikeGO.AddComponent<TilemapRenderer>();

        TilemapCollider2D collider = spikeGO.AddComponent<TilemapCollider2D>();
        collider.isTrigger = true;

        Spike spikeScript = spikeGO.AddComponent<Spike>();
        SerializedObject so = new SerializedObject(spikeScript);
        SerializedProperty damageProp = so.FindProperty("damage");
        if (damageProp != null)
            damageProp.floatValue = 1f;
        so.ApplyModifiedProperties();

        // 6. Set rendering order (render above ground/wall, below player effects)
        TilemapRenderer renderer = spikeGO.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 1;

        // 7. Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log($"✅ Spike tilemap created under Grid.{spikeName} (Layer: {SPIKE_LAYER_NAME})");
        EditorUtility.DisplayDialog("Success",
            $"Spike tilemap '{spikeName}' created successfully!\n\n" +
            "Now you can:\n" +
            "1. Paint spike tiles on the 'Spikes' tilemap layer\n" +
            "2. The TilemapCollider2D is already set to Is Trigger = true\n" +
            "3. Spike.cs is attached and ready to detect the player",
            "OK");
    }
}
