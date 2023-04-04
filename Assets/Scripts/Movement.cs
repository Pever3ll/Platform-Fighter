using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float horizontal;
    private float vertical;
    public Rigidbody2D rb;

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        //rb.AddForce(new Vector2(horizontal, 0) * Time.deltaTime * 100, ForceMode2D.Force);
        rb.AddForce(new Vector2(1,0), ForceMode2D.Force);
        //Debug.Log(Time.deltaTime);
    }
}
