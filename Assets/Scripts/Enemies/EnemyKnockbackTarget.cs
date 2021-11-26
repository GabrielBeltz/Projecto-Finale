using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyAttackTarget))]
public class EnemyKnockbackTarget : MonoBehaviour
{
    [HideInInspector]
    public EnemyAttackTarget enemyAttackTarget;
    [HideInInspector]
    public Rigidbody2D rb;
    [Header("Tempo do knockback")]
    public float knockbackTime;
    float knockbackTimer;

    public bool isKnockbacked
    {
        get
        {
            return knockbackTimer > Time.time;
        }
    }

    [Header("Multiplicador do knockback recebido")] 
    public float knockbackReceived;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
        enemyAttackTarget = this.GetComponent<EnemyAttackTarget>();
    }

    private void OnEnable()
    {
        enemyAttackTarget.onAttackReceived += Knockback;    
    }

    private void OnDisable()
    {
        enemyAttackTarget.onAttackReceived -= Knockback;
    }

    void Knockback(PlayerMeleeAttack playerMeleeAttack, Vector3 pos)
    {
        knockbackTimer = Time.time + knockbackTime;
        Vector3 a = this.transform.position - pos;
        rb.AddForce(a.normalized * playerMeleeAttack.knockback * knockbackReceived, ForceMode2D.Force);
    }
}
