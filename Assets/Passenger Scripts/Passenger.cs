using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class Passenger : ScriptableObject
{
    private SystemScript sysScript;
    private StatusScript statusScript;

    public Station start, end;
    public Dictionary<Node, bool> waypoints = new Dictionary<Node, bool>();
    public Citizen citizen;
    public int passengerMultiplier;

    public Node nextWaypoint;
    public Station currentStation;

    public void Init(Citizen C, Station s, Station e, int citizenMultiplier)
    {
        citizen = C;
        start = s;
        end = e;
        passengerMultiplier = citizenMultiplier;
        sysScript = FindObjectOfType<SystemScript>();
        statusScript = FindObjectOfType<StatusScript>();

        Debug.Log($"I am taking the train from {s.District.Name} to {e.District.Name}!");

        StartJourney();
    }

    public void StartJourney()
    {
        statusScript.activePaxNumber += passengerMultiplier;

        Journey journey = sysScript.GetJourney(start.District, end.District);

        foreach (Node n in journey.waypoints)
        {
            waypoints.Add(n, false);
        }

        //to help calculate fare revenue
        statusScript.totalDistanceTravelled += journey.trainLineDistance * passengerMultiplier;

        UpdateWaypoints(start);

        start.waitingQueue.Add(this); //add the passenger to the platform at the start station
        start.passengersWaiting += passengerMultiplier;
    }

    public void UpdateWaypoints(Station station)
    {
        waypoints[station.GetNode()] = true;
        Debug.Log($"I have visited {station.District.Name}");
    }

    //returns true if passenger reached their destination
    public bool ReachedDestination()
    {
        if (!waypoints.ContainsValue(false))
        {
            statusScript.activePaxNumber -= passengerMultiplier;
            Debug.Log($"I reached my destination at {end.District.Name!}");
            return true;
        }
        else
        {
            return false;
        }
    }

}
