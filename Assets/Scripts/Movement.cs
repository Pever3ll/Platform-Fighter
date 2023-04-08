using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float horizontal;
    private float vertical;
    public Rigidbody2D rb;
    public float accel;
    public float maxSpeed;
    private float tempMaxSpeed;
    public float accelConst;
    public float friction;

    // Update is called once per frame
    void Update()
    {
        //Gets Inputs
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
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
            //Debug.Log("Fricforce: " + fricForce);
            //Debug.Log("velChange: " + velChange);
            //Debug.Log("velocity: " + velocity.x);
            if ((velocity.x + velChange) * direction <= 0)
            {
                rb.velocity = new Vector2(0, velocity.y);
            }
            else
            {
                rb.AddForce(new Vector2(fricForce, 0), ForceMode2D.Force);
            }
            //Debug.Log("Future Vel: " + (velocity.x + velChange) * direction);
        }
    }
}
