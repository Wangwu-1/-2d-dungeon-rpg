using UnityEngine;

/// <summary>
/// 根据玩家距离生成/回收敌人。
/// 玩家离开范围 → 回收活着的敌人
/// 敌人死亡后玩家再进入范围 → 刷新
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemyPrefab;

    [Header("Range")]
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float despawnRadius = 20f;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;

    private Enemy _currentEnemy;
    private bool _enemyDead;

    // ── Lifecycle ──

    private void Start()
    {
        // 玩家可能已经在范围内，立即生成
        TrySpawn();
    }

    private void Update()
    {
        TrySpawn();
        TryDespawn();
    }

    private void OnDestroy()
    {
        // Spawner 自身被销毁时断开事件连接
        if (_currentEnemy != null)
        {
            _currentEnemy.OnDied -= OnEnemyDied;
        }
    }

    // ── Core ──

    private void TrySpawn()
    {
        // 已经有敌人活着 → 不用生成
        if (_currentEnemy != null) return;

        float dist = GetPlayerDistance();
        if (dist > spawnRadius) return;

        // 敌方已死且还未离开过范围 → 不生成（等玩家离开再进才刷新）
        if (_enemyDead) return;

        SpawnEnemy();
    }

    private void TryDespawn()
    {
        // 没有活着的敌人 → 不需要回收
        if (_currentEnemy == null) return;

        float dist = GetPlayerDistance();
        if (dist <= despawnRadius) return;

        DespawnEnemy();
    }

    private void SpawnEnemy()
    {
        _currentEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        _currentEnemy.OnDied += OnEnemyDied;
        _enemyDead = false;
    }

    private void DespawnEnemy()
    {
        if (_currentEnemy != null)
        {
            _currentEnemy.OnDied -= OnEnemyDied;
            Destroy(_currentEnemy.gameObject);
            _currentEnemy = null;
        }

        // 敌人未死就离开 → 重置死亡标记，下次进入刷全新的
        _enemyDead = false;
    }

    private void OnEnemyDied()
    {
        if (_currentEnemy != null)
        {
            _currentEnemy.OnDied -= OnEnemyDied;
            _currentEnemy = null;
        }

        _enemyDead = true;
    }

    // ── Helpers ──

    private float GetPlayerDistance()
    {
        if (Player.Instance == null) return float.MaxValue;
        return Vector2.Distance(transform.position, Player.Instance.transform.position);
    }

    // ── Debug ──

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(0, 1, 0, 0.15f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = new Color(1, 1, 0, 0.08f);
        Gizmos.DrawSphere(transform.position, despawnRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, despawnRadius);
    }
}
