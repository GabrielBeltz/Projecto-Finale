using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New AbilitiesInfo List", menuName = "Inventory System/Abilities Info List")]
public class AbilitiesInfos : ScriptableObject
{
    public List<AbilityInfo> list;

    public AbilityInfo GetFullInfo(string name) => list.Find(abillity => abillity.name == name);
}

[System.Serializable]
public class AbilityInfo
{
    public string name;
    public Sprite icon;
    public AbilityRankInfo[] ranks = new AbilityRankInfo[3];
}

[System.Serializable]
public class AbilityRankInfo
{
    [TextArea(1,20)] public string Description;
    [HideInInspector] public string internalDescription
    {
        get => Description.Length > 0 ? Description : "Sample Text lol";
    }
}