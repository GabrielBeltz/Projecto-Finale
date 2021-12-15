using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    //private Animator animator;
    private FrameInputs inputs;
    private Vector3 dir;
    private bool facingLeft;
    [SerializeField] private Collider2D limboTrigger;

    [Header("CollisorDetections")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float grounderOffset = -1, grounderRadius = 0.2f;
    public bool IsGrounded;
    public static event Action OnTouchedGround;
    private readonly Collider2D[] _ground = new Collider2D[1];

    [Header("Jumping")]
    [SerializeField] private float initialJumpSpeed = 20;
    [SerializeField] private float minJumpSpeed = 15;
    private float maxFallSpeed
    {
        get
        {
            return -minJumpSpeed - (fallingAcceleration * timeUntilMaxFallingSpeed);
        }
    }
    [SerializeField] private float fallingAcceleration = 7.5f;
    [SerializeField] private float timeUntilMaxFallingSpeed = 2;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpTime;
    [SerializeField] private float extraJumpTime;
    float timeLeftGrounded;
    [SerializeField] private bool hasJumped;
    public static event Action OnJump;

    [Header("Walking")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float _acceleration = 2f;
    [SerializeField] private float maxWalkingPenalty = 0.1f;
    float currentMovementLerpSpeed = 100;
    [SerializeField] private float currentWalkingPenalty;
    [SerializeField] private float jumpManeuverabilityPercentage;

    [Header("Dashing")]
    [SerializeField] private float dashLength = 0.2f;
    [SerializeField] private float dashCooldown = 3f;
    float dashSpeed = 30, dashCooldownTimer;
    public static event Action OnStartDashing, OnStopDashing;

    [Header("Combat")]
    public int TotalHealth;
    public int currentHealth;
    [SerializeField] private float knockbackTime, selfKnockBackTime, groundImpactKnockbackTime;
    [SerializeField] private float extraUngroundedKnockbackTime;
    [SerializeField] private PlayerMeleeAttack defaultAttack;
    [SerializeField] private List<PlayerMeleeAttack> playerAttacks;
    [SerializeField] private float timeOfLastAttack = 10f;
    [SerializeField] LayerMask attackLayerMask;
    [SerializeField] AttackFeedback attackFeedback;
    PlayerMeleeAttack lastAttack;
    float knockbackTimer;
    bool downwardAttack;
    public bool isKnockbacked
    {
        get
        {
            return knockbackTimer > Time.time;
        }
    }

    [Header("Interactions")]
    public float interactionRadius;
    public LayerMask interactionLayer;
    
    [Header("Scene References")]
    [SerializeField] Animator myAnimator;
    [SerializeField] AudioSource soundEmitter;
    [SerializeField] LimboController limboController;

    [Header("Audios")]
    public List<AudioClip> audioclips;

    private bool hasDashed;
    private bool dashing;
    private float timeStartedDash, lastXInput;
    private Vector3 dashDir;

    public Inventory inventory;

    void Start()
    {
        timeOfLastAttack = 0;
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
        lastAttack = defaultAttack;
    }

    void Update()
    {
        GatherInputs();

        HandleGrounding();
        
        if (!isKnockbacked)
        {
            HandleWalking();

            HandleJumping();

            //HandleDashing();
        }
    }

    private void FixedUpdate()
    {
        if (downwardAttack && Input.GetButton("Fire1"))
        {
            Attack();
        }
    }

    private void GatherInputs()
    {
        inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        inputs.X = Input.GetAxis("Horizontal");

        if (inputs.X != lastXInput)
        {
            SetFacingDirection(inputs.X < 0);
        }

        dir = new Vector3(inputs.RawX, 0, 0);

        if (Input.GetButtonDown("Fire1") && !isKnockbacked && !limboController.IsInLimboMode)
        {
            ExecuteAttack();
        }

        if (downwardAttack && Input.GetButtonUp("Fire1"))
        {
            downwardAttack = false;
            Attack();
        }

        if (Input.GetButtonDown("Fire2"))
        {
            ExecuteInteraction();   
        }

        //if(dir != Vector3.zero) animator.transform.forward = _dir;
        //animator.SetInteger("RawY", (int)inputs.RawY);

        //facingLeft = inputs.RawX != 1 && (inputs.RawX == -1 || facingLeft);
    }

    private void SetFacingDirection(bool left)
    {
        this.transform.localScale = left ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }

    #region ColisorDetections

    private void HandleGrounding()
    {
        var grounded = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(0, grounderOffset), grounderRadius, _ground, groundMask) > 0;

        if(!IsGrounded && grounded)
        {
            if (Mathf.Approximately( Mathf.Round(rb.velocity.y), maxFallSpeed))
            {
                knockbackTimer = Time.time + groundImpactKnockbackTime;
            }

            IsGrounded = true;
            hasDashed = false;
            hasJumped = false;
            currentMovementLerpSpeed = 100;
            //animator.SetBool("Grounded", true);
            OnTouchedGround?.Invoke();
            //transform.SetParent(_ground[0].transform);
        }
        else if(IsGrounded && !grounded)
        {
            IsGrounded = false;
            timeLeftGrounded = Time.time;
            //animator.SetBool("Grounded", false);
            //transform.SetParent(null);
        }
    }

    #endregion

    #region Walking

    private void HandleWalking()
    {
        var normalizedDir = dir.normalized;

        if(dir != Vector3.zero)
        {
            currentWalkingPenalty += _acceleration * Time.deltaTime;
        }
        else
        {
            currentWalkingPenalty -= maxWalkingPenalty * Time.deltaTime;
        }
        currentWalkingPenalty = Mathf.Clamp(currentWalkingPenalty, maxWalkingPenalty, 2f);

        var targetVel = new Vector3(normalizedDir.x * currentWalkingPenalty * walkSpeed, rb.velocity.y, normalizedDir.z);

        var idealVel = new Vector3(targetVel.x, rb.velocity.y, targetVel.z);

        if (IsGrounded)
        {
            rb.velocity = Vector3.MoveTowards(rb.velocity, idealVel, currentMovementLerpSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 jumpIdealVel = new Vector3(idealVel.x * jumpManeuverabilityPercentage, idealVel.y, idealVel.z * jumpManeuverabilityPercentage);
            rb.velocity = Vector3.MoveTowards(rb.velocity, jumpIdealVel, currentMovementLerpSpeed * Time.deltaTime);
        }

        myAnimator.SetFloat("Speed", Mathf.Abs(targetVel.x));
    }

    #endregion

    #region Jumping

    private void HandleJumping()
    {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(IsGrounded || Time.time < timeLeftGrounded + coyoteTime)
            {
                if(!hasJumped)
                {
                    ExecuteJump(new Vector2(rb.velocity.x, minJumpSpeed));
                }
            }
        }

        void ExecuteJump(Vector3 dir)
        {
            rb.velocity = new Vector2(rb.velocity.x, initialJumpSpeed);
            PlaySound(audioclips.Find(audioClip => audioClip.name == "Jump"));
            hasJumped = true;
            myAnimator.SetTrigger("Jump");
            OnJump?.Invoke();
            timeLeftGrounded = Time.time;
        }

        if (Mathf.Approximately(rb.velocity.y, 0) && !IsGrounded)
        {
            hasJumped = false;
        }

        if (hasJumped)
        {
            float newjumpSpeed = Mathf.Clamp(rb.velocity.y, minJumpSpeed, initialJumpSpeed);
            // Se o jogador segurar o espa�o, o pulo continua normalmente. Se n�o, aplica for�a pra diminuir o pulo dele.
            if (!(Input.GetKey(KeyCode.Space) && Time.time < timeLeftGrounded + extraJumpTime + jumpTime))
            {
                if (Time.time > timeLeftGrounded + jumpTime)
                {
                    hasJumped = false;
                }
                else
                {
                    rb.gravityScale = 1;
                    rb.velocity = new Vector2(rb.velocity.x, newjumpSpeed);
                }
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, newjumpSpeed);
                rb.gravityScale = 1;
            }
        }
        else if(!isKnockbacked)
        {
            rb.gravityScale = 1;
            float fallVelocity = Mathf.Clamp(-minJumpSpeed - ((Time.time - timeLeftGrounded) * fallingAcceleration), maxFallSpeed, -minJumpSpeed);
            rb.velocity = new Vector2(rb.velocity.x, fallVelocity);
            ;
        }
    }

    #endregion

    #region Dash

    public void HandleDashing()
    {
        if (dashCooldownTimer < Time.time)
        {
            if (Input.GetKeyDown(KeyCode.E) && !hasDashed && dir.x != 0)
            {
                dashDir = new Vector3(inputs.RawX, 0, 0).normalized;
                //if(dashDir == Vector3.zero) dashDir = animator.transform.forward;

                dashCooldownTimer = Time.time + dashCooldown;
                hasDashed = true;
                dashing = true;
                timeStartedDash = Time.time;
                rb.gravityScale = 0;
                OnStartDashing?.Invoke();
            }
        }

        if(dashing)
        {
            rb.velocity = dashDir * dashSpeed;

            if(Time.time >= timeStartedDash + dashLength)
            {
                dashing = false;
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y > 3 ? 3 : rb.velocity.y);
                rb.gravityScale = 1;
                if(IsGrounded) hasDashed = false;
                OnStopDashing?.Invoke();
            }
        }
    }
    #endregion

    #region Combat

    public void ExecuteAttack()
    {
        if (timeOfLastAttack < Time.time)
        {
            downwardAttack = false;
            Vector2 attackInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            lastAttack = GetNextAttack(attackInput);
            timeOfLastAttack = Time.time + lastAttack.cooldown;
            Attack();
        }
    }

    public Vector3 GetAttackDirection()
    {
        float rawY = Input.GetAxisRaw("Vertical");
        if (rawY != 0)
        {
            if (rawY > 0)
            {
                return this.transform.up;
            }
            else
            {
                return -this.transform.up;
            }
        }
        else
        {
            return this.transform.right * -this.transform.localScale.x;
        }
    }

    public void Attack()
    {
        Vector3 attackPos = this.transform.position + ((GetAttackDirection() * (lastAttack.range /2)));
        float attackRotation = Quaternion.LookRotation(GetAttackDirection(), GetAttackDirection()).eulerAngles.x;
        Vector3 attackSize = new Vector3(0.25f, 0.75f, lastAttack.range / 2);
        RaycastHit2D attackCollider = Physics2D.BoxCast(attackPos, attackSize, attackRotation, GetAttackDirection(), lastAttack.range/2, attackLayerMask);
        bool playSound = !downwardAttack;

        if (attackCollider)
        {
            attackSize = new Vector3(attackSize.x, attackSize.y, Vector2.Distance(attackPos, attackCollider.point));
            downwardAttack = false;
            playSound = !downwardAttack;
            float selfKnockbackReceived = 0;
            EnemyAttackTarget target = attackCollider.transform.GetComponent<EnemyAttackTarget>();
            if (target != null)
            {
                target.ReceiveAttack(lastAttack, this.transform.position);
                selfKnockbackReceived = target.selfKnockbackReceived;
            }

            if (selfKnockbackReceived != 0)
            {
                rb.velocity = Vector3.zero;
                float multiplier = Mathf.Clamp(selfKnockbackReceived, 0, 1);
                knockbackTimer = Time.time + (selfKnockBackTime * multiplier);
            }
            rb.AddForce(-GetAttackDirection() * lastAttack.selfKnockback * selfKnockbackReceived, ForceMode2D.Force);
        }
        else
        {
            if (GetAttackDirection() == Vector3.down)
            {
                downwardAttack = true;
            }

            if (Mathf.Approximately(inputs.X, 0f))
            {
                rb.AddForce(GetAttackDirection() * lastAttack.selfKnockback, ForceMode2D.Force);
            }
        }

        soundEmitter.Stop();

        if (playSound)
        {
            PlaySound(lastAttack.sound);
        }

        attackFeedback.CallFeedback(attackSize, attackPos, attackRotation, lastAttack.cooldown);
        myAnimator.SetTrigger(lastAttack.animatorTrigger);
    }

    public PlayerMeleeAttack GetNextAttack(Vector2 input)
    {
        if (input.x >= 0 && input.y == 0)
        {
            PlayerMeleeAttack foundAttack = playerAttacks.Find(attck => attck.comboInfo.previousAttack == lastAttack.name);
            if (foundAttack == null)
            {
                return defaultAttack;
            }
            else if (timeOfLastAttack - lastAttack.cooldown + foundAttack.comboInfo.timeBetween < Time.time)
            {
                return foundAttack;
            }
        }
        else if (input.y > 0)
        {
            PlayerMeleeAttack foundAttack = playerAttacks.Find(attck => attck.comboInfo.previousAttack == lastAttack.name && attck.direction == AttackDirection.Up);
            if (foundAttack == null)
            {
                foundAttack = playerAttacks.Find(attck => attck.direction == AttackDirection.Up);
                return foundAttack;
            }
            else if (timeOfLastAttack - lastAttack.cooldown + foundAttack.comboInfo.timeBetween < Time.time)
            {
                return foundAttack;
            }
        }
        else if (input.y < 0)
        {
            PlayerMeleeAttack foundAttack = playerAttacks.Find(attck => attck.comboInfo.previousAttack == lastAttack.name && attck.direction == AttackDirection.Down);
            if (foundAttack == null)
            {
                foundAttack = playerAttacks.Find(attck => attck.direction == AttackDirection.Down);
                return foundAttack;
            }
            else if (timeOfLastAttack - lastAttack.cooldown + foundAttack.comboInfo.timeBetween < Time.time)
            {
                return foundAttack;
            }
        }

        return defaultAttack;
    }

    public void ReceiveDamage(int damage, Vector3 knockback)
    {
        currentHealth -= damage;
        rb.velocity = Vector3.zero;
        rb.AddForce(new Vector2(knockback.x, knockback.y/2), ForceMode2D.Force);
        if (IsGrounded)
        {
            knockbackTimer = Time.time + knockbackTime;
        }
        else
        {
            knockbackTimer = Time.time + knockbackTime + extraUngroundedKnockbackTime;
        }
    }

    #endregion

    #region Interactions

    void ExecuteInteraction()
    {
        Collider2D[] interactionColliders = Physics2D.OverlapCircleAll(this.transform.position, interactionRadius, interactionLayer);

        foreach (Collider2D col in interactionColliders)
        {
            col.GetComponent<Interactable>().Interact();
        }
    }

    #endregion

    public void PlaySound(AudioClip clipToPlay)
    {
        if (soundEmitter.isPlaying)
        {
            soundEmitter.Stop();
        }

        soundEmitter.clip = clipToPlay;
        soundEmitter.Play();
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
            inventory.AddItem(item.item, 1);
            Destroy(collision.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        inventory.Container.Clear();
    }
}
