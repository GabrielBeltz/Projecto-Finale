using UnityEngine;

public class PauseController : MonoBehaviour
{
    public bool AcceptInput = true;
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
    }

    void Unpause() => Time.timeScale = playerTimeScale;

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
}
