using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextDisplayer : MonoBehaviour
{
    public GameObject dialogBox;
    public TextMeshProUGUI textMeshUGUI;

    public void DisplayText(string textToDisplay)
    {
        dialogBox.SetActive(true);
        textMeshUGUI.text = textToDisplay;
    }
}
