using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D _rb;
    private FrameInputs _inputs;
    private Vector3 _dir;

    [Header("CollisorDetections")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _grounderOffset = -1, _grounderRadius = 0.2f;
    public bool IsGrounded;
    public static event Action OnTouchedGround;
    private readonly Collider2D[] _ground = new Collider2D[1];

    [Header("Jumping")]
    [SerializeField] private float _initialJumpSpeed = 20;
    [SerializeField] private float _minJumpSpeed = 15, _fallingAcceleration = 7.5f, _timeUntilMaxFallingSpeed = 2, _coyoteTime = 0.2f, _jumpTime, _extraJumpTime;
    [SerializeField] private bool _hasJumped;
    private float _maxFallSpeed => -_minJumpSpeed - (_fallingAcceleration * _timeUntilMaxFallingSpeed);
    float _timeLeftGrounded;
    public static event Action OnJump;

    [Header("Walking")]
    [SerializeField] private float _walkSpeed = 8f;
    [SerializeField] private float _acceleration = 2f, _maxWalkingPenalty = 0.1f, _currentWalkingPenalty, _jumpManeuverabilityPercentage;
    float _currentMovementLerpSpeed = 100;

    [Header("Dashing")]
    [SerializeField] private float _dashLength = 0.2f;
    [SerializeField] private float _dashCooldown = 3f, _dashSpeed = 30;
    float _dashCooldownTimer;
    public static event Action OnStartDashing, OnStopDashing;

    [Header("Combat")]
    [SerializeField] LayerMask _attackLayerMask;
    public int TotalHealth, CurrentHealth;
    [SerializeField] private float _knockbackTime, _selfKnockBackTime, _groundImpactKnockbackTime, _extraUngroundedKnockbackTime;
    [SerializeField] private PlayerMeleeAttack _defaultAttack;
    [SerializeField] private List<PlayerMeleeAttack> _playerAttacks;
    [SerializeField] private float _timeOfLastAttack = 10f;
    [SerializeField] AttackFeedback _attackFeedback;
    PlayerMeleeAttack _lastAttack;
    float _knockbackTimer;
    bool _downwardAttack;
    public bool IsKnockbacked  => _knockbackTimer > Time.time;

    [Header("Interactions")]
    public float InteractionRadius;
    public LayerMask InteractionLayer;
    
    [Header("Scene References")]
    [SerializeField] Animator _myAnimator;
    [SerializeField] AudioSource _soundEmitter;

    [Header("Audios")]
    public List<AudioClip> Audioclips;

    private bool _hasDashed, _dashing;
    private float _timeStartedDash;
    private Vector3 _dashDir;

    public Inventory Inventory;

    void Start()
    {
        _timeOfLastAttack = 0;
        _rb = GetComponent<Rigidbody2D>();
        _lastAttack = _defaultAttack;
    }

    void Update()
    {
        GatherInputs();

        HandleGrounding();
        
        if (!IsKnockbacked)
        {
            HandleWalking();

            HandleJumping();

            //HandleDashing();
        }
    }

    private void FixedUpdate()
    {
        if (_downwardAttack && Input.GetButton("Fire1"))
        {
            Attack();
        }
    }

    private void GatherInputs()
    {
        _inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        _inputs.X = Input.GetAxis("Horizontal");

        if (_inputs.X != 0)
        {
            SetFacingDirection(_inputs.X < 0);
        }

        _dir = new Vector3(_inputs.RawX, 0, 0);

        if (Input.GetButtonDown("Fire1") && !IsKnockbacked)
        {
            ExecuteAttack();
        }

        if (_downwardAttack && Input.GetButtonUp("Fire1"))
        {
            _downwardAttack = false;
            Attack();
        }

        if (Input.GetButtonDown("Fire2"))
        {
            ExecuteInteraction();   
        }
    }

    private void SetFacingDirection(bool left)
    {
        this.transform.localScale = left ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }

    #region ColisorDetections

    private void HandleGrounding()
    {
        var grounded = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(0, _grounderOffset), _grounderRadius, _ground, _groundMask) > 0;

        if(!IsGrounded && grounded)
        {
            if (Mathf.Approximately( Mathf.Round(_rb.velocity.y), _maxFallSpeed))
            {
                _knockbackTimer = Time.time + _groundImpactKnockbackTime;
            }

            IsGrounded = true;
            _hasDashed = false;
            _hasJumped = false;
            _currentMovementLerpSpeed = 100;
            OnTouchedGround?.Invoke();
        }
        else if(IsGrounded && !grounded)
        {
            IsGrounded = false;
            _timeLeftGrounded = Time.time;
        }
    }

    #endregion

    #region Walking

    private void HandleWalking()
    {
        var normalizedDir = _dir.normalized;

        if(_dir != Vector3.zero)
        {
            _currentWalkingPenalty += _acceleration * Time.deltaTime;
        }
        else
        {
            _currentWalkingPenalty -= _maxWalkingPenalty * Time.deltaTime;
        }
        _currentWalkingPenalty = Mathf.Clamp(_currentWalkingPenalty, _maxWalkingPenalty, 2f);

        var targetVel = new Vector3(normalizedDir.x * _currentWalkingPenalty * _walkSpeed, _rb.velocity.y, normalizedDir.z);

        var idealVel = new Vector3(targetVel.x, _rb.velocity.y, targetVel.z);

        if (IsGrounded)
        {
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, idealVel, _currentMovementLerpSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 jumpIdealVel = new Vector3(idealVel.x * _jumpManeuverabilityPercentage, idealVel.y, idealVel.z * _jumpManeuverabilityPercentage);
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, jumpIdealVel, _currentMovementLerpSpeed * Time.deltaTime);
        }

        _myAnimator.SetFloat("Speed", Mathf.Abs(targetVel.x));
    }

    #endregion

    #region Jumping

    private void HandleJumping()
    {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(IsGrounded || Time.time < _timeLeftGrounded + _coyoteTime)
            {
                if(!_hasJumped)
                {
                    ExecuteJump(new Vector2(_rb.velocity.x, _minJumpSpeed));
                }
            }
        }

        void ExecuteJump(Vector3 dir)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _initialJumpSpeed);
            PlaySound(Audioclips.Find(audioClip => audioClip.name == "Jump"));
            _hasJumped = true;
            _myAnimator.SetTrigger("Jump");
            OnJump?.Invoke();
            _timeLeftGrounded = Time.time;
        }

        if (Mathf.Approximately(_rb.velocity.y, 0) && !IsGrounded)
        {
            _hasJumped = false;
        }

        if (_hasJumped)
        {
            float newjumpSpeed = Mathf.Clamp(_rb.velocity.y, _minJumpSpeed, _initialJumpSpeed);
            // Se o jogador segurar o espa�o, o pulo continua normalmente. Se n�o, aplica for�a pra diminuir o pulo dele.
            if (!(Input.GetKey(KeyCode.Space) && Time.time < _timeLeftGrounded + _extraJumpTime + _jumpTime))
            {
                if (Time.time > _timeLeftGrounded + _jumpTime)
                {
                    _hasJumped = false;
                }
                else
                {
                    _rb.gravityScale = 1;
                    _rb.velocity = new Vector2(_rb.velocity.x, newjumpSpeed);
                }
            }
            else
            {
                _rb.velocity = new Vector2(_rb.velocity.x, newjumpSpeed);
                _rb.gravityScale = 1;
            }
        }
        else if(!IsKnockbacked)
        {
            _rb.gravityScale = 1;
            float fallVelocity = Mathf.Clamp(-_minJumpSpeed - ((Time.time - _timeLeftGrounded) * _fallingAcceleration), _maxFallSpeed, -_minJumpSpeed);
            _rb.velocity = new Vector2(_rb.velocity.x, fallVelocity);
        }
    }

    #endregion

    #region Dash

    public void HandleDashing()
    {
        if (_dashCooldownTimer < Time.time)
        {
            if (Input.GetKeyDown(KeyCode.E) && !_hasDashed && _dir.x != 0)
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

        if(_dashing)
        {
            _rb.velocity = _dashDir * _dashSpeed;

            if(Time.time >= _timeStartedDash + _dashLength)
            {
                _dashing = false;
                _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y > 3 ? 3 : _rb.velocity.y);
                _rb.gravityScale = 1;
                if(IsGrounded) _hasDashed = false;
                OnStopDashing?.Invoke();
            }
        }
    }
    #endregion

    #region Combat

    public void ExecuteAttack()
    {
        if (_timeOfLastAttack < Time.time)
        {
            _downwardAttack = false;
            Vector2 attackInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _lastAttack = GetNextAttack(attackInput);
            _timeOfLastAttack = Time.time + _lastAttack.cooldown;
            Attack();
        }
    }

    public Vector3 GetAttackDirection()
    {
        float rawY = Input.GetAxisRaw("Vertical");
        return rawY != 0 ? rawY > 0 ? this.transform.up : -this.transform.up : this.transform.right * -this.transform.localScale.x;
    }

    public void Attack()
    {
        Vector3 attackPos = this.transform.position + ((GetAttackDirection() * (_lastAttack.range /2)));
        float attackRotation = Quaternion.LookRotation(GetAttackDirection(), GetAttackDirection()).eulerAngles.x;
        Vector3 attackSize = new Vector3(0.25f, 0.75f, _lastAttack.range / 2);
        RaycastHit2D attackCollider = Physics2D.BoxCast(attackPos, attackSize, attackRotation, GetAttackDirection(), _lastAttack.range/2, _attackLayerMask);
        bool playSound = !_downwardAttack;

        if (attackCollider)
        {
            attackSize = new Vector3(attackSize.x, attackSize.y, Vector2.Distance(attackPos, attackCollider.point));
            _downwardAttack = false;
            playSound = !_downwardAttack;
            float selfKnockbackReceived = 0;
            EnemyAttackTarget target = attackCollider.transform.GetComponent<EnemyAttackTarget>();
            if (target != null)
            {
                target.ReceiveAttack(_lastAttack, this.transform.position);
                selfKnockbackReceived = target.selfKnockbackReceived;
            }

            if (selfKnockbackReceived != 0)
            {
                _rb.velocity = Vector3.zero;
                float multiplier = Mathf.Clamp(selfKnockbackReceived, 0, 1);
                _knockbackTimer = Time.time + (_selfKnockBackTime * multiplier);
            }
            _rb.AddForce(_lastAttack.selfKnockback * selfKnockbackReceived * -GetAttackDirection(), ForceMode2D.Force);
        }
        else
        {
            if (GetAttackDirection() == Vector3.down)
            {
                _downwardAttack = true;
            }

            if (Mathf.Approximately(_inputs.X, 0f))
            {
                _rb.AddForce(GetAttackDirection() * _lastAttack.selfKnockback, ForceMode2D.Force);
            }
        }

        _soundEmitter.Stop();

        if (playSound)
        {
            PlaySound(_lastAttack.sound);
        }

        _attackFeedback.CallFeedback(attackSize, attackPos, attackRotation, _lastAttack.cooldown);
        _myAnimator.SetTrigger(_lastAttack.animatorTrigger);
    }

    public PlayerMeleeAttack GetNextAttack(Vector2 input)
    {
        if (input.x >= 0 && input.y == 0)
        {
            PlayerMeleeAttack foundAttack = _playerAttacks.Find(attck => attck.comboInfo.previousAttack == _lastAttack.name);
            if (foundAttack == null)
            {
                return _defaultAttack;
            }
            else if (_timeOfLastAttack - _lastAttack.cooldown + foundAttack.comboInfo.timeBetween < Time.time)
            {
                return foundAttack;
            }
        }
        else if (input.y > 0)
        {
            PlayerMeleeAttack foundAttack = _playerAttacks.Find(attck => attck.comboInfo.previousAttack == _lastAttack.name && attck.direction == AttackDirection.Up);
            if (foundAttack == null)
            {
                foundAttack = _playerAttacks.Find(attck => attck.direction == AttackDirection.Up);
                return foundAttack;
            }
            else if (_timeOfLastAttack - _lastAttack.cooldown + foundAttack.comboInfo.timeBetween < Time.time)
            {
                return foundAttack;
            }
        }
        else if (input.y < 0)
        {
            PlayerMeleeAttack foundAttack = _playerAttacks.Find(attck => attck.comboInfo.previousAttack == _lastAttack.name && attck.direction == AttackDirection.Down);
            if (foundAttack == null)
            {
                foundAttack = _playerAttacks.Find(attck => attck.direction == AttackDirection.Down);
                return foundAttack;
            }
            else if (_timeOfLastAttack - _lastAttack.cooldown + foundAttack.comboInfo.timeBetween < Time.time)
            {
                return foundAttack;
            }
        }

        return _defaultAttack;
    }

    public void ReceiveDamage(int damage, Vector3 knockback)
    {
        CurrentHealth -= damage;
        _rb.velocity = Vector3.zero;
        _rb.AddForce(new Vector2(knockback.x, knockback.y/2), ForceMode2D.Force);
        _knockbackTimer = IsGrounded ? Time.time + _knockbackTime : Time.time + _knockbackTime + _extraUngroundedKnockbackTime;
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

    public void PlaySound(AudioClip clipToPlay)
    {
        if (_soundEmitter.isPlaying)
        {
            _soundEmitter.Stop();
        }

        _soundEmitter.clip = clipToPlay;
        _soundEmitter.Play();
    }

    private struct FrameInputs
    {
        public float X;
        public int RawX;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var item = collision.GetComponent<Item>();
        if(item)
        {
            Inventory.AddItem(item.item, 1);
            Destroy(collision.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        Inventory.Container.Clear();
    }
}
