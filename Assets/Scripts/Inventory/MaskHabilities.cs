using UnityEngine;

public class MaskHabilities : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private bool hasDashMask = false;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if(hasDashMask)
        {
            playerController.HandleDashing();
        }
        else
        {
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item;
        if(collision.gameObject.TryGetComponent<Item>(out item))
        {
            if(item.item.name == "DashMask")
            hasDashMask = true;
            Destroy(collision.gameObject);
        }
    }
}
