using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargerBehaviour : MonoBehaviour
{
    public enum State { Patrol, Attack };
    public State currentState;
    public Transform frontFeet;
    public ContactFilter2D contactFilter2D;
    public LayerMask wantedLayer;
    private Transform _player;
    private RaycastHit2D[] _hit = new RaycastHit2D[1];
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private float _moveSpeed, _hitDistance, _modifiedSpeed;
    [SerializeField] private GameObject _groundArea;
    [SerializeField] private bool _isPlayerInMyPatrolArea, _getPlayerPos, _finishAttackMode;


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        Invoke("GetGroundObject", 0.3f);
        _player = GameObject.Find("Player").GetComponent<Transform>();
        _isPlayerInMyPatrolArea = false;
        _finishAttackMode = true;
        _getPlayerPos = false;
        currentState = State.Patrol;
    }

    private void Update()
    {
        _isPlayerInMyPatrolArea = IsPlayerInMyPatrolArea();
    }

    private void FixedUpdate()
    {
        switch(currentState)
        {
            case State.Patrol:
                Patroling();
                break;

            case State.Attack:
                Attack();
                break;
        }
    }

    private void Patroling()
    {
        _getPlayerPos = false;
        _finishAttackMode = false;

        if(!_isPlayerInMyPatrolArea)
        {
            _modifiedSpeed = _moveSpeed * Mathf.Sign(transform.localScale.x);
            _rb.velocity = new Vector2(_modifiedSpeed, 0f);

            if(Physics2D.Raycast(frontFeet.position, frontFeet.right, contactFilter2D, _hit, _hitDistance) > Mathf.Epsilon || Physics2D.Raycast(frontFeet.position, -Vector3.up, contactFilter2D, _hit, _hitDistance * 3f) == 0f)
            {
                RotateAll();
            }
        }
        else
        {
            currentState = State.Attack;
        }
    }

    private void Attack()
    {
        if(!_getPlayerPos)
        {
            bool playerToTheRight = _player.position.x > transform.position.x ? true : false;

            if(playerToTheRight != IsFacingRight())
            {
                RotateAll();
            }
            _getPlayerPos = true;
        }

        if(!_finishAttackMode)
        {
            _modifiedSpeed = _moveSpeed * Mathf.Sign(transform.localScale.x) * 3f;
            _rb.velocity = new Vector2(_modifiedSpeed, 0f);
            
            if(Physics2D.Raycast(frontFeet.position, frontFeet.right, contactFilter2D, _hit, _hitDistance) > Mathf.Epsilon || Physics2D.Raycast(frontFeet.position, -Vector3.up, contactFilter2D, _hit, _hitDistance * 3f) == 0f)
            {
                _finishAttackMode = true;
            }
        }
        else
        {
            currentState = State.Patrol;
        }
    }

    private void GetGroundObject()
    {
        RaycastHit2D hitResult = Physics2D.Raycast(frontFeet.position, -Vector2.up, 0.5f);

        if(hitResult.collider != null)
        {
            Debug.Log(hitResult.collider.name);
            _groundArea = hitResult.collider.gameObject;
        }
    }

    private bool IsPlayerInMyPatrolArea()
    {
        return _groundArea.GetInstanceID() == PlayerController.instance.actualGroundObject.GetInstanceID() ? true : false;
    }

    private bool IsFacingRight()
    {
        return transform.localScale.x > Mathf.Epsilon;
    }

    private void Rotate()
    {
        this.transform.localScale = new Vector2(-Mathf.Sign(_rb.velocity.x), this.transform.localScale.y);
    }

    private void RotateAll()
    {
        if(IsFacingRight())
        {
            //Move Right
            frontFeet.rotation = Quaternion.Euler(new Vector3(0f, 0f, -180f));
        }
        else
        {
            //Move Left
            frontFeet.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        }

        Rotate();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(frontFeet.position, frontFeet.right * _hitDistance);
        Gizmos.DrawRay(frontFeet.position, -Vector3.up * _hitDistance * 3f);
    }
}
