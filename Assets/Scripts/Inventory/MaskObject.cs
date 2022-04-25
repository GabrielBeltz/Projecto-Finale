using UnityEngine;

[CreateAssetMenu(fileName = "New Mask Object", menuName = "Inventory System/Items/MaskItem")]
public class MaskObject : ItemObject
{
    public Habilities habilities;

    public void Awake()
    {
        type = ItemType.AbilityItem;
    }
}

public enum Habilities { Random, Upgrade, Dash, Jump, Attack, Health, Hook, Tantrum, Knives, Boomerang, Shield }
