using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirStates : PlayerStates
{
    public PlayerAirStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }


    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0, rb.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (player.IsWallDetected())
        {
            statemachine.ChangeStates(player.WallSlideStates);
        }

        // �ڿ���ʱ������ˮƽ�ƶ�
        if(Xinput != 0)
            player.SetVelocity(Xinput * player.MoveSpeed * .8f, rb.velocity.y);


        if (player.IsGroundDetected())
            statemachine.ChangeStates(player.idleStates);
    }
}
