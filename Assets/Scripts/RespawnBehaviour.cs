using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAttackTarget))]
public class RespawnBehaviour : MonoBehaviour
{
    public Vector3 originalPosition;

    public bool Respawns;
    public float respawningTime;
    float respawningTimer;
    EnemyAttackTarget enemyAttackTarget;
    public Collider2D col;
    public Renderer rdr;
    public Rigidbody2D rb;
    RigidbodyConstraints2D constraints;

    private void Start()
    {
        enemyAttackTarget = this.GetComponent<EnemyAttackTarget>();
        if (rb != null)
        {
            constraints = rb.constraints;
        }
    }

    private void Update()
    {
        if (enemyAttackTarget.currentHealth <= 0)
        {
            respawningTimer += Time.deltaTime;
            if (respawningTimer > respawningTime)
            {
                if (Respawns)
                {
                    Respawn();
                }
            }
        }
    }

    void Respawn()
    {
        respawningTimer = 0;
        col.enabled = true;
        rdr.enabled = true;
        enemyAttackTarget.currentHealth = enemyAttackTarget.TotalHealth;
        if (rb != null)
        {
            rb.constraints = constraints;
        }
    }
}
