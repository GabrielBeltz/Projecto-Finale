using System.Collections;
using UnityEngine;

public class PlayerTantrum : MonoBehaviour
{
    public PlayerMeleeAttack TantrumAttack;
    public LayerMask hitMask;
    public GameObject TantrumGameObject;
    PlayerInputs PlayerInputs;
    float lastAttack = -10f;
    Animator animator;

    public void Awake()
    {
        PlayerInputs = GetComponent<PlayerInputs>();
        animator = TantrumGameObject.GetComponent<Animator>();
    }

    public void HandleTantrum(int rank)
    {
        if(rank < 1) return;
        if(PlayerInputs.GetInputDown("Tantrum") && Time.time > lastAttack + TantrumAttack.cooldown) ExecuteTantrum(rank);
    }

    void ExecuteTantrum(int rank)
    {
        animator.SetTrigger("Tantrum");
        lastAttack = Time.time;
        float range = TantrumAttack.range;
        float damage = TantrumAttack.damage;

        if(rank > 1) for(int i = 1; i < rank; i++) range += TantrumAttack.range / i * 0.5f;
        if(rank > 1) for(int i = 1; i < rank; i++) damage += TantrumAttack.damage / i * 0.5f;

        Collider2D[] collider2Ds = new Collider2D[20];
        int colliders = Physics2D.OverlapCircleNonAlloc(transform.position, range, collider2Ds, hitMask);
        collider2Ds = new Collider2D[colliders];
        Physics2D.OverlapCircleNonAlloc(transform.position, range, collider2Ds, hitMask);

        foreach(var collider in collider2Ds)
        {
            if(collider.TryGetComponent<EnemyAttackTarget>(out EnemyAttackTarget target))
            {
                target.ReceiveDamageCall(damage, transform.position);
            }
        }

        TantrumGameObject.transform.position = transform.position;
        TantrumGameObject.transform.localScale = new Vector3(range * 2, range * 2, 1f);
        TantrumGameObject.SetActive(true);
    }
}
