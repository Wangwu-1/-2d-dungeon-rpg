using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableDoorTilemap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int hitsToBreak = 2;
    [SerializeField] private TileBase crackedTile; // 可选：第一击后替换的裂纹瓦片

    [Header("Visual Feedback")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    private Tilemap tilemap;
    private Dictionary<Vector3Int, int> tileHitCounts = new Dictionary<Vector3Int, int>();

    // 缓存的白方形精灵，用作闪烁覆盖层（兼容任何渲染管线）
    private static Sprite flashSprite;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
            Debug.LogError("BreakableDoorTilemap needs a Tilemap component on the same GameObject.");

        // 创建白色方形精灵用于闪烁
        if (flashSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            flashSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
            flashSprite.name = "BreakableDoorFlash";
        }
    }

    /// <summary>
    /// 攻击指定圆形区域内的所有 tile。由玩家攻击调用。
    /// </summary>
    public void HitArea(Vector3 center, float radius)
    {
        if (tilemap == null) return;

        Vector3Int centerCell = tilemap.WorldToCell(center);
        int cellRadius = Mathf.CeilToInt(radius / tilemap.cellSize.x) + 1;

        BoundsInt bounds = new BoundsInt(
            centerCell.x - cellRadius, centerCell.y - cellRadius, 0,
            cellRadius * 2, cellRadius * 2, 1
        );

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;

            // 检查该 tile 的中心是否在攻击圆内
            Vector3 tileCenter = tilemap.GetCellCenterWorld(pos);
            float dist = Vector2.Distance(tileCenter, center);
            if (dist <= radius)
            {
                HitTile(pos);
            }
        }
    }

    private void HitTile(Vector3Int cellPos)
    {
        // 计数 +1
        if (!tileHitCounts.ContainsKey(cellPos))
            tileHitCounts[cellPos] = 0;
        tileHitCounts[cellPos]++;

        if (tileHitCounts[cellPos] >= hitsToBreak)
        {
            DestroyTile(cellPos);
        }
        else
        {
            // 视觉反馈
            StartCoroutine(FlashTile(cellPos));

            // 替换裂纹瓦片
            if (crackedTile != null)
                tilemap.SetTile(cellPos, crackedTile);
        }
    }

    private void DestroyTile(Vector3Int cellPos)
    {
        tilemap.SetTile(cellPos, null);
        tileHitCounts.Remove(cellPos);
    }

    private IEnumerator FlashTile(Vector3Int cellPos)
    {
        // 方法1: Tilemap 顶点色（兼容的材质会生效）
        tilemap.SetColor(cellPos, hitFlashColor);

        // 方法2: 覆盖层精灵 — 在任何渲染管线都可靠
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
        Vector3 cellSize = tilemap.cellSize;

        GameObject overlay = new GameObject("DoorFlash");
        overlay.transform.position = worldPos;
        overlay.transform.localScale = new Vector3(cellSize.x * 0.9f, cellSize.y * 0.9f, 1);
        overlay.layer = gameObject.layer;

        SpriteRenderer sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = flashSprite;
        sr.color = hitFlashColor;
        sr.sortingOrder = 100; // 在几乎所有东西之上渲染

        yield return new WaitForSeconds(flashDuration);

        // 恢复 tile 颜色
        if (tilemap.HasTile(cellPos))
            tilemap.SetColor(cellPos, Color.white);

        Destroy(overlay);
    }
}
