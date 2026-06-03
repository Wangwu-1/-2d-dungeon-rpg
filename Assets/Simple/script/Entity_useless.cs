using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_useless : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator anim;

    [Header("Collision info")]
    [SerializeField] protected Transform GroundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected LayerMask WhatIsGround;
    [Space]
    [SerializeField] protected Transform WallCheck;
    [SerializeField] protected float WallCheckdistance;

    protected bool isWallDetected;
    protected bool isGround;

    protected  float xInput;

    protected int Facingdir = 1;
    protected bool FacingRight = true;



    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        CollisionChecks();
    }

    protected virtual void CollisionChecks()
    {
        isGround = Physics2D.Raycast(GroundCheck.position, Vector2.down, groundCheckDistance, WhatIsGround);
        isWallDetected = Physics2D.Raycast(WallCheck.position, Vector2.right, WallCheckdistance * Facingdir, WhatIsGround);
    }

    protected virtual void Flip()
    {
        Facingdir = Facingdir * -1;
        FacingRight = !FacingRight;
        transform.Rotate(0, 180, 0);
    }

    protected virtual void FlipControllor()
    {
        if (rb.velocity.x < 0 && FacingRight)
        {
            Flip();
        }
        else if (rb.velocity.x > 0 && !FacingRight)
        {
            Flip();
        }
    }



    protected virtual void OnDrawGizmos()
    {

        Gizmos.DrawLine(GroundCheck.position, new Vector3(GroundCheck.position.x, GroundCheck.position.y - groundCheckDistance));
     
        Gizmos.DrawLine(WallCheck.position, new Vector3( WallCheck.position.x + WallCheckdistance * Facingdir , WallCheck.position.y));

    }

}
