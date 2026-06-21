using UnityEngine;

public class SkeletonHitState : EnemyState
{
    private SkeletonEnemy skeleton;

    public SkeletonHitState(Enemy _enemy, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemy, _stateMachine, _animBoolName)
    {
        skeleton = _enemy as SkeletonEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = skeleton.stunDuration;

        // Apply knockback away from the player (set velocity directly to avoid FlipController)
        if (skeleton.player != null)
        {
            int knockbackDir = skeleton.transform.position.x > skeleton.player.position.x ? 1 : -1;
            skeleton.rb.velocity = new Vector2(knockbackDir * skeleton.knockbackForceX, skeleton.knockbackForceY);
        }
        else
        {
            skeleton.rb.velocity = new Vector2(-skeleton.facingDir * skeleton.knockbackForceX, skeleton.knockbackForceY);
        }

        // Trigger damage flash on the skeleton's sprite
        DamageFlash flash = skeleton.GetComponentInChildren<DamageFlash>();
        if (flash != null)
            flash.Flash();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            if (skeleton.IsPlayerDetected())
                stateMachine.ChangeState(skeleton.battleState);
            else
                stateMachine.ChangeState(skeleton.idleState);
        }
    }
}
