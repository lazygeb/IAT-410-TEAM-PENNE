using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySight : MonoBehaviour
{
    SpriteRenderer spr;
    public bool objectInRange;
    Transform lineOfSightEnd;
    Transform itztli, tlaloc, catalyst;

    Transform target;
    //, tlaloc, catalyst, target;
    Transform seenTarget;

    void Start()
    {
        objectInRange = false;
        lineOfSightEnd = this.transform.GetChild(0).transform;
        // objects that trigger the enemy
        itztli = GameObject.FindWithTag("Itztli").transform;
        tlaloc = GameObject.FindWithTag("Tlaloc").transform;
        catalyst = GameObject.FindWithTag("Catalyst").transform;
    }

    void FixedUpdate()
    {
        if (GetComponentInParent<EnemyBehaviour>().health > 0) {
            if (CanObjectBeSeen()) {
                // Debug.LogError(GetComponentInParent<EnemyBehaviour>().getState());
                if (GetComponentInParent<EnemyBehaviour>().getState() == "patrol") StopCoroutine("Patrol");
                GetComponentInParent<EnemyBehaviour>().ChaseTarget(seenTarget);
            } else {
                if (GetComponentInParent<EnemyBehaviour>().getState() == "enraged") {
                    GetComponentInParent<EnemyBehaviour>().StopChase(seenTarget);
                }
            }
        }
    }

    bool CanObjectBeSeen()
    {
        if (objectInRange) {
            if (ObjectInFOV()) {
                if (!ObjectHiddenByObstacles()) {
                    return true;
                }
            } 
        } 
        return false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Itztli" || col.transform.tag == "Tlaloc" || col.transform.tag == "Catalyst") {
            target = col.transform;
            objectInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Itztli" || col.transform.tag == "Tlaloc" || col.transform.tag == "Catalyst") {
            objectInRange = false;
        }
    }

    bool ObjectInFOV()
    {
        // direction from enemy to target
        Vector2 directionToTarget = target.position - transform.position;
        Debug.DrawLine(transform.position, target.position, Color.magenta);

        // the centre of the enemy's field of view, the direction of looking directly ahead
        Vector2 lineOfSight = lineOfSightEnd.position - transform.position;
        Debug.DrawLine(transform.position, lineOfSightEnd.position, Color.yellow);

        // angle between target position and enemy's centre fov
        float angle = Vector2.Angle(directionToTarget, lineOfSight);

        if (angle < 60f) {
            return true;
        }

        return false;
    }

    bool ObjectHiddenByObstacles()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, target.position - transform.position, distanceToTarget);
        Debug.DrawRay(transform.position, target.position - transform.position, Color.blue);
        foreach(RaycastHit2D hit in hits) {
            if (hit.transform.tag == "Godot") {
                continue;
            }

            if (hit.transform.CompareTag("Itztli") || hit.transform.CompareTag("Tlaloc") || hit.transform.tag == "Catalyst") {
                seenTarget = hit.transform;
                return false;
            } else {
                return true;
            }
        }
        return true;
    }
}
