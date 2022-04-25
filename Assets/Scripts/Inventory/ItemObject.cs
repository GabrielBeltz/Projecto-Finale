using UnityEngine;

public enum ItemType { AbilityItem, KeyItem, NoteItem, Default }

public abstract class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType type;
}