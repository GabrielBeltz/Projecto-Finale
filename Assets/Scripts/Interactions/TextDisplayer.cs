using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextDisplayer : MonoBehaviour
{
    public static TextDisplayer Instance;
    public GameObject dialogBox;
    public TextMeshProUGUI textMeshUGUI;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void DisplayText(string textToDisplay)
    {
        dialogBox.SetActive(true);
        textMeshUGUI.text = textToDisplay;
    }
}
