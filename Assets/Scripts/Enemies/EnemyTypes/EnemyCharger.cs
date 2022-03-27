using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyPatrolWalls))]
public class EnemyCharger : MonoBehaviour
{
    [Header("Configs")]
    public float SeeingPlayerSpeedMultiplier = 1.5f;
    public float HeightDistanceToSeePlayer = 2;
    public float WidthDistanceToSeePlayer = 7.5f;
    [Header("Live Feedback")]
    [SerializeField] bool PreparingAttack;
    [SerializeField] bool Attacking;
    [Header("Needed to Work")]
    [SerializeField] string PlayerTag;
    PlayerController _player;
    EnemyPatrolWalls _patrolScript;

    float originalSpeed;

    private void Awake()
    {
        _patrolScript = GetComponent<EnemyPatrolWalls>();
        _player = GameObject.FindGameObjectWithTag(PlayerTag).GetComponent<PlayerController>();
        if(_player == null) gameObject.SetActive(false);
        originalSpeed = _patrolScript.speed;
    }

    void Update()
    {
        if(CheckPlayerPosition())
        {
            _patrolScript.speed = originalSpeed * SeeingPlayerSpeedMultiplier  * Mathf.Sign(_patrolScript.speed);
            LookAtPlayer();
        }
        else
        {
            _patrolScript.speed = originalSpeed * Mathf.Sign(_patrolScript.speed);
        }
    }
    
    bool CheckPlayerPosition()
    {
        Collider2D[] raycastHit2Ds = Physics2D.OverlapCircleAll(transform.position, WidthDistanceToSeePlayer, ~2);
        if(raycastHit2Ds.Length > 0)
        {
            foreach(var hit in raycastHit2Ds)
            {
                if(hit.gameObject.CompareTag(PlayerTag)) 
                {
                    if(hit.transform.position.y - HeightDistanceToSeePlayer < transform.position.y && hit.transform.position.y + HeightDistanceToSeePlayer > transform.position.y) return true;
                    else if(hit.transform.position.y + HeightDistanceToSeePlayer > transform.position.y && hit.transform.position.y - HeightDistanceToSeePlayer < transform.position.y) return true;
                }
            }
        }

        return false;
    }

    void LookAtPlayer()
    {
        bool playerToTheRight = _player.transform.position.x > transform.position.x ? true : false;
        bool lookingAtRight = transform.localScale.x > 0;

        if(playerToTheRight != lookingAtRight) _patrolScript.Backtrack();

    }
}
