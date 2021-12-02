using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonEnemyBehaviour : MonoBehaviour
{
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
        if (target == null)
        {
            Patrol();
        }
        else
        {
            ChaseTarget();
        }

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
            if (Vector3.Distance(transform.position, target.position) > 15)
            {
                if (!isAtacking)
                {
                    target = null;
                    initialPos = transform.position;
                    Patrol();
                }
            }
        }
    }

    void Patrol()
    {
        rightLocation = new Vector3(initialPos.x + sideWalkingDistance, transform.position.y, transform.position.z);
        leftLocation = new Vector3(initialPos.x - sideWalkingDistance, transform.position.y, transform.position.z);

        if (goingRight)
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }
        else
        {
            transform.position -= transform.right * speed * Time.deltaTime;
        }

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
        
        if (target.position.x < transform.position.x)
        {
            if (!isAtacking)
            {
                if (Vector3.Distance(transform.position, target.position) >= 5)
                {
                    transform.position -= transform.right * speed * 2 * Time.deltaTime;
                }
                else
                {
                    targetIsOnRight = false;
                    isAtacking = true;
                }
            }
        }
        else
        {
            if (!isAtacking)
            {
                if (Vector3.Distance(transform.position, target.position) >= 5)
                {
                    transform.position += transform.right * speed * 2 * Time.deltaTime;
                }
                else
                {
                    targetIsOnRight = true;
                    isAtacking = true;
                }
                
            }
        }
    }

    void Attack()
    {
        timerToAttack -= Time.deltaTime;
        if (timerToAttack <= 0)
        {
            isDashing = true;

                if (isDashing)
                {
                    StartCoroutine(DashtimerToAttack());
                    if (!targetIsOnRight)
                    {
                        transform.position -= transform.right * dashSpeed * Time.deltaTime;
                    }
                    else
                    {
                        transform.position += transform.right * dashSpeed * Time.deltaTime;
                    }
                }
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
