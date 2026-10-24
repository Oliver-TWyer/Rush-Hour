using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] AudioSource buttonClickSFX;

    public void PlayGame()
    {
        SceneManager.LoadScene("MainGame");
        Time.timeScale = 1.0f;
    }

    public void PlayButtonClick()
    {
        buttonClickSFX.Play();
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
