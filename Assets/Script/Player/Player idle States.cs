using UnityEngine;

public class PlayeridleStates : PlayerGroundStates
{
    public PlayeridleStates(Player _plater, PlayerStatesMachine _statemachine, string _animBoolName) : base(_plater, _statemachine, _animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // 从空中落地后会继承水平速度 → 自然减速到停
        if (Mathf.Abs(player.rb.velocity.x) > 0.01f)
        {
            float newX = Mathf.Lerp(player.rb.velocity.x, 0,
                1 - Mathf.Exp(-player.decelerationRate * Time.deltaTime));
            if (Mathf.Abs(newX) < 0.05f) newX = 0f;
            player.rb.velocity = new Vector2(newX, player.rb.velocity.y);
        }

        if (Xinput == player.Facedir && player.IsWallDetected())
            return;

        if (Xinput != 0 && !player.IsBusy)
            statemachine.ChangeStates(player.Movementstates);
    }
}
