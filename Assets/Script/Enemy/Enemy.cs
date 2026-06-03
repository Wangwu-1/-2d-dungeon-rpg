using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed;
    public float maxHealth;
    [HideInInspector] public float currentHealth;
    public float damage;
    public float attackCooldown;
    [HideInInspector] public float lastTimeAttacked;

    [Header("Detection")]
    public float playerCheckDistance;
    public float attackDistance = 1.8f;
    public float attackRangeY = 1.5f;

    [Header("Collision")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected LayerMask whatIsWall;

    public int facingDir { get; private set; } = 1;
    protected bool facingRight = true;
    public bool isDead { get; protected set; }

    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public EnemyStateMachine stateMachine { get; private set; }
    public Collider2D cd { get; private set; }
    public Transform player { get; private set; }
    #endregion

    protected virtual void Awake()
    {
        stateMachine = new EnemyStateMachine();
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        cd = GetComponent<Collider2D>();
        FindPlayer();
        currentHealth = maxHealth;

        // High mass prevents player from pushing enemy (collision still works)
        rb.mass = 10000f;

        // Auto-add damage flash to sprite renderer
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.GetComponent<DamageFlash>() == null)
            sr.gameObject.AddComponent<DamageFlash>();
    }

    public void FindPlayer()
    {
        Player p = FindObjectOfType<Player>();
        player = p?.transform;
    }

    protected virtual void Update()
    {
        stateMachine.currentState.Update();
    }

    public virtual void SetVelocity(float _xVelocity, float _yVelocity)
    {
        rb.velocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity);
    }

    public void SetVelocityZero()
    {
        rb.velocity = Vector2.zero;
    }

    public virtual bool CanAttack()
    {
        return Time.time >= lastTimeAttacked + attackCooldown;
    }

    public virtual bool IsPlayerDetected()
    {
        if (player == null)
        {
            FindPlayer();
            return false;
        }
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= playerCheckDistance;
    }

    public virtual bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= attackDistance && Mathf.Abs(transform.position.y - player.position.y) <= attackRangeY;
    }

    public virtual bool IsGroundDetected()
    {
        // Cast from slightly above collider bottom to avoid edge-of-collider issues
        Vector2 origin = new Vector2(groundCheck.position.x, cd.bounds.min.y + 0.05f);
        return Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, whatIsGround);
    }

    public virtual bool IsWallDetected()
    {
        // Cast from the edge of the collider in the facing direction
        Bounds bounds = cd.bounds;
        Vector2 origin = new Vector2(bounds.center.x + bounds.extents.x * facingDir, wallCheck.position.y);
        LayerMask mask = whatIsWall.value != 0 ? whatIsWall : whatIsGround;
        return Physics2D.Raycast(origin, Vector2.right * facingDir, wallCheckDistance, mask);
    }

    protected virtual void OnDrawGizmos()
    {
        if (groundCheck != null && cd != null)
        {
            Vector2 groundOrigin = new Vector2(groundCheck.position.x, cd.bounds.min.y + 0.05f);
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * groundCheckDistance);
        }

        if (wallCheck != null && cd != null)
        {
            Bounds bounds = cd.bounds;
            Vector2 wallOrigin = new Vector2(bounds.center.x + bounds.extents.x * facingDir, wallCheck.position.y);
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * facingDir * wallCheckDistance);
        }

        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + playerCheckDistance * facingDir, transform.position.y));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));
    }

    public virtual void Flip()
    {
        facingDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public virtual void FlipController(float _x)
    {
        if (_x < 0 && facingRight)
            Flip();
        else if (_x > 0 && !facingRight)
            Flip();
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public virtual void Damage(float _damage)
    {
        if (isDead) return;
        // Will be overridden by skeleton
    }

    public virtual void Die()
    {
        isDead = true;
    }

    public virtual void DestroyAfterDelay(float delay)
    {
        Destroy(gameObject, delay);
    }
}
