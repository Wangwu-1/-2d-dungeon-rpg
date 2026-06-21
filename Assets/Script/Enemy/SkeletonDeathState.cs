using UnityEngine;

public class SkeletonDeathState : EnemyState
{
    private SkeletonEnemy skeleton;
    private Animator anim;
    private string deathStateName = "Skeleton_death";

    private float stayDuration = 1f;
    private bool isAnimationFinished = false;

    public SkeletonDeathState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        skeleton = _enemy as SkeletonEnemy;
    }

    public override void Enter()
    {
        base.Enter();

        anim = skeleton.GetComponent<Animator>();
        skeleton.Die();

        stateTimer = 0f;
        isAnimationFinished = false;

        anim.Play(deathStateName, 0, 0f);
    }

    public override void Update()
    {
        base.Update();

        if (!isAnimationFinished && anim != null)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(deathStateName) && stateInfo.normalizedTime >= 1f)
            {
                isAnimationFinished = true;
                stateTimer = stayDuration;
            }
        }

        if (isAnimationFinished && stateTimer < 0)
            skeleton.DestroySelf();
    }
}
