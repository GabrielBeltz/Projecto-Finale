using UnityEngine;
// Classe criada pra evitar refer�ncias diretas de GameObjects que n�o existem em algumas cenas mas s�o marcados como DontDestroyOnLoad
public class CommonInteractionsEvents : MonoBehaviour
{
    public void InteractionText(string text) => TextDisplayer.Instance.DisplayText(text);

    public void EndInteractionText() 
    { 
        if(TextDisplayer.Instance.dialogBox != null) TextDisplayer.Instance.dialogBox.SetActive(false);
    } 

    public void LoadScene(string sceneName) => SceneTransitionManager.Instance.LoadScene(sceneName);

    public void SetKeyTrue(string keyName)
    {
        InteractionKey key = new InteractionKey();
        key.name = keyName;
        key.value = true;
        InteractionManager.Instance.SetKey(key);
    }
}
