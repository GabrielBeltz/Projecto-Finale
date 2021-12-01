using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Classe criada pra evitar referências diretas de GameObjects que não existem em algumas cenas mas são marcados como DontDestroyOnLoad
public class CommonInteractionsEvents : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneTransitionManager.Instance.LoadScene(sceneName);
    }
}
