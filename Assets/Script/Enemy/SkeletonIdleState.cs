using UnityEngine;

public class SkeletonIdleState : EnemyState
{
    private SkeletonEnemy skeleton;

    public SkeletonIdleState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        skeleton = _enemy as SkeletonEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = skeleton.idleTime;
    }

    public override void Update()
    {
        base.Update();

        if (skeleton.IsPlayerDetected())
        {
            stateMachine.ChangeState(skeleton.battleState);
            return;
        }

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(skeleton.moveState);
        }
    }
}
