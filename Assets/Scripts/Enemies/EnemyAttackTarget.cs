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

    [Header("Needed to Work")]
#if UNITY_EDITOR
    [Help("Se esses ficarem nulos o script pega do próprio gameObject ou o primeiro que encontrar nos filhos.", UnityEditor.MessageType.Info)]
#endif
    public Collider2D Collider;
    public ParticleSystem ParticleSystem;
    public Renderer[] Renderer;
    public Rigidbody2D RigidBody;
    public AudioSource audioSource;
    public MonoBehaviour[] BehavioursToDisable;
    public GameObject[] GameObjectsToDeactivate;
    RigidbodyConstraints2D constraints;

    float defaultPlayerDamage;

    private void Awake()
    {
        if(Collider == null)
            if(!TryGetComponent(out Collider)) 
                Collider = GetComponentInChildren<Collider2D>();
        if(Renderer == null)
            if(!TryGetComponent(out Renderer)) 
                Renderer = GetComponentsInChildren<Renderer>();
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
    }

    private void OnEnable()
    {
        currentHealth = TotalHealth;
        onAttackReceived += ReceiveAttack;

        if(DamageReceived > 0)
        {
            switch(DeathType)
            {
                case EnemyDeathType.DisableAllButAudio:
                    onDeath += DisableAllButAudio;

                    if(RigidBody == null) break;
                
                    if(constraints == RigidbodyConstraints2D.None) constraints = RigidBody.constraints;
                    else RigidBody.constraints = constraints;

                    break;
                case EnemyDeathType.DeactivateSelf:
                    
                    onDeath += DeactivateSelf;
                    
                    break;
            }
        }

        if (hitSound.Length > 0 && audioSource != null) onAttackReceived += PlayCalculatedHitSound;
    }

    private void Start()
    {
        float b = StatsManager.Instance.Damage.totalValue;
        float a = PlayerController.Instance.DefaultAttack.damage;
        defaultPlayerDamage = a * b;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 12) 
        {
            ReceiveAttackCall(PlayerController.Instance.DashAttack, collision.transform.position);
            PlaySoundAtMyPosition();
        }
    }

    private void OnDisable()
    {
        onAttackReceived -= ReceiveAttack;

        switch(DeathType)
        {
            case EnemyDeathType.DisableAllButAudio:
                onDeath -= DisableAllButAudio;
                break;
            case EnemyDeathType.DeactivateSelf:
                onDeath -= DeactivateSelf;
                break;
        }

        if (hitSound.Length > 0 && audioSource != null) onAttackReceived -= PlayCalculatedHitSound;
        Destroy(gameObject);
    }

    public void ReceiveAttackCall(PlayerMeleeAttack playerMeleeAttack, Vector3 pos) => onAttackReceived?.Invoke(playerMeleeAttack, pos);

    public void ReceiveDamageCall(float damage, Vector3 pos)
    {
        ReceiveDamage(damage);
        if(DamageReceived > 0) PlayCalculatedHitSound(null, pos);
    }

    void ReceiveAttack(PlayerMeleeAttack playerMeleeAttack, Vector3 pos)
    {
        if(playerMeleeAttack.direction == receivedAttackDirection || receivedAttackDirection == AttackDirection.Front)
        ReceiveDamage(playerMeleeAttack.damage);
    }

    void ReceiveDamage(float damage)
    {
        currentHealth -= (damage * StatsManager.Instance.Damage.totalValue) * DamageReceived;
        if (currentHealth <= 0) onDeath?.Invoke();
    }

    void DisableAllButAudio()
    {
        Collider.enabled = false;

        if(ParticleSystem != null) ParticleSystem.Play();

        foreach(var rdrdrdr in Renderer)
        {
            if(rdrdrdr != null)
            rdrdrdr.enabled = false;
        }

        foreach(MonoBehaviour behav in BehavioursToDisable) behav.enabled = false;

        foreach(var gObj in GameObjectsToDeactivate) gObj.SetActive(false);

        if(RigidBody == null) return;

        RigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void DeactivateSelf() => this.gameObject.SetActive(false);

    void PlaySoundAtMyPosition()
    {
        audioSource.transform.localPosition = Vector3.zero;
        PlayHitSound();
    }

    void PlayCalculatedHitSound(PlayerMeleeAttack playerMeleeAttack, Vector3 pos) 
    {
        Vector3 soundPosition = Collider.ClosestPoint(pos);
        audioSource.transform.position = soundPosition;
        PlayHitSound();
    }

    void PlayHitSound()
    {
        if (audioSource.isPlaying) audioSource.Stop();
        if(hitSound.Length < 1) return;
        
        audioSource.clip = hitSound[Random.Range(0, hitSound.Length)];
        audioSource.Play();
    }
}

public enum EnemyDeathType { DisableAllButAudio, DeactivateSelf }