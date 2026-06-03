using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStates : PlayerGroundStates
{
    public PlayerMovementStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
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

        player.SetVelocity(Xinput * player.MoveSpeed, rb.velocity.y);

        if (Xinput == 0)
            statemachine.ChangeStates(player.idleStates);

    }
}
