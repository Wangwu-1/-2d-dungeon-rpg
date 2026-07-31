using UnityEngine;

public class LeviathanHitState : EnemyState
{
    private LeviathanEnemy leviathan;

    public LeviathanHitState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        leviathan = _enemy as LeviathanEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = leviathan.stunDuration;

        // Trigger damage flash on the sprite
        DamageFlash flash = leviathan.GetComponentInChildren<DamageFlash>();
        if (flash != null)
            flash.Flash();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(leviathan.idleState);
        }
    }
}
