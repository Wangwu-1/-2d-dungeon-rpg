using UnityEngine;

public class PlayerWallJumpStates : PlayerStates
{
    public PlayerWallJumpStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        statestime = 1f;

        player.SetVelocity(player.wallJumpForceX * -player.Facedir, player.Jumpforce);

    }

    public override void Exit()
    {
        // Don't call base.Exit() — that would SetBool("Jump", false) and cause
        // a one-frame flicker before AirStates sets it back to true.
    }

    public override void Update()
    {
        base.Update();

        if(statestime < 0)
        {
            statemachine.ChangeStates(player.AirStates);
        }
    }
}
