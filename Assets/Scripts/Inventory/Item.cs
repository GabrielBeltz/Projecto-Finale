using UnityEngine;

public class Item : MonoBehaviour
{
    public bool Upgrade;
    public MaskObject item;
    public Ability assignedAbility;

    public Collider2D[] myColliders;
    public Renderer[] myRenderers;
    public ParticleSystem myParticleSystem;

    void OnEnable() 
    {
        if(PlayerController.Instance != null) assignedAbility = Upgrade? new Ability() : PlayerController.Instance.AbilitiesController.GetRandomAbility();
    }

    public void NewAbility() => PlayerController.Instance.AbilitiesController.NewAbilityInteraction(this);

    public void UpgradeAbility() => PlayerController.Instance.AbilitiesController.UpgradeInteraction();

    public void EndInteraction(Ability input) 
    { 
        assignedAbility = input;
        if(assignedAbility.Rank > 0) return;
        gameObject.SetActive(false);
    } 
}
