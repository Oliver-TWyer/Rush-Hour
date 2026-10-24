using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnimationScript : MonoBehaviour
{
    [SerializeField] Transform textPos;
    [SerializeField] Transform bankPos;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject textCanvas;

    private Vector3 endPos;
    private Vector3 screenPos;
    private bool textMoving;

    void Start()
    {
        //set position below bank balance on screen
        screenPos = new Vector3(bankPos.position.x, bankPos.position.y-200, bankPos.position.z);
        textMoving = false;
    }

    void Update()
    {
        if (textMoving && textPos.position != endPos)
        {
            textPos.position = Vector3.MoveTowards(textPos.position, endPos, Time.deltaTime * 200);
        }
        else { textMoving = false; textCanvas.SetActive(false); }
    }

    public void MoneyAnimation(double amount) //script for money text to move 
    {
        //if money is added to balance
        if (amount > 0)
        {
            textPos.position = screenPos;
            endPos = bankPos.position;

            text.text = $"+{amount}";
            text.color = Color.green;
            textCanvas.SetActive(true);
            textMoving = true;
        }
        else
        //if money is deducted from balance
        {
            textPos.position = bankPos.position;
            endPos = screenPos;

            text.text = $"{amount}";
            text.color = Color.red;
            textCanvas.SetActive(true);
            textMoving = true;
        }
    }
}
