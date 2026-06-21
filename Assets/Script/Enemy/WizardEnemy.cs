using UnityEngine;

public class WizardEnemy : Enemy
{
    #region States
    public WizardIdleState idleState { get; private set; }
    public WizardMoveState moveState { get; private set; }
    public WizardBattleState battleState { get; private set; }
    public WizardAttackState attackState { get; private set; }
    public WizardHitState hitState { get; private set; }
    public WizardDeathState deathState { get; private set; }
    #endregion

    [Header("Wizard Specific")]
    public float idleTime = 1.5f;
    public float battleRange = 8f;
    public float stunDuration = 0.5f;
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;
    public float attackRadius = 1.8f;
    [Header("Heavy Attack (Attack2 — 25% chance)")]
    public float heavyAttackDamageMultiplier = 2f;
    public float heavyAttackRadiusMultiplier = 1.3f;

    protected override void Awake()
    {
        base.Awake();

        idleState = new WizardIdleState(this, stateMachine, "");
        moveState = new WizardMoveState(this, stateMachine, "isMoving");
        battleState = new WizardBattleState(this, stateMachine, "isMoving");
        attackState = new WizardAttackState(this, stateMachine, "isAttacking");
        hitState = new WizardHitState(this, stateMachine, "isHit");
        deathState = new WizardDeathState(this, stateMachine, "isDead");
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
