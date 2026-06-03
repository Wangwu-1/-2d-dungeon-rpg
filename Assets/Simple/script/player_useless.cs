using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_useless : Entity_useless
{
    // Start is called before the first frame update

    [Header("Dash info")]
    [SerializeField] private float dashDurations; 
    private float dashtime;
    [SerializeField] private float dashspeed;
    [SerializeField] private float dashcoldown;
    private float dashColdownTimer;


    [Header("Atteck info")]
    [SerializeField] private float comboTime = .3f;
    [SerializeField] private float Atteckmove;
    private float comboTimeWindow;
    private bool isAttecking;
    private int comboCounter;



    [Header("Moving info")]
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float JumpForce;
    
    
     
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {

        dashColdownTimer -= Time.deltaTime;
        dashtime -= Time.deltaTime;
        comboTimeWindow -= Time.deltaTime;

        InputChecking();
        Movement();

      


        FlipControllor();
        CollisionChecks();

        AnimatorControllor();


    }

    public void AtteckOver()
    {
        isAttecking = false;
        comboCounter++;
        if (comboCounter > 2)
            comboCounter = 0;
    }


    

    private void InputChecking()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.J))
        {
            ComboAttecking();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            DashAbility();
        }

        void ComboAttecking()
        {
            if (!isGround)
                return;

            if (comboTimeWindow < 0)
                comboCounter = 0;

            isAttecking = true;
            comboTimeWindow = comboTime;
        }
    }

    private void DashAbility()
    {
        if(!isAttecking && dashColdownTimer < 0 )
        {
        
            dashtime = dashDurations;

            dashColdownTimer = dashcoldown;

        }
    }

    private void AnimatorControllor()
    {           
        bool isMoving = rb.velocity.x != 0;

        anim.SetFloat("yvelocity", rb.velocity.y);
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isGround", isGround);
        anim.SetBool("isDashing", dashtime > 0);
        anim.SetBool("isAttecking", isAttecking);
        anim.SetInteger("comboCounter", comboCounter);

    }


    private void Jump()
    {
        

        if(isGround)
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
    }


    private void Movement()
    {
        if (isAttecking)
        {
            rb.velocity = new Vector2(0, 0);
        }
        else if(dashtime > 0)
        {
            rb.velocity = new Vector2(Facingdir * dashspeed, 0);
        }
        else
        {
            rb.velocity = new Vector2(xInput * MoveSpeed, rb.velocity.y);
        }

    }

    



}
