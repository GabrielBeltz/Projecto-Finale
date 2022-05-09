using UnityEngine;

public class PauseController : MonoBehaviour
{
    public bool AcceptInput = true;
    public MaskHabilities maskHabilities;
    public GameObject PausePanel;
    public AbilityView ActiveA, ActiveB, Passive;
    float playerTimeScale = 1, gameplayTimeScale = 1;
    bool playerPaused;

    public void PlayerPause()
    {
        if(!AcceptInput) return;
        playerPaused = !playerPaused;
        if(playerPaused) Pause();
        else Unpause();
    }

    void Pause()
    {
        playerTimeScale = Time.timeScale;
        Time.timeScale = 0;
        AbilitiesEnum passive, activeA, activeB;
        System.Enum.TryParse<AbilitiesEnum>(PlayerController.Instance.AbilitiesController.Passive.ToString(), out passive);
        System.Enum.TryParse<AbilitiesEnum>(PlayerController.Instance.AbilitiesController.ActiveA.ToString(), out activeA);
        System.Enum.TryParse<AbilitiesEnum>(PlayerController.Instance.AbilitiesController.ActiveB.ToString(), out activeB);
        Passive.Activate(PlayerController.Instance.AbilitiesController.AbilitiesInfos.GetFullInfo(passive.ToString()), PlayerController.Instance.AbilitiesController.GetAbilityRank(passive));
        ActiveA.Activate(PlayerController.Instance.AbilitiesController.AbilitiesInfos.GetFullInfo(activeA.ToString()), PlayerController.Instance.AbilitiesController.GetAbilityRank(activeA));
        ActiveB.Activate(PlayerController.Instance.AbilitiesController.AbilitiesInfos.GetFullInfo(activeB.ToString()), PlayerController.Instance.AbilitiesController.GetAbilityRank(activeB));
        PausePanel.SetActive(true);
    }

    void Unpause() 
    {
        Time.timeScale = playerTimeScale;
        PausePanel.SetActive(false);
    } 

    public void GameplayPause()
    {
        AcceptInput = false;
        gameplayTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    public void GameplayUnpause()
    {
        AcceptInput = true;
        Time.timeScale = gameplayTimeScale;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
