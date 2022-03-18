using UnityEngine;
using UnityEngine.Events;

public class EnemyAttackTarget : MonoBehaviour
{
    public UnityAction<PlayerMeleeAttack, Vector3> onAttackReceived;
    public UnityAction onDeath;
    [Header("Stats")]
    public float TotalHealth;
    public float currentHealth;
    public AttackDirection receivedAttackDirection;
    [Header("Multiplicadores")]
    public float DamageReceived;
    public float selfKnockbackReceived;
    [Header("Mortes")]
    public EnemyDeathType DeathType;
    public AudioClip[] hitSound;
    bool hasRespawnBehaviour;

    [Header("Needed to Work")]
    [Help("Se esses ficarem nulos o script pega do próprio gameObject ou o primeiro que encontrar nos filhos.", UnityEditor.MessageType.Info)]
    public Collider2D Collider;
    public Renderer Renderer;
    public Rigidbody2D RigidBody;
    public AudioSource audioSource;
    RigidbodyConstraints2D constraints;

    private void Awake()
    {
        if(Collider == null)
            if(!TryGetComponent(out Collider)) 
                Collider = GetComponentInChildren<Collider2D>();
        if(Renderer == null)
            if(!TryGetComponent(out Renderer)) 
                Renderer = GetComponentInChildren<Renderer>();
        if(RigidBody == null)
            if(!TryGetComponent(out RigidBody))
            {
                var rb = GetComponentInChildren<Rigidbody2D>();
                RigidBody = rb != null ? rb : null;
            }
        if(audioSource == null)
            if(!TryGetComponent(out audioSource)) 
            {
                var aS = GetComponentInChildren<AudioSource>();
                audioSource = aS != null ? aS : null;
            }

        hasRespawnBehaviour = TryGetComponent(out RespawnBehaviour respawn);
        if(respawn != null) hasRespawnBehaviour &= respawn.respawningTime > 0;
    }

    private void OnEnable()
    {
        currentHealth = TotalHealth;
        onAttackReceived += ReceiveDamage;

        if(hasRespawnBehaviour) DeathType = EnemyDeathType.DisableRendererAndCollider;

        if(DamageReceived > 0)
        {
            switch(DeathType)
            {
                case EnemyDeathType.DisableRendererAndCollider:
                    onDeath += DefaultDeath;

                    if(RigidBody == null) break;
                
                    if(constraints == RigidbodyConstraints2D.None)
                    {
                        constraints = RigidBody.constraints;
                    }
                    else
                    {
                        RigidBody.constraints = constraints;
                    }
                    break;
                case EnemyDeathType.DeactivateSelf:
                    onDeath += DeactivateSelf;
                    break;
            }
        }

        if (hitSound.Length > 0 && audioSource != null) onAttackReceived += PlayHitSound;
    }

    private void OnDisable()
    {
        onAttackReceived -= ReceiveDamage;

        switch(DeathType)
        {
            case EnemyDeathType.DisableRendererAndCollider:
                onDeath -= DefaultDeath;
                break;
            case EnemyDeathType.DeactivateSelf:
                onDeath -= DeactivateSelf;
                break;
        }

        if (hitSound.Length > 0 && audioSource != null) onAttackReceived -= PlayHitSound;
    }

    public void ReceiveAttack(PlayerMeleeAttack playerMeleeAttack, Vector3 pos) => onAttackReceived?.Invoke(playerMeleeAttack, pos);

    void ReceiveDamage(PlayerMeleeAttack playerMeleeAttack, Vector3 pos)
    {
        if (playerMeleeAttack.direction == receivedAttackDirection || receivedAttackDirection == AttackDirection.Front)
        {
            currentHealth -= playerMeleeAttack.damage * DamageReceived;

            if (currentHealth <= 0) onDeath?.Invoke();
        }
    }

    void DefaultDeath()
    {
        Collider.enabled = false;
        Renderer.enabled = false;

        if(RigidBody == null) return;

        RigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void DeactivateSelf() => this.gameObject.SetActive(false);

    void PlayHitSound(PlayerMeleeAttack playerMeleeAttack, Vector3 pos)
    {
        Vector3 soundPosition = Collider.ClosestPoint(pos);
        audioSource.transform.position = soundPosition;

        if (audioSource.isPlaying) audioSource.Stop();

        audioSource.clip = hitSound[Random.Range(0, hitSound.Length)];
        audioSource.Play();
    }

}

public enum EnemyDeathType { DisableRendererAndCollider, DeactivateSelf }