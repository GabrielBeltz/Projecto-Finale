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

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == limboLayer)
        {
            other.gameObject.GetComponent<SphereCollider>().enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == limboLayer)
        {
            other.gameObject.GetComponent<SphereCollider>().enabled = false;
        }
    }
}
