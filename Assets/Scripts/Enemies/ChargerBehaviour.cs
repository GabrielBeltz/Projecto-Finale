using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargerBehaviour : MonoBehaviour
{
    public Transform frontFeet;
    public ContactFilter2D contactFilter2D;
    [SerializeField] private Rigidbody2D _rb;
    public float moveSpeed, hitDistance;
    private float _modifiedSpeed;
    RaycastHit2D[] hit = new RaycastHit2D[1];


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
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
