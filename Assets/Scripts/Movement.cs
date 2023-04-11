using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public float horizontal;
    private float vertical;
    public new BoxCollider2D collider;
    public Rigidbody2D rb;
    public float accel;
    public float maxSpeed;
    public float accelConst;
    public float friction;
    public float jumpForce;
    
    private float tempMaxSpeed;
    private bool isGrounded;
    private float groundSlope;
    private bool jumpPressed = false;

    void Start()
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        //Gets Inputs
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }
        //jumpPressed = Input.GetKeyDown(KeyCode.Space);
    }

    void FixedUpdate()
    {
        //Sets Max velocity as joystick dependent
        if (horizontal != 0)
        {
            tempMaxSpeed = maxSpeed * Mathf.Abs(horizontal);
        }
        else
        {
            tempMaxSpeed = maxSpeed;
        }

        Bounds bounds = collider.bounds;
        Vector2 loc1 = bounds.min;
        Vector2 loc2 = new Vector2(bounds.max.x, bounds.min.y);
        RaycastHit2D ray1 = Physics2D.Raycast(loc1, Vector2.down, 0.05f);
        RaycastHit2D ray2 = Physics2D.Raycast(loc2, Vector2.down, 0.05f);
        // Debug.Log((bool)ray1.collider);
        // Debug.Log((bool)ray2.collider);
        if (ray1.collider && ray2.collider)
        {
            isGrounded = true;
            groundSlope = 0;
        }
        else if (ray1.collider)
        {
            groundSlope = -Mathf.Atan2(ray1.normal.x, ray1.normal.y)*Mathf.Rad2Deg;
            isGrounded = true;
        }
        else if (ray2.collider)
        {
            groundSlope = -Mathf.Atan2(ray2.normal.x, ray2.normal.y)*Mathf.Rad2Deg;
            isGrounded = true;
        }
        else
        {
            groundSlope = 0;
            Vector2 down = new Vector2(0, 0.05f);
            RaycastHit2D ray3 = Physics2D.Linecast(loc1 - down, loc2 - down);
            if (ray3.collider)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }
        // Debug.Log(isGrounded);
        // Debug.Log(angle);

        //Debug.Log(tempMaxSpeed);
        Vector2 velocity = rb.velocity;
        float direction = velocity.x / Mathf.Abs(velocity.x);
        
        //Horizontal Movement
        rb.AddForce(new Vector2(horizontal * accel * accelConst,0), ForceMode2D.Force);
        //Checks for horizontal speed
        if (Mathf.Abs(rb.velocity.x) >= tempMaxSpeed)
        {
            //Sets to terminal velocity
            rb.velocity = new Vector2(direction * tempMaxSpeed, velocity.y);
            rb.AddForce(new Vector2(-direction * accel * accelConst * Mathf.Abs(horizontal),0), ForceMode2D.Force);
        }
        
        //Friction
        if (horizontal == 0 && velocity.x != 0)
        {
            float fricForce = -direction * accelConst * friction;
            float velChange = fricForce * Time.deltaTime;
            if ((velocity.x + velChange) * direction <= 0)
            {
                rb.velocity = new Vector2(0, velocity.y);
            }
            else
            {
                rb.AddForce(new Vector2(fricForce, 0), ForceMode2D.Force);
            }
        }

        if (jumpPressed)
        {
            rb.velocity = new Vector2(velocity.x, jumpForce);
            jumpPressed = false;
        }
    }

}
