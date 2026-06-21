using UnityEngine;

public class WizardBattleState : EnemyState
{
    private WizardEnemy wizard;
    private Transform player;

    public WizardBattleState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        wizard = _enemy as WizardEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        player = wizard.player;
    }

    public override void Update()
    {
        base.Update();

        if (player == null)
        {
            wizard.FindPlayer();
            player = wizard.player;
            if (player == null)
            {
                stateMachine.ChangeState(wizard.idleState);
                return;
            }
        }

        float playerDistance = Vector2.Distance(wizard.transform.position, player.position);

        if (playerDistance > wizard.battleRange + 2f)
        {
            stateMachine.ChangeState(wizard.idleState);
            return;
        }

        if (wizard.IsPlayerInAttackRange() && wizard.CanAttack())
        {
            stateMachine.ChangeState(wizard.attackState);
            return;
        }

        if (!wizard.IsGroundDetected() || wizard.IsWallDetected())
        {
            stateMachine.ChangeState(wizard.idleState);
            return;
        }

        // Face the player
        if (player.position.x > wizard.transform.position.x && wizard.facingDir == -1)
            wizard.Flip();
        else if (player.position.x < wizard.transform.position.x && wizard.facingDir == 1)
            wizard.Flip();

        wizard.SetVelocity(wizard.facingDir * wizard.moveSpeed * 1.3f, rb.velocity.y);
    }
}
