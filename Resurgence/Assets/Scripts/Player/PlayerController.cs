﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum State {                                   //easy way to deal with cutscenes, ability charging, etc...
        Ready,
        Busy,
    }

    public Animator animator;
    float prevY, currY;
    public State abilityState;

    /*movement controls */
    public float speed;
    public float jumpForce;
    private float moveInput;

    /*auxillary stuff */
    private Rigidbody2D rb;

    [HideInInspector]
    public bool facingRight = true;
    public bool canMove = true;

    /*jump object references and variables */
    private bool isGrounded;        //is the player on the ground?
    public Transform groundCheck;   //to check if plyer is on the ground
    public float checkRadius;       //leeway for floor
    public LayerMask whatIsGround;  //to choose what layer is considered ground
    public LayerMask whatIsObject;  //ditta but for the objects

    /*single jump flag */
    // private bool jumped = false;

    Camera cam;
    private Vector2 screenBounds;
    private float playerWidth, playerHeight;
    Vector3 viewPos;

    void Awake() {

    }

    //on start, gets the rigidbody of the player
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        abilityState = State.Ready;                   //making sure the player starts ready
        cam = Camera.main;
        screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z));
        playerWidth = transform.GetComponent<SpriteRenderer>().bounds.size.x;
        currY = this.transform.position.y;
    }

    void FixedUpdate() {
        if (abilityState == State.Busy)
        {
            if (Input.GetButtonUp("Transpose")) {
                abilityState = State.Ready;
            } else {
                return;       //disabling everything else
            }
        }

        //considered objects ground to jump off of
        isGrounded = (Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround))
                  || (Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsObject));

        // moveInput = Input.GetAxis("I_joystick");
        // Debug.LogError(Input.GetAxis("I_joystick"));

        if (Input.GetAxis("I_joystick") < -0.3 || Input.GetAxis("I_joystick") > 0.3) {
            moveInput = Input.GetAxis("I_joystick");
            Debug.LogError(Input.GetAxis("I_joystick"));
        } else {
            moveInput = Input.GetAxis("I_Horizontal");
        }
// #if UNITY_EDITOR
//         moveInput = Input.GetAxis("Horizontal");
// #endif
        //Debug.Log(moveInput);
        if (canMove) {
            //rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            rb.AddForce(new Vector2(moveInput * speed, rb.velocity.y));
        }

        if (((!facingRight && moveInput > 0) || (facingRight && moveInput < 0)) && canMove) flip();

        if (transform.position.x <= -screenBounds.x+playerWidth) {
            transform.position = new Vector2(-screenBounds.x+playerWidth, transform.position.y);
        } else if (transform.position.x >= screenBounds.x-playerWidth) {
            transform.position = new Vector2(screenBounds.x-playerWidth, transform.position.y);
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
         if (col.gameObject.tag == "Laser") {
            GameManager.Instance.ResetScene();
            return;
         }
    }

    //just for jumping for now
    void Update() {
        // animator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("I_Horizontal")*34f));
        animator.SetFloat("Speed", Mathf.Abs(moveInput * 34f));

        prevY = currY;
        currY = this.transform.position.y;

        // if (isGrounded) jumped = false;
        if (abilityState == State.Busy) return;       //disabling everything else

        if ((Input.GetButtonDown("I_Jump") || Input.GetKeyDown(KeyCode.W)) && isGrounded) {
            jump();
        }

        if (!isGrounded && currY > prevY) {
            animator.SetBool("IsJumping", true);
        } else {
            animator.SetBool("IsJumping", false);
        }
    }

    void jump() {
        if (canMove)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    //flipping the character direction
    private void flip() {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    //freezes player position
    public void freeze() {
        rb.bodyType = RigidbodyType2D.Static;
    }

    //unfreezes player position
    public void unfreeze() {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public bool getIsGrounded()
    {
        return isGrounded;
    }
}
