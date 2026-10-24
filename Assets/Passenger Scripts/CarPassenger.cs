using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;

public class CarPassenger : ScriptableObject
{
    private StatusScript statusScript;
    private SystemScript sysScript;

    public Citizen citizen;
    private District start, end;

    public double journeyTime;
    public double timer;
    private bool activeJourney;
    public DateTime finishTime;
    public void Init(Citizen C, District s, District e)
    {
        citizen = C;
        start = s;
        end = e;
        timer = 0;
        journeyTime = 0;

        statusScript = FindObjectOfType<StatusScript>();
        sysScript = FindObjectOfType<SystemScript>();

        StartCarJourney();
    }
    public void StartCarJourney()
    {
        statusScript.activeCarJourneys.Add(this);

        //multiply journey time by random number to reflect road traffic
        journeyTime = GetJourneyTime()*UnityEngine.Random.Range(2,6);

        finishTime = DateTime.Now.AddSeconds(journeyTime);
    }
    public double GetJourneyTime()
    {
        return sysScript.GetJourney(start, end).straightLineDistance;
    }
}
