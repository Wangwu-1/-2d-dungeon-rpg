using UnityEngine;
using System.Collections.Generic;

public class GhostTrail : MonoBehaviour
{
    private static Queue<GameObject> pool = new Queue<GameObject>();
    private static Transform poolRoot;

    private SpriteRenderer ghostSr;
    private SpriteRenderer targetSr;
    private float fadeDuration = 0.3f;

    public static void Spawn(SpriteRenderer source, int facedir)
    {
        GameObject go = GetFromPool();
        go.transform.position = source.transform.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.SetActive(true);
        go.GetComponent<GhostTrail>().Init(source, facedir);
    }

    private static GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            GameObject go = pool.Dequeue();
            if (go != null) return go;
        }

        if (poolRoot == null)
            poolRoot = new GameObject("GhostPool").transform;

        var go2 = new GameObject("DashGhost");
        go2.AddComponent<SpriteRenderer>();
        go2.AddComponent<GhostTrail>();
        go2.transform.SetParent(poolRoot);
        return go2;
    }

    private static void ReturnToPool(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }

    public static void ClearPool()
    {
        foreach (var go in pool)
            Destroy(go);
        pool.Clear();
    }

    public void Init(SpriteRenderer source, int facedir)
    {
        targetSr = source;
        ghostSr = GetComponent<SpriteRenderer>();

        ghostSr.sprite = targetSr.sprite;
        ghostSr.sortingOrder = targetSr.sortingOrder + 1;

        ghostSr.flipX = targetSr.flipX;
        if (facedir == -1)
            ghostSr.flipX = !ghostSr.flipX;

        Color c = targetSr.color;
        c.a = 0.4f;
        ghostSr.color = c;

        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            Color c = ghostSr.color;
            c.a = Mathf.Lerp(0.4f, 0f, t);
            ghostSr.color = c;
            yield return null;
        }
        ReturnToPool(gameObject);
    }
}
