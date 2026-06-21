using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 一键制作 Wizard Prefab —— 自动切片 Sprite、创建动画、Animator Controller、Prefab。
/// 使用方法：Tools → Wizard → Setup Wizard Prefab
///
/// Wizard 法术攻击待后续修改，攻击目前先用近战 OverlapCircle。
/// </summary>
public static class WizardSetupTool
{
    private const string SPRITE_FOLDER = "Assets/Materials/Monsters/Wizard";
    private const string ANIMATION_FOLDER = "Assets/Materials/Monsters/Wizard/Animations";
    private const string PREFAB_PATH = "Assets/Prefabs/Enemies";
    private const float PIXELS_PER_UNIT = 16f;

    // Sprite cell size
    private const int CELL_WIDTH = 77;
    private const int CELL_HEIGHT = 190;

    [MenuItem("Tools/Wizard/Setup Wizard Prefab _%#W")]
    static void SetupWizardPrefab()
    {
        if (!CheckAssets()) return;

        if (!EditorUtility.DisplayDialog("Setup Wizard Prefab",
            "This will:\n" +
            "1. Slice wizard sprites (77x190 frames)\n" +
            "2. Create animation clips\n" +
            "3. Create Animator Controller\n" +
            "4. Create Wizard.prefab\n\n" +
            "Proceed?", "Yes, go ahead", "Cancel"))
            return;

        EnsureAnimationFolder();

        EditorUtility.DisplayProgressBar("Wizard Setup", "Slicing sprites...", 0.1f);
        SliceWizardSprites();

        BuildAnimationsAndPrefab();
    }

    [MenuItem("Tools/Wizard/Regenerate Animations && Prefab")]
    static void RegenerateAnimationsAndPrefab()
    {
        if (!CheckAssets()) return;

        if (!EditorUtility.DisplayDialog("Regenerate Wizard Animation",
            "This will SKIP sprite slicing and:\n" +
            "1. Re-create all animation clips from sliced sprites\n" +
            "2. Re-create Animator Controller\n" +
            "3. Re-create Wizard.prefab\n\n" +
            "Proceed?", "Yes, go ahead", "Cancel"))
            return;

        EnsureAnimationFolder();
        BuildAnimationsAndPrefab();
    }

    [MenuItem("Tools/Wizard/Rebuild Controller && Prefab Only")]
    static void RebuildControllerAndPrefab()
    {
        if (!AssetDatabase.IsValidFolder(ANIMATION_FOLDER))
        {
            EditorUtility.DisplayDialog("Error", "Animation folder not found. Run Setup first.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Rebuild Wizard Controller & Prefab",
            "This will:\n" +
            "1. Load your existing animation clips from disk\n" +
            "2. Rebuild Animator Controller (keeps your manual animation intact)\n" +
            "3. Rebuild Wizard.prefab\n\n" +
            "(Skips sprite slicing and clip creation entirely)\n\n" +
            "Proceed?", "Yes, go ahead", "Cancel"))
            return;

        // Clean up orphan clip (old wizard_attack1 from earlier tool runs)
        string orphanPath = $"{ANIMATION_FOLDER}/wizard_attack1.anim";
        AnimationClip orphan = AssetDatabase.LoadAssetAtPath<AnimationClip>(orphanPath);
        if (orphan != null)
        {
            AssetDatabase.DeleteAsset(orphanPath);
            Debug.Log("Cleaned up orphan: wizard_attack1.anim");
        }
        AssetDatabase.Refresh();

        // Load all existing .anim files from the folder
        string[] animGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { ANIMATION_FOLDER });
        var loadedClips = new Dictionary<string, AnimationClip>();
        foreach (string guid in animGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
                loadedClips[clip.name] = clip;
        }

        // Verify we have the essential clips
        string[] requiredClips = { "Wizard_idle", "Wizard_run", "Wizard_attack", "Wizard_attack2", "Wizard_hit", "Wizard_death" };
        bool missing = false;
        foreach (string name in requiredClips)
        {
            if (!loadedClips.ContainsKey(name))
            {
                Debug.LogWarning($"Missing animation clip: {name}");
                missing = true;
            }
        }

        if (missing && !EditorUtility.DisplayDialog("Missing Clips",
            "Some expected animation clips were not found (see console).\n" +
            "Continue anyway?", "Continue", "Cancel"))
            return;

        EditorUtility.DisplayProgressBar("Wizard Setup", "Rebuilding Animator Controller...", 0.3f);
        AnimatorController controller = CreateAnimatorController(loadedClips);

        if (controller == null)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Error", "Failed to create Animator Controller!", "OK");
            return;
        }

        EditorUtility.DisplayProgressBar("Wizard Setup", "Rebuilding prefab...", 0.6f);
        GameObject prefab = CreateWizardPrefab(controller);

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to create prefab!", "OK");
            return;
        }

        EditorUtility.DisplayDialog("Success!",
            $"Wizard Controller & Prefab rebuilt:\n{PREFAB_PATH}/Wizard.prefab\n\n" +
            "Your manual animations were preserved.",
            "OK");

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    static bool CheckAssets()
    {
        if (!AssetDatabase.IsValidFolder(SPRITE_FOLDER))
        {
            EditorUtility.DisplayDialog("Error", $"Wizard sprite folder not found: {SPRITE_FOLDER}", "OK");
            return false;
        }
        string[] pngGuids = AssetDatabase.FindAssets("t:texture", new[] { SPRITE_FOLDER });
        if (pngGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No PNG files found in Wizard folder!", "OK");
            return false;
        }
        return true;
    }

    static void EnsureAnimationFolder()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "../", ANIMATION_FOLDER));
        AssetDatabase.Refresh();
    }

    static void BuildAnimationsAndPrefab()
    {
        EditorUtility.DisplayProgressBar("Wizard Setup", "Creating animation clips...", 0.3f);

        var clipDefs = new (string clipName, string sheetName, int frameCount, int sampleRate, bool loop)[]
        {
            ("Wizard_idle",   "Idle",    18, 6,  true),   // looping
            ("Wizard_run",    "Run",     24, 8,  true),   // looping
            ("Wizard_attack",  "Attack1", 24, 10, false),  // one-shot
            ("Wizard_attack2", "Attack2", 24, 10, false),  // one-shot
            ("Wizard_hit",    "Hit",     12, 8,  false),  // one-shot
            ("Wizard_death",  "Death",   21, 8,  false),  // one-shot
            ("Wizard_jump",   "Jump",     6, 6,  true),   // looping
            ("Wizard_fall",   "Fall",     6, 6,  true),   // looping
        };

        var createdClips = new Dictionary<string, AnimationClip>();
        foreach (var (clipName, sheetName, frameCount, sampleRate, loop) in clipDefs)
        {
            AnimationClip clip = CreateAnimationClip(clipName, sheetName, frameCount, sampleRate, loop);
            if (clip != null)
                createdClips[clipName] = clip;
        }

        EditorUtility.DisplayProgressBar("Wizard Setup", "Creating Animator Controller...", 0.6f);
        AnimatorController controller = CreateAnimatorController(createdClips);

        if (controller == null)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Error", "Failed to create Animator Controller!", "OK");
            return;
        }

        EditorUtility.DisplayProgressBar("Wizard Setup", "Creating prefab...", 0.8f);
        GameObject prefab = CreateWizardPrefab(controller);

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to create prefab!", "OK");
            return;
        }

        EditorUtility.DisplayDialog("Success!",
            $"Wizard Prefab created at:\n{PREFAB_PATH}/Wizard.prefab\n\n" +
            "Attack1 = normal, Attack2 = heavy (25% chance, 2x damage).",
            "OK");

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    // ──────────────────────────────────────────────
    // Sprite Slicing
    // ──────────────────────────────────────────────

    static void SliceWizardSprites()
    {
        string[] pngPaths = Directory.GetFiles(
            Path.Combine(Application.dataPath, "../", SPRITE_FOLDER), "*.png");

        foreach (string fullPath in pngPaths)
        {
            string relativePath = "Assets" + fullPath.Replace(Application.dataPath, "").Replace('\\', '/');
            string fileName = Path.GetFileNameWithoutExtension(fullPath);

            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer == null) continue;

            // Get texture dimensions
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            if (texture == null) continue;

            int texWidth = texture.width;
            int texHeight = texture.height;

            // Calculate frame count
            int frameCount = texWidth / CELL_WIDTH;
            if (texWidth % CELL_WIDTH != 0)
            {
                Debug.LogWarning($"Wizard/{fileName}.png width {texWidth} is not evenly divisible by cell width {CELL_WIDTH}. Frames may be misaligned.");
            }

            // Configure sprite mode to Multiple
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PIXELS_PER_UNIT;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            // Build sprite rects as SpriteMetaData (the modern API)
#pragma warning disable 0618 // spritesheet is deprecated but still works
            var spriteMetaDataList = new List<SpriteMetaData>();
            for (int i = 0; i < frameCount; i++)
            {
                var smd = new SpriteMetaData
                {
                    name = $"{fileName}_{i}",
                    rect = new Rect(i * CELL_WIDTH, 0, CELL_WIDTH, CELL_HEIGHT),
                    pivot = new Vector2(0.5f, 0.5f),
                    alignment = (int)SpriteAlignment.Center,
                };
                spriteMetaDataList.Add(smd);
            }
            importer.spritesheet = spriteMetaDataList.ToArray();
#pragma warning restore 0618

            // Apply changes
            importer.SaveAndReimport();

            Debug.Log($"Sliced Wizard/{fileName}.png → {frameCount} frames ({CELL_WIDTH}x{CELL_HEIGHT})");
        }
    }

    // ──────────────────────────────────────────────
    // Animation Clips
    // ──────────────────────────────────────────────

    static AnimationClip CreateAnimationClip(string clipName, string sheetName, int frameCount, int sampleRate, bool loop)
    {
        // Load the sprites from the sliced texture
        string spritePath = $"{SPRITE_FOLDER}/{sheetName}.png";
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritePath);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError($"No sprites found at {spritePath}. Make sure slicing was done first.");
            return null;
        }

        // Filter only Sprite objects and sort by name (index)
        var spriteList = new List<Sprite>();
        foreach (var obj in sprites)
        {
            if (obj is Sprite sprite)
                spriteList.Add(sprite);
        }
        spriteList.Sort((a, b) => string.Compare(a.name, b.name));

        if (spriteList.Count < frameCount)
        {
            Debug.LogWarning($"Expected {frameCount} sprites but found {spriteList.Count} for {sheetName}");
        }

        // Create the animation clip
        AnimationClip clip = new AnimationClip();
        clip.name = clipName;
        clip.frameRate = sampleRate;

        // Build sprite keyframe curve
        var keyFrames = new ObjectReferenceKeyframe[frameCount];
        float frameDuration = 1f / sampleRate;
        for (int i = 0; i < frameCount && i < spriteList.Count; i++)
        {
            keyFrames[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = spriteList[i]
            };
        }

        // Bind the curve to SpriteRenderer.m_Sprite
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyFrames);

        // Set loop settings
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        settings.loopBlend = false;
        settings.keepOriginalPositionY = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Delete old clip before saving (avoid GUID corruption from CopySerialized)
        string clipPath = $"{ANIMATION_FOLDER}/{clipName}.anim";
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.Refresh();
        }

        AssetDatabase.CreateAsset(clip, clipPath);

        AssetDatabase.Refresh();
        Debug.Log($"Created animation: {clipPath} ({frameCount} frames, {sampleRate} fps)");

        return AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
    }

    // ──────────────────────────────────────────────
    // Animator Controller
    // ──────────────────────────────────────────────

    static AnimatorController CreateAnimatorController(Dictionary<string, AnimationClip> clips)
    {
        string controllerPath = $"{ANIMATION_FOLDER}/Wizard.controller";

        // Delete existing controller to rebuild from scratch
        AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(controllerPath);
            AssetDatabase.Refresh();
        }

        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        if (controller == null)
        {
            Debug.LogError("Failed to create AnimatorController!");
            return null;
        }

        // Add parameters
        controller.AddParameter("isMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isAttacking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isHeavyAttack", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isHit", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isDead", AnimatorControllerParameterType.Bool);

        // Get the base layer state machine
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        // ── Create states ──
        var idleState = stateMachine.AddState("Wizard_idle");
        if (clips.ContainsKey("Wizard_idle")) idleState.motion = clips["Wizard_idle"];

        var moveState = stateMachine.AddState("Wizard_run");
        if (clips.ContainsKey("Wizard_run")) moveState.motion = clips["Wizard_run"];

        var attackState = stateMachine.AddState("Wizard_attack");
        if (clips.ContainsKey("Wizard_attack")) attackState.motion = clips["Wizard_attack"];

        var attack2State = stateMachine.AddState("Wizard_attack2");
        if (clips.ContainsKey("Wizard_attack2")) attack2State.motion = clips["Wizard_attack2"];

        var hitState = stateMachine.AddState("Wizard_hit");
        if (clips.ContainsKey("Wizard_hit")) hitState.motion = clips["Wizard_hit"];

        var deathState = stateMachine.AddState("Wizard_death");
        if (clips.ContainsKey("Wizard_death")) deathState.motion = clips["Wizard_death"];

        // ── Set default state ──
        stateMachine.defaultState = idleState;

        // ── Transitions from Any State ──
        // Priority order: isDead > isHit > isAttacking+isHeavyAttack > isAttacking
        // Any → isDead → Death
        var anyToDeath = stateMachine.AddAnyStateTransition(deathState);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "isDead");
        anyToDeath.duration = 0.1f;
        anyToDeath.hasExitTime = false;

        // Any → isHit → Hit
        var anyToHit = stateMachine.AddAnyStateTransition(hitState);
        anyToHit.AddCondition(AnimatorConditionMode.If, 0, "isHit");
        anyToHit.duration = 0.1f;
        anyToHit.hasExitTime = false;

        // Any → isAttacking + isHeavyAttack → Attack2 (listed before Attack for priority)
        var anyToAttack2 = stateMachine.AddAnyStateTransition(attack2State);
        anyToAttack2.AddCondition(AnimatorConditionMode.If, 0, "isHeavyAttack");
        anyToAttack2.AddCondition(AnimatorConditionMode.If, 0, "isAttacking");
        anyToAttack2.duration = 0.1f;
        anyToAttack2.hasExitTime = false;

        // Any → isAttacking → Attack (normal — matches when isHeavyAttack is false)
        var anyToAttack = stateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "isAttacking");
        anyToAttack.duration = 0.1f;
        anyToAttack.hasExitTime = false;

        // ── Idle → isMoving → Move ──
        var idleToMove = idleState.AddTransition(moveState);
        idleToMove.AddCondition(AnimatorConditionMode.If, 0, "isMoving");
        idleToMove.duration = 0.25f;
        idleToMove.hasExitTime = false;

        // ── Move → !isMoving → Idle ──
        var moveToIdle = moveState.AddTransition(idleState);
        moveToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isMoving");
        moveToIdle.duration = 0.25f;
        moveToIdle.hasExitTime = false;
        moveToIdle.canTransitionToSelf = true;

        // ── Attack → !isAttacking → Idle (via exit time) ──
        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isAttacking");
        attackToIdle.duration = 0.25f;
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;

        // ── Attack2 → !isAttacking → Idle ──
        var attack2ToIdle = attack2State.AddTransition(idleState);
        attack2ToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isAttacking");
        attack2ToIdle.duration = 0.25f;
        attack2ToIdle.hasExitTime = true;
        attack2ToIdle.exitTime = 0.9f;

        // ── Hit → !isHit → Idle ──
        var hitToIdle = hitState.AddTransition(idleState);
        hitToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isHit");
        hitToIdle.duration = 0.25f;
        hitToIdle.hasExitTime = true;
        hitToIdle.exitTime = 0.9f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssetIfDirty(controller);

        Debug.Log($"Created Animator Controller: {controllerPath}");
        return controller;
    }

    // ──────────────────────────────────────────────
    // Prefab Creation
    // ──────────────────────────────────────────────

    static GameObject CreateWizardPrefab(AnimatorController controller)
    {
        // Ensure prefab directory exists
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "../", PREFAB_PATH));

        // ── Create the root GameObject ──
        GameObject wizardGO = new GameObject("Wizard", typeof(SpriteRenderer), typeof(Animator),
            typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(WizardEnemy));

        // ── SpriteRenderer ──
        SpriteRenderer sr = wizardGO.GetComponent<SpriteRenderer>();
        // Assign the first frame of Idle as default sprite
        string idlePath = $"{SPRITE_FOLDER}/Idle.png";
        Object[] idleSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(idlePath);
        if (idleSprites != null && idleSprites.Length > 0)
        {
            foreach (var obj in idleSprites)
            {
                if (obj is Sprite sprite && sprite.name == "Idle_0")
                {
                    sr.sprite = sprite;
                    break;
                }
            }
            // Fallback: first sprite
            if (sr.sprite == null && idleSprites.Length > 0 && idleSprites[0] is Sprite firstSprite)
                sr.sprite = firstSprite;
        }

        // ── Animator ──
        Animator animator = wizardGO.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        // ── Rigidbody2D ──
        Rigidbody2D rb = wizardGO.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 4f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.mass = 1f;

        // ── BoxCollider2D ──
        BoxCollider2D box = wizardGO.GetComponent<BoxCollider2D>();
        // Auto-size to sprite
        if (sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            box.size = new Vector2(spriteSize.x * 0.5f, spriteSize.y * 0.8f);
            box.offset = new Vector2(0, -spriteSize.y * 0.1f);
        }

        // ── Create child: Ground Check ──
        GameObject groundCheck = new GameObject("Ground Check");
        groundCheck.transform.SetParent(wizardGO.transform);
        groundCheck.transform.localPosition = Vector3.zero;

        // ── Create child: Wall Check ──
        GameObject wallCheck = new GameObject("Wall Check");
        wallCheck.transform.SetParent(wizardGO.transform);
        wallCheck.transform.localPosition = Vector3.zero;

        // ── WizardEnemy component ──
        WizardEnemy wizard = wizardGO.GetComponent<WizardEnemy>();
        SerializedObject so = new SerializedObject(wizard);

        // Set transforms (serialized via FindProperty because they're protected)
        so.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        so.FindProperty("wallCheck").objectReferenceValue = wallCheck.transform;
        so.FindProperty("groundCheckDistance").floatValue = 0.5f;
        so.FindProperty("wallCheckDistance").floatValue = 0.5f;

        // Set LayerMasks
        so.FindProperty("whatIsGround").intValue = 1 << 3;  // Layer 3 (Ground)
        so.FindProperty("whatIsWall").intValue = 1 << 6;    // Layer 6 (Wall)

        // Stats
        so.FindProperty("moveSpeed").floatValue = 2f;
        so.FindProperty("maxHealth").floatValue = 4f;
        so.FindProperty("damage").floatValue = 1f;
        so.FindProperty("attackCooldown").floatValue = 1.5f;
        so.FindProperty("playerCheckDistance").floatValue = 10f;
        so.FindProperty("attackDistance").floatValue = 1.8f;
        so.FindProperty("attackRangeY").floatValue = 1.5f;

        // Wizard-specific
        so.FindProperty("idleTime").floatValue = 1.5f;
        so.FindProperty("battleRange").floatValue = 8f;
        so.FindProperty("stunDuration").floatValue = 0.5f;
        so.FindProperty("knockbackForceX").floatValue = 5f;
        so.FindProperty("knockbackForceY").floatValue = 3f;
        so.FindProperty("attackRadius").floatValue = 1.8f;

        // Heavy attack
        so.FindProperty("heavyAttackDamageMultiplier").floatValue = 2f;
        so.FindProperty("heavyAttackRadiusMultiplier").floatValue = 1.3f;

        so.ApplyModifiedProperties();

        // ── Save as Prefab ──
        string prefabPath = $"{PREFAB_PATH}/Wizard.prefab";

        // Delete existing if present
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
            AssetDatabase.DeleteAsset(prefabPath);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(wizardGO, prefabPath);

        // Clean up the temporary scene object
        Object.DestroyImmediate(wizardGO);

        if (prefab != null)
        {
            Debug.Log($"✅ Wizard prefab created at: {prefabPath}");
        }
        else
        {
            Debug.LogError("Failed to create Wizard prefab!");
        }

        AssetDatabase.Refresh();
        return prefab;
    }
}
