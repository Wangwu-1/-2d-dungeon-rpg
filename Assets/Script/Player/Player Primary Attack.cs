using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttack : PlayerStates
{

    private int comboCounter;

    private float LastTimeAttack;
    private float   combowindow = 2;

    [Header("Attack Hitbox")]
    [SerializeField] private float attackRadius = 2.5f;


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
        Debug.Log(comboCounter);

        statestime = .1f;

    }

    public override void Exist()
    {
        base.Exist();

        comboCounter++;
        LastTimeAttack = Time.time;

        if (comboCounter > 2)
            comboCounter = 0;

        player.StartCoroutine("BusyFor", .15f);
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
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRadius);

        Debug.Log("Attack check at " + attackPos + " radius " + attackRadius + " found " + hitEnemies.Length + " colliders");

        bool hitSomething = false;
        foreach (Collider2D hit in hitEnemies)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("Hit enemy: " + enemy.name);
                enemy.Damage(1f);
                hitSomething = true;
            }
        }

        if (hitSomething)
            player.HitStop(0.05f);
    }
}
