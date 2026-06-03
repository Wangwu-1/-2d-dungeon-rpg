using UnityEngine;

public class PlayerHitState : PlayerStates
{
    private float knockbackForceX = 5f;
    private float knockbackForceY = 3f;

    public PlayerHitState(Player _player, PlayerStatesMachine _statemachine, string _animBoolName)
        : base(_player, _statemachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        rb = player.rb;
        triggerCalled = false;
        statestime = 0.3f;

        // Knockback in the attack direction (away from attacker)
        float dir = player.lastHitKnockbackDirX;
        player.rb.velocity = new Vector2(dir * knockbackForceX, knockbackForceY);
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

    public override void Exist()
    {
        base.Exist();
        if (player.sr != null)
            player.sr.color = Color.white;
    }
}
