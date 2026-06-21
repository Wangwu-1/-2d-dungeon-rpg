using UnityEngine;

public class PlayerGroundStates : PlayerStates
{
    public PlayerGroundStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Mouse0) && !player.IsBusy)
            statemachine.ChangeStates(player.primaryAttack);


        // ����뿪���棬���л�������״̬
        if (!player.IsGroundDetected())
            statemachine.ChangeStates(player.AirStates);


        if (Input.GetKeyDown(KeyCode.Space) && player.IsGroundDetected())
            statemachine.ChangeStates(player.JumpStates);
    }
}
