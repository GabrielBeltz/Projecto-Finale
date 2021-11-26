using System.Collections;
using System.Collections.Generic;
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
        for (int j = 0; j < interactions[interactionIndex].events.Length; j++)
        {
            interactions[interactionIndex].events[j]?.Invoke();
        }
        OnInteract?.Invoke();

        interactionIndex = Mathf.Min(interactionIndex + 1, interactions.Length - 1);
    }


}

[System.Serializable]
public class Interaction
{
    public UnityEvent[] events;
}