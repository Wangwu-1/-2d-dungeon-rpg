using UnityEngine;

public class PlayerDashStates : PlayerStates
{
    private const string JumpAnimBool = "Jump";

    private int ghostSpawnCounter;

    public PlayerDashStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 从 WallJump 转来时，"Jump" 动画 bool 没有被清除（WallJump.Exit 跳过了 base.Exit）
        // 必须手动关掉，否则动画机会卡在 Jump/坠落动画上不播放 Dash
        player.anim.SetBool(JumpAnimBool, false);

        // 保证角色面朝 dash 方向
        if (player.DashDirection != player.Facedir)
            player.Flip();

        // 进入时立即设置速度，不等第一帧 Update
        // 否则从 wallslide 转入时会有 1 帧的 fall velocity，导致 dash 不生效
        player.rb.velocity = new Vector2(player.DashSpeed * player.DashDirection, 0);

        statestime = player.DashDuration;
        ghostSpawnCounter = 0;
    }

    public override void Exit()
    {
        base.Exit();

        player.SetVelocity(0, rb.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        player.rb.velocity = new Vector2(player.DashSpeed * player.DashDirection, 0);

        // Spawn ghost trail every 3 frames
        ghostSpawnCounter++;
        if (ghostSpawnCounter % 3 == 0 && player.sr != null && player.sr.sprite != null)
        {
            GhostTrail.Spawn(player.sr, player.Facedir);
        }

        if (statestime < 0)
        {
            if (player.IsGroundDetected())
                statemachine.ChangeStates(player.idleStates);
            else
                statemachine.ChangeStates(player.AirStates);
        }
    }
}
