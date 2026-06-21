using UnityEngine;

public class WizardDeathState : EnemyState
{
    private WizardEnemy wizard;
    private Animator anim;
    private string deathStateName = "Wizard_death";

    private float stayDuration = 1f;
    private bool isAnimationFinished = false;

    public WizardDeathState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        wizard = _enemy as WizardEnemy;
    }

    public override void Enter()
    {
        base.Enter();

        anim = wizard.GetComponent<Animator>();
        wizard.Die();

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
            wizard.DestroySelf();
    }
}
