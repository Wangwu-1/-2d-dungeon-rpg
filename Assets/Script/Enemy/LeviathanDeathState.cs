using UnityEngine;

public class LeviathanDeathState : EnemyState
{
    private LeviathanEnemy leviathan;
    private string deathStateName = "leviathan_die";

    private float stayDuration = 1f;
    private bool isAnimationFinished = false;

    public LeviathanDeathState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        leviathan = _enemy as LeviathanEnemy;
    }

    public override void Enter()
    {
        base.Enter();

        leviathan.Die();

        stateTimer = 0f;
        isAnimationFinished = false;
    }

    public override void Update()
    {
        base.Update();

        if (!isAnimationFinished && leviathan.anim != null)
        {
            AnimatorStateInfo stateInfo = leviathan.anim.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(deathStateName) && stateInfo.normalizedTime >= 1f)
            {
                isAnimationFinished = true;
                stateTimer = stayDuration;
            }
        }

        if (isAnimationFinished && stateTimer < 0)
            leviathan.DestroySelf();
    }
}
