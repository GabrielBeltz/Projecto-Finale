using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    public int damage;
    public float playerKnockback;
    PlayerController playerController;

    private void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D (Collider2D collision)
    {
        if(collision.gameObject.layer == 12) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 knockingKnocking = (playerController.transform.position - this.transform.position).normalized * playerKnockback;
            playerController.ReceiveDamage(damage, knockingKnocking);
        }
    }
}
