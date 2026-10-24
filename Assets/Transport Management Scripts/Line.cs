using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class Line : MonoBehaviour
{
    public string Name;
    public List<Train> TrainsOperating;
    public Train trainModel;

    private List<District> DistrictsServed;
    public List<Station> StationsServed;
    public Station destA, destB; //each line has 2 possible destinations

    //line drawing to screen
    public LineRenderer lr;
    private Transform[] points;
    public List<Transform> wayPoints;
    [SerializeField] Line linePrefab;

    public double TotalJourneyTime;

    private SystemScript sysScript;
    private TrainManagerScript tms;
    private LineManager lm;
    private BankManagerScript bank;
    private StatusScript statusScript;

    private double spawnInterval, remainingQuantity, timer;
    public double totalTrains;
    public int totalPassengersCarried;
    public double paxPerSquare;


    private void Start()
    {
        timer = 0;
        totalTrains = 0;
        totalPassengersCarried = 0;
        paxPerSquare = 0;

        sysScript = FindObjectOfType<SystemScript>();
        tms = FindObjectOfType<TrainManagerScript>();
        lm = FindObjectOfType<LineManager>();
        bank = FindObjectOfType<BankManagerScript>();
        statusScript = FindObjectOfType<StatusScript>();

        TrainsOperating = new List<Train>();
        DistrictsServed = new List<District>();
        StationsServed = new List<Station>();

        sysScript.SelectedLine = this;

        if (sysScript.newLineMode)
        {
            sysScript.ListOfLines.Add(this);
        }
    }

    private void Update()
    {
        //spawn trains at a regular interval so they don't bunch up
        if (remainingQuantity > 0)
        {
            if(timer > spawnInterval)
            {
                SpawnTrain();
                timer = 0;
            }
            else { timer += Time.deltaTime; }
        }
    }
    public string GetName()
    {
        return Name;
    }

    public int GetTotalTrains()
    {
        return TrainsOperating.Count;
    }

    public List<Train> GetTrains()
    {
        return TrainsOperating;
    }

    //method to add trains to line
    public void SetupTrains(Train model, double quantity, double SpawnInterval)
    {
        trainModel = model;
        spawnInterval = SpawnInterval;
        remainingQuantity = quantity;
        totalTrains += quantity;
        SpawnTrain();
    }

    public void SpawnTrain()
    {
        Train newTrain = null;

        newTrain = Instantiate(trainModel, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
        newTrain.Init(this);
        TrainsOperating.Add(newTrain);
        remainingQuantity--;

        if (sysScript.SelectedLine == this)
        {
            lm.UpdateTrainCount();
        }
    }

    public int GetTotalCapacity()
    {
        int capacity = 0;

        foreach (Train train in TrainsOperating)
        {
            capacity += train.GetCapacity();
        }

        return capacity;
    }

    public int GetTotalPax()
    {
        int total = 0;

        foreach (Train train in TrainsOperating)
        {
            total += train.GetPax();
        }

        return total;
    }

    public List<Station> GetStations()
    {
        return StationsServed;
    }

    //draw line to screen
    //when a district is clicked this is called
    public void AddWayPoint(Transform transform) 
    {
        wayPoints.Add(transform); //add the transform of the district to the list
        SetupLine(wayPoints.ToArray()); //redraw the line

    }
    public void SetupLine(Transform[] points) //called when line is created
    {
        lr.positionCount = points.Length;
        this.points = points;

        reDraw();
    }

    void reDraw() //called when line is changed or when the line is created
    {
        for (int i = 0; i < points.Length; i++)
        {
            lr.SetPosition(i, points[i].position);
        }
    }

    //create edges between each station on this line
    //called when complete line button is pressed
    public void CreateEdges()
    {
        for (int s = 0; s < StationsServed.Count-1; s++) //go through each station on the line
        {
            //calculate journey time based on transform position (one square is approx 1 min)
            double distance = CalculateJourneyTime(StationsServed[s].District.transform.position, StationsServed[s + 1].District.transform.position);

            StationsServed[s].GetNode().AddEdge(StationsServed[s+1].GetNode(), distance, this);
            TotalJourneyTime += distance;

            //add total no of track segments for maintenance costs
            bank.noOfTrackSegments += (int)distance;
            bank.UpdateMaintenanceCost();
        }
    }

    public void SetDestinations(Station stn)
    {
        if(destA == null){destA = stn; destA.SetStationIcon(); } else {destB = stn; destB.SetStationIcon(); }
    }

    public double CalculateJourneyTime(Vector3 start, Vector3 end)
    {
        //use pythagoras theorem to find distance between 2 co-ordinates
        double distancesSquared = Math.Pow( (start.x - end.x), 2) + Math.Pow( (start.y - end.y), 2);
        double distance = Math.Sqrt( distancesSquared );
       
        return Math.Round(distance, 4);
    }

    //if right click then refund last piece of track built
    public void RefundTrackFromLastDestination()
    {
        Station stnToRemove = StationsServed.Last();

        wayPoints.Remove(stnToRemove.District.transform);
        SetupLine(wayPoints.ToArray());

        StationsServed.Remove(stnToRemove);
        stnToRemove.RemoveLine(this);
        stnToRemove.SetStationIcon(); //reset station icon

        SetDestinations(StationsServed.Last());

        bank.Refund(bank.CalculatePriceOfTrack(stnToRemove.District.transform.position, StationsServed.Last().District.transform.position));
    }

    public void DeleteLine()
    {
        if (statusScript.waveActivated)
        {
            //error message as cannot delete line during rush hour
            sysScript.ErrorMessage(6);
        }
        else
        {
            foreach (Edge e in sysScript.ListOfEdges)
            {
                if (e.GetLine() == this)
                {
                    e.Delete();
                }
            }

            sysScript.UpdateRatings();

            foreach (Train t in TrainsOperating)
            {
                t.DeleteTrain();
            }

            foreach (Station stn in StationsServed)
            {
                stn.LinesServed.Remove(this);
                stn.SetStationIcon();
            }


            //remove track segments from maintenance costs
            bank.noOfTrackSegments -= (int)sysScript.journeys[(destA.District, destB.District)].trainLineDistance; 
            bank.UpdateMaintenanceCost();

            sysScript.ListOfLines.Remove(this);


            DestroyLine();
        }
    }

    public void DestroyLine()
    {
        Destroy(this.lr);
        Destroy(gameObject);
    }

    //redraw line to add an extension or reroute
    public void RedrawLine()
    {
        if(TrainsOperating.Count > 0)
        {
            sysScript.ErrorMessage(4);
        }
        else
        {
            lm.CloseLineManager();

            sysScript.ResetToolBar($"Redrawing {Name}", "Click on the districts you would like to add to this line");
            sysScript.newLineMode = true;
            sysScript.SelectedLine = this;

            //display old line path for reference
            lm.DrawOldLinePath(wayPoints.ToArray());

            double trackRefundTotal = 0;
            for (int i = 0; i < wayPoints.Count-1; i++)
            {
                trackRefundTotal += bank.CalculatePriceOfTrack(wayPoints[i].position, wayPoints[i+1].position);
            }

            bank.Refund(trackRefundTotal);

            wayPoints.Clear();
            SetupLine(wayPoints.ToArray());

            foreach (Edge e in sysScript.ListOfEdges)
            {
                if (e.GetLine() == this) { e.Delete(); }
            }

            foreach (Station stn in StationsServed)
            {
                stn.LinesServed.Remove(this);
                stn.SetStationIcon();
            }
            sysScript.newLineButtonText.text = "Complete Line";

            destA = null;
            destB = null;
            StationsServed.Clear();

            sysScript.UpdateRatings();
        }      
    }
}