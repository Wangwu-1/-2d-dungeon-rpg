using UnityEngine;

public class PlayerAirStates : PlayerStates
{
    public PlayerAirStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }


    public override void Exit()
    {
        base.Exit();
        // 保留水平速度，让下一个状态继承。不设 0，否则空中切状态（Jump→Air, Air→落地）
        // 时水平速度被清空，配上指数加速要重新爬很久，手感卡卡的
    }

    public override void Update()
    {
        base.Update();

        if (player.IsWallDetected())
        {
            statemachine.ChangeStates(player.WallSlideStates);
        }

        // 空中指数加减速（空中操控感更弱，靠 airAccelMultiplier 控制）
        float targetX = Xinput * player.MoveSpeed * 0.8f;
        float rate = (Xinput != 0 ? player.accelerationRate : player.decelerationRate) * player.airAccelMultiplier;
        player.SetVelocityXExponential(targetX, rate);

        if (player.IsGroundDetected())
        {
            // 落地时有输入 → 直接切 Move，跳过 Idle 帧，避免抽搐感
            if (Xinput != 0)
                statemachine.ChangeStates(player.Movementstates);
            else
                statemachine.ChangeStates(player.idleStates);
        }
    }
}
