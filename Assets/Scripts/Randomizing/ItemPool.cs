using UnityEngine;
using System.Collections.Generic;

[System.Serializable] [CreateAssetMenu(fileName = "Random Item Pool", menuName = "Item Pool")]
public class ItemPool : ScriptableObject
{
    [SerializeField] List<ItemObject> Itens = new List<ItemObject>();

    public ItemObject GetRandomItem() => Itens[Random.Range(0, Itens.Count)];
}