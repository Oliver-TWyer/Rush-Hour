using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenScript : MonoBehaviour
{
    [SerializeField] public GameObject loadingScreenCanvas;
    [SerializeField] public TextMeshProUGUI loadingScreenText;

    public Dictionary<string, string> loadingScreenDict;

    void Start()
    {
        //loading screen dictionary stores states as strings,
        //and key = message displayed on screen

        loadingScreenDict = new Dictionary<string, string>();

        loadingScreenDict.Add("Rush Hour", "Starting Rush Hour...");
        loadingScreenDict.Add("Map Expansion", "Generating New Districts...");
        loadingScreenDict.Add("Load Main Game", "Loading Map...");
    }

    public void OpenLoadingScreen(string state)
    { 
        loadingScreenCanvas.SetActive(true);
        loadingScreenText.text = loadingScreenDict[state];
    }

    public void CloseLoadingScreen()
    {
        loadingScreenCanvas.SetActive(false);
    }
}
