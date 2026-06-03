using UnityEngine;

public class SkeletonBattleState : EnemyState
{
    private SkeletonEnemy skeleton;
    private Transform player;

    public SkeletonBattleState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        skeleton = _enemy as SkeletonEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        player = skeleton.player;
    }

    public override void Update()
    {
        base.Update();

        if (player == null)
        {
            // Try to find the player again before giving up
            skeleton.FindPlayer();
            player = skeleton.player;
            if (player == null)
            {
                stateMachine.ChangeState(skeleton.idleState);
                return;
            }
        }

        float playerDistance = Vector2.Distance(skeleton.transform.position, player.position);

        if (playerDistance > skeleton.battleRange + 2f)
        {
            stateMachine.ChangeState(skeleton.idleState);
            return;
        }

        if (skeleton.IsPlayerInAttackRange() && skeleton.CanAttack())
        {
            stateMachine.ChangeState(skeleton.attackState);
            return;
        }

        if (!skeleton.IsGroundDetected() || skeleton.IsWallDetected())
        {
            stateMachine.ChangeState(skeleton.idleState);
            return;
        }

        // Face the player
        if (player.position.x > skeleton.transform.position.x && skeleton.facingDir == -1)
            skeleton.Flip();
        else if (player.position.x < skeleton.transform.position.x && skeleton.facingDir == 1)
            skeleton.Flip();

        skeleton.SetVelocity(skeleton.facingDir * skeleton.moveSpeed * 1.3f, rb.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
