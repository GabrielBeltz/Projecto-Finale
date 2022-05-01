using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class AbilityView
{
    public Sprite DefaultSprite;
    public Image AbilityIcon;
    public TextMeshProUGUI AbilityName, Rank1, Rank2, Rank3;
    public Button Button;
    Color active = Color.black, inactive = Color.gray;

    public void Activate(AbilityInfo info, int Rank) => Activate(info, Rank, true);

    public void Activate(AbilityInfo info, int Rank, bool buttonInteractable)
    {
        if(info != null) 
        {
            if(info.icon != null && AbilityIcon != null) AbilityIcon.overrideSprite = info.icon;
            if(AbilityName != null) AbilityName.text = info.name;
            if(Rank1 != null) 
            { 
                Rank1.text = info.ranks[0].internalDescription;
                Rank1.color = Rank > 0 ? active : inactive;
            } 
            if(Rank2 != null) 
            {
                Rank2.text = info.ranks[1].internalDescription;
                Rank2.color = Rank > 1 ? active : inactive;
            } 
            if(Rank3 != null) 
            { 
                Rank3.text = info.ranks[2].internalDescription;
                Rank3.color = Rank > 2 ? active : inactive;
            }
            if(Button != null) Button.interactable = Rank > 2 ? !buttonInteractable : buttonInteractable;
        }
        else
        {
            AbilityName.text = "Empty Slot";
            if(Rank1 != null) 
            { 
                Rank1.text = "Rank 1 Description";
                Rank1.color = inactive;
            } 
            if(Rank2 != null) 
            {
                Rank2.text = "Rank 2 Description";
                Rank2.color = inactive;
            } 
            if(Rank3 != null) 
            { 
                Rank3.text = "Rank 3 Description";
                Rank3.color = inactive;
            }
            if(Button != null) Button.interactable = false;
        }
    }
}
