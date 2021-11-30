using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flyingEnemyBehaviour : MonoBehaviour
{
    Vector3 originalPos, newPos;
    public float maxDistanceX, maxDistanceY, threshold;
    public float speed, frequency, magnitude;

    Collider2D[] objects; 
    Transform target;

    [Header("Atack Variables")]
    public float atackTime;
    public GameObject ProjectilePrefab;
    bool canAtack = true;
    

    void Start()
    {
        originalPos = transform.position;
        Patrol();
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfTarget();

        if (transform.position.x < newPos.x + threshold && transform.position.x > newPos.x - threshold)
        {
            if (transform.position.y < newPos.y + threshold && transform.position.y > newPos.y - threshold)
            {
                Patrol();
            }
        }

        transform.position = Vector3.Lerp(transform.position, newPos, speed * Time.deltaTime) + transform.up * Mathf.Sin(Time.time * frequency) * magnitude;

        Atack();
    }

    void Patrol()
    {
        if (target == null)
        {
            float X = Random.Range(originalPos.x - maxDistanceX, originalPos.x + maxDistanceX);
            float Y = Random.Range(originalPos.y - maxDistanceY, originalPos.y + maxDistanceY);
            newPos = new Vector3(X, Y, originalPos.z);
        }
        else
        {
            float X = Random.Range(target.transform.position.x - maxDistanceX, target.transform.position.x + maxDistanceX);
            float Y = Random.Range(target.transform.position.y + 4, target.transform.position.y + 7);
            newPos = new Vector3(X, Y, originalPos.z);
        }
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
            if (Vector3.Distance(transform.position, target.position) > 30)
            {
                target = null;
                Patrol();
            }
        }
    }

    void Atack()
    {
        if (target != null && canAtack)
        {
            Instantiate(ProjectilePrefab, transform.position, Quaternion.identity, transform.parent = null);
        }

        projectileBehaviour[] projectile = FindObjectsOfType<projectileBehaviour>();
        canAtack = projectile.Length == 0;
    }
}