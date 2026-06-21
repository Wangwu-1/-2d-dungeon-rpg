using UnityEngine;

public class WizardIdleState : EnemyState
{
    private WizardEnemy wizard;

    public WizardIdleState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        wizard = _enemy as WizardEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = wizard.idleTime;
    }

    public override void Update()
    {
        base.Update();

        if (wizard.IsPlayerDetected())
        {
            stateMachine.ChangeState(wizard.battleState);
            return;
        }

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(wizard.moveState);
        }
    }
}
