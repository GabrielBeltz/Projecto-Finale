using UnityEngine;

public class Item : MonoBehaviour
{
    public MaskObject item;

    public Collider2D[] myColliders;
    public Renderer[] myRenderers;
    public ParticleSystem myParticleSystem;

    public void NewAbility() 
    {
        if(item.assignedAbility.Rank == 0) StatsManager.Instance.PlayerController.AbilitiesController.NewAbilityInteraction(item, this);
        else
        {
        }
    } 

    public void EndInteraction(Ability input)
    {
        if(input.Rank == 0) Deactivate();
        else item.assignedAbility = input;
    }

    public void Deactivate()
    {
        myParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);

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
