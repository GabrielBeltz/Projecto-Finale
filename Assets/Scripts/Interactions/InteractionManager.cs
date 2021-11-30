using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    static public InteractionManager Instance;

    private void Start()
    {
        if (InteractionManager.Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public List<InteractionKey> keys;

    public bool GetKey(InteractionKey interactionKey)
    {
        return keys.Find(key => key.name == interactionKey.name).value == interactionKey.value;
    }

    public void SetKey(InteractionKey interactionKey)
    {
        keys.Find(key => key.name == interactionKey.name).value = interactionKey.value;
    }
}

[System.Serializable]
public class Interaction
{
    // Por padrão deixei 1, mas deixando em 0 no editor impede a interação de automaticamente passar para a próxima
    public int indexMod = 1;
    public UnityEvent events;
    // keys que desbloqueiam essa interação. por enquanto elas tem somente comportamento OR,
    // qualquer uma delas que retornar como verdadeiras desbloqueia a interação
    public InteractionKey[] getKeys;  
    // keys setadas na interação, para desbloquear outras interações
    public InteractionKey[] setKeys;
}

[System.Serializable]
public class InteractionKey
{
    public string name;
    public bool value;
}