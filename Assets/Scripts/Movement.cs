using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public float horizontal;
    
    [SerializeField]
    private new BoxCollider2D collider;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float accel;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float accelConst;
    [SerializeField]
    private float friction;
    [SerializeField]
    private float jumpForce;
    
    private float tempMaxSpeed;
    private bool isGrounded;
    private float groundSlope;
    private bool jumpPressed = false;
    private bool hasJumped;
    private bool wasGrounded;
    private float prevGroundSlope;


    // Update is called once per frame
    void Update()
    {
        //Gets Inputs
        horizontal = Input.GetAxis("Horizontal");
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
        
        //Get Vel dir
        Vector2 velocity = rb.velocity;
        float direction = velocity.x / Mathf.Abs(velocity.x);
        if (direction != direction) direction = 0;
        
        Bounds bounds = collider.bounds;
        Vector2 locLeft = bounds.min;
        Vector2 locRight = new Vector2(bounds.max.x, bounds.min.y);
        RaycastHit2D rayLeft = Physics2D.Raycast(locLeft, Vector2.down, 0.05f);
        RaycastHit2D rayRight = Physics2D.Raycast(locRight, Vector2.down, 0.05f);
        Debug.DrawRay(locRight, Vector2.down * 0.05f, Color.blue, Time.deltaTime);
        Debug.DrawRay(locLeft, Vector2.down * 0.05f, Color.blue, Time.deltaTime);
        
        if (rayLeft.collider && rayRight.collider)
        {
            isGrounded = true;
            groundSlope = 0;
        }
        else if (rayLeft.collider)
        {
            groundSlope = -Mathf.Atan2(rayLeft.normal.x, rayLeft.normal.y)*Mathf.Rad2Deg;
            groundSlope = groundSlope < 0 ? groundSlope : 0;
            isGrounded = true;
        }
        else if (rayRight.collider)
        {
            groundSlope = -Mathf.Atan2(rayRight.normal.x, rayRight.normal.y)*Mathf.Rad2Deg;
            groundSlope = groundSlope > 0 ? groundSlope : 0;
            isGrounded = true;
        }
        else
        {
            groundSlope = 0;
            Vector2 down = new Vector2(0, 0.05f);
            RaycastHit2D ray3 = Physics2D.Linecast(locLeft - down, locRight - down);
            Debug.DrawLine(locLeft - down, locRight - down, Color.blue, Time.deltaTime);
            if (ray3.collider)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }
        // Debug.Log((Vector2)(Quaternion.Euler(0,0,groundSlope)*Vector2.right));

        if (groundSlope < 0)
        {
            ;
        }
        
        //Run off force to stick player to ground
        if (!isGrounded && wasGrounded && !hasJumped && (prevGroundSlope * direction <= 0))
        {
            var loc = (Vector2)bounds.center - new Vector2(bounds.extents.x * direction, bounds.extents.y);
            var hit = Physics2D.Raycast(loc, Vector2.down, 0.3f);
            Debug.DrawRay(loc, Vector2.down * 0.3f, Color.red, 0.6f);
            if (hit.collider)
            {
                groundSlope = -Mathf.Atan2(hit.normal.x, hit.normal.y) * Mathf.Rad2Deg;
                rb.position -= (loc - hit.centroid);
                Debug.DrawRay(hit.centroid, hit.normal, Color.cyan, 1);
                isGrounded = true;
                Vector2 dirVec = -Vector2.Perpendicular(hit.normal);
                rb.velocity = dirVec * rb.velocity.x;
            }
        }
        
        //Get Slope vector
        /*Vector2 slopeVector = groundSlope == 0 ? Vector2.right
            : -Vector2.Perpendicular(rayLeft.collider ? rayLeft.normal : rayRight.normal);*/
        Vector2 slopeVector = (Vector2)(Quaternion.Euler(0, 0, groundSlope) * Vector2.right);

        //Horizontal Movement
        // rb.AddForce(new Vector2(horizontal * accel * accelConst,0), ForceMode2D.Force);
        rb.AddForce(slopeVector * (horizontal * accel * accelConst), ForceMode2D.Force);
        //Checks for horizontal speed
        if (groundSlope == 0)
        {
            
            if (prevGroundSlope != 0 && !hasJumped)
            {
                rb.velocity = new Vector2(direction * rb.velocity.magnitude, 0);
            }
            if (Mathf.Abs(rb.velocity.x) >= tempMaxSpeed)
            {
                //Sets to terminal velocity
                rb.velocity = new Vector2(direction * tempMaxSpeed, rb.velocity.y);
                rb.AddForce(new Vector2(-direction * accel * accelConst * Mathf.Abs(horizontal), 0), ForceMode2D.Force);
            }
        }
        else
        {
            if (rb.velocity.normalized * direction != slopeVector)
            {
                rb.velocity = slopeVector * rb.velocity.magnitude;
            }
            if (rb.velocity.magnitude >= tempMaxSpeed)
            {
                //Sets to terminal velocity
                rb.velocity = slopeVector * (tempMaxSpeed * direction);
                // rb.AddForce(new Vector2(-direction * accel * accelConst * Mathf.Abs(horizontal),0), ForceMode2D.Force);
                rb.AddForce(slopeVector * (-direction * accel * accelConst * Mathf.Abs(horizontal)), ForceMode2D.Force);
            }
            //rb.AddForce(slopeVector * (-Physics2D.gravity.y * rb.gravityScale * Mathf.Sin(groundSlope*Mathf.Deg2Rad)));
            rb.AddForce(-Physics2D.gravity * rb.gravityScale);
        }
        
        
        //Friction
        if (horizontal == 0 && rb.velocity.x != 0)
        {
            if (isGrounded)
            {
                Vector2 fricForce = slopeVector * (-direction * accelConst * friction);
                Vector2 futureVel = (rb.velocity + fricForce * Time.deltaTime) * direction;
                if (futureVel.x <= 0 || futureVel.y < 0)
                {
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    rb.AddForce(fricForce, ForceMode2D.Force);
                }
                
                /*float fricForce = -direction * accelConst * friction;
                float velChange = fricForce * Time.deltaTime;
                if ((velocity.x + velChange) * direction <= 0)
                {
                    rb.velocity = new Vector2(0, velocity.y);
                }
                else
                {
                    rb.AddForce(new Vector2(fricForce, 0), ForceMode2D.Force);
                }*/
            }
        }

        hasJumped = jumpPressed;
        
        if (jumpPressed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            groundSlope = 0;
            jumpPressed = false;
        }

        wasGrounded = isGrounded;
        prevGroundSlope = groundSlope;
    }

}
