using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScript : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] AudioSource gameOverSound;
    [SerializeField] GameObject rushHourSummaryPanel;
    public void OpenGameOver()
    {
        panel.SetActive(true);
        gameOverSound.Play();

        //rush hour summary shows alongside game over screen
        rushHourSummaryPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(615,0,0);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
