using System;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public bool HasDashed, Dashing;
    float _timeStartedDash;
    Vector3 _dashDir;

    Rigidbody2D _rb;
    PlayerInputs plInputs;

    [Header("Dashing")]
    [SerializeField] float _dashLength = 0.2f;
    [SerializeField] float _dashCooldown = 3f, _dashSpeed = 30;
    float _dashCooldownTimer;
    public static event Action OnStartDashing, OnStopDashing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        PlayerController.Instance.OnTouchedGround += OnGrounded;
        PlayerController.Instance.OnPlayerReceiveKnockback += OnKnockback;
    }

    public void HandleDashing(int rank)
    {
        if(rank < 1) return;

        if (_dashCooldownTimer < Time.time)
        {
            if (plInputs.GetInput("Dash") && !HasDashed && plInputs.Inputs.RawX != 0)
            {
                _dashDir = new Vector3(plInputs.Inputs.RawX, 0, 0).normalized;

                _dashCooldownTimer = Time.time + _dashCooldown;
                HasDashed = true;
                Dashing = true;
                _timeStartedDash = Time.time;
                _rb.gravityScale = 0;
                OnStartDashing?.Invoke();
            }
        }

        if(!Dashing) return; 
        if(Time.time >= _timeStartedDash + (_dashLength * StatsManager.Instance.DashLength.totalValue)) EndDash();
        
        if(!Dashing) return; 
        _rb.velocity = _dashDir * _dashSpeed;
        _rb.gravityScale = 0;

        if(rank < 2) return;
        gameObject.layer = 11;

        if(rank < 3) return;
        gameObject.layer = 12;
    }

    
    void EndDash() 
    {
        float speed = PlayerController.Instance.WalkSpeed;
        Dashing = false;
        _rb.velocity = new Vector3(Mathf.Clamp(_rb.velocity.x, plInputs.Inputs.RawX * -speed, plInputs.Inputs.RawX * speed), _rb.velocity.y > 3 ? 3 : _rb.velocity.y);
        _rb.gravityScale = PlayerController.Instance.GravityScale;
        if(PlayerController.Instance.IsGrounded) HasDashed = false;
        OnStopDashing?.Invoke();
        gameObject.layer = 10;
    }

    void OnGrounded() => HasDashed = false;

    void OnKnockback(Vector3 input)
    {
        if(Dashing) EndDash();
    }
}
