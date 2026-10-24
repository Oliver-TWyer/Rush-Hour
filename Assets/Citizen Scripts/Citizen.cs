using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Citizen : ScriptableObject
{
    public District Home;
    public District Work;
    public Passenger passenger;

    //one instance of citizen may represent up to 10 in game citizens
    //(see design section 9.1)
    public int citizenMultiplier;

    protected SystemScript sysScript;
    protected StatusScript statusScript;

    public virtual void Init()
    {
        sysScript = FindObjectOfType<SystemScript>();
        statusScript = FindObjectOfType<StatusScript>();

        citizenMultiplier = UnityEngine.Random.Range(1, 10);

        AssignHomeandWork();
    }

    public void AssignHomeandWork()
    {
        if (UnityEngine.Random.Range(1, 5) < 4)
        {
            Home = ReturnResidential();
        }
        else
        {
            Home = ReturnBusiness();
        }

        if (UnityEngine.Random.Range(1, 5) < 4)
        {
            Work = ReturnBusiness();
        }
        else
        {
            Work = ReturnResidential();
        }

        Home.Population += citizenMultiplier;
        Work.Employment += citizenMultiplier;
    }

    //morning rush hour
    public void GoHomeToWork()
    {
        if (Home != Work)
        {
            CheckJourney(Home, Work);
        }
    }

    //evening rush hour
    public void GoWorkToHome()
    {
        if (Home != Work)
        {
            CheckJourney(Work, Home);
        }
    }

    //decides whether the citizen will take the car or train to complete their journey
    public void CheckJourney(District start, District end)
    {
        Journey journey = sysScript.GetJourney(start, end);

        //if there is a path between the start and end node
        if (journey.pathFound)
        {
            if (journey.rating == 10)
            {
                CreatePassenger(start.station, end.station);
            }
            else //likelihood the citizen will take the train depends on rating
            {
                if (sysScript.MakeDecision(journey.rating))
                {
                    CreatePassenger(start.station, end.station);
                }
                //passenger may still prefer to use the car if the train journey takes too long
                else { TakeTheCar(start, end); statusScript.abortedBecauseOfLongJourneyTime++; }
            }
        }
        else { Home.carUsage++; TakeTheCar(start, end); } // take car if no path
    }

    //create a car passenger
    public void TakeTheCar(District start, District end)
    {
        CarPassenger cpsg = ScriptableObject.CreateInstance<CarPassenger>();
        cpsg.Init(this, start, end);
    }

    //create a train passenger
    public void CreatePassenger(Station start, Station end)
    {
        Passenger psg = ScriptableObject.CreateInstance<Passenger>();
        psg.Init(this, start, end, citizenMultiplier);
    }

    private District ReturnResidential()
    {
        return sysScript.ListOfResidentialDistricts[UnityEngine.Random.Range(0, sysScript.ListOfResidentialDistricts.Count)];
    }

    private District ReturnBusiness()
    {
        return sysScript.ListOfBusinessDistricts[UnityEngine.Random.Range(0, sysScript.ListOfBusinessDistricts.Count)];
    }
}