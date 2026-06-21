using UnityEngine;

public class PlayerPrimaryAttack : PlayerStates
{

    private int comboCounter;

    private float LastTimeAttack;
    private float   combowindow = 2;

    public PlayerPrimaryAttack(Player _player, PlayerStatesMachine _statemachine, string _animBoolName) : base(_player, _statemachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (Time.time >= LastTimeAttack + combowindow)
        {
            comboCounter = 0;
        }

        player.anim.SetInteger("ComboCounter", comboCounter);

        player.SetVelocity(player.AttackMovement[comboCounter].x * player.Facedir, player.AttackMovement[comboCounter].y);
        statestime = .1f;

    }

    public override void Exit()
    {
        base.Exit();

        comboCounter++;
        LastTimeAttack = Time.time;

        if (comboCounter > 2)
            comboCounter = 0;

        player.StartCoroutine(player.BusyFor(.15f));
    }

    public override void Update()
    {
        base.Update();
        player.SetVelocity(0, 0);

        if (triggerCalled)
        {
            ApplyAttackDamage();
            statemachine.ChangeStates(player.idleStates);
        }
    }

    private void ApplyAttackDamage()
    {
        Vector2 attackPos = (Vector2)player.transform.position + Vector2.right * player.Facedir * 1.2f;
        int layerMask = LayerMask.GetMask("Default", "Enemy");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, player.attackRadius, layerMask);

        bool hitSomething = false;
        foreach (Collider2D hit in hitEnemies)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Damage(1f);
                hitSomething = true;
            }

            BreakableDoorTilemap door = hit.GetComponent<BreakableDoorTilemap>();
            if (door != null)
            {
                door.HitArea(attackPos, player.attackRadius);
                hitSomething = true;
            }
        }

        if (hitSomething)
        {
            player.HitStop(0.05f);
            player.ScreenShake();
        }
    }
}
