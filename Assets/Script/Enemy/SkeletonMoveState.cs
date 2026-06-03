using UnityEngine;

public class SkeletonMoveState : EnemyState
{
    private SkeletonEnemy skeleton;

    public SkeletonMoveState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        skeleton = _enemy as SkeletonEnemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (skeleton.IsPlayerDetected())
        {
            stateMachine.ChangeState(skeleton.battleState);
            return;
        }

        if (!skeleton.IsGroundDetected() || skeleton.IsWallDetected())
        {
            Debug.Log("Skeleton hit edge/wall, flipping back to idle");
            skeleton.Flip();
            stateMachine.ChangeState(skeleton.idleState);
            return;
        }

        skeleton.SetVelocity(skeleton.moveSpeed * skeleton.facingDir, rb.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
