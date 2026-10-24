using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using static UnityEngine.GraphicsBuffer;

public class Train : MonoBehaviour
{
    public string Model;
    public double Cost;
    public float Speed;
    public int MaxCapacity;

    protected Line Line;

    public int currentPax = 0;
    private Station currentStation;   
    private Station destination;
    public Station nextStation;

    private int i;
    private float timer;

    public GameObject trainPrefab;
    [SerializeField] GameObject trainLineColour;
    private SpriteRenderer sr;
    private SpriteRenderer lineColourSr;

    private List<Passenger> listOfPassengers;
    List<Passenger> passengersToRemove;

    private SystemScript sysScript;
 
    void Update()
    {
        if (Line != null)
        { 
            if (transform.position == nextStation.District.transform.position) //if train has reached the next station
            {
                currentStation = nextStation;

                //check if reached destination
                if (currentStation == destination)
                {
                    //stop for 1s
                    if (timer >= 1)
                    {
                        //switch destination to other end of line
                        if (destination == Line.destB)
                        {
                            destination = Line.destA;
                            i = Line.StationsServed.Count - 1;
                            nextStation = Line.StationsServed[i];
                        }
                        else
                        {
                            destination = Line.destB;
                            i = 0;
                            nextStation = Line.StationsServed[i];
                        }

                        //deboard any passengers
                        CheckPassengers();

                        //board passengers
                        currentStation.BoardPassengers(this);

                        //change colour depending on capacity
                        SetColour();

                        timer = 0;
                    }
                    else
                    {
                        timer += Time.deltaTime;
                    }

                }
                //if station isn't a destination station
                else
                {
                    //stop for 1s
                    if (timer >= 1)
                    {
                        //increment next station variable
                        if (destination == Line.destA) { i--; }
                        else if (destination == Line.destB) { i++; }
                        nextStation = Line.StationsServed[i];

                        //deboard any passengers
                        CheckPassengers();

                        //board passengers
                        currentStation.BoardPassengers(this);
                        SetColour();

                        //change colour depending on capacity
                        timer = 0;

                    }
                    else
                    {
                        timer += Time.deltaTime;
                    }
                }
            }
            else
            {
                //move towards next station
                transform.position = Vector3.MoveTowards(transform.position, nextStation.District.transform.position, Time.deltaTime * Speed);
            }

            if(nextStation != currentStation)
            {
                //The following code in this if statement was not written by me - used ChatGPT

                // Get the direction from the current object to the target
                Vector3 direction = nextStation.District.transform.position - currentStation.District.transform.position;

                // Calculate the angle in the Z plane (2D direction)
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                targetAngle -= 90f;

                // Smoothly rotate the current Z angle towards the target angle
                float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, 1000 * Time.deltaTime);

                // Apply the new angle only to the Z-axis, keeping X and Y fixed
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }

    // method overridden by children train objects
    public virtual void Init(Line line) 
    {
        Line = line;
        Model = null;
        Cost = 0;
        Speed = 0;
        MaxCapacity = 0;

        ConfigureTrain();
    }
    public void ConfigureTrain()
    {
        i = 0;
        timer = 0;
        sr = GetComponent<SpriteRenderer>();
        lineColourSr = trainLineColour.GetComponent<SpriteRenderer>();
        listOfPassengers = new List<Passenger>();
        passengersToRemove = new List<Passenger>();
        sysScript = FindObjectOfType<SystemScript>();

        destination = Line.destB;
        currentStation = Line.StationsServed[i];
        nextStation = Line.StationsServed[i + 1];

        transform.position = new Vector2(currentStation.District.transform.position.x, currentStation.District.transform.position.y);
        SetColour();
        SetTrainLineColour();
    }

    public void SetTrainLineColour()
    {
        lineColourSr.color = Line.lr.startColor;
    }

    //colour changes depending on capacity
    public void SetColour()
    {
        Color colour;
        double percentage = Convert.ToDouble(GetPax()) / Convert.ToDouble(GetCapacity());
        
        if (percentage > 0.85)
        {
            colour = Color.red;
        }
        else if (percentage > 0.5)
        {
            colour = Color.yellow;
        }
        else
        {
            colour = Color.green;
        }

        sr.color = colour;
    }

    public int GetPax()
    {
        return currentPax;
    }

    public int GetCapacity()
    {
        return MaxCapacity;
    }

    public void AddPassenger(Passenger passenger)
    {
        listOfPassengers.Add(passenger);
        currentPax += passenger.passengerMultiplier;
        Line.totalPassengersCarried += passenger.passengerMultiplier; //add total passengers to line total
    }
    public void RemovePassengers()
    {
        foreach(Passenger p in passengersToRemove)
        {
            if (p.ReachedDestination())
            {
                p.citizen.passenger = null;
                Destroy(p);
            }
            else
            {
                Debug.Log("Passenger has left the train.");
                currentStation.waitingQueue.Add(p);
                currentStation.passengersWaiting += p.passengerMultiplier;
            }
            listOfPassengers.Remove(p);
            currentPax -= p.passengerMultiplier;
        }
        passengersToRemove.Clear();
        
    }
    public void CheckPassengers()
    {
        foreach (Passenger passenger in listOfPassengers)
        {
            //passenger has visited the current station
            passenger.UpdateWaypoints(currentStation);

            //deboard passenger if train's next station isn't one of the passenger's waypoints
            if (!passenger.waypoints.ContainsKey(nextStation.GetNode()) || 
                (passenger.waypoints.ContainsKey(nextStation.GetNode()) && 
                passenger.waypoints[nextStation.GetNode()] == true)) 
            {
                passengersToRemove.Add(passenger);
            }
        }

        RemovePassengers();
    }

    private void OnMouseEnter()
    {
        //show train capacity if mouse over object
        if (!sysScript.toolbarPanel.activeInHierarchy)
        {
            sysScript.ResetToolBar($"{Line.GetName()} ({Model})", $"{currentPax}/{MaxCapacity} passengers");
            sysScript.UpdateToolBar($"{currentPax}/{MaxCapacity} passengers", sr.color);
        }
    }
    private void OnMouseExit()
    {
        if(!sysScript.newLineMode && !sysScript.journeyPlanMode && !sysScript.infoViewMode)
        {
            sysScript.HideToolBar();
        }     
    }

    public void DeleteTrain()
    {
        Destroy(gameObject);
    }
}

