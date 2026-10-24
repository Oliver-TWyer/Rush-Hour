using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorMessageScript : MonoBehaviour
{
    Dictionary<int, string> errors = new Dictionary<int, string>();

    [SerializeField] GameObject errorMessagePanel;
    [SerializeField] TextMeshProUGUI errorMessageText;

    [SerializeField] AudioSource errorSoundEffect;

    private void Start()
    {
        //train manager errors
        errors.Add(0, "All train models must be the same for each line!<br>Remove ALL trains before changing train model for this line.");
        errors.Add(1, "Trains cannot be removed during rush hour!<br>Please wait until the rush hour is over.");
        errors.Add(2, "Please select a train model!");

        //insufficient funds
        errors.Add(3, "You do not have enough money!");

        //cannot redraw line while trains are running
        errors.Add(4, "Cannot redraw line while trains are running!");

        //cannot start rush hour if lines dont have trains running
        errors.Add(5, "Cannot start rush hour as one or more lines don't have trains operating!");

        //cannot delete line if rush hour is active
        errors.Add(6, "Cannot delete line during rush hour!");

        //name input invalid
        errors.Add(7, "Name input is invalid!");
    }
    public void ErrorMessage(int errorCode)
    {
        errorMessagePanel.SetActive(true);
        errorMessageText.text = errors[errorCode];
        errorSoundEffect.Play();
    }

    public void Close()
    {
        errorMessagePanel.SetActive(false);
    }
}
