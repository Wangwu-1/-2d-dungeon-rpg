using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Attack Detail")]
    public Vector2[] AttackMovement;

    [Header("Health")]
    public float maxHealth = 5;
    [HideInInspector] public float currentHealth;
    public event System.Action<float, float> OnHealthChanged;
    [Header("Invincibility")]
    public float invincibleDuration = 0.5f;
    [HideInInspector] public bool isInvincible { get; private set; }
    public bool isDead { get; private set; }

    [Header("Attack")]
    public float attackRadius = 2.5f;

    [Header("Knockback")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;

    [Header("Screen Shake")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.3f;

    public bool IsBusy { get; private set; }
    [HideInInspector] public bool isTransitioning;
    [HideInInspector] public float lastHitKnockbackDirX;
    private Vector2 _lastSafePosition;

    public static Player Instance { get; private set; }

    public void ScreenShake()
    {
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 originalPos = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            cam.transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = originalPos;
    }

    [Header("Move info")]
    public float MoveSpeed = 12f;
    public float Jumpforce = 12f;

    [Header("Dash info")]
    public float DashSpeed;
    public float DashDuration;
    [SerializeField]private float DashCoolDown;
    private float Dashtimer;
    public float DashDirection { get; private set; }

    [Header("WallJump info")]
    public float wallJumpForceX = 5f;

    [Header("Movement Smoothing")]
    public float accelerationRate = 12f;
    public float decelerationRate = 18f;
    public float airAccelMultiplier = 0.5f;



    [Header("collision info")]
    [SerializeField] private Transform Wallcheck;
    [SerializeField] private float WallCheckdistence;
    [SerializeField] private LayerMask WhatIsWall;

    public int Facedir { get; private set; } = 1;
    private bool Faceright = true;
    private int _platformLayerIndex = -1;
    private HashSet<Collider2D> _groundContacts = new HashSet<Collider2D>();

    #region Component
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public SpriteRenderer sr { get; private set; }
    public Collider2D cd { get; private set; }
    #endregion


    #region States
    public PlayerStatesMachine statesmachine { get; private set; }
    public PlayeridleStates idleStates { get; private set; }
    public PlayerMovementStates Movementstates { get; private set; }
    public PlayerAirStates AirStates { get; private set; }
    public PlayerJumpStates JumpStates { get; private set; }
    public PlayerDashStates DashStates { get; private set; }
    public PlayerWallSlideStates WallSlideStates { get; private set; }
    public PlayerWallJumpStates WallJumpStates { get; private set; }



    public PlayerPrimaryAttack primaryAttack { get; private set; }
    public PlayerHitState hitState { get; private set; }
    #endregion


    private void Awake()
    {
        Instance = this;

        statesmachine    = new PlayerStatesMachine();

        idleStates       = new PlayeridleStates(this, statesmachine,"Idle");

        Movementstates   = new PlayerMovementStates(this, statesmachine, "Move");

        JumpStates       = new PlayerJumpStates(this, statesmachine, "Jump");

        AirStates        = new PlayerAirStates(this, statesmachine, "Jump");

        DashStates       = new PlayerDashStates(this, statesmachine, "Dash");

        WallSlideStates  = new PlayerWallSlideStates(this, statesmachine, "Wallslide");

        WallJumpStates   = new PlayerWallJumpStates(this, statesmachine, "Jump");


        primaryAttack    = new PlayerPrimaryAttack(this, statesmachine, "Attack");
        hitState         = new PlayerHitState(this, statesmachine, "");
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        cd = GetComponent<Collider2D>();

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        _lastSafePosition = transform.position;
        statesmachine.Initialize(idleStates);

        _platformLayerIndex = LayerMask.NameToLayer("Platform");
    }

    private void Update()
    {
        statesmachine.currentStates.Update();

        CheckOnDashInput();

        TrackSafePosition();

    }


    public IEnumerator BusyFor(float _second)
    {
        IsBusy = true;

        yield return new WaitForSeconds(_second);

        IsBusy = false;
    }



    public void AnimationTrigger() => statesmachine.currentStates.AnimationFinishTrigger();

    public void HitStop(float duration)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }


    private void CheckOnDashInput()
    {
        Dashtimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && Dashtimer < 0)
        {
            Dashtimer = DashCoolDown;

            DashDirection = Input.GetAxisRaw("Horizontal");

            // 贴墙状态 -> 强制朝墙反方向 dash
            if (statesmachine.currentStates == WallSlideStates)
            {
                DashDirection = -Facedir;
            }
            else if (statesmachine.currentStates == WallJumpStates)
            {
                // 蹬墙跳时，角色已经 Flip 面朝远离墙的方向
                // 不按方向键时默认朝当前朝向 dash（继续远离墙）
                if (DashDirection == 0)
                    DashDirection = Facedir;
            }
            else
            {
                if (DashDirection == 0)
                    DashDirection = Facedir;

                // 在地面时阻挡朝向墙 dash
                if (IsWallDetected() && DashDirection == Facedir)
                    return;
            }

            statesmachine.ChangeStates(DashStates);
        }
    }

    public void SetVelocity(float _xvelocity,float _yvelocity)
    {
        rb.velocity = new Vector2(_xvelocity, _yvelocity);
        FlipControllor(_xvelocity);
    }

    /// <summary>
    /// 指数趋近式水平移动：vel.x = lerp(vel.x, targetX, 1 - exp(-rate * dt))
    /// rate 越大加速越快，适用于有惯性的手感
    /// </summary>
    public void SetVelocityXExponential(float targetX, float rate)
    {
        float lerpFactor = 1 - Mathf.Exp(-rate * Time.deltaTime);
        float newX = Mathf.Lerp(rb.velocity.x, targetX, lerpFactor);

        // 接近目标时直接归位，防止微漂移
        if (Mathf.Abs(newX - targetX) < 0.1f)
            newX = targetX;

        rb.velocity = new Vector2(newX, rb.velocity.y);
        FlipControllor(newX);
    }


    public bool IsGroundDetected()
    {
        return _groundContacts.Count > 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;
        bool isGround = layer == 3; // Ground (Layer 3)
        bool isPlatform = _platformLayerIndex != -1 && layer == _platformLayerIndex;
        if (!isGround && !isPlatform) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                // Platform 层：需在表面上方 + 非上升状态
                if (isPlatform && rb.velocity.y > 0.05f)
                    continue;

                _groundContacts.Add(collision.collider);
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;
        bool isGround = layer == 3;
        bool isPlatform = _platformLayerIndex != -1 && layer == _platformLayerIndex;
        if (isGround || isPlatform)
            _groundContacts.Remove(collision.collider);
    }
    public bool IsWallDetected()
    {
        Collider2D col = cd;
        Bounds bounds = col.bounds;
        Vector2 origin = new Vector2(bounds.center.x + bounds.extents.x * Facedir, bounds.center.y);
        return Physics2D.Raycast(origin, Vector2.right * Facedir, WallCheckdistence, WhatIsWall);
    }
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.white;
            Bounds bounds = col.bounds;
            Vector2 origin = new Vector2(bounds.center.x + bounds.extents.x * Facedir, bounds.center.y);
            Gizmos.DrawLine(origin, origin + Vector2.right * Facedir * WallCheckdistence);
        }
    }


    public void Flip()
    {
        Facedir = Facedir * -1;
        Faceright = !Faceright;
        transform.Rotate(0, 180, 0);
    }


    public void FlipControllor(float _xvelocity)
    {
        if (rb.velocity.x < 0 && Faceright)
        {
            Flip();
        }
        else if (rb.velocity.x > 0 && !Faceright)
        {
            Flip();
        }
    }

    public void TakeDamage(float _damage, float knockbackDirX = 0)
    {
        if (isInvincible || isDead) return;

        currentHealth -= _damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        ScreenShake();

        if (currentHealth <= 0)
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            this.enabled = false;
        }
        else
        {
            lastHitKnockbackDirX = knockbackDirX != 0 ? knockbackDirX : -Facedir;
            StartCoroutine(InvincibilityFrames());
            statesmachine.ChangeStates(hitState);
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }

    private void TrackSafePosition()
    {
        if (IsGroundDetected() && !isInvincible && !isDead)
            _lastSafePosition = transform.position;
    }

    public void TakeSpikeDamage(float damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        ScreenShake();

        if (currentHealth <= 0)
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            this.enabled = false;
        }
        else
        {
            // 传送回安全位置
            transform.position = _lastSafePosition;
            rb.velocity = Vector2.zero;

            // 标记为尖刺伤害，HitState 跳过 knockback
            lastHitKnockbackDirX = 0;
            StartCoroutine(InvincibilityFrames());
            statesmachine.ChangeStates(hitState);
        }
    }

}
