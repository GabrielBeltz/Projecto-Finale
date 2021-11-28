using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public int interactionIndex;
    public Interaction[] interactions;
    public UnityAction OnInteract;
    public UnityEvent onExitInteraction;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
             onExitInteraction?.Invoke();
        }
    }

    public virtual void Interact()
    {
        int overrideIndex = 0;
        bool overrideInteraction = false;
        for (int i = 0; i < interactions.Length; i++)
        {
            if (interactions[i].getKeys.Length > 0)
            {
                for (int j = 0; j < interactions[i].getKeys.Length; j++)
                {
                    overrideInteraction = InteractionManager.Instance.GetKey(interactions[i].getKeys[j]);
                    overrideIndex = i;
                }
            }
        }

        if (overrideInteraction)
        {
            interactionIndex = overrideIndex;
        }

        for (int j = 0; j < interactions[interactionIndex].events.Length; j++)
        {
            interactions[interactionIndex].events[j]?.Invoke();
        }

        if (interactions[interactionIndex].setKeys.Length > 0)
        {
            for (int i = 0; i < interactions[interactionIndex].setKeys.Length; i++)
            {
                InteractionManager.Instance.SetKey(interactions[interactionIndex].setKeys[i]);
            }
        }

        OnInteract?.Invoke();

        interactionIndex = Mathf.Min(interactionIndex + interactions[interactionIndex].indexMod, interactions.Length - 1);
    }
}