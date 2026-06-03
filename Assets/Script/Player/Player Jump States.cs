using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpStates : PlayerAirStates
{
    public PlayerJumpStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        rb.velocity = new Vector2(rb.velocity.x, player.Jumpforce);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        Xinput = Input.GetAxisRaw("Horizontal");
        player.SetVelocity(Xinput * player.MoveSpeed, rb.velocity.y);

        // Variable jump height: cut upward speed when Space released
        if (!Input.GetKey(KeyCode.Space) && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

        if (rb.velocity.y < 0)
        {
            statemachine.ChangeStates(player.AirStates);
        }
    }
}
