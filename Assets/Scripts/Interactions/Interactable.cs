using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public int InteractionIndex = 0;
    public Interaction[] Interactions;
    public UnityAction OnInteract;
    public UnityEvent OnExitInteraction;

    private void Start()
    {
        if(Interactions.Length == 0)
        {
            Debug.LogWarning($"Interações não setadas. [Clique aqui para achar o objeto na hierarquia]", this);
            enabled = false;
        }
    }

    public virtual void Interact()
    {
        CheckOverridingInteraction();

        if(Interactions[InteractionIndex].indexMod == 0)
        { 
            OnExitInteraction?.Invoke();
            return;
        }

        Interactions[InteractionIndex].events?.Invoke();

        if (Interactions[InteractionIndex].setKeys.Length > 0)
        {
            for (int i = 0; i < Interactions[InteractionIndex].setKeys.Length; i++)
            {
                InteractionManager.Instance.SetKey(Interactions[InteractionIndex].setKeys[i]);
            }
        }

        OnInteract?.Invoke();

        InteractionIndex = Mathf.Min(InteractionIndex + Interactions[InteractionIndex].indexMod, Interactions.Length - 1);
    }

    void CheckOverridingInteraction()
    {
        int overrideIndex = 0;
        bool overrideInteraction = false;
        for (int i = 0; i < Interactions.Length; i++)
        {
            if (Interactions[i].getKeys.Length > 0)
            {
                switch(Interactions[i].getKeysMode)
                {
                    case Interaction.InteractionKeysMode.OR:
                        for (int j = 0; j < Interactions[i].getKeys.Length; j++)
                        {
                            overrideInteraction = InteractionManager.Instance.GetKey(Interactions[i].getKeys[j]);
                            overrideIndex = i;
                        }
                        break;
                    case Interaction.InteractionKeysMode.AND:
                        overrideInteraction = true;
                        for (int j = 0; j < Interactions[i].getKeys.Length; j++)
                        {
                            overrideInteraction &= InteractionManager.Instance.GetKey(Interactions[i].getKeys[j]);
                            overrideIndex = i;
                        }
                        break;
                }
                
            }
        }

        InteractionIndex = overrideInteraction? overrideIndex : InteractionIndex;
    }
}