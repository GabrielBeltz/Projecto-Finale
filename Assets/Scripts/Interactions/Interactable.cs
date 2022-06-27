using System;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public int InteractionIndex = 0, lastIndex;
    public Interaction[] Interactions;
    public Action<bool> OnInteract;
    public UnityEvent OnExitInteraction;
    bool _interacting;

    private void Start()
    {
        if(Interactions.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name} Não possui interações setadas.", this);
            enabled = false;
        }
        else OnInteract += SetInteracting;
    }

    private void OnDisable() => SetInteracting(false);

    public virtual void Interact()
    {
        OnInteract?.Invoke(true);
        if(Interactions[lastIndex].indexMod == 0) CheckOverridingInteraction();

        Interactions[InteractionIndex].events?.Invoke();
        if (Interactions[InteractionIndex].setKeys.Length > 0)
        {
            for (int i = 0; i < Interactions[InteractionIndex].setKeys.Length; i++)
            {
                InteractionManager.Instance.SetKey(Interactions[InteractionIndex].setKeys[i]);
            }
        }

        int nextInteractionIndex = Mathf.Min(InteractionIndex + Interactions[InteractionIndex].indexMod, Interactions.Length - 1);

        if(nextInteractionIndex == lastIndex && _interacting)
        {
            OnInteract?.Invoke(false);
        }
        else
        {
            lastIndex = InteractionIndex;
            InteractionIndex = nextInteractionIndex;
        }
    }

    void SetInteracting(bool value)
    {
        PlayerController.Instance.PlInputs.CanMove = !value;
        _interacting = value;

        if(!value) OnExitInteraction?.Invoke();
    }

    void CheckOverridingInteraction()
    {
        int overrideIndex = 0;
        bool overrideInteraction = false;
        for (int i = 0; i < Interactions.Length; i++)
        {
            if (Interactions[i].getKeys.Length > 0)
            {
                if(Interactions[i].ChecksBool)
                {
                    switch(Interactions[i].getKeysMode)
                    {     case Interaction.InteractionKeysMode.OR:
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
                if(Interactions[i].ChecksInt)
                {
                    for (int j = 0; j < Interactions[i].getKeys.Length; j++)
                    {
                        switch(Interactions[i].getKeys[j].intComparisonMode)
                        {
                            case InteractionKey.IntComparisonMode.Equals:
                                overrideInteraction = InteractionManager.Instance.GetKeyEquals(Interactions[i].getKeys[j].name, Interactions[i].getKeys[j].intValue);
                                break;
                            case InteractionKey.IntComparisonMode.GreaterThan:
                                overrideInteraction = InteractionManager.Instance.GetKeyGreaterThan(Interactions[i].getKeys[j].name, Interactions[i].getKeys[j].intValue);
                                break;
                            case InteractionKey.IntComparisonMode.LowerThan:overrideInteraction = InteractionManager.Instance.GetKeyLowerThan(Interactions[i].getKeys[j].name, Interactions[i].getKeys[j].intValue);
                                break;
                            default:
                                break;
                        }
                        overrideIndex = i;
                    }
                }
            }
        }

        InteractionIndex = overrideInteraction? overrideIndex : InteractionIndex;
    }
}