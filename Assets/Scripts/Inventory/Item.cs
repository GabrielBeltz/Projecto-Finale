using UnityEngine;

public class Item : MonoBehaviour
{
    public bool Upgrade;
    public MaskObject item;
    public Ability assignedAbility;

    public Collider2D[] myColliders;
    public Renderer[] myRenderers;
    public ParticleSystem myParticleSystem;

    private void Start() => assignedAbility = Upgrade? new Ability() : StatsManager.Instance.PlayerController.AbilitiesController.GetRandomAbility();

    public void NewAbility() => StatsManager.Instance.PlayerController.AbilitiesController.NewAbilityInteraction(this);

    public void UpgradeAbility() => StatsManager.Instance.PlayerController.AbilitiesController.UpgradeInteraction();

    public void EndInteraction(Ability input)
    {   
        assignedAbility = input;
        Deactivate();
    }

    public void Deactivate()
    {
        if(assignedAbility.Rank > 0) return;

        if(myParticleSystem != null) myParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        foreach(var col in myColliders)
        {
            col.enabled = false;
        }

        foreach(var rdr in myRenderers)
        {
            rdr.enabled = false;
        }
    }
}
