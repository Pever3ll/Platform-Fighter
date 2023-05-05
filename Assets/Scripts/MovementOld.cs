using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementOld : MonoBehaviour
{
    private float _horizontal;
    private float _tempMaxSpeed;
    private bool _isGrounded;
    private float _groundSlope;
    private bool _jumpPressed = false;
    private bool _hasJumped;
    private bool _wasGrounded;
    private float _prevGroundSlope;
    
    [SerializeField] private new BoxCollider2D collider;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float accel;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelConst;
    [SerializeField] private float friction;
    [SerializeField] private float jumpForce;

    // Update is called once per frame
    void Update()
    {
        //Gets Inputs
        _horizontal = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpPressed = true;
        }
        //jumpPressed = Input.GetKeyDown(KeyCode.Space);
    }

    void FixedUpdate()
    {
        //Sets Max velocity as joystick dependent
        if (_horizontal != 0)
        {
            _tempMaxSpeed = maxSpeed * Mathf.Abs(_horizontal);
        }
        else
        {
            _tempMaxSpeed = maxSpeed;
        }
        
        // Debug.Log(Physics2D.Raycast(new Vector2(0, -1), Vector2.up, 2).normal);
        // Debug.DrawRay(new Vector2(0, -1), Vector2.up * 2, Color.blue, Time.deltaTime);
        // Debug.DrawRay(Physics2D.Raycast(new Vector2(0, -1), Vector2.up, 2).point, Vector3.left, Color.green, Time.deltaTime);
        
        
        //Get Vel dir
        Vector2 velocity = rb.velocity;
        float direction = velocity.x / Mathf.Abs(velocity.x);
        if (float.IsNaN(direction)) direction = 0;
        
        // Fires two rays at bottom corners
        float checkDist = 0.05f;
        Bounds bounds = collider.bounds;
        Vector2 locLeft = bounds.min;
        Vector2 locRight = new Vector2(bounds.max.x, bounds.min.y);
        RaycastHit2D rayLeft = Physics2D.Raycast(locLeft, Vector2.down, checkDist);
        RaycastHit2D rayRight = Physics2D.Raycast(locRight, Vector2.down, checkDist);
        Debug.DrawRay(locRight, Vector2.down * checkDist, Color.blue, Time.deltaTime, false);
        Debug.DrawRay(locLeft, Vector2.down * checkDist, Color.blue, Time.deltaTime, false);
        //if both rays hit then slope = 0
        if (rayLeft.collider && rayRight.collider)
        {
            _isGrounded = true;
            _groundSlope = 0;
            Debug.Log("Down: " + rayLeft.distance);
            Debug.Log("Up: " + (bool)Physics2D.Raycast(locLeft - new Vector2(0, rayLeft.distance - 0.0000001f), Vector2.up, checkDist).collider);
            Debug.DrawRay(locLeft - new Vector2(0, rayLeft.distance - 0.0000001f), Vector2.up * checkDist , Color.cyan, Time.deltaTime, false);
            Debug.Log("Touch: " + rayLeft.collider.OverlapPoint(rayLeft.point - new Vector2(0, 0.0000001f)));
        }//if one ray is hit, then it takes the angle of that point
        else if (rayLeft.collider)
        {
            _groundSlope = -Mathf.Atan2(rayLeft.normal.x, rayLeft.normal.y)*Mathf.Rad2Deg;
            _groundSlope = _groundSlope < 0 ? _groundSlope : 0;
            _isGrounded = true;
        }
        else if (rayRight.collider)
        {
            _groundSlope = -Mathf.Atan2(rayRight.normal.x, rayRight.normal.y)*Mathf.Rad2Deg;
            _groundSlope = _groundSlope > 0 ? _groundSlope : 0;
            _isGrounded = true;
            Debug.Log(rayRight.collider.GetShapeHash());
        }//if none are hit, linecast to check if there is ground underneath
        else
        {
            _groundSlope = 0;
            Vector2 down = new Vector2(0, checkDist);
            RaycastHit2D ray3 = Physics2D.Linecast(locLeft - down, locRight - down);
            Debug.DrawLine(locLeft - down, locRight - down, Color.blue, Time.deltaTime);
            if (ray3.collider)
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }
        }
        // Debug.Log((Vector2)(Quaternion.Euler(0,0,groundSlope)*Vector2.right));

        
        //Run off force to stick player to ground
        if (!_isGrounded && _wasGrounded && !_hasJumped && (_prevGroundSlope * direction <= 0))
        {
            var loc = (Vector2)bounds.center - new Vector2(bounds.extents.x * direction, bounds.extents.y);
            var hit = Physics2D.Raycast(loc, Vector2.down, 0.3f);
            Debug.DrawRay(loc, Vector2.down * 0.3f, Color.red, 0.6f);
            if (hit.collider)
            {
                _groundSlope = -Mathf.Atan2(hit.normal.x, hit.normal.y) * Mathf.Rad2Deg;
                rb.position -= (loc - hit.point);
                Debug.DrawRay(hit.point, hit.normal, Color.cyan, 1);
                _isGrounded = true;
                Vector2 dirVec = -Vector2.Perpendicular(hit.normal);
                rb.velocity = dirVec * rb.velocity.x;
            }
        }
        
        //Get Slope vector
        /*Vector2 slopeVector = groundSlope == 0 ? Vector2.right
            : -Vector2.Perpendicular(rayLeft.collider ? rayLeft.normal : rayRight.normal);*/
        Vector2 slopeVector = (Vector2)(Quaternion.Euler(0, 0, _groundSlope) * Vector2.right);

        //Horizontal Movement
        // rb.AddForce(new Vector2(horizontal * accel * accelConst,0), ForceMode2D.Force);
        rb.AddForce(slopeVector * (_horizontal * accel * accelConst), ForceMode2D.Force);
        //Checks for horizontal speed
        if (_groundSlope == 0)
        {
            
            if (_prevGroundSlope != 0 && !_hasJumped)
            {
                rb.velocity = new Vector2(direction * rb.velocity.magnitude, 0);
                //isGrounded = true;
            }
            if (Mathf.Abs(rb.velocity.x) >= _tempMaxSpeed)
            {
                //Sets to terminal velocity
                rb.velocity = new Vector2(direction * _tempMaxSpeed, rb.velocity.y);
                rb.AddForce(new Vector2(-direction * accel * accelConst * Mathf.Abs(_horizontal), 0), ForceMode2D.Force);
            }
        }
        else
        {
            if (_prevGroundSlope != _groundSlope && _wasGrounded && rb.velocity.normalized * direction != slopeVector)
            {
                //rb.velocity = slopeVector * (direction * rb.velocity.magnitude);
            }
            if (rb.velocity.magnitude >= _tempMaxSpeed)
            {
                //Sets to terminal velocity
                rb.velocity = slopeVector * (_tempMaxSpeed * direction);
                // rb.AddForce(new Vector2(-direction * accel * accelConst * Mathf.Abs(horizontal),0), ForceMode2D.Force);
                rb.AddForce(slopeVector * (-direction * accel * accelConst * Mathf.Abs(_horizontal)), ForceMode2D.Force);
            }
            //rb.AddForce(slopeVector * (-Physics2D.gravity.y * rb.gravityScale * Mathf.Sin(_groundSlope*Mathf.Deg2Rad)));
            rb.AddForce(-Physics2D.gravity * rb.gravityScale);
        }
        
        
        //Friction
        if (_horizontal == 0 && rb.velocity.x != 0)
        {
            if (_isGrounded)
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

        _hasJumped = _jumpPressed;
        
        if (_jumpPressed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            _groundSlope = 0;
            _jumpPressed = false;
        }

        _wasGrounded = _isGrounded;
        _prevGroundSlope = _groundSlope;
    }

}
