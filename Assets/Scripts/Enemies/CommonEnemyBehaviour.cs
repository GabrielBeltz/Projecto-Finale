using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonEnemyBehaviour : MonoBehaviour
{
    public Animator myAnimator;
    public Transform target;
    Collider2D[] objects;

    public float sideWalkingDistance, speed, dashSpeed, dashDuration, timerToAttack;
    float initialTimerToAttack;
    Vector3 initialPos, leftLocation, rightLocation;
    public bool goingRight, isAtacking, isDashing;

    bool targetIsOnRight;

    void Start()
    {
        initialPos = transform.position;
        initialTimerToAttack = timerToAttack;
    }

    void Update()
    {
        float rotation;
        if (target == null)
        {
            myAnimator.SetTrigger("Walk");
            Patrol();

            if (goingRight)
            {
                rotation = 0;
            }
            else
            {
                rotation = 180;
            }
        }
        else
        {
            if (target.transform.position.x > transform.position.x)
            {
                targetIsOnRight = true;
            }
            else
            {
                targetIsOnRight = false;
            }

            if (targetIsOnRight)
            {
                rotation = 0;
            }
            else
            {
                rotation = 180;
            }

            myAnimator.SetTrigger("Attack");
            ChaseTarget();
        }


        this.transform.rotation = Quaternion.Euler(0, rotation, 0);

        CheckIfTarget();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 10);
    }

    void CheckIfTarget()
    {
        objects = Physics2D.OverlapCircleAll(transform.position, 10);
        foreach (Collider2D item in objects)
        {
            if (item.tag == "Player")
            {
                target = item.transform;
            }
        }

        if (target != null)
        {
            if (Vector3.Distance(transform.position, target.position) > 10)
            {
                target = null;
                initialPos = transform.position;
                Patrol();
            }
        }
    }

    void Patrol()
    {
        rightLocation = new Vector3(initialPos.x + sideWalkingDistance, transform.position.y, transform.position.z);
        leftLocation = new Vector3(initialPos.x - sideWalkingDistance, transform.position.y, transform.position.z);

        transform.position += transform.right * speed * Time.deltaTime;

        if (transform.position.x < leftLocation.x)
        {
            if (!goingRight)
            {
                goingRight = true;
            }
        }

        if (transform.position.x > rightLocation.x)
        {
            if (goingRight)
            {
                goingRight = false;
            }
        }
    }

    void ChaseTarget()
    {
        if (isAtacking)
        {
            Invoke("Attack", 0);
        }
        
        if (!isAtacking)
        {
            if (Vector3.Distance(transform.position, target.position) >= 5)
            {
                transform.position += transform.right * speed * 2 * Time.deltaTime;
            }
            else
            {
                isAtacking = true;
            }
        }
    }

    void Attack()
    {
        timerToAttack -= Time.deltaTime;
        if (timerToAttack <= 0)
        {
            transform.position += transform.right * dashSpeed * Time.deltaTime;
        }
    }

    IEnumerator DashtimerToAttack()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        isAtacking = false;
        timerToAttack = initialTimerToAttack;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Player")
        {
            CancelInvoke("Attack");
            isDashing = false;
            isAtacking = false;
            timerToAttack = initialTimerToAttack;
        }
    }


}
