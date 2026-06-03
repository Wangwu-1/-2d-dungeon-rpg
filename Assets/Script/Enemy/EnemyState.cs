using UnityEngine;

public class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemy;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected bool triggerCalled;

    protected string animBoolName;

    public EnemyState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
    {
        enemy = _enemy;
        stateMachine = _stateMachine;
        animBoolName = _animBoolName;
    }

    public virtual void Enter()
    {
        if (!string.IsNullOrEmpty(animBoolName))
            enemy.anim.SetBool(animBoolName, true);
        rb = enemy.rb;
        triggerCalled = false;
        stateTimer = 0;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void Exit()
    {
        if (!string.IsNullOrEmpty(animBoolName))
            enemy.anim.SetBool(animBoolName, false);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}
