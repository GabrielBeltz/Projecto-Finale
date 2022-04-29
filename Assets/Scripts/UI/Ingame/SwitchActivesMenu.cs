using UnityEngine;

public class SwitchActivesMenu : MonoBehaviour
{
    public PauseController Pause;
    public Ability switchingAbility;
    public AbilityView AbilityA, AbilityB, NewAbility;
    public MaskHabilities maskHabilities;

    private void Update()
    {
        if(Input.GetButtonDown("AbilityA")) SwitchAbility(0);
        else if(Input.GetButtonDown("AbilityB")) SwitchAbility(1);
    }

    public void Activate(AbilityInfo infoA, int rankA, AbilityInfo infoB, int rankB, AbilityInfo newInfo, Ability newAbility)
    {
        Pause.GameplayPause();
        AbilityA.Activate(infoA, rankA);
        AbilityB.Activate(infoB, rankB);
        NewAbility.Activate(newInfo, newAbility.Rank);
        switchingAbility = newAbility;
    }

    public void SwitchAbility(int slot)
    {
        maskHabilities.DeactivateAbility(slot);
        maskHabilities.ActivateAbility(switchingAbility, slot);

        // Tela de confirmação depois daqui
        Back();
    }

    public void Back()
    {
        gameObject.SetActive(false);
        Pause.GameplayUnpause();
    }
}