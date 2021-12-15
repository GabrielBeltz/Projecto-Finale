using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskHabilities : MonoBehaviour
{
    private PlayerController playerController;
    private LimboController limboController;
    [SerializeField] private bool hasDashMask = false;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        limboController = GetComponent<LimboController>();
    }

    void Update()
    {
        if(hasDashMask && limboController.IsInLimboMode)
        {
            playerController.HandleDashing();
        }
        else
        {
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "DashMask")
        {
            hasDashMask = true;
            Destroy(collision.gameObject);
        }
    }

}
