using UnityEngine;

public class PlayerMovementStates : PlayerGroundStates
{
    public PlayerMovementStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // 指数加减速：有输入就加速到目标速度，松手就减速到停
        float targetX = Xinput * player.MoveSpeed;
        float rate = Xinput != 0 ? player.accelerationRate : player.decelerationRate;
        player.SetVelocityXExponential(targetX, rate);

        // 松手后速度趋近 0 才切 Idle，避免急停
        if (Xinput == 0 && Mathf.Abs(player.rb.velocity.x) < 0.05f)
            statemachine.ChangeStates(player.idleStates);

    }
}
