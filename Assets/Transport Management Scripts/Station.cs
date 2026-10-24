using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Station : MonoBehaviour
{
    public District District;
    public int passengersWaiting;

    public List<Line> LinesServed;
    public List<Passenger> waitingQueue;

    private SystemScript sysScript;
    private BankManagerScript bank;
    private SpriteRenderer sr;

    //pathfinding references associarted with this station object
    public Djikstra djikstra;
    private Node Node;
    private List<Node> visited;
    private List<Node> reachableNodes;

    //reference to prefabs
    [SerializeField] GameObject singleLineStation;
    [SerializeField] GameObject interchangeStation;

    //Init called from district
    public void Init(District district)
    {
        District = district;
        sysScript = GameObject.FindGameObjectWithTag("sysScr").GetComponent<SystemScript>();
        djikstra = ScriptableObject.CreateInstance<Djikstra>();
        bank = GameObject.FindObjectOfType<BankManagerScript>();

        sr = singleLineStation.GetComponent<SpriteRenderer>();

        LinesServed = new List<Line>();
        waitingQueue = new List<Passenger>();
        visited = new List<Node> ();

        //create a node for this station
        Node = sysScript.AddStation(this);

        //increment number of stations for maintenance costs
        bank.noOfStations++;
        bank.UpdateMaintenanceCost();

        //UI
        PositionStations();
        SetStationIcon();
    }

    private void PositionStations()
    {
        singleLineStation.transform.position = District.transform.position;
        interchangeStation.transform.position = District.transform.position;
    }

    //run when a new line is created (new edges on graph)
    public void UpdateDjikstra()
    {
        //returns a list of reachable nodes
        reachableNodes = sysScript.InitialiseSearch(Node);

        //run djikstras algorithm to update distance table
        djikstra.Init(reachableNodes, Node);
    }

    //returns distance to a given node
    public double ReturnDistance(Node endNode)
    {
        return djikstra.ReturnDistance(endNode);
    }

    public District GetDistrict()
    {
        return District;
    }

    //add a line to this station
    public void AddLine(Line line)
    {
        LinesServed.Add(line);
    }

    public void RemoveLine(Line line)
    {  LinesServed.Remove(line); }

    public Node GetNode()
    {
        return Node;
    }

    public List<Line> GetLines()
    {
        return LinesServed;
    }

    //called when a train arrives at station
    //details of passenger multipliers found in design section 9
    //detailed diagram of boarding process found in design section 11
    public void BoardPassengers(Train train)
    {
        List<Passenger> passengersToRemove = new List<Passenger>();

        foreach(Passenger passenger in waitingQueue)
        {
            //if the next station of the train is the same as the next station of the passenger
            if (passenger.waypoints.ContainsKey(train.nextStation.GetNode()) && passenger.waypoints[train.nextStation.GetNode()] == false)
            {
                if(train.GetCapacity() >= (train.GetPax() + passenger.passengerMultiplier))
                {
                    Debug.Log("Passenger has boarded the train");
                    train.AddPassenger(passenger); //add the passenger to the train
                    passengersToRemove.Add(passenger); //remove the passenger from the 'platform'
                }               
            }
        }

        foreach(Passenger p in passengersToRemove)
        {
            waitingQueue.Remove(p);
            passengersWaiting -= p.passengerMultiplier;
        }
        passengersToRemove.Clear();
    }

    public void SetStationIcon()
    {
        interchangeStation.SetActive(false);
        singleLineStation.SetActive(false);

        if (LinesServed.Count > 1)
        {
            interchangeStation.SetActive(true);
        }
        else if (LinesServed.Count == 1) { singleLineStation.SetActive(true); UpdateColour(); }
        else { singleLineStation.SetActive(true); sr.color = Color.black; }
    }
    public void UpdateColour() { sr.color = LinesServed[0].lr.startColor; }
}
