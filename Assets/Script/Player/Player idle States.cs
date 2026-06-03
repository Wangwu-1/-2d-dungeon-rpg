using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayeridleStates : PlayerGroundStates
{
    public PlayeridleStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
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

        if (Xinput == player.Facedir && player.IsWallDetected())
            return;

        if (Xinput != 0 && !player.IsBusy)
            statemachine.ChangeStates(player.Movementstates);
    }
}
