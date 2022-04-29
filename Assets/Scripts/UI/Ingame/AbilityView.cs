using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class AbilityView
{
    public Image AbilityIcon;
    public TextMeshProUGUI AbilityName, Rank1, Rank2, Rank3;
    Color active = Color.black, inactive = Color.gray;

    public void Activate(AbilityInfo info, int Rank)
    {
        if(info.icon != null && AbilityIcon != null) AbilityIcon.overrideSprite = info.icon;
        if(AbilityName != null) AbilityName.text = info.name;
        if(Rank1 != null) 
        { 
            Rank1.text = info.ranks[0].internalDescription;
            Rank1.color = Rank > 0 ? active : inactive;
        } 
        if(Rank1 != null) 
        {
            Rank2.text = info.ranks[1].internalDescription;
            Rank2.color = Rank > 1 ? active : inactive;
        } 
        if(Rank1 != null) 
        { 
            Rank3.text = info.ranks[2].internalDescription;
            Rank3.color = Rank > 2 ? active : inactive;
        } 
    }
}
