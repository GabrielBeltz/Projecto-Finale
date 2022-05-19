using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D _rb;

    public AbilityRank AbilityRanks; 

    [HideInInspector] public MaskHabilities AbilitiesController;
    float RegainedHealth;

    [Header("Walking")]
    public Transform model;
    [SerializeField] float _baseWalkSpeed = 8f;
    [SerializeField] float _jumpManeuverabilityPercentage;
    public float WalkSpeed { get => StatsManager.Instance.MoveSpeed.totalValue * _baseWalkSpeed; }
    float _shieldSpeedMultiplier = 1f;
    [HideInInspector] public bool StopMoving;

    [Header("Jumping")]
    [SerializeField] float _initialJumpSpeed = 20;
    public float _fallingAcceleration = 7.5f, _timeUntilMaxFallingSpeed = 2, _coyoteTime = 0.2f, _jumpTime, _extraJumpTime;
    [HideInInspector] public bool HasJumped, FallImpact, DoubleJumpCharged, WallOnRight, WallOnLeft, OnWall;
    float _maxFallSpeed => (-_initialJumpSpeed - (_fallingAcceleration * _timeUntilMaxFallingSpeed)) * (CurrentHealth > 0 ? 1 : 3);
    public float GripMaxTime = 3f, GravityScale;
    [HideInInspector] public float TimeLeftGrounded, GripTimer;

    [Header("Combat")]
    public bool Invulnerable;
    public float InvulnerableTimeAfterDamage;
    public int DeathCount = 0;
    public int TotalHealth;
    [SerializeField] List<PlayerMeleeAttack> _playerAttacks;
    public PlayerMeleeAttack DefaultAttack, DashAttack;
    [SerializeField] LayerMask _attackLayerMask;
    int _currentHealth;
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            OnPlayerHealthChanged?.Invoke(CurrentHealth, ModdedTotalHealth);
        }
    }
    public int ModdedTotalHealth { get => Mathf.FloorToInt(TotalHealth * StatsManager.Instance.Health.totalValue); }
    [SerializeField] float _knockbackTime, _selfKnockBackTime, _groundImpactKnockbackTime, _extraUngroundedKnockbackTime;
    float _timeOfLastAttack = 10f;
    AttackFeedback _attackFeedback;
    PlayerMeleeAttack _lastAttack;
    float _knockbackTimer;
    public bool IsKnockbacked 
    { 
        get => _knockbackTimer > Time.time; 
        set => _knockbackTimer = value ? Mathf.Infinity : Time.time;
    }

    [Header("Interactions")]
    public float InteractionRadius;
    public LayerMask InteractionLayer;
    
    [Header("Scene References")]
    public Animator MyAnimator;
    [SerializeField] AudioSource _soundEmitter;
    [HideInInspector] public HUDController HUDController;

    [Header("Audios")]
    public List<AudioClip> Audioclips;

    [Header("CollisorDetections")]
    [SerializeField] LayerMask _groundMask;
    [SerializeField] LayerMask _wallsMask;
    float _grounderOffset = -1, _grounderRadius = 0.2f;
    float _fullHealHeight;
    public bool IsGrounded;
    private readonly Collider2D[] _ground = new Collider2D[1];
    [HideInInspector] public GameObject actualGroundObject;

    FootStepController FootStepController;
    [HideInInspector] public PlayerInputs PlInputs;
    public static PlayerController Instance;
    PlayerDash dash;
    PlayerShield shield;
    PlayerHook hook;
    PlayerTantrum tantrum;

    //Actions
    public Action OnTouchedGround, OnPlayerDeath, OnPlayerFullHealth, OnJump;
    public Action<Vector3> OnPlayerReceiveKnockback;
    public Action<int, int> OnPlayerHealthChanged; 

    private void Awake()
    {
        if(Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;

        AbilitiesController = GetComponent<MaskHabilities>();
        _attackFeedback = GetComponentInChildren<AttackFeedback>();
        dash = GetComponent<PlayerDash>();
        shield = GetComponent<PlayerShield>();
        hook = GetComponent<PlayerHook>();
        tantrum = GetComponent<PlayerTantrum>();
        PlInputs = GetComponent<PlayerInputs>();
        _rb = GetComponent<Rigidbody2D>();
        FootStepController = GetComponentInChildren<FootStepController>();
    }

    void Start()
    {
        _timeOfLastAttack = 0;
        _lastAttack = DefaultAttack;
        _fullHealHeight = transform.position.y + 2f;
        GripTimer = GripMaxTime;
        CurrentHealth = ModdedTotalHealth;

        OnPlayerDeath += PlayerDeath;
        OnPlayerDeath += ResetSkills;
        OnPlayerFullHealth += PlayerFullHeal;
    }

    private void LateUpdate()
    {
        // Não mudar a ordem desses métodos pra evitar bug. Se for adicionar alguma coisa, coloque num lugar apropriado ou no fim.
        HandleGrounding();
        HandleWalling();
        HandleJumping();

        if (!IsKnockbacked && !MyAnimator.GetBool("FellDown"))
        {
            _shieldSpeedMultiplier = shield.HandleShield(AbilityRanks.ShieldRank);
            HandleWalking();
            dash.HandleDashing(AbilityRanks.DashRank);
            hook.HandleHooking(AbilityRanks.HookRank);
            tantrum.HandleTantrum(AbilityRanks.TantrumRank);
        }

        HandleAnimation();
    }
    
    public void CorrectFacingDirection()
    {
        if(Mathf.Sign(transform.lossyScale.x) < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    public void SetFacingDirection(bool left)
    {
        // Flipar o X do sprite renderer futuramente
        model.transform.localScale = new Vector3(-0.3f, 0.3f, left? 0.3f : -0.3f);
    }

    #region ColisorDetections

        private void HandleGrounding()
    {
        var grounded = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(0, _grounderOffset, 0), _grounderRadius, _ground, _groundMask) > 0;

        actualGroundObject = grounded ? _ground[0].gameObject : gameObject;
        Transform parent = null;
        if(grounded)
        {
            platformov platMover;
            if(_ground[0].TryGetComponent<platformov>(out platMover))
            {
                if(platMover.rot == 0f) parent = platMover.transform;
            }
            GripTimer = Mathf.Clamp(GripTimer + Time.deltaTime, 0, GripMaxTime);
            transform.SetParent(parent);
        }

        if(!IsGrounded && grounded)
        {
            if(CurrentHealth < 1 && transform.position.y < _fullHealHeight) OnPlayerFullHealth?.Invoke(); 

            if(0 < CurrentHealth)
            {
                if(FallImpact) 
                {
                    _knockbackTimer = Time.time + _groundImpactKnockbackTime;
                    MyAnimator.SetBool("FellDown", true);
                    PlaySound(Audioclips[1]);
                    hook.EndAiming();
                    _rb.velocity = Vector3.zero;
                }
                else FootStepController.PlayOneShot(FootStepController.RandomSolidClip(), 0.5f);
            }

            DoubleJumpCharged = AbilityRanks.MobilityRank > 2;
            IsGrounded = true;
            HasJumped = false;
            OnTouchedGround?.Invoke();
        }
        else if(IsGrounded && !grounded)
        {
            IsGrounded = false;
            TimeLeftGrounded = Time.time;
        }
    }

    void HandleWalling()
    {
        if(AbilityRanks.MobilityRank < 1) return;
        RaycastHit2D[] hits = new RaycastHit2D[1];
        WallOnLeft = Physics2D.RaycastNonAlloc(transform.position, Vector2.left, hits, 0.55f, _wallsMask) > 0 && !IsGrounded;
        WallOnRight = Physics2D.RaycastNonAlloc(transform.position, Vector2.right, hits, 0.55f, _wallsMask) > 0 && !IsGrounded;
    }

    #endregion

    #region Animation

    void HandleAnimation()
    {
        if(PlInputs.Inputs.X != 0) SetFacingDirection(_rb.velocity.x < 0);
        CorrectFacingDirection();
        MyAnimator.SetBool("Grounded", IsGrounded);
        MyAnimator.SetFloat("VerticalSpeed", _rb.velocity.y);
    }

    #endregion

    #region Walking

    private void HandleWalking()
    {
        if(StopMoving) _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
        else
        {
            _rb.velocity = IsGrounded
            ? new Vector2(WalkSpeed * (PlInputs.Inputs.RawX * _shieldSpeedMultiplier), _rb.velocity.y)
            : new Vector2(WalkSpeed * _jumpManeuverabilityPercentage * (PlInputs.Inputs.RawX * _shieldSpeedMultiplier), _rb.velocity.y);
            MyAnimator.SetFloat("Speed", Mathf.Abs(WalkSpeed * PlInputs.Inputs.RawX));
        }
    }

    #endregion

    #region Jumping

    private void HandleJumping()
    {
        if (Mathf.Approximately(_rb.velocity.y, 0) && !IsGrounded) HasJumped = false;
        if(_rb.velocity.y < 0) _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Clamp(_rb.velocity.y, _maxFallSpeed, 0));
        FallImpact = _rb.velocity.y <= _maxFallSpeed * 0.95f;

        if(OnWall && GripTimer > 0)
        {
            GripTimer -= Time.deltaTime;
            _rb.gravityScale = 0;
            if(IsKnockbacked) return;
            _rb.velocity = new Vector2(0, -GravityScale / 2);
            if(AbilityRanks.MobilityRank > 1) _rb.velocity = new Vector2(0, (GravityScale / 2) * PlInputs.Inputs.RawY);
            return;
        }

        if(HasJumped)
        {
            if((Input.GetButton("Jump") && Time.time < TimeLeftGrounded + _extraJumpTime + _jumpTime)) _rb.gravityScale = 0;
            else if(Time.time > TimeLeftGrounded + _jumpTime) HasJumped = false;
        }
        else if(!dash.Dashing) _rb.gravityScale = GravityScale;
    }

    public void ExecuteJump(bool coyoteJump)
    {
        PlaySound(Audioclips[0]);
        MyAnimator.SetBool("FellDown", false);
        TimeLeftGrounded = coyoteJump? TimeLeftGrounded : Time.time;
        HasJumped = true;
        _rb.velocity = new Vector2(_rb.velocity.x, _initialJumpSpeed);
        if(OnWall) 
        {
            _rb.velocity = new Vector2(_initialJumpSpeed * Mathf.Sign(transform.lossyScale.x), _initialJumpSpeed * 1.5f);
            _knockbackTimer = Time.time + 0.2f;
            SetFacingDirection(Mathf.Sign(transform.lossyScale.x) < 0);
        } 
        OnJump?.Invoke();
    }

    #endregion

    #region Combat

    public void ExecuteAttack()
    {
        if(_timeOfLastAttack > Time.time) return;
        Vector2 attackInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _lastAttack = GetNextAttack(attackInput);
        _timeOfLastAttack = Time.time + _lastAttack.cooldown;
        Attack();
    }

    Vector3 GetAttackDirection() => Input.GetAxisRaw("Vertical") != 0 ? Input.GetAxisRaw("Vertical") > 0 ? transform.up : -transform.up : transform.right * -transform.localScale.x;

    public void Attack()
    {
        Vector3 attackDirection = GetAttackDirection();
        Vector3 attackPos = transform.position + ((attackDirection * (AbilityRanks.AttackRank > 1 ? _lastAttack.range : (_lastAttack.range /2))));
        float attackRotation = attackDirection.y != 0 ? 90 * attackDirection.y : 0;
        
        Vector3 attackSize = AbilityRanks.AttackRank > 2 ? new Vector3(0.4f, 1.2f, _lastAttack.range) : AbilityRanks.AttackRank > 1 ? new Vector3(0.25f, 0.75f, _lastAttack.range) : new Vector3(0.25f, 0.75f, _lastAttack.range / 2);

        RaycastHit2D[] attackColliders = Physics2D.BoxCastAll(attackPos, attackSize, attackRotation, attackDirection, _lastAttack.range/2, _attackLayerMask);

        for(int i = 0; i < attackColliders.Length; i++)
        {
            float selfKnockbackReceived = 0;
            EnemyAttackTarget target;
            if (attackColliders[i].transform.TryGetComponent<EnemyAttackTarget>(out target))
            {
                target.ReceiveAttackCall(_lastAttack, transform.position);
                selfKnockbackReceived = target.selfKnockbackReceived;

                #region Vampirirism
                if(AbilityRanks.HealthRank > 2)
                {
                    RegainedHealth += _lastAttack.damage * 0.05f;
                    if(RegainedHealth > 1)
                    {
                        RegainedHealth = 0;
                        ReceiveHealing(1);
                    }
                }
                #endregion
            }

            if (selfKnockbackReceived != 0)
            { 
                _rb.velocity = Vector3.zero;
                float multiplier = Mathf.Clamp(selfKnockbackReceived, 0, 1);
                _knockbackTimer = Time.time + (_selfKnockBackTime * multiplier);
                _rb.AddForce(_lastAttack.selfKnockback * selfKnockbackReceived * -attackDirection, ForceMode2D.Force);
            }

            if(AbilityRanks.AttackRank < 3)
            {
                i = attackColliders.Length;
                attackSize = new Vector3(attackSize.x, attackSize.y, Vector2.Distance(attackPos, attackColliders[0].point));
            } 
        }

        if(attackColliders.Length < 1)
            if (PlInputs.Inputs.RawX == 0f) _rb.AddForce(attackDirection * _lastAttack.selfKnockback, ForceMode2D.Force);

        _soundEmitter.Stop();

        PlaySound(_lastAttack.sound);

        _attackFeedback.CallFeedback(attackSize, attackPos, attackRotation, _lastAttack.cooldown, attackColliders.Length > 0);
        MyAnimator.SetTrigger(_lastAttack.animatorTrigger);
    }

    public PlayerMeleeAttack GetNextAttack(Vector2 input)
    {
        if (input.x == 0 && input.y == 0)
        {
            PlayerMeleeAttack foundAttack = _playerAttacks.Find(attck => attck.comboInfo.previousAttack == _lastAttack.name);

            if (foundAttack == null) return DefaultAttack;
            else if (_timeOfLastAttack - _lastAttack.cooldown + foundAttack.comboInfo.timeBetween > Time.time) return foundAttack;
        }
        else if (input.y > 0)
        {
            PlayerMeleeAttack foundAttack = _playerAttacks.Find(attck => attck.comboInfo.previousAttack == _lastAttack.name && attck.direction == AttackDirection.Up);

            if (foundAttack == null) return _playerAttacks.Find(attck => attck.direction == AttackDirection.Up);
            else if (_timeOfLastAttack - _lastAttack.cooldown + foundAttack.comboInfo.timeBetween > Time.time) return foundAttack;
        }
        else if (input.y < 0)
        {
            PlayerMeleeAttack foundAttack = _playerAttacks.Find(attck => attck.comboInfo.previousAttack == _lastAttack.name && attck.direction == AttackDirection.Down);

            if(foundAttack == null) return _playerAttacks.Find(attck => attck.direction == AttackDirection.Down);
            else if(_timeOfLastAttack - _lastAttack.cooldown + foundAttack.comboInfo.timeBetween > Time.time) return foundAttack;
        }

        return DefaultAttack;
    }

    public void ReceiveDamage(int damageAmount, Vector3 knockback)
    {
        if(Invulnerable) return;

        if(damageAmount > 0)
        {
            CurrentHealth -= damageAmount;
            // Hurt Sound
        }

        if(knockback != Vector3.zero) ReceiveKnockback(knockback);
        if(CurrentHealth < 1) OnPlayerDeath?.Invoke();
        StartCoroutine(InvulnerableTimer());
    }

    IEnumerator InvulnerableTimer()
    {
        Invulnerable = true;
        yield return new WaitForSeconds(InvulnerableTimeAfterDamage);
        Invulnerable = false;
    }

    public void ReceiveKnockback(Vector3 knockback)
    {
        float knockbackResistance = StatsManager.Instance.KnockbackResistance.totalValue;
        _rb.velocity = Vector3.zero;
        _rb.AddForce(new Vector2(knockback.x, knockback.y * 1.5f) * knockbackResistance, ForceMode2D.Impulse);
        _knockbackTimer = IsGrounded ? Time.time + (_knockbackTime * knockbackResistance) : Time.time + ((_knockbackTime + _extraUngroundedKnockbackTime) * knockbackResistance);
    }

    public void ReceiveHealing(int healingAmount) => CurrentHealth = Mathf.Min(CurrentHealth + healingAmount, ModdedTotalHealth);

    void PlayerFullHeal()
    { 
        CurrentHealth = ModdedTotalHealth;
        gameObject.layer = 10;
        _knockbackTimer = 0;
    }

    void PlayerDeath()
    {
        DeathCount++;
        MyAnimator.SetFloat("Speed", 0);
        MyAnimator.SetBool("FellDown", true);
        gameObject.layer = 7;
        _knockbackTimer = Mathf.Infinity;
    }

    #endregion

    #region Interactions

    public void ExecuteInteraction()
    {
        Collider2D[] interactionColliders = Physics2D.OverlapCircleAll(this.transform.position, InteractionRadius, InteractionLayer);

        if(interactionColliders.Length > 0)
            if(interactionColliders[0].TryGetComponent<Interactable>(out Interactable a)) a.Interact();
    }

    #endregion
    
    public void PlaySound(AudioClip clipToPlay) => _soundEmitter.PlayOneShot(clipToPlay);

    void ResetSkills() 
    { 
        AbilityRanks.MobilityRank = 0;
        AbilityRanks.AttackRank = 0;
        AbilityRanks.HookRank = 0;
        AbilityRanks.TantrumRank = 0;
        AbilityRanks.DashRank = 0;
        AbilityRanks.ShieldRank = 0;
    }

    [System.Serializable]
    public class AbilityRank { public int MobilityRank, AttackRank, HookRank, TantrumRank, DashRank, ShieldRank, HealthRank; }
}
