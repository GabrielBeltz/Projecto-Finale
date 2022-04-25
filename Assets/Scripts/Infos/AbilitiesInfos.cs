using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New AbilitiesInfo List", menuName = "Inventory System/Abilities Info List")]
public class AbilitiesInfos : ScriptableObject
{
    public List<AbilityInfo> list;

    public string GetRankDescription(string abilityName, int index) => list.Find(ability => ability.name == abilityName).ranks[index].Description;
}

[System.Serializable]
public class AbilityInfo
{
    public string name;
    public AbilityRankInfo[] ranks = new AbilityRankInfo[3];
}

[System.Serializable]
public class AbilityRankInfo
{
    public Sprite sprite;
    [TextArea(1,20)] public string Description;
}