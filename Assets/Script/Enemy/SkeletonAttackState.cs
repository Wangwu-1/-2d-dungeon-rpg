using UnityEngine;

public class SkeletonAttackState : EnemyState
{
    private SkeletonEnemy skeleton;
    private bool hasDealtDamage;

    public SkeletonAttackState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        skeleton = _enemy as SkeletonEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        skeleton.lastTimeAttacked = Time.time;
        skeleton.SetVelocityZero();
        stateTimer = 0.7f;
        hasDealtDamage = false;
    }

    public override void Update()
    {
        base.Update();

        if (!hasDealtDamage && stateTimer < 0.4f)
        {
            ApplyAttackDamage();
            hasDealtDamage = true;
        }

        if (stateTimer < 0)
        {
            if (skeleton.IsPlayerDetected())
                stateMachine.ChangeState(skeleton.battleState);
            else
                stateMachine.ChangeState(skeleton.idleState);
        }
    }

    private void ApplyAttackDamage()
    {
        Vector2 attackPos = (Vector2)skeleton.transform.position + Vector2.right * skeleton.facingDir * skeleton.attackRadius * 0.5f;
        int playerLayer = LayerMask.GetMask("Default");
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPos, skeleton.attackRadius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                Player player = hit.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(skeleton.damage, skeleton.facingDir);
                }
            }
        }
    }

}
