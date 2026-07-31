using UnityEngine;

public class LeviathanIdleState : EnemyState
{
    private LeviathanEnemy leviathan;

    public LeviathanIdleState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        leviathan = _enemy as LeviathanEnemy;
    }

    public override void Update()
    {
        base.Update();

        if (leviathan.IsPlayerDetected() && leviathan.IsPlayerInAttackRange() && leviathan.CanAttack())
        {
            stateMachine.ChangeState(leviathan.attackState);
        }
    }
}
