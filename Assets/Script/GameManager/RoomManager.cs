using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private Color fadeColor = Color.black;

    private Image fadeImage;
    private Canvas fadeCanvas;
    private CinemachineVirtualCamera vcam;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateFadeCanvas();
    }

    private void Start()
    {
        vcam = FindObjectOfType<CinemachineVirtualCamera>();
    }

    private void CreateFadeCanvas()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(transform);
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create full-screen black Image
        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        fadeImage = imgGO.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 开始房间过渡：淡出 → 传送玩家 → 摄像机瞬移 → 淡入
    /// </summary>
    public static void TransitionTo(Transform spawnPoint)
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("RoomManager");
            go.AddComponent<RoomManager>();
        }

        Instance.StartCoroutine(Instance.TransitionRoutine(spawnPoint));
    }

    private IEnumerator TransitionRoutine(Transform spawnPoint)
    {
        Player player = Player.Instance;
        if (player == null || spawnPoint == null) yield break;

        // 1. Freeze player input
        player.isTransitioning = true;

        // 2. Fade to black
        yield return StartCoroutine(FadeRoutine(0, 1, fadeDuration));

        // 3. Teleport player
        player.transform.position = spawnPoint.position;
        player.rb.velocity = Vector2.zero;

        // 4. Snap camera instantly to new position
        if (vcam != null)
            vcam.PreviousStateIsValid = false;

        // 5. Brief pause
        yield return new WaitForSecondsRealtime(0.1f);

        // 6. Fade in
        yield return StartCoroutine(FadeRoutine(1, 0, fadeDuration));

        // 7. Unfreeze player
        player.isTransitioning = false;
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, toAlpha);
    }
}
