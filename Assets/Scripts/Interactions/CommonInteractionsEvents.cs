using UnityEngine;
// Classe criada pra evitar referências diretas de GameObjects que não existem em algumas cenas mas são marcados como DontDestroyOnLoad
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
