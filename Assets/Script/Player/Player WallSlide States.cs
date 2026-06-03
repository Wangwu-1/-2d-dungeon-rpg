using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideStates : PlayerStates
{
    public PlayerWallSlideStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exist()
    {
        base.Exist();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            statemachine.ChangeStates(player.WallJumpStates);
            return;
        }



        if(Xinput != 0 && player.Facedir != Xinput)
        {
            statemachine.ChangeStates(player.idleStates);
        }



        if (Yinput < 0)
            rb.velocity = new Vector2(0, rb.velocity.y );
        else
            rb.velocity = new Vector2(0, rb.velocity.y * .4f);


        if (player.IsGroundDetected())
        {
            statemachine.ChangeStates(player.idleStates);
        }
    }
}
