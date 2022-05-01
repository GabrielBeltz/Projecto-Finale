using UnityEngine;

public class SwitchPassiveMenu : MonoBehaviour
{
    public PauseController Pause;
    public Ability switchingAbility;
    public AbilityView OldAbility, NewAbility;
    public MaskHabilities maskHabilities;

    private void Update()
    {
        if(Input.GetButtonDown("Submit")) SwitchAbility(2);
        else if(Input.GetButtonDown("Cancel")) Back();
    }

    public void Activate(AbilityInfo oldInfo, int oldRank, AbilityInfo newInfo, Ability newAbility)
    {
        Pause.GameplayPause();
        OldAbility.Activate(oldInfo, oldRank);
        NewAbility.Activate(newInfo, newAbility.Rank);
        switchingAbility = newAbility;
    }

    public void SwitchAbility(int slot)
    {
        maskHabilities.DeactivateAbility(slot);
        maskHabilities.ActivateAbility(switchingAbility, slot);

        Back();
    }

    public void Back()
    {
        gameObject.SetActive(false);
        Pause.GameplayUnpause();
    }
}