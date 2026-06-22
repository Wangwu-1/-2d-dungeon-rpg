using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("视差系数：0 = 贴屏幕不动（无穷远），1 = 随世界移动（不视差）")]
    [SerializeField] private float parallaxFactor = 0.5f;

    [Tooltip("是否限制为水平视差（侧滚式）")]
    [SerializeField] private bool horizontalOnly = true;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ParallaxEffect: No Main Camera found!");
            enabled = false;
            return;
        }
        startPosition = transform.position;
        cameraStartPosition = mainCamera.transform.position;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // 相机从起始位置移动了多少
        Vector3 cameraDelta = mainCamera.transform.position - cameraStartPosition;

        if (horizontalOnly)
            cameraDelta.y = 0;

        // parallaxFactor=0 → 完全跟随相机（贴在屏幕上，无穷远）
        // parallaxFactor=1 → 不随相机偏移（与世界同速滚动）
        transform.position = startPosition + cameraDelta * (1f - parallaxFactor);
    }
}
