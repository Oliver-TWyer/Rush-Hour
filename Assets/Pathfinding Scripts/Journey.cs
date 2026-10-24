using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class Journey : ScriptableObject
{
    public District start, end;
    public double straightLineDistance;
    public double trainLineDistance;
    public List<Node> waypoints;
    private List<Line> linesTaken;
    public double rating;
    public bool pathFound;

    private SystemScript sysScript;

    public void Init(District s, District e)
    {
        start = s;
        end = e;

        sysScript = FindObjectOfType<SystemScript>();

        straightLineDistance = GetStraightLineDistance();
        pathFound = false;
        CalculateRating();
    }

    public void UpdateJourney()
    {
        //if there is a station at either end of the journey
        if (start.stationBuilt && end.stationBuilt) 
        {
            //Search for all reachable nodes from the start node, and see if the end node can be reached
            //(Search if there is a path from the start node to the end node)
            if (sysScript.InitialiseSearch(start.station.GetNode()).Contains(end.station.GetNode())) 
            {
                //get start node's instance of dijkstra to get waypoints to end node
                Djikstra djk = start.station.djikstra;
                pathFound = true;

                //store the waypoints for this particular journey
                (waypoints, linesTaken) = djk.ReturnWaypoints(end.station.GetNode()); 

                //include start station in waypoints
                waypoints.Insert(0,start.station.GetNode());

                //get the distance of the train journey
                trainLineDistance = start.station.ReturnDistance(end.station.GetNode());

                CalculateRating();
            }
            //no path found
            else { pathFound = false; }
        }
        else { pathFound = false; CalculateRating(); }
    }

    private double GetStraightLineDistance()
    {
        Vector3 startPos = start.transform.position; //gets transform position of the start and end station
        Vector3 endPos = end.transform.position;

        // use pythagoras theorem to find distance between 2 co - ordinates
        double distancesSquared = Math.Pow((startPos.x - endPos.x), 2) + Math.Pow((startPos.y - endPos.y), 2);
        double distance = Math.Sqrt(distancesSquared);

        return Math.Round(distance, 4); //calculates straight line distance
    }

    private void CalculateRating()
    {
        //rate the journey based on length ratios
        if (pathFound) { rating = Math.Round(((straightLineDistance / trainLineDistance) * 10), 1); }
        else { rating = 10/straightLineDistance; }
    }
}
