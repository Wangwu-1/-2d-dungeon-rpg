using UnityEngine;

public class LeviathanAttackState : EnemyState
{
    private LeviathanEnemy leviathan;
    private bool hasDealtDamage;

    private const float ATTACK_DURATION = 1.2f;   // hand-keyed attack clip has no AnimationTrigger event, so time-driven

    public LeviathanAttackState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        leviathan = _enemy as LeviathanEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        leviathan.lastTimeAttacked = Time.time;
        leviathan.SetVelocityZero();
        stateTimer = ATTACK_DURATION;
        hasDealtDamage = false;
    }

    public override void Update()
    {
        base.Update();

        // Deal damage once, midway through the attack
        if (!hasDealtDamage && stateTimer < ATTACK_DURATION * 0.5f)
        {
            ApplyAttackDamage();
            hasDealtDamage = true;
        }

        if (stateTimer < 0)
            stateMachine.ChangeState(leviathan.idleState);
    }

    private void ApplyAttackDamage()
    {
        Vector2 attackPos = (Vector2)leviathan.transform.position + Vector2.right * leviathan.facingDir * leviathan.attackRadius * 0.5f;
        int playerLayer = LayerMask.GetMask("Default");
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPos, leviathan.attackRadius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                Player player = hit.GetComponent<Player>();
                if (player != null)
                    player.TakeDamage(leviathan.damage, leviathan.facingDir);
            }
        }
    }
}
