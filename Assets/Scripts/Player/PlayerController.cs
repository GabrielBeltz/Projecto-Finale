using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D _rb;

    [Header("Abilities")]
    public int MobilityRank;
    public int AttackRank, HookRank, TantrumRank, DashRank, KnivesRank, RangedRank, ShieldRank, HealthRank;
    public Action CallAbilityA, CallAbilityB;
    [HideInInspector]public MaskHabilities AbilitiesController;
    float RegainedHealth;
    
    [Header("Walking")]
    [SerializeField] float _baseWalkSpeed = 8f;
    [SerializeField] float _jumpManeuverabilityPercentage;
    float _moddedWalkSpeed { get => StatsManager.Instance.MoveSpeed.totalValue * _baseWalkSpeed; }

    [Header("Jumping")]
    [SerializeField] float _initialJumpSpeed = 20;
    public float _fallingAcceleration = 7.5f, _timeUntilMaxFallingSpeed = 2, _coyoteTime = 0.2f, _jumpTime, _extraJumpTime, _gravityScale;
    [HideInInspector] public bool _hasJumped, _fallImpact, _doubleJumpCharged;
    float _maxFallSpeed => (-_initialJumpSpeed - (_fallingAcceleration * _timeUntilMaxFallingSpeed)) * (CurrentHealth > 0 ? 1 : 3);
    [HideInInspector] public float GripTimer, _timeLeftGrounded, gripMaxTime = 3f;
    public static event Action OnJump;
    [HideInInspector] public bool WallOnRight, WallOnLeft, OnWall;

    [Header("Dashing")]
    [SerializeField] float _dashLength = 0.2f;
    [SerializeField] float _dashCooldown = 3f, _dashSpeed = 30;
    float _dashCooldownTimer;
    public static event Action OnStartDashing, OnStopDashing;

    [Header("Combat")]
    public PlayerMeleeAttack DefaultAttack;
    [SerializeField] List<PlayerMeleeAttack> _playerAttacks;
    [SerializeField] LayerMask _attackLayerMask;
    public int TotalHealth;
    int _currentHealth;
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            InterfacePlayerHP();
        }
    }
    public int ModdedTotalHealth { get => Mathf.FloorToInt(TotalHealth * StatsManager.Instance.Health.totalValue); }
    [SerializeField] float _knockbackTime, _selfKnockBackTime, _groundImpactKnockbackTime, _extraUngroundedKnockbackTime;
    [SerializeField] float _timeOfLastAttack = 10f;
    [SerializeField] AttackFeedback _attackFeedback;
    PlayerMeleeAttack _lastAttack;
    float _knockbackTimer;
    public bool IsKnockbacked 
    { 
        get => _knockbackTimer > Time.time; 
        set => _knockbackTimer = value ? Mathf.Infinity : Time.time;
    }
    public static Action OnPlayerDeath, OnPlayerFullHealth;

    [Header("Interactions")]
    public float InteractionRadius;
    public LayerMask InteractionLayer;
    
    [Header("Scene References")]
    public Animator MyAnimator;
    [SerializeField] AudioSource _soundEmitter;
    public Transform UIScaler;

    [Header("Audios")]
    public List<AudioClip> Audioclips;

    [Header("UI")]
    public GameObject HealthIconPrefab;
    public Vector2 FullHDHealthIconsPivot;
    List<InstantiatedUIHP> Hearts;

    [Header("CollisorDetections")]
    [SerializeField] LayerMask _groundMask;
    [SerializeField] LayerMask _wallsMask;
    [SerializeField] float _grounderOffset = -1, _grounderRadius = 0.2f;
    [SerializeField] GameObject _frontFeet;
    float _fullHealHeight;
    public bool IsGrounded;
    public static event Action OnTouchedGround;
    private readonly Collider2D[] _ground = new Collider2D[1];
    public GameObject actualGroundObject;

    bool _hasDashed, _dashing;
    float _timeStartedDash;
    Vector3 _dashDir;

    FootStepController FootStepController;
    [HideInInspector] public PlayerInputs PlInputs;
    public static PlayerController instance;

    void Start()
    {
        if(instance != null && instance != this.gameObject)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        PlInputs = GetComponent<PlayerInputs>();
        Hearts = new List<InstantiatedUIHP>();
        _timeOfLastAttack = 0;
        _rb = GetComponent<Rigidbody2D>();
        _lastAttack = DefaultAttack;
        FootStepController = GetComponentInChildren<FootStepController>();
        _fullHealHeight = transform.position.y + 2f;
        GripTimer = gripMaxTime;
        CurrentHealth = ModdedTotalHealth;

        OnPlayerDeath += PlayerDeath;
        OnPlayerDeath += ResetSkills;
        OnPlayerFullHealth += PlayerFullHeal;
    }

    void Update()
    {
        PlInputs.GatherUnpausedInputs();
        if(!(Time.timeScale > 0)) return;
        PlInputs.GatherInputs();
        GetGroundObject();
    }

    private void LateUpdate()
    {
        HandleGrounding();
        HandleWalling();
        HandleJumping();

        if (!IsKnockbacked && !MyAnimator.GetBool("FellDown"))
        {
            HandleWalking();
            HandleDashing(DashRank);
        }

        HandleAnimation();
    }


    public void SetFacingDirection(bool left) => this.transform.localScale = left ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

    #region ColisorDetections

    private void HandleGrounding()
    {
        var grounded = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(0, _grounderOffset, 0), _grounderRadius, _ground, _groundMask) > 0;

        if(grounded) GripTimer = Mathf.Clamp(GripTimer + Time.deltaTime, 0, gripMaxTime);

        if(!IsGrounded && grounded)
        {
            if(CurrentHealth == 0 && transform.position.y < _fullHealHeight) OnPlayerFullHealth?.Invoke(); 

            if(0 < CurrentHealth)
            {
                if(_fallImpact) 
                {
                    _knockbackTimer = Time.time + _groundImpactKnockbackTime;
                    MyAnimator.SetBool("FellDown", true);
                    PlaySound(Audioclips[1]);
                }
                else FootStepController.PlayOneShot(FootStepController.RandomSolidClip(), 0.5f);
            }

            _doubleJumpCharged = MobilityRank > 2;
            IsGrounded = true;
            _hasDashed = false;
            _hasJumped = false;
            OnTouchedGround?.Invoke();
        }
        else if(IsGrounded && !grounded)
        {
            IsGrounded = false;
            _timeLeftGrounded = Time.time;
        }
    }

    void HandleWalling()
    {
        if(MobilityRank < 1) return;
        RaycastHit2D[] hits = new RaycastHit2D[1];
        WallOnLeft = Physics2D.RaycastNonAlloc(transform.position, Vector2.left, hits, 0.55f, _wallsMask) > 0 && !IsGrounded;
        WallOnRight = Physics2D.RaycastNonAlloc(transform.position, Vector2.right, hits, 0.55f, _wallsMask) > 0 && !IsGrounded;
    }

    private void GetGroundObject()
    {
        RaycastHit2D hitResult = Physics2D.Raycast(_frontFeet.transform.position, -Vector2.up, 0.5f);

        if(hitResult.collider != null)
        {
            actualGroundObject = hitResult.collider.gameObject;
        }
        else
        {
            actualGroundObject = _frontFeet.gameObject;
        }
    }

    #endregion

    #region Animation

    void HandleAnimation()
    {
        MyAnimator.SetBool("Grounded", IsGrounded);
        MyAnimator.SetFloat("VerticalSpeed", _rb.velocity.y);
    }

    #endregion

    #region Walking

    private void HandleWalking()
    {
        _rb.velocity = IsGrounded
            ? new Vector2(_moddedWalkSpeed * PlInputs.Inputs.RawX, _rb.velocity.y)
            : new Vector2(_moddedWalkSpeed * _jumpManeuverabilityPercentage * PlInputs.Inputs.RawX, _rb.velocity.y);

        MyAnimator.SetFloat("Speed", Mathf.Abs(_baseWalkSpeed * PlInputs.Inputs.RawX));
    }

    #endregion

    #region Jumping

    private void HandleJumping()
    {
        if (Mathf.Approximately(_rb.velocity.y, 0) && !IsGrounded) _hasJumped = false;
        if(_rb.velocity.y < 0) _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Clamp(_rb.velocity.y, _maxFallSpeed, 0));
        _fallImpact = _rb.velocity.y <= _maxFallSpeed * 0.95f;

        if(OnWall && GripTimer > 0)
        {
            GripTimer -= Time.deltaTime;
            _rb.gravityScale = 0;
            if(IsKnockbacked) return;
            _rb.velocity = new Vector2(0, -_gravityScale / 2);
            if(MobilityRank > 1) _rb.velocity = new Vector2(0, (_gravityScale / 2) * PlInputs.Inputs.RawY);
            return;
        }

        if (_hasJumped)
        {
            if ((Input.GetButton("Jump") && Time.time < _timeLeftGrounded + _extraJumpTime + _jumpTime)) _rb.gravityScale = 0;
            else if (Time.time > _timeLeftGrounded + _jumpTime) _hasJumped = false;
        }
        else if(!_dashing) _rb.gravityScale = _gravityScale;
    }

    public void ExecuteJump(bool coyoteJump)
    {
        PlaySound(Audioclips[0]);
        MyAnimator.SetBool("FellDown", false);
        _timeLeftGrounded = coyoteJump? _timeLeftGrounded : Time.time;
        _hasJumped = true;
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

    #region Dash

    public void HandleDashing(int rank)
    {
        if(rank < 1) return;

        if (_dashCooldownTimer < Time.time)
        {
            if (PlInputs.GetInput("Dash") && !_hasDashed && PlInputs.Inputs.RawX != 0)
            {
                _dashDir = new Vector3(PlInputs.Inputs.RawX, 0, 0).normalized;

                _dashCooldownTimer = Time.time + _dashCooldown;
                _hasDashed = true;
                _dashing = true;
                _timeStartedDash = Time.time;
                _rb.gravityScale = 0;
                OnStartDashing?.Invoke();
            }
        }

        if(!_dashing) return; 
        if(Time.time >= _timeStartedDash + (_dashLength * StatsManager.Instance.DashLength.totalValue)) EndDash();
        
        if(!_dashing) return; 
        _rb.velocity = _dashDir * _dashSpeed;
        _rb.gravityScale = 0;

        if(rank < 2) return;
        gameObject.layer = 11;

        if(rank < 3) return;
        gameObject.layer = 12;
    }

    void EndDash() 
    {
        _dashing = false;
        _rb.velocity = new Vector3(Mathf.Clamp(_rb.velocity.x, PlInputs.Inputs.RawX * -_moddedWalkSpeed, PlInputs.Inputs.RawX * _moddedWalkSpeed), _rb.velocity.y > 3 ? 3 : _rb.velocity.y);
        _rb.gravityScale = _gravityScale;
        if(IsGrounded) _hasDashed = false;
        OnStopDashing?.Invoke();
        gameObject.layer = 10;
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

    public Vector3 GetAttackDirection() => Input.GetAxisRaw("Vertical") != 0 ? Input.GetAxisRaw("Vertical") > 0 ? this.transform.up : -this.transform.up : this.transform.right * -this.transform.localScale.x;

    public void Attack()
    {
        Vector3 attackPos = this.transform.position + ((GetAttackDirection() * (AttackRank > 1 ? _lastAttack.range : (_lastAttack.range /2))));
        float attackRotation = Quaternion.LookRotation(GetAttackDirection(), GetAttackDirection()).eulerAngles.x;
        Vector3 attackSize = AttackRank > 2 ? new Vector3(0.4f, 1.2f, _lastAttack.range) : AttackRank > 1 ? new Vector3(0.25f, 0.75f, _lastAttack.range) : new Vector3(0.25f, 0.75f, _lastAttack.range / 2);

        RaycastHit2D[] attackColliders = Physics2D.BoxCastAll(attackPos, attackSize, attackRotation, GetAttackDirection(), _lastAttack.range/2, _attackLayerMask);

        for(int i = 0; i < attackColliders.Length; i++)
        {
            float selfKnockbackReceived = 0;
            EnemyAttackTarget target;
            if (attackColliders[i].transform.TryGetComponent<EnemyAttackTarget>(out target))
            {
                target.ReceiveAttackCall(_lastAttack, transform.position);
                selfKnockbackReceived = target.selfKnockbackReceived;

                #region Vampirirism
                if(HealthRank > 2)
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
                _rb.AddForce(_lastAttack.selfKnockback * selfKnockbackReceived * -GetAttackDirection(), ForceMode2D.Force);
            }

            if(AttackRank < 3)
            {
                i = attackColliders.Length;
                attackSize = new Vector3(attackSize.x, attackSize.y, Vector2.Distance(attackPos, attackColliders[0].point));
            } 
        }

        if(attackColliders.Length < 1)
            if (PlInputs.Inputs.RawX == 0f) _rb.AddForce(GetAttackDirection() * _lastAttack.selfKnockback, ForceMode2D.Force);

        _soundEmitter.Stop();

        PlaySound(_lastAttack.sound);

        _attackFeedback.CallFeedback(attackSize, attackPos, attackRotation, _lastAttack.cooldown);
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
        CurrentHealth -= damageAmount;
        if(knockback != Vector3.zero)
        {
            // Hurt Sound
            if(_dashing) EndDash();
            float knockbackResistance = StatsManager.Instance.KnockbackResistance.totalValue;
            _rb.velocity = Vector3.zero;
            _rb.AddForce(new Vector2(knockback.x, knockback.y * 1.5f) * knockbackResistance, ForceMode2D.Force);
            _knockbackTimer = IsGrounded ? Time.time + (_knockbackTime * knockbackResistance) : Time.time + ((_knockbackTime + _extraUngroundedKnockbackTime) * knockbackResistance);
        }
        
        if(CurrentHealth < 1) OnPlayerDeath?.Invoke();
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

    #region UI Handling

    public void InterfacePlayerHP()
    {
        for(int i = 0; i < ModdedTotalHealth; i++)
        {
            if(Hearts.Count <= i)
            {
                Hearts.Add(new InstantiatedUIHP(Instantiate(HealthIconPrefab, UIScaler)));
            }
            else if(!Hearts[i].Prefab.activeSelf) Hearts[i].Prefab.SetActive(true);
            Hearts[i].rect.anchoredPosition = new Vector2(FullHDHealthIconsPivot.x * (i + 1), (FullHDHealthIconsPivot.y - 1080f));
            Hearts[i].image.color = i < CurrentHealth ? Color.white : Color.black;
        }

        if(ModdedTotalHealth < Hearts.Count)
        {
            for(int i = 0; i < Hearts.Count - ModdedTotalHealth; i++)
            {
                Hearts[Hearts.Count - i - 1].Prefab.SetActive(false);
            }
        }
    }

    #endregion
    
    public void PlaySound(AudioClip clipToPlay) => _soundEmitter.PlayOneShot(clipToPlay);

    public class InstantiatedUIHP
    {
        public GameObject Prefab;
        public Image image;
        public RectTransform rect;

        public InstantiatedUIHP(GameObject instantiatedPrefab)
        {
            Prefab = instantiatedPrefab;
            image = Prefab.GetComponent<Image>();
            rect = Prefab.GetComponent<RectTransform>();
        }
    }

    void ResetSkills() 
    { 
        MobilityRank = 0;
        AttackRank = 0;
        HookRank = 0;
        TantrumRank = 0;
        DashRank = 0;
        KnivesRank = 0;
        RangedRank = 0;
        ShieldRank = 0;
    }
}
