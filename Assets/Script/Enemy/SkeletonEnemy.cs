using UnityEngine;

public class SkeletonEnemy : Enemy
{
    #region States
    public SkeletonIdleState idleState { get; private set; }
    public SkeletonMoveState moveState { get; private set; }
    public SkeletonBattleState battleState { get; private set; }
    public SkeletonAttackState attackState { get; private set; }
    public SkeletonHitState hitState { get; private set; }
    public SkeletonDeathState deathState { get; private set; }
    #endregion

    [Header("Skeleton Specific")]
    public float idleTime = 1.5f;
    public float battleRange = 8f;
    public float stunDuration = 0.5f;
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;
    public float attackRadius = 1.5f;

    protected override void Awake()
    {
        base.Awake();

        idleState = new SkeletonIdleState(this, stateMachine, "");
        moveState = new SkeletonMoveState(this, stateMachine, "isMoving");
        battleState = new SkeletonBattleState(this, stateMachine, "isMoving");
        attackState = new SkeletonAttackState(this, stateMachine, "isAttacking");
        hitState = new SkeletonHitState(this, stateMachine, "isHit");
        deathState = new SkeletonDeathState(this, stateMachine, "isDead");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public override void Damage(float _damage)
    {
        if (isDead) return;

        currentHealth -= _damage;
        Debug.Log("Skeleton HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            stateMachine.ChangeState(deathState);
        }
        else
        {
            stateMachine.ChangeState(hitState);
        }
    }

    public override void Die()
    {
        base.Die();
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        cd.enabled = false;
    }

    public void DestroySelf()
    {
        Destroy(gameObject, 1.5f);
    }
}
