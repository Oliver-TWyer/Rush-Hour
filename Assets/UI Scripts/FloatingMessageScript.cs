using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingMessageScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] GameObject panel;

    public void ShowMessage(string text)
    {
        textBox.text = text;
        panel.SetActive(true);
    }

    public void HideMessage()
    {
        panel.SetActive(false);
    }
}
