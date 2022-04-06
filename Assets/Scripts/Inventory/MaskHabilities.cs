using UnityEngine;

public class MaskHabilities : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private bool hasDash1, hasDash2, hasDash3;

    void Awake() => playerController = GetComponent<PlayerController>();

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

        item.Deactivate();
    }

    void ActivateDash() => playerController.DashRank++;

    void ActivateDoubleJump() => playerController.ExtraJumpsMax++;
}
