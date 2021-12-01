using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Classe criada pra evitar refer�ncias diretas de GameObjects que n�o existem em algumas cenas mas s�o marcados como DontDestroyOnLoad
public class CommonInteractionsEvents : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneTransitionManager.Instance.LoadScene(sceneName);
    }
}
