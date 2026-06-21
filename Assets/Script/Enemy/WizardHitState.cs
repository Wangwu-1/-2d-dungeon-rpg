using UnityEngine;

public class WizardHitState : EnemyState
{
    private WizardEnemy wizard;

    public WizardHitState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        wizard = _enemy as WizardEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = wizard.stunDuration;

        // Apply knockback away from the player
        if (wizard.player != null)
        {
            int knockbackDir = wizard.transform.position.x > wizard.player.position.x ? 1 : -1;
            wizard.rb.velocity = new Vector2(knockbackDir * wizard.knockbackForceX, wizard.knockbackForceY);
        }
        else
        {
            wizard.rb.velocity = new Vector2(-wizard.facingDir * wizard.knockbackForceX, wizard.knockbackForceY);
        }

        // Trigger damage flash
        DamageFlash flash = wizard.GetComponentInChildren<DamageFlash>();
        if (flash != null)
            flash.Flash();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            if (wizard.IsPlayerDetected())
                stateMachine.ChangeState(wizard.battleState);
            else
                stateMachine.ChangeState(wizard.idleState);
        }
    }
}
