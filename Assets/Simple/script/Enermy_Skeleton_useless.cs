using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enermy_Skeleton : Entity_useless
{

    [Header("Moving info")]
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float JumpForce;



    protected override void Start()
    {
        base.Start();     
    }





    protected override void Update()
    {
        base.Update();
        if (!isGround || isWallDetected)
        {
            Flip();

        }

        rb.velocity = new Vector2(MoveSpeed * Facingdir, rb.velocity.y);

    }


    protected override void CollisionChecks()
    {
        base.CollisionChecks();
    }


}
