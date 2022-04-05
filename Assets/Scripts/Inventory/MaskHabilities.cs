using UnityEngine;

public class MaskHabilities : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private bool hasDash1, hasDash2, hasDash3;

    void Awake() => playerController = GetComponent<PlayerController>();

    void Update()
    {
        if(hasDash3) playerController.HandleDashing(3);
        else if(hasDash2) playerController.HandleDashing(2);
        else if(hasDash1) playerController.HandleDashing(1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item;
        if(!collision.gameObject.TryGetComponent<Item>(out item)) return;
        if(item.item is MaskObject) 
        {
            var a = item.item as MaskObject;
            switch(a.habilities)
            {
                case Habilities.Dash:
                    ActivateDash();
                    break;
                case Habilities.DoubleJump:
                    ActivateDoubleJump();
                    break;
            }
        }

        Destroy(collision.gameObject);
    }

    void ActivateDash()
    {
        if(hasDash1)
            if(hasDash2) hasDash3 = true;
            else hasDash2 = true;
        else hasDash1 = true;
    }

    void ActivateDoubleJump()
    {
        playerController.ExtraJumpsMax++;
    }
}
