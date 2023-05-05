using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    [SerializeField] private Collider2D col;

// Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Bounds colBounds = col.bounds;
        float checkExtra = 0.1f;
        Vector2 leftDown = colBounds.min;
        Vector2 rightDown = new Vector2(colBounds.min.x + colBounds.size.x, colBounds.min.y);
        if (false /*iisgrounded*/)
        {
            Vector2 leftUp = new Vector2(colBounds.max.x - colBounds.size.x, colBounds.max.y);
            Vector2 rightUp = colBounds.max;
        }

        bool leftDownTouching;

        Collider2D colLeftDown = Physics2D.OverlapPoint(leftDown);
        Collider2D colRightDown = Physics2D.OverlapPoint(rightDown);
        RaycastHit2D hitLeftDown, hitRightDown;
        
        if (colLeftDown)
        {
            hitLeftDown = Physics2D.Raycast(new Vector2(colBounds.center.x - colBounds.extents.x, colBounds.center.y));
        }
        if (colRightDown)
        {
                
        }
    }
}
