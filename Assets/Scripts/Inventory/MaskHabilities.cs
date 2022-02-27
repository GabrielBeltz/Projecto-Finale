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
        if(collision.gameObject.name == "DashMask")
        {
            hasDashMask = true;
            Destroy(collision.gameObject);
        }
    }

}
