using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    static public InteractionManager Instance;
    public List<InteractionKey> loadedKeys;
    [Header("Needed to Work")]
    public KeysList keyList;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
        loadedKeys = keyList.keys;
    }

    public bool GetKey(InteractionKey interactionKey) => loadedKeys.Find(key => key.name == interactionKey.name).value == interactionKey.value;

    public bool GetKey(string keyName) => loadedKeys.Find(key => key.name == keyName).value;

    public void SetKey(InteractionKey interactionKey) => loadedKeys.Find(key => key.name == interactionKey.name).value = interactionKey.value;
}

[System.Serializable]
public class Interaction
{
    public int indexMod = 1; // Se for 1, passa pro próximo index de interação, 0 para a interação e -1 volta na interação anterior.
    public UnityEvent events;
    public InteractionKeysMode getKeysMode; // se setadas para "or" qualquer uma das keys desbloqueiam, se for "and" é necessário todas as keys
    public InteractionKey[] getKeys; // keys que desbloqueiam essa interação. 
    public InteractionKey[] setKeys; // keys setadas na interação, para desbloquear outras interações

    public enum InteractionKeysMode
    {
        OR, AND
    }
}

[System.Serializable]
public class InteractionKey
{
    public string name;
    public bool value;
}