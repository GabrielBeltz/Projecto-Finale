using UnityEngine;

[CreateAssetMenu(fileName = "New Mask Object", menuName = "Inventory System/Items/MaskItem")]
public class MaskObject : ItemObject
{
    public Habilities habilities;
    [TextArea(15, 20)] public string maskDescription;

    public void Awake()
    {
        type = ItemType.AbilityItem;
    }
}

public enum Habilities { Dash, DoubleJump }
