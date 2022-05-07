using UnityEngine;

public class EnemyPatrolWalls : MonoBehaviour
{
    [Header("Stats")]
    public bool StartsGoingLeft;
    public bool neverRotates;
    public float speed, floorOffset;
    float modifiedSpeed;

    [Header("Needed to Work")]
    public ContactFilter2D contactFilter;
    public Rigidbody2D rb;
    public LayerMask groundLayerMask;
    public Transform rotaDown, rotaUp, frontFeet;
    public EnemyKnockbackTarget target;
    bool justBackTracked;
    RaycastHit2D[] hit = new RaycastHit2D[1];

    private void Start()
    {
        if(transform.lossyScale.x < 0) speed = -speed;
        if(StartsGoingLeft) Backtrack();
    }

    void Update()
    {
        modifiedSpeed = speed * Mathf.Sign(transform.localScale.x);

        if (target.isKnockbacked) modifiedSpeed = 0;

        if(neverRotates)
        {
            if (Physics2D.Raycast(rotaUp.position, -rotaUp.transform.up, contactFilter, hit, 0.25f) > 0 && !justBackTracked) Backtrack();
            else if (Physics2D.Raycast(frontFeet.position, -transform.up, contactFilter, hit, 0.15f) > 0) Move(true);
            else if (Physics2D.Raycast(rotaDown.position, -rotaDown.transform.up, contactFilter, hit, 0.25f) > 0 && !justBackTracked) Backtrack();
            else Backtrack();
        }
        else
        {
            if (Physics2D.Raycast(rotaUp.position, -rotaUp.transform.up, contactFilter, hit, 0.25f) > 0 && !justBackTracked) RotationMove(false);
            else if (Physics2D.Raycast(frontFeet.position, -transform.up, contactFilter, hit, 1f) > 0) RotationMove(true);
            else if (Physics2D.Raycast(rotaDown.position, -rotaDown.transform.up, contactFilter, hit, 0.25f) > 0 && !justBackTracked) RotationMove(false);
            else Backtrack();
        }
    }

    void RotationMove(bool recast)
    {
        Move(recast);
        Rotate();
    }

    void Move(bool recast)
    {
        justBackTracked = false;
        if(recast) Physics2D.Raycast(transform.position, -this.transform.up, contactFilter, hit, 1f);
        transform.position = hit[0].point + (hit[0].normal * floorOffset);
        rb.velocity = new Vector3(-hit[0].normal.y, hit[0].normal.x, 0);
        rb.velocity *= -modifiedSpeed;
    }

    void Rotate() => transform.rotation = Quaternion.LookRotation(transform.forward, hit[0].normal);

    public void Backtrack()
    {
        this.transform.localScale = new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
        justBackTracked = true;
    }
}
