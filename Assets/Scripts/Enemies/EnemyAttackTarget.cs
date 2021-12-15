using UnityEngine;
using UnityEngine.Events;

public class EnemyAttackTarget : MonoBehaviour
{
    public UnityAction<PlayerMeleeAttack, Vector3> onAttackReceived;
    public UnityAction onDeath;
    [Header("Stats")]
    public float TotalHealth;
    public float currentHealth;
    public UnityEvent onDeathEvent;
    public AttackDirection receivedAttackDirection;
    [Header("Multiplicadores")]
    public float DamageReceived;
    public float selfKnockbackReceived;
    [Header("Configs")]
    public bool defaultDeath;
    public AudioClip[] hitSound;

    [Header("Needed to Work")]
    public Collider2D col;
    public Renderer rdr;
    public Rigidbody2D rb;
    public AudioSource audioSource;
    RigidbodyConstraints2D constraints;

    private void OnEnable()
    {
        currentHealth = TotalHealth;
        onAttackReceived += ReceiveDamage;
        if (defaultDeath)
        {
            onDeath += DefaultDeath;
            if (rb != null)
            {
                constraints = rb.constraints;
            }
        }

        if (hitSound.Length > 0 && audioSource != null)
        {
            onAttackReceived += PlayHitSound;
        }
    }

    private void OnDisable()
    {
        onAttackReceived -= ReceiveDamage;
        if (defaultDeath)
        {
            onDeath -= DefaultDeath;
        }

        if (hitSound.Length > 0 && audioSource != null)
        {
            onAttackReceived -= PlayHitSound;
        }
    }

    public void ReceiveAttack(PlayerMeleeAttack playerMeleeAttack, Vector3 pos)
    {
        onAttackReceived?.Invoke(playerMeleeAttack, pos);
    }

    void ReceiveDamage(PlayerMeleeAttack playerMeleeAttack, Vector3 pos)
    {
        if (playerMeleeAttack.direction == receivedAttackDirection || receivedAttackDirection == AttackDirection.Front)
        {
            currentHealth -= playerMeleeAttack.damage * DamageReceived;

            if (currentHealth <= 0)
            {
                onDeath?.Invoke();
                onDeathEvent?.Invoke();
            }
        }
    }

    void DefaultDeath()
    {
        col.enabled = false;
        rdr.enabled = false;
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void PlayHitSound(PlayerMeleeAttack playerMeleeAttack, Vector3 pos)
    {
        Vector3 soundPosition = col.ClosestPoint(pos);
        audioSource.transform.position = soundPosition;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = hitSound[Random.Range(0, hitSound.Length)];
        audioSource.Play();
    }
}