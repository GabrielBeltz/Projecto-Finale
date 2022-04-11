using UnityEngine;

public class MaskHabilities : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private bool hasDash1, hasDash2, hasDash3;
    float likelyPickDash = 1f, likelyPickDoubleJump = 1f;
    public float DeramdomizeFactor = 0.34f; // nesse valor todos os upgrades começam com 1 de chance, e essa chance diminui em 0.34 a cada rank pego.

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
                case Habilities.Random:
                    PickRandomly();
                    break;
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

    void DetermineChance()
    {
        likelyPickDash = 1 - Mathf.Clamp01(playerController.DashRank * DeramdomizeFactor);
        likelyPickDoubleJump = 1 - Mathf.Clamp01(playerController.ExtraJumpsMax * DeramdomizeFactor);
    }

    void PickRandomly()
    {
        DetermineChance();
        float chance = likelyPickDash + likelyPickDoubleJump;
        if(Random.Range(0, chance) < likelyPickDash) ActivateDash();
        else ActivateDoubleJump();
        DetermineChance();
    }

    void ActivateDash() => playerController.DashRank++;

    void ActivateDoubleJump() => playerController.ExtraJumpsMax++;
}
