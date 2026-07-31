using UnityEngine;

public class LeviathanEnemy : Enemy
{
    #region States
    public LeviathanIdleState idleState { get; private set; }
    public LeviathanAttackState attackState { get; private set; }
    public LeviathanHitState hitState { get; private set; }
    public LeviathanDeathState deathState { get; private set; }
    #endregion

    [Header("Leviathan Specific")]
    public float stunDuration = 1f;
    public float attackRadius = 3f;

    protected override void Awake()
    {
        base.Awake();

        idleState = new LeviathanIdleState(this, stateMachine, "");
        attackState = new LeviathanAttackState(this, stateMachine, "isAttacking");
        hitState = new LeviathanHitState(this, stateMachine, "isHit");
        deathState = new LeviathanDeathState(this, stateMachine, "isDead");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
        anim.Play("leviathan_in");
    }

    public override void Damage(float _damage)
    {
        if (isDead) return;

        currentHealth -= _damage;
        if (currentHealth <= 0)
            stateMachine.ChangeState(deathState);
        else
            stateMachine.ChangeState(hitState);
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
