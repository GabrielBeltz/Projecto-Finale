using UnityEngine;

public class Item : MonoBehaviour
{
    public MaskObject item;
    public Ability assignedAbility;

    public Collider2D[] myColliders;
    public Renderer[] myRenderers;
    public ParticleSystem myParticleSystem;

    private void Start() => assignedAbility = StatsManager.Instance.PlayerController.AbilitiesController.GetRandomAbility();

    public void NewAbility() => StatsManager.Instance.PlayerController.AbilitiesController.NewAbilityInteraction(this);

    private void OnTriggerEnter(Collider other)
    {
        if(!(other.CompareTag("Player"))) return;

        // mostrar card de info da habilidade
    }

    public void EndInteraction(Ability input)
    {   assignedAbility = input;
        Deactivate();
    }

    public void Deactivate()
    {
        if(assignedAbility.Rank > 0) return;

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
