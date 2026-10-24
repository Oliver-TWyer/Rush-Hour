using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManagerScript : MonoBehaviour
{
    [SerializeField] GameObject panel;

    private bool panelActive;
    private void Start()
    {
        panelActive = false;
    }

    public void Display()
    {
        panel.SetActive(true);

        panelActive = true;

        //pause the game, trains stop moving around
        Time.timeScale = 0;  
    }

    public void Resume()
    {
        panel.SetActive(false);
        panelActive = false;

        //time set back to normal so trains etc start moving
        Time.timeScale = 1; 
    }

    public void GoToMainMenu()
    {
        panelActive = false;
        SceneManager.LoadScene("Main Menu");
    }

    public void GoToDesktop()
    {
        Application.Quit();
    }

    public void EscKeyPressed()
    {
        if (panelActive)
        {
            Resume();
        }
        else
        {
            Display();
        }
    }
}
