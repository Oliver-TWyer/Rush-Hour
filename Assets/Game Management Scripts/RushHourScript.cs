using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RushHourScript : MonoBehaviour
{
    //script references
    private SystemScript sysScript;
    private StatusScript statusScript;
    private FloatingMessageScript floatingMessageScript;
    private LoadingScreenScript loadingScreenScript;

    private DateTime nextAvailableRHTime;

    private bool mouseIsHovering;
    private bool rushHourAvailable;

    [SerializeField] Button button;
    [SerializeField] AudioSource ButtonSFX;
    private void Start()
    {
        sysScript = FindObjectOfType<SystemScript>();
        statusScript = FindObjectOfType<StatusScript>();
        floatingMessageScript = FindObjectOfType<FloatingMessageScript>();
        loadingScreenScript = FindObjectOfType<LoadingScreenScript>();

        rushHourAvailable = false;
        nextAvailableRHTime = DateTime.Now.AddSeconds(10);

        button.interactable = false;
    }

    private void Update()
    {
        if(mouseIsHovering)
        { 
            // Calculate the time difference
            TimeSpan timeDifference = nextAvailableRHTime - DateTime.Now;

            // Get the total number of seconds
            double seconds = Math.Round(timeDifference.TotalSeconds,0);

            if(seconds > 0)
            {
                floatingMessageScript.ShowMessage($"Please wait {seconds}s before starting a rush hour");
            }
            else { TryRushHour(); }
        }

        //if enough time has passed since last rush hour
        if(nextAvailableRHTime < DateTime.Now)
        {
            button.interactable = true;
            rushHourAvailable = true;
        }
    }

    //morning and evening rush hours can be called
    public void MorningRush()
    {
        sysScript.UpdateRatings();

        statusScript.UpdateStatus("Morning Rush Hour<br>Citizens are going to work");
        foreach(Citizen c in sysScript.ListOfCitizens)
        {
            c.GoHomeToWork();
        }
        statusScript.ActivateWave();
        loadingScreenScript.CloseLoadingScreen();
    }
    public void EveningRush()
    {
        sysScript.UpdateRatings();

        statusScript.UpdateStatus("Evening Rush Hour<br>Citizens are going home from work");
        foreach (Citizen c in sysScript.ListOfCitizens)
        {
            c.GoWorkToHome();
        }
        statusScript.ActivateWave();
        loadingScreenScript.CloseLoadingScreen();
    }

    //when mouse enters the rush hour button
    public void TryRushHour()
    {
        //check if enough time passed
        if (DateTime.Now > nextAvailableRHTime) 
        {
            button.interactable = true;
            floatingMessageScript.ShowMessage("Citizens will travel to their destinations along the most" +
                " convienient route - you can make money from ticket revenue.");
        }
        else if (statusScript.waveActivated)
        {
            floatingMessageScript.ShowMessage("Please wait for the rush hour to finish before starting a new one.");
        }
        else
        {
            //rush hour can be started
            button.interactable = false;
            mouseIsHovering = true;
        }
    }
    public void MouseExitedButton()
    {
        mouseIsHovering = false;
        floatingMessageScript.HideMessage();
    }

    //when rush hour button is clicked
    public void LoadRushHour()
    {
        StartCoroutine(StartRushHour());
    }
    public IEnumerator StartRushHour()
    {
        bool trainsRunningOnAllLines = true;

        foreach (Line l in sysScript.ListOfLines)
        {
            if (l.totalTrains == 0)
            {
                trainsRunningOnAllLines = false;
            }
        }

        //check there are trains running on all lines
        //no trains = passengers get stuck at platform
        if (trainsRunningOnAllLines)
        {
            rushHourAvailable = false;

            //if enough time has passed
            if (DateTime.Now > nextAvailableRHTime) 
            {
                //ensures that the loading screen is called before further lines of code
                loadingScreenScript.OpenLoadingScreen("Rush Hour");
                yield return null;

                //update next available time
                nextAvailableRHTime = DateTime.Now.AddSeconds(60);

                //reset statistics
                foreach (Line l in sysScript.ListOfLines)
                {
                    l.totalPassengersCarried = 0;
                }

                button.interactable = false;

                //morning or evening rush hour
                int num = UnityEngine.Random.Range(1, 3);
                switch (num)
                {
                    case 1:
                        MorningRush();
                        break;
                    case 2:
                        EveningRush();
                        break;
                }
            }
        }
        //error message if 1 or more lines don't have trains running
        else { sysScript.ErrorMessage(5); } 
    }
}
