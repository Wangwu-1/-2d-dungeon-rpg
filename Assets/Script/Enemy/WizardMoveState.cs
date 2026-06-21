using UnityEngine;

public class WizardMoveState : EnemyState
{
    private WizardEnemy wizard;

    public WizardMoveState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        wizard = _enemy as WizardEnemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (wizard.IsPlayerDetected())
        {
            stateMachine.ChangeState(wizard.battleState);
            return;
        }

        if (!wizard.IsGroundDetected() || wizard.IsWallDetected())
        {
            wizard.Flip();
            stateMachine.ChangeState(wizard.idleState);
            return;
        }

        wizard.SetVelocity(wizard.moveSpeed * wizard.facingDir, rb.velocity.y);
    }
}
