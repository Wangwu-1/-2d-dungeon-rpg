using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    private SpriteRenderer targetSr;
    private SpriteRenderer ghostSr;
    private Color startColor;
    private float fadeDuration = 0.3f;

    public void Init(SpriteRenderer source, int facedir)
    {
        targetSr = source;
        ghostSr = GetComponent<SpriteRenderer>();
        if (ghostSr == null)
            ghostSr = gameObject.AddComponent<SpriteRenderer>();

        ghostSr.sprite = targetSr.sprite;
        ghostSr.sortingOrder = targetSr.sortingOrder + 1;

        // 父物体用 Rotate(0,180,0) 翻转，残影是根物体没有父级旋转
        // 需要手动处理 flipX：面朝左时反转 flipX
        ghostSr.flipX = targetSr.flipX;
        if (facedir == -1)
            ghostSr.flipX = !ghostSr.flipX;

        startColor = targetSr.color;
        startColor.a = 0.4f;
        ghostSr.color = startColor;

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
        Destroy(gameObject);
    }
}
