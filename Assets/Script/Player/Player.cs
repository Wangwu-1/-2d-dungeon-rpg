using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    [Header("Attack Detail")]
    public Vector2[] AttackMovement;

    [Header("Health")]
    public float maxHealth = 5;
    [HideInInspector] public float currentHealth;
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
    [SerializeField] private CinemachineImpulseSource impulseSource;

    public bool IsBusy { get; private set; }
    [HideInInspector] public float lastHitKnockbackDirX;

    public void SetImpulseSource(CinemachineImpulseSource source) => impulseSource = source;

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



    [Header("collision info")]
    [SerializeField] private Transform Groundcheck;
    [SerializeField] private float GroundCheckdistence;
    [SerializeField] private Transform Wallcheck;
    [SerializeField] private float WallCheckdistence;
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private LayerMask WhatIsWall;

    public int Facedir { get; private set; } = 1;
    private bool Faceright = true;

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
        statesmachine.Initialize(idleStates);

        // Setup screen shake
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<CinemachineImpulseListener>() == null)
            cam.gameObject.AddComponent<CinemachineImpulseListener>();

        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }
    }

    private void Update()
    {
        statesmachine.currentStates.Update();

        CheckOnDashInput();

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


    public bool IsGroundDetected() => Physics2D.Raycast(Groundcheck.position, Vector2.down, GroundCheckdistence, WhatIsGround);
    public bool IsWallDetected()
    {
        Collider2D col = cd;
        Bounds bounds = col.bounds;
        Vector2 origin = new Vector2(bounds.center.x + bounds.extents.x * Facedir, bounds.center.y);
        return Physics2D.Raycast(origin, Vector2.right * Facedir, WallCheckdistence, WhatIsWall);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(Groundcheck.position, new Vector3(Groundcheck.position.x, Groundcheck.position.y - GroundCheckdistence));
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
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
        Debug.Log("Player HP: " + currentHealth + "/" + maxHealth);

        if (impulseSource != null)
            impulseSource.GenerateImpulse();

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

}
