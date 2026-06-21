using UnityEngine;

public class PlayerHitState : PlayerStates
{
    public PlayerHitState(Player _player, PlayerStatesMachine _statemachine, string _animBoolName)
        : base(_player, _statemachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        statestime = 0.3f;

        // Knockback in the attack direction (away from attacker)
        float dir = player.lastHitKnockbackDirX;

        // 尖刺伤害（dir == 0）已在 Player.TakeSpikeDamage 中传送，跳过 knockback
        if (dir != 0)
            player.rb.velocity = new Vector2(dir * player.knockbackForceX, player.knockbackForceY);
        player.StartCoroutine(player.BusyFor(0.3f));

        // Flash sprite red
        if (player.sr != null)
            player.sr.color = Color.red;
    }

    public override void Update()
    {
        base.Update();

        if (statestime < 0)
        {
            if (player.IsGroundDetected())
                statemachine.ChangeStates(player.idleStates);
            else
                statemachine.ChangeStates(player.AirStates);
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (player.sr != null)
            player.sr.color = Color.white;
    }
}
