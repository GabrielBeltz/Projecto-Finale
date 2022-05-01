using UnityEngine;
using TMPro;

public class UpgradeAbilitiesMenu : MonoBehaviour
{
    public PauseController Pause;
    public TextMeshProUGUI TopText;
    public AbilityView ActiveAbilityA, ActiveAbilityB, PassiveAbility;
    public MaskHabilities maskHabilities;

    private void Update()
    {
        if(Input.GetButtonDown("AbilityA") && ActiveAbilityA.Button.IsInteractable()) UpgradeAbility(0);
        else if(Input.GetButtonDown("AbilityB") && ActiveAbilityB.Button.IsInteractable()) UpgradeAbility(1);
        else if(Input.GetButtonDown("Submit") && PassiveAbility.Button.IsInteractable()) UpgradeAbility(2);
        else if(Input.GetButtonDown("Cancel")) Back();
    }

    public void Activate(AbilityInfo activeInfoA, int activeRankA, AbilityInfo activeInfoB, int activeRankB, AbilityInfo passiveInfo, int passiveAbilityRank)
    {
        Pause.GameplayPause();
        ActiveAbilityA.Activate(activeInfoA, activeRankA);
        ActiveAbilityB.Activate(activeInfoB, activeRankB);
        PassiveAbility.Activate(passiveInfo, passiveAbilityRank);

        if(!ActiveAbilityA.Button.IsInteractable() && !ActiveAbilityB.Button.IsInteractable() && !PassiveAbility.Button.IsInteractable())
        {
            TopText.text = "Doesn't look like you can upgrade any abilities. Sorry!";
        }
    }

    public void UpgradeAbility(int slot)
    {
        maskHabilities.UpgradeAbility(slot);
        Back();
    }

    public void Back()
    {
        gameObject.SetActive(false);
        Pause.GameplayUnpause();
    }
}