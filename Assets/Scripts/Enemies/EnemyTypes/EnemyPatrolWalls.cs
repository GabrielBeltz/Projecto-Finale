using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolWalls : MonoBehaviour
{
    [Header("Stats")]
    public MoveDirection direction;
    public float speed;
    public float floorOffset;
    public bool backtracking;
    public bool neverRotates;
    float modifiedSpeed;

    [Header("Needed to Work")]
    public ContactFilter2D contactFilter;
    public Rigidbody2D rb;
    public LayerMask groundLayerMask;
    public Transform rotationFeet, upperRotationFeet, frontFeet;
    public EnemyKnockbackTarget target;
    bool justBackTracked;
    RaycastHit2D[] hit = new RaycastHit2D[1];

    public enum MoveDirection
    {
        right, down, left, up
    }

    private void Start()
    {
        if (direction == MoveDirection.left)
        {
            floorOffset = -floorOffset;
        }
    }

    void FixedUpdate()
    {
        modifiedSpeed = speed;

        if (target.isKnockbacked) modifiedSpeed = 0;

        // A prioridade é UpperRotation > Move > Rotation > Backtrack
        // Se o upperRotation detectar um chão, é quase ctz q uma parede vai entrar na frente do caminho, então não faz sentido continuar movendo
        // Se o Move for válido, é quase ctz que executar a rotação vai fazer o movimento entrar dentro do chão

        if (Physics2D.Raycast(upperRotationFeet.position, -upperRotationFeet.transform.up, contactFilter, hit, 0.25f) > 0 && !justBackTracked)
        {
            if (neverRotates)
            {
                Backtrack();
                return;
            }

            Rotate(false);
        }
        else if (Physics2D.Raycast(frontFeet.position, -this.transform.up, contactFilter, hit, 0.15f) > 0)
        {
            Move();
        }
        else if (Physics2D.Raycast(rotationFeet.position, -rotationFeet.transform.up, contactFilter, hit, 0.25f) > 0 && !justBackTracked)
        {
            if (neverRotates)
            {
                Backtrack();
                return;
            }

            Rotate(true);
        }
        else if(backtracking)
        {
            Backtrack();
        }
    }

    void Move()
    {
        justBackTracked = false;
        switch (direction)
        {
            case MoveDirection.right:
                rb.velocity = new Vector3(modifiedSpeed, 0, 0);
                this.transform.position = new Vector3(this.transform.position.x, hit[0].point.y + floorOffset, this.transform.position.z);
                break;
            case MoveDirection.down:
                rb.velocity = new Vector3(0, -modifiedSpeed, 0);
                this.transform.position = new Vector3(hit[0].point.x + floorOffset, this.transform.position.y, this.transform.position.z);
                break;
            case MoveDirection.left:
                rb.velocity = new Vector3(-modifiedSpeed, 0, 0);
                this.transform.position = new Vector3(this.transform.position.x, hit[0].point.y - floorOffset, this.transform.position.z);
                break;
            case MoveDirection.up:
                rb.velocity = new Vector3(0, modifiedSpeed, 0);
                this.transform.position = new Vector3(hit[0].point.x - floorOffset, this.transform.position.y, this.transform.position.z);
                break;
        }
    }

    void Rotate(bool down)
    {
        justBackTracked = false;
        if (down)
        {
            switch (direction)
            {
                case MoveDirection.right:
                    this.transform.position = new Vector2(hit[0].point.x + floorOffset, hit[0].point.y);
                    this.transform.rotation = Quaternion.Euler(0, 0, -90);
                    direction = MoveDirection.down;
                    break;
                case MoveDirection.down:
                    this.transform.position = new Vector2(hit[0].point.x, hit[0].point.y - floorOffset);
                    this.transform.rotation = Quaternion.Euler(0, 0, -180);
                    direction = MoveDirection.left;
                    break;
                case MoveDirection.left:
                    this.transform.position = new Vector2(hit[0].point.x - floorOffset, hit[0].point.y);
                    this.transform.rotation = Quaternion.Euler(0, 0, -270);
                    direction = MoveDirection.up;
                    break;
                case MoveDirection.up:
                    this.transform.position = new Vector2(hit[0].point.x, hit[0].point.y + floorOffset);
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);
                    direction = MoveDirection.right;
                    break;
            }
        }
        else
        {
            switch (direction)
            {
                case MoveDirection.right:
                    this.transform.position = new Vector2(hit[0].point.x - floorOffset, hit[0].point.y);
                    this.transform.rotation = Quaternion.Euler(0, 0, -270);
                    direction = MoveDirection.up;
                    break;
                case MoveDirection.down:
                    this.transform.position = new Vector2(hit[0].point.x, hit[0].point.y + floorOffset);
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);
                    direction = MoveDirection.right;
                    break;
                case MoveDirection.left:
                    this.transform.position = new Vector2(hit[0].point.x + floorOffset, hit[0].point.y);
                    this.transform.rotation = Quaternion.Euler(0, 0, -90);
                    direction = MoveDirection.down;
                    break;
                case MoveDirection.up:
                    this.transform.position = new Vector2(hit[0].point.x, hit[0].point.y - floorOffset);
                    this.transform.rotation = Quaternion.Euler(0, 0, 180);
                    direction = MoveDirection.left;
                    break;
            }
        }
    }

    public void Backtrack()
    {
        this.transform.localScale = new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
        speed = -speed;
        justBackTracked = true;
    }
}

