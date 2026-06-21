using UnityEngine;

public class WizardAttackState : EnemyState
{
    private WizardEnemy wizard;
    private bool hasDealtDamage;
    private bool isHeavyAttack;

    public WizardAttackState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        wizard = _enemy as WizardEnemy;
    }

    public override void Enter()
    {
        base.Enter();

        wizard.lastTimeAttacked = Time.time;
        wizard.SetVelocityZero();
        hasDealtDamage = false;

        // 25% chance for heavy attack (Attack2)
        isHeavyAttack = Random.value < 0.25f;

        // Heavy attack is slower but hits harder
        stateTimer = isHeavyAttack ? 1.0f : 0.7f;

        // Tell animator which attack to play
        wizard.anim.SetBool("isHeavyAttack", isHeavyAttack);
    }

    public override void Update()
    {
        base.Update();

        float damageTriggerTime = isHeavyAttack ? 0.55f : 0.4f;

        if (!hasDealtDamage && stateTimer < damageTriggerTime)
        {
            ApplyAttackDamage();
            hasDealtDamage = true;
        }

        if (stateTimer < 0)
        {
            if (wizard.IsPlayerDetected())
                stateMachine.ChangeState(wizard.battleState);
            else
                stateMachine.ChangeState(wizard.idleState);
        }
    }

    public override void Exit()
    {
        wizard.anim.SetBool("isHeavyAttack", false);
        base.Exit();
    }

    // TODO: 改为法术攻击 — 发射法术投射物（projectile）而非近战检测
    private void ApplyAttackDamage()
    {
        float damage = isHeavyAttack
            ? wizard.damage * wizard.heavyAttackDamageMultiplier
            : wizard.damage;

        float radius = isHeavyAttack
            ? wizard.attackRadius * wizard.heavyAttackRadiusMultiplier
            : wizard.attackRadius;

        float knockbackDir = isHeavyAttack ? wizard.facingDir * 1.5f : wizard.facingDir;

        Vector2 attackPos = (Vector2)wizard.transform.position + Vector2.right * wizard.facingDir * radius * 0.5f;
        int playerLayer = LayerMask.GetMask("Default");
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPos, radius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                Player player = hit.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(damage, knockbackDir);
                }
            }
        }
    }
}
