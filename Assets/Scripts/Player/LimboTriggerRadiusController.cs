using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimboTriggerRadiusController : MonoBehaviour
{
    [SerializeField] private int limboLayer;

    private void Start()
    {
        limboLayer = 7;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.layer == limboLayer)
        {
            collision.gameObject.GetComponent<CircleCollider2D>().enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.layer == limboLayer)
        {
            collision.gameObject.GetComponent<CircleCollider2D>().enabled = false;
        }
    }
}
