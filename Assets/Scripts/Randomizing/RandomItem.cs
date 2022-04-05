using UnityEngine;

public class RandomItem : MonoBehaviour
{
    public ItemPool[] itemPool;

    void Start() { if(itemPool != null) GetComponent<Item>().item = itemPool[Random.Range(0, itemPool.Length)].GetRandomItem(); }
}
