using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    static public InteractionManager Instance;
    public List<InteractionKey> loadedKeys;

    private void Start()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }

    public bool GetKey(InteractionKey interactionKey) => loadedKeys.Find(key => key.name == interactionKey.name).value == interactionKey.value;
    public bool GetKeyBool(string keyName) => loadedKeys.Find(key => key.name == keyName).value;
    public bool GetKeyGreaterThan(string keyName, int comparison) => loadedKeys.Find(key => key.name == keyName).intValue > comparison;
    public bool GetKeyLowerThan(string keyName, int comparison) => loadedKeys.Find(key => key.name == keyName).intValue < comparison;
    public bool GetKeyEquals(string keyName, int comparison) => loadedKeys.Find(key => key.name == keyName).intValue == comparison;
    public int GetKeyInt(string keyName) => loadedKeys.Find(key => key.name == keyName).intValue;

    public void SetKey(InteractionKey interactionKey) 
    {
        loadedKeys.Find(key => key.name == interactionKey.name).value = interactionKey.value;
        loadedKeys.Find(key => key.name == interactionKey.name).intValue = interactionKey.intValue;
    } 
    public void SetKey(string keyName, bool value) => loadedKeys.Find(key => key.name == keyName).value = value;
    public void SetKey(string keyName, int value) => loadedKeys.Find(key => key.name == keyName).intValue = value;
}

[System.Serializable]
public class Interaction
{
    public string ID;
    public int indexMod = 1; // Se for 1, passa pro pr�ximo index de intera��o, 0 para a intera��o e -1 volta na intera��o anterior.
    public UnityEvent events;
    public bool ChecksInt, ChecksBool;
    public InteractionKeysMode getKeysMode; // se setadas para "or" qualquer uma das keys desbloqueiam, se for "and" � necess�rio todas as keys
    public InteractionKey[] getKeys; // keys que desbloqueiam essa intera��o. 
    public InteractionKey[] setKeys; // keys setadas na intera��o, para desbloquear outras intera��es

    public enum InteractionKeysMode { OR, AND }
}

[System.Serializable]
public class InteractionKey
{
    public string name;
    public bool value;
    public int intValue;
    public IntComparisonMode intComparisonMode;
    public enum IntComparisonMode { Equals, GreaterThan, LowerThan }
    
}