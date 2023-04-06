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

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        rb.AddForce(new Vector2(horizontal * accel,0), ForceMode2D.Force);
        
        if (Mathf.Abs(rb.velocity.x) >= maxSpeed)
        {
            Vector2 velocity = rb.velocity;
            float direction = velocity.x / Mathf.Abs(velocity.x);
            rb.velocity = new Vector2(direction * maxSpeed, velocity.y);
            rb.AddForce(new Vector2(-direction * accel * Mathf.Abs(horizontal),0), ForceMode2D.Force);
            /*
            if (rb.velocity.x < 1)
            {
                rb.velocity = new Vector2(-maxSpeed, rb.velocity.y);
                rb.AddForce(new Vector2(accel * Mathf.Abs(horizontal),0), ForceMode2D.Force);
            }else if (rb.velocity.x > 1)
            {
                rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
                rb.AddForce(new Vector2(-accel * Mathf.Abs(horizontal),0), ForceMode2D.Force);
            }
            */
        }
    }
}
