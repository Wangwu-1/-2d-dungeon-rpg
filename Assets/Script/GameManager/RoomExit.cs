using UnityEngine;

public class RoomExit : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform spawnPoint; // 目标房间的出生点

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只响应玩家，且过渡中不重复触发
        Player player = other.GetComponent<Player>();
        if (player == null || player.isTransitioning) return;

        if (spawnPoint == null)
        {
            Debug.LogError($"RoomExit on {name}: spawnPoint is not assigned!", this);
            return;
        }

        RoomManager.TransitionTo(spawnPoint);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, spawnPoint.position);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
        }
    }
}
