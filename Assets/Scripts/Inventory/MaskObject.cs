using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mask Object", menuName = "Inventory System/Items/MaskItem")]
public class MaskObject : ItemObject
{
    public Habilities habilities;
    [TextArea(15, 20)] public string maskDiscriprion;

    public void Awake()
    {
        type = ItemType.MaskItem;
    }
}

public enum Habilities { Dash, Smash }
