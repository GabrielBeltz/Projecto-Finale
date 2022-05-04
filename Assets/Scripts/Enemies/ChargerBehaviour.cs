using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargerBehaviour : MonoBehaviour
{
    public Transform frontFeet;
    public ContactFilter2D contactFilter2D;
    public LayerMask wantedLayer;
    [SerializeField] private Rigidbody2D _rb;
    private BoxCollider2D _myPatrolAreaCollider;
    public float moveSpeed, hitDistance;
    private float _modifiedSpeed;
    RaycastHit2D[] hit = new RaycastHit2D[1];
    [SerializeField] GameObject _groundArea;
    [SerializeField] private bool isPlayerInMyPatrolArea = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        Invoke("GetGroundObject", 0.3f);
    }

    private void Update()
    {
        IsPlayerInMyPatrolArea();
        isPlayerInMyPatrolArea = IsPlayerInMyPatrolArea();
    }

    private void FixedUpdate()
    {
        _modifiedSpeed = moveSpeed * Mathf.Sign(transform.localScale.x);
        _rb.velocity = new Vector2(_modifiedSpeed, 0f);

        if(Physics2D.Raycast(frontFeet.position, frontFeet.right, contactFilter2D, hit, hitDistance) > Mathf.Epsilon || Physics2D.Raycast(frontFeet.position, -Vector3.up, contactFilter2D, hit, hitDistance * 3f) == 0f)
        {
            if(IsFacingRight())
            {
                //Move Right
                frontFeet.rotation = Quaternion.Euler(new Vector3(0f, 0f, -180f));
            }
            else
            {
                //Move Left
                frontFeet.rotation = Quaternion.Euler(new Vector3(0f, 0f, -0f));
            }

            Rotate();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(frontFeet.position, frontFeet.right * hitDistance);
        Gizmos.DrawRay(frontFeet.position, -Vector3.up * hitDistance * 3f);
    }
}
