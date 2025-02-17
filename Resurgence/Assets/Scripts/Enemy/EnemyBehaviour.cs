using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public string state = "patrol";

    private float speed, savedSpeed;

    bool blocked = false, squishLeft = false, squishRight = false;

    public int health = 1;

    public float prevX, currX;

    public Sprite enragedSprite, patrolSprite;

    public Animator animator;
    public float deathDelay = 0f;
    bool dontLoop = true;
    private Vector3 facing;

    void Start()
    {
        state = "patrol";
        speed = GetComponent<EnemyMovement>().speed;
        savedSpeed = speed;
        patrolSprite = GetComponent<SpriteRenderer>().sprite;

        facing = this.transform.right;
    }

    void Update() 
    {
        // if godot is ever moon walking, flip the sprite
        if (currX > prevX && this.transform.localScale.x < 0) {
            this.GetComponent<SpriteRenderer>().flipX = true;
        } else if (currX < prevX && this.transform.localScale.x > 0) {
            this.GetComponent<SpriteRenderer>().flipX = true;
        }

        if (squishLeft && squishRight) health--;
        death();
    }

    void death()
    {
        if (health == 0) {
            this.GetComponent<SpriteRenderer>().sortingOrder = 100;
            this.GetComponent<Rigidbody2D>().simulated = false;
            animator.SetInteger("Health", 0);
            if (dontLoop){
                FindObjectOfType<AudioManager>().Play("GodotDeath");
                dontLoop = false;
            }
        }
    }

    public string getState()
    {
        return state;
    }

    // state should only equal "patrol" or "enraged"
    public void setState(string input)
    {
        state = input;
    }

    public void ChaseTarget(Transform target)
    {
        animator.SetBool("Engaged", true);
        state = "enraged";
        // GetComponent<SpriteRenderer>().sprite = enragedSprite;
        // StartCoroutine(Delay(1));
        StartCoroutine(Chase(target));
    }

    public void StopChase(Transform target)
    {
        animator.SetBool("Engaged", false);
        state = "patrol";
        // GetComponent<SpriteRenderer>().sprite = patrolSprite;
        // StartCoroutine(Delay(1));
        StopCoroutine(Chase(target));
        GetComponent<EnemyMovement>().StartCoroutine("Patrol");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col != null && (col.gameObject.layer == 9)) {
            blocked = true;
        }

        if (col.gameObject.tag == "Fissurable")
        {
            if (col.gameObject.name == "PlatformLeft")
            {
                squishLeft = true;
            }
            else if (col.gameObject.name == "PlatformRight")
            {
                squishRight = true;
            }
        }
        
        if (col != null && (col.gameObject.layer == 11 || col.gameObject.layer == 10)) {
            GameManager.Instance.ResetScene();
            //GetComponent<EnemyMovement>().StartCoroutine("Patrol");
        } 
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col != null && col.gameObject.name == "solid" && col.gameObject.transform.parent.GetComponentInChildren<BuoyancyEffector2D>().density > 0) {
            if (state == "patrol") GetComponent<EnemyMovement>().StopCoroutine("Patrol");
            if (state == "enraged") StopCoroutine("Chase");
        } 
    }

    void OnCollisionExit2D(Collision2D col) {
        if (col != null && (col.gameObject.layer == 9)) {
            blocked = false;
        }

        if (col.gameObject.tag == "Fissurable")
        {
            if (col.gameObject.name == "PlatformLeft")
            {
                squishLeft = false;
            }
            else if (col.gameObject.name == "PlatformRight")
            {
                squishRight = false;
            }
        }

        if (col != null && col.gameObject.tag == "solid" && col.gameObject.transform.parent.GetComponentInChildren<BuoyancyEffector2D>().density == 0) {
            if (state == "patrol") GetComponent<EnemyMovement>().StartCoroutine("Patrol");
            if (state == "enraged") StartCoroutine("Chase");
        }
    }

    IEnumerator Chase(Transform target)
    {
        yield return new WaitForSeconds(0.5f);
        while (state == "enraged") {
            if (!blocked) {
                prevX = GetComponent<EnemyMovement>().getPrevX();
                currX = GetComponent<EnemyMovement>().getCurrX();
                if (health > 0) this.transform.position = Vector2.MoveTowards(this.transform.position, new Vector2(target.position.x, transform.position.y), (speed*Time.deltaTime)/0.5f);
                if (target.position.x > this.transform.position.x && prevX > currX) {
                    Vector2 newScale = this.transform.localScale;
                    newScale.x *= -1;
                    this.transform.localScale = newScale;
                } else if (target.position.x < this.transform.position.x && prevX < currX) {
                    Vector2 newScale = this.transform.localScale;
                    newScale.x *= -1;
                    this.transform.localScale = newScale;
                }
            }
            yield return null;
        }
    }

    // IEnumerator Delay(float sec)
    // {
    //     yield return new WaitForSeconds(sec);
    // }
}
