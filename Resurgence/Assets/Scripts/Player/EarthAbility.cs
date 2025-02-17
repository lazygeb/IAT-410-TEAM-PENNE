﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthAbility : MonoBehaviour
{
    public Animator animator;
    /*Auxillary stuff */
    public PlayerController pc;                           //reference to the player
    public bool canTranspose = false, canFissure = false; //ability flags
    private bool abilityLock = false;
    GameObject switchObject = null;
    Collider2D transposeObj = null;


    /*transpose skill variables */
    public Vector2 pushForce = new Vector2(0, 0);         //the reference vector2 for the transpose force
    public float charge = 0;                              //the temporary variable to charge transpose
    public float minCharge;                               //minimum force the player starts charging
    public float maxCharge;                               //maximum force the player can charge

    private IEnumerator transposeCoroutine;               //coroutine reference to get the colider

    /*fissure variables */
    private IEnumerator fissureCoroutine;               //coroutine reference to get the two objects
    private GameObject fissureWall;                     //to test if the wall is fissureable

    void Start()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void Update() {
        // //for consistency and error handling
        // if (!Input.GetButton("Transpose"))
        // {
        //     // pc.unfreeze();
        //     pc.abilityState = PlayerController.State.Ready;
        // }
        // if (pc.abilityState == PlayerController.State.Busy) return;

        //better transpose
        if (Input.GetButtonDown("Transpose")) {
            animator.SetTrigger("RockAbility");
            // if (canTranspose && transposeCoroutine != null)
            if (canTranspose)
            {
                canTranspose = false;
                transpose();
                // pc.abilityState = PlayerController.State.Busy;

                // StartCoroutine(transposeCoroutine); //begins to charge if not on cooldown

            }
        }

        //fissure
        if (Input.GetButtonDown("Fissure")) {
            animator.SetTrigger("RockAbility");
            if (canFissure) {
                if (fissureWall != null) {
                    Destroy(fissureWall);
                    fissureWall = null;
                    canFissure = false;
                } else if (fissureCoroutine != null) {
                    FindObjectOfType<AudioManager>().Play("Fissure");
                    StartCoroutine(fissureCoroutine);
                    Debug.Log("fissure coroutine started");
                }
            }
        }

        if (Input.GetButtonDown("iSwitch")) {
            if (switchObject != null) {
                switchObject.GetComponent<SwitchBehaviour>().toggleState();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        //aux stuff
        if (col.gameObject.tag == "Current") {
            col.GetComponent<LineBehaviour>().toggleConnected();
        }

        if (abilityLock) return;


        if (col != null && col.gameObject.tag == "BinarySwitch") {
            switchObject = col.gameObject;
        }


        //transpose
        if (col.gameObject.tag == "Object") {
            transposeObj = col;
            canTranspose = true;
            // charge = minCharge;        //reset the charge meter
            // transposeCoroutine = chargeTranspose(col);
        }
        
        //fissure
        if (col.gameObject.tag == "Fissurable") {
            canFissure = true;
            Debug.Log("check");
            fissureCoroutine = fissure(col.transform.parent.GetChild(0).GetComponent<Collider2D>(), 
                                       col.transform.parent.GetChild(1).GetComponent<Collider2D>()
                                       );
        } else if (col.gameObject.tag == "Cracked") {
            canFissure = true;
            fissureWall = col.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D col) 
    {
        if (col.gameObject.tag == "Current") {
            col.GetComponent<LineBehaviour>().resetCurrent();
            col.GetComponent<LineBehaviour>().toggleConnected();
        }

        if (abilityLock) return;

        //transpose
        if (col.gameObject.tag == "Object") {
            canTranspose = false;
        }


        //fissure
        if (col.gameObject.tag == "Fissurable" || col.gameObject.tag == "Cracked") {
            canFissure = false;
        }

        if (col != null && col.gameObject.tag == "BinarySwitch") {
            switchObject = null;
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col != null && col.gameObject.tag == "Current") {
            col.GetComponent<LineBehaviour>().setPosition(1, new Vector2(pc.transform.position.x, 0f));
        }
    }

    public void lockAbilities() {
        canFissure = false;
        canTranspose = false;
        abilityLock = true;
    }

    public void unlockAbilities() {
        canTranspose = false;
        canFissure = false;
        abilityLock = false;
    }

    public void transpose() {
        if (transposeObj== null) return;
        FindObjectOfType<AudioManager>().Play("Transpose-Short");
        pushForce = new Vector2(charge, charge);

        if ((pc.facingRight && pushForce.x < 0) || (!pc.facingRight && pushForce.x > 0)) {
            pushForce.x *= -1;
        }

        //Debug.LogError(pushForce);
        // if (col != null) 
        transposeObj.attachedRigidbody.AddForce(pushForce, ForceMode2D.Impulse);

        //camera shake
        StartCoroutine(CameraManager.Instance.cameraShake(.15f, .1f));
        transposeObj = null;
    }

    /*fissure ability */
    private IEnumerator fissure(Collider2D col1, Collider2D col2) {
        //camera shake
        StartCoroutine(CameraManager.Instance.cameraShake(3f, .07f));

        float elapsed = 0.0f;
        bool open;

        open = (col1.transform.localPosition.x == 0) ? true : false;

        while (elapsed < 3) { 
            float moveX = Mathf.Lerp(0, 3f, Time.deltaTime);

            if (open) {
                col1.transform.position += Vector3.left*moveX;
                col2.transform.position += Vector3.right*moveX;
            } else {
                col1.transform.position += Vector3.right * moveX;
                col2.transform.position += Vector3.left * moveX;
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (!open) {
            col1.transform.localPosition = Vector3.zero;
            col2.transform.localPosition = Vector3.zero;
        }
        yield return new WaitForEndOfFrame();
    }



    /* HELPER FUNCTIONS */


    /*expects a start time and end time to target to (for adaptability) */
    private IEnumerator timer(int time, int endTime) {
        while (time < endTime) {
            time++;
            yield return new WaitForSeconds(1);
        }

        canTranspose = true;
    }

    private void freezeObject(Collider2D col) {
        col.attachedRigidbody.bodyType = RigidbodyType2D.Static;
    }

    private void unfreezeObject(Collider2D col) {
        col.attachedRigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

}
