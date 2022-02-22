using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectileBehaviour : MonoBehaviour
{
    public GameObject target;
    public float speed, lifetime, timeToExplosion;

    bool explode;
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        Invoke("SelfDestruct", lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        if (!explode)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
        else
        {
            StartCoroutine(Explode());
        }

        if (Vector3.Distance(transform.position, target.transform.position) < 2)
        {
            explode = true;
        }
    }

    void SelfDestruct()
    {
        explode = true;
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(timeToExplosion);
        transform.localScale = new Vector3(3,3,1);

        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D item in objects)
        {
            if (item.tag == "Player")
            {
                GetComponent<EnemyContactDamage>().enabled = true;
            }
        }

        StartCoroutine(SelfDestroy());
    }

    IEnumerator SelfDestroy()
    {
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            transform.localScale = new Vector3(3,3,1);

            Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (Collider2D item in objects)
            {
                if (item.tag == "Player")
                {
                    GetComponent<EnemyContactDamage>().enabled = true;
                }
            }

            StartCoroutine(SelfDestroy());
        }
    }
}
