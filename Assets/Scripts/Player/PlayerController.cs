using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D _rb;
    private FrameInputs _inputs;
    private Vector3 _dir;

    [Header("CollisorDetections")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _grounderOffset = -1, _grounderRadius = 0.2f;
    float _fullHealHeight;
    public bool IsGrounded;
    public static event Action OnTouchedGround;
    private readonly Collider2D[] _ground = new Collider2D[1];

    [Header("Jumping")]
    public int JumpRank;
    [SerializeField] private float _initialJumpSpeed = 20, _fallingAcceleration = 7.5f, _timeUntilMaxFallingSpeed = 2, _coyoteTime = 0.2f, _jumpTime, _extraJumpTime, _gravityScale;
    private bool _hasJumped, _fallImpact, _doubleJumpCharged;
    private float _maxFallSpeed => (-_initialJumpSpeed - (_fallingAcceleration * _timeUntilMaxFallingSpeed)) * (CurrentHealth > 0 ? 1 : 3);
    float _timeLeftGrounded;
    public static event Action OnJump;

    [Header("Walking")]
    [SerializeField] private float _baseWalkSpeed = 8f;
    [SerializeField] private float _jumpManeuverabilityPercentage;
    float _moddedWalkSpeed { get => StatsManager.Instance.MoveSpeed.totalValue * _baseWalkSpeed; }

    [Header("Dashing")]
    public int DashRank;
    [SerializeField] private float _dashLength = 0.2f;
    [SerializeField] private float _dashCooldown = 3f, _dashSpeed = 30;
    float _dashCooldownTimer;
    public static event Action OnStartDashing, OnStopDashing;

    [Header("Combat")]
    public PlayerMeleeAttack DefaultAttack;
    [SerializeField] private List<PlayerMeleeAttack> _playerAttacks;
    [SerializeField] LayerMask _attackLayerMask;
    public int TotalHealth, CurrentHealth;
    [SerializeField] private float _knockbackTime, _selfKnockBackTime, _groundImpactKnockbackTime, _extraUngroundedKnockbackTime;
    [SerializeField] private float _timeOfLastAttack = 10f;
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
    [SerializeField] Animator _myAnimator;
    [SerializeField] AudioSource _soundEmitter;
    public Transform UIScaler;

    [Header("Audios")]
    public List<AudioClip> Audioclips;

    [Header("UI")]
    public GameObject HealthIconPrefab;
    public Vector2 FullHDHealthIconsPivot;
    List<InstantiatedUIHP> Hearts;

    private bool _hasDashed, _dashing;
    private float _timeStartedDash;
    private Vector3 _dashDir;

    public Inventory Inventory;
    FootStepController FootStepController;

    void Start()
    {
        Hearts = new List<InstantiatedUIHP>();
        _timeOfLastAttack = 0;
        _rb = GetComponent<Rigidbody2D>();
        _lastAttack = DefaultAttack;
        FootStepController = GetComponentInChildren<FootStepController>();
        _fullHealHeight = transform.position.y + 2f;
        InterfacePlayerHP();

        OnPlayerDeath += PlayerDeath;
        OnPlayerFullHealth += PlayerFullHeal;
    }

    void Update()
    {
        if(!(Time.timeScale > 0)) return;
        GatherInputs();
        HandleGrounding();
        HandleJumping();

        if (!IsKnockbacked || !_myAnimator.GetBool("FellDown"))
        {
            HandleWalking();
            HandleDashing(DashRank);
        }

        HandleAnimation();
    }

    void GatherInputs()
    {
        _inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        _inputs.X = Input.GetAxis("Horizontal");
        _dir = new Vector3(_inputs.RawX, 0, 0);

        if (_inputs.X != 0) SetFacingDirection(_inputs.X < 0);

        if(IsKnockbacked || _myAnimator.GetBool("FellDown")) return;
        if (Input.GetButtonDown("Fire1")) ExecuteAttack();
        if (Input.GetButtonDown("Fire2")) ExecuteInteraction(); 

        if(Input.GetButtonDown("Jump"))
        {
            if(!_hasJumped)
            {
                if(IsGrounded) ExecuteJump(false);
                else if(Time.time < _timeLeftGrounded + _coyoteTime) ExecuteJump(true);
                else if(_doubleJumpCharged) 
                {
                    _doubleJumpCharged = false;
                    ExecuteJump(true);
                }
            }
        }
    }

    void SetFacingDirection(bool left) => this.transform.localScale = left ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

    #region ColisorDetections

    private void HandleGrounding()
    {
        var grounded = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(0, _grounderOffset, 0), _grounderRadius, _ground, _groundMask) > 0;

        if(!IsGrounded && grounded)
        {
            if(CurrentHealth < TotalHealth && transform.position.y < _fullHealHeight) OnPlayerFullHealth?.Invoke(); 

            if(0 < CurrentHealth)
            {
                if(_fallImpact) 
                {
                    _knockbackTimer = Time.time + _groundImpactKnockbackTime;
                    _myAnimator.SetBool("FellDown", true);
                    PlaySound(Audioclips[1]);
                }
                else FootStepController.PlayOneShot(FootStepController.RandomSolidClip(), 0.5f);
            }

            if(JumpRank > 2) _doubleJumpCharged = true;
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

    #endregion

    #region Animation

    void HandleAnimation()
    {
        _myAnimator.SetBool("Grounded", IsGrounded);
        _myAnimator.SetFloat("VerticalSpeed", _rb.velocity.y);
    }

    #endregion

    #region Walking

    private void HandleWalking()
    {
        if(_dir.x != 0) _myAnimator.SetBool("FellDown", false);

        _rb.velocity = IsGrounded
            ? new Vector2(_moddedWalkSpeed * _dir.x, _rb.velocity.y)
            : new Vector2(_moddedWalkSpeed * _jumpManeuverabilityPercentage * _dir.x, _rb.velocity.y);

        _myAnimator.SetFloat("Speed", Mathf.Abs(_baseWalkSpeed * _dir.x));
    }

    #endregion

    #region Jumping

    private void HandleJumping()
    {
        if (Mathf.Approximately(_rb.velocity.y, 0) && !IsGrounded) _hasJumped = false;
        if(_rb.velocity.y < 0) _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Clamp(_rb.velocity.y, _maxFallSpeed, 0));
        _fallImpact = _rb.velocity.y <= _maxFallSpeed * 0.95f;

        if (_hasJumped)
        {
            if ((Input.GetButton("Jump") && Time.time < _timeLeftGrounded + _extraJumpTime + _jumpTime))
            {
                _rb.gravityScale = 0;
            }
            else if (Time.time > _timeLeftGrounded + _jumpTime) _hasJumped = false;
        }
        else if(!_dashing) _rb.gravityScale = _gravityScale;
    }

    void ExecuteJump(bool coyoteJump)
    {
        PlaySound(Audioclips[0]);
        _myAnimator.SetBool("FellDown", false);
        _timeLeftGrounded = coyoteJump? _timeLeftGrounded : Time.time;
        _hasJumped = true;
        _rb.velocity = new Vector2(_rb.velocity.x, _initialJumpSpeed);
        OnJump?.Invoke();
    }

    #endregion

    #region Dash

    public void HandleDashing(int rank)
    {
        if(rank < 1) return;

        if (_dashCooldownTimer < Time.time)
        {
            if (Input.GetButton("Fire3") && !_hasDashed && _dir.x != 0)
            {
                _dashDir = new Vector3(_inputs.RawX, 0, 0).normalized;

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
        _rb.velocity = new Vector3(Mathf.Clamp(_rb.velocity.x, _dir.normalized.x * -_moddedWalkSpeed, _dir.normalized.x * _moddedWalkSpeed), _rb.velocity.y > 3 ? 3 : _rb.velocity.y);
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
        Vector3 attackPos = this.transform.position + ((GetAttackDirection() * (_lastAttack.range /2)));
        float attackRotation = Quaternion.LookRotation(GetAttackDirection(), GetAttackDirection()).eulerAngles.x;
        Vector3 attackSize = new Vector3(0.25f, 0.75f, _lastAttack.range / 2);
        RaycastHit2D attackCollider = Physics2D.BoxCast(attackPos, attackSize, attackRotation, GetAttackDirection(), _lastAttack.range/2, _attackLayerMask);

        if (attackCollider)
        {
            attackSize = new Vector3(attackSize.x, attackSize.y, Vector2.Distance(attackPos, attackCollider.point));
            float selfKnockbackReceived = 0;
            EnemyAttackTarget target = attackCollider.transform.GetComponent<EnemyAttackTarget>();
            if (target != null)
            {
                target.ReceiveAttackCall(_lastAttack, transform.position);
                selfKnockbackReceived = target.selfKnockbackReceived;
            }

            if (selfKnockbackReceived != 0)
            { 
                _rb.velocity = Vector3.zero;
                float multiplier = Mathf.Clamp(selfKnockbackReceived, 0, 1);
                _knockbackTimer = Time.time + (_selfKnockBackTime * multiplier);
                _rb.AddForce(_lastAttack.selfKnockback * selfKnockbackReceived * -GetAttackDirection(), ForceMode2D.Force);
            }
        }
        else if (Mathf.Approximately(_inputs.X, 0f)) _rb.AddForce(GetAttackDirection() * _lastAttack.selfKnockback, ForceMode2D.Force);

        _soundEmitter.Stop();

        PlaySound(_lastAttack.sound);

        _attackFeedback.CallFeedback(attackSize, attackPos, attackRotation, _lastAttack.cooldown);
        _myAnimator.SetTrigger(_lastAttack.animatorTrigger);
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

    public void ReceiveDamage(int damage, Vector3 knockback)
    {
        if(_dashing) EndDash();

        float knockbackResistance = StatsManager.Instance.KnockbackResistance.totalValue;
        CurrentHealth -= damage;
        _rb.velocity = Vector3.zero;
        _rb.AddForce(new Vector2(knockback.x, knockback.y * 1.5f) * knockbackResistance, ForceMode2D.Force);
        _knockbackTimer = IsGrounded ? Time.time + (_knockbackTime * knockbackResistance) : Time.time + ((_knockbackTime + _extraUngroundedKnockbackTime) * knockbackResistance);
        
        InterfacePlayerHP();
        if(CurrentHealth < 1) OnPlayerDeath?.Invoke();
    }

    void PlayerFullHeal()
    { 
        CurrentHealth = TotalHealth;
        gameObject.layer = 10;
        _knockbackTimer = 0;
        InterfacePlayerHP();
    }

    void PlayerDeath()
    {
        _myAnimator.SetFloat("Speed", 0);
        _myAnimator.SetBool("FellDown", true);
        gameObject.layer = 7;
        _knockbackTimer = Mathf.Infinity;
    }

    #endregion

    #region Interactions

    void ExecuteInteraction()
    {
        Collider2D[] interactionColliders = Physics2D.OverlapCircleAll(this.transform.position, InteractionRadius, InteractionLayer);

        foreach (Collider2D col in interactionColliders)
        {
            col.GetComponent<Interactable>().Interact();
        }
    }

    #endregion

    #region UI Handling

    void InterfacePlayerHP()
    {
        for(int i = 0; i < TotalHealth; i++)
        {
            if(Hearts.Count <= i)
            {
                Hearts.Add(new InstantiatedUIHP(Instantiate(HealthIconPrefab, UIScaler)));
            }
            Hearts[i].rect.anchoredPosition = new Vector2(FullHDHealthIconsPivot.x * (i + 1), (FullHDHealthIconsPivot.y - 1080f));
            Hearts[i].image.color = i < CurrentHealth ? Color.white : Color.black;
        }
    }

    #endregion
    
    public void PlaySound(AudioClip clipToPlay) => _soundEmitter.PlayOneShot(clipToPlay);

    private struct FrameInputs
    {
        public float X;
        public int RawX;
    }

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

    private void OnApplicationQuit() => Inventory.Container.Clear();
}
