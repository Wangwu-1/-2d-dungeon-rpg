using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates 

{

    protected PlayerStatesMachine statemachine;
    protected Player player;

    protected Rigidbody2D rb;
    protected float Xinput;
    protected float Yinput;
    protected float Yvelocity;
    private string animBoolName;

    protected float statestime;
    protected bool triggerCalled;

    public PlayerStates(Player _plater, PlayerStatesMachine _statemachine,string _animBoolName)
    {
        this.player = _plater;
        this.statemachine = _statemachine;
        this.animBoolName = _animBoolName;
    }

    public virtual void Enter()
    {
        if (!string.IsNullOrEmpty(animBoolName))
            player.anim.SetBool(animBoolName, true);

        rb = player.rb;

        triggerCalled = false;

    }

    public virtual void Update()
    {

        statestime -= Time.deltaTime;

        Xinput = Input.GetAxisRaw("Horizontal");
        Yinput = Input.GetAxisRaw("Vertical");

        player.anim.SetFloat("Yvelocity", rb.velocity.y);

    }

    
    public virtual void Exit()
    {
        if (!string.IsNullOrEmpty(animBoolName))
            player.anim.SetBool(animBoolName, false);

    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }

}