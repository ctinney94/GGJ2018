﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    public Sprite[] m_sprites;
    int frame;
    
    private int lives = 3;
    public float maxHorizVelocity = 1.0f;
    public bool wallJumping = false;
    private float wallJumpTimer = 0.0f;
    public int totalJumps = 2;
    [SerializeField]
    private int jumpsRemaining = 2;
    public Rigidbody2D body;
    Vector2 jumpForce = new Vector2(0, 500);
    BoxCollider2D collider;
    Vector2 rayPos;
    bool dj = true;
    SpriteRenderer spRenderer;
    private void Awake()
    {
        instance=this;
        jumpsRemaining = totalJumps;
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        rayPos = new Vector2(transform.position.x, collider.bounds.min.y);
        spRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {  
        rayPos = new Vector2(transform.position.x, collider.bounds.min.y);
        if (wallJumping) {
            wallJumpTimer += Time.deltaTime;
            if (wallJumpTimer >= 0.1f) {
                wallJumping = false;
            }
        }
        else {
            if (Controller.LX() != 0) {
                body.velocity += 3 * Time.deltaTime * new Vector2(25*Controller.LX(), 0);
            }
            else {
                body.velocity -= 5 * Time.deltaTime * new Vector2(body.velocity.x, 0);
            }
            if (Math.Abs(body.velocity.x) > maxHorizVelocity) {
                body.velocity = new Vector2(Mathf.Clamp(body.velocity.x, -maxHorizVelocity, maxHorizVelocity) , body.velocity.y);
            }
            if((Controller.Button()&ControllerButton.CIRCLE)!=0&&Controller.LX()!=0)
            {
                body.AddForce(new Vector2(800* Controller.LX(), 0));
            }
            spRenderer.flipX = body.velocity.x < 0;
        }
        if (OnGround())
        {
            if (Mathf.Abs(body.velocity.x)>.5f)
            {
                spRenderer.sprite=m_sprites[Mathf.RoundToInt(frame/20)];
                frame = frame<80 ? frame+1 : 0;
            }
            else
            {
                spRenderer.sprite=m_sprites[5+Mathf.RoundToInt(Time.time%1)];
            }
        }
        else if (OnWall() && !slamming)
        {
            if (Controller.LX() > 0.25f && !spRenderer.flipX)
             spRenderer.sprite=m_sprites[7];
            else if (Controller.LX() < -0.25f && spRenderer.flipX)
             spRenderer.sprite=m_sprites[7];
            else
            spRenderer.sprite=m_sprites[4];
        }
            else
            spRenderer.sprite=m_sprites[4];
        Jump();
    }

    IEnumerator Slide()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 15 * (spRenderer.flipX ? -1 : 1)));
        body.AddForce(new Vector2(30 * (spRenderer.flipX ? -1 : 1), 0));
        yield return new WaitForSeconds(0.2f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 15 * (spRenderer.flipX ? 1 : -1)));
    }
    #region Jumping Shit
    bool OnGround()
    {
        Debug.DrawLine(rayPos,rayPos+(Vector2.down*(0.05f+Mathf.Abs(body.velocity.y/100))),Color.red,0.1f);
        if (Physics2D.Raycast(rayPos, Vector2.down, 0.05f+Mathf.Abs(body.velocity.y/100)))
        {
            return true;
        }
        return false;
    }

    bool OnWall()
    {
        Vector3 vec=new Vector3(spRenderer.flipX ? collider.bounds.min.x : collider.bounds.max.x, transform.position.y,0);
        Debug.DrawLine(vec,vec+(spRenderer.flipX ? -transform.right*0.05f: transform.right*0.05f),Color.green,0.1f);
        if (Physics2D.Raycast(vec, spRenderer.flipX ? -transform.right : transform.right, 0.05f)) 
        {
             return true;
        }
        return false;
    }

    //For breaking boxes
    public float lastyVel=0;
    void LateUpdate()
    {
        lastyVel=body.velocity.y;
    }

    void Jump()
    {
        if (OnGround())
        {
            slamming=false;
            jumpsRemaining = totalJumps;
            frame=0;
        }
        if ((Controller.Button() & ControllerButton.DOWN) != 0 && jumpsRemaining != totalJumps)
            {
                if ((Controller.ButtonDown() & ControllerButton.SQUARE) != 0)
             {   StartCoroutine(SlamPause(dj));
                dj = true;
            }
            }
        if ((Controller.ButtonDown() & ControllerButton.CROSS) != 0)
        {      
            if ((Controller.Button() & (spRenderer.flipX ? ControllerButton.LEFT : ControllerButton.RIGHT)) != 0)
            {
                if (spRenderer.flipX)
                {
                    if (Physics2D.Raycast(new Vector2(collider.bounds.min.x, transform.position.y), -transform.right, 0.1f))
                    {
                        body.AddForce(jumpForce + new Vector2(400, 0));
                        wallJumping = true;
                        wallJumpTimer = 0.0f;
                        jumpsRemaining = 1;
                    }
                }
                else
                {
                    if (Physics2D.Raycast(new Vector2(collider.bounds.max.x, transform.position.y), transform.right, 0.1f))
                    {
                        body.AddForce(jumpForce + new Vector2(-400, 0));
                        wallJumping = true;
                        wallJumpTimer = 0.0f;
                        jumpsRemaining = 1;
                    }
                }
            }
            if (!wallJumping) {
                if (jumpsRemaining != 0) {
                    body.AddForce(jumpForce);
                    jumpsRemaining--;
                }
            }
        }
    }

    public bool slamming=false;
    IEnumerator SlamPause(bool _dj)
    {
        slamming=true;
        body.velocity = Vector2.zero;
        body.simulated = false;
        float lerpy = 0;
        body.constraints =RigidbodyConstraints2D.None;
        while (lerpy<1)
        {
            lerpy += Time.deltaTime*4;
            transform.rotation = Quaternion.Euler(0, 0, lerpy* 360);
            yield return new WaitForEndOfFrame();
        }
        body.constraints =RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        body.simulated = true;
        body.AddForce(-jumpForce * 3);
    }
    public void Death()
    {
        Debug.Log("You died");
        lives--;
        //reset to point or what;
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D _col)
    {

    }
}
enum JumpState
{
    Once = 0,
    Twice = 1,
    Slam = 2,
    Nope = 3,    
}