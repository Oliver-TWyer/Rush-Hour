using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using UnityEditor.Experimental;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BankManagerScript : MonoBehaviour
{
    private SystemScript sysScript;

    private double globalCostMultiplier;

    [SerializeField] TextMeshProUGUI balanceText;
    [SerializeField] TextMeshProUGUI movingMoneyText;
    [SerializeField] TextMeshProUGUI maintenanceCostText;

    //values defined in inspector
    [SerializeField] double balance;

    //constant money values
    const double trackCostMultiplier = 50;
    const double stationCost = 100;
    const double farePerSquare = 0.5;

    //maintenance costs and constant values
    public int noOfTrackSegments;
    public int noOfStations;
    public int noOfTrains;

    const double trackMaintenanceCost = 40;
    const double stationMaintenanceCost = 100;
    const double trainMaintenanceCost = 100;

    private int numberOfExpands;

    [SerializeField] AudioSource purchaseSuccessfulSound;

    private TextAnimationScript textAnimation;

    void Start()
    {
        sysScript = FindObjectOfType<SystemScript>();
        textAnimation = FindObjectOfType<TextAnimationScript>();

        balanceText.text = balance.ToString();
        numberOfExpands = 1;
        globalCostMultiplier = 1;
    }

    //returns T/F if purchase successful
    public bool Purchase(double amount)
    {
        if (balance >= amount)
        {
            balance -= amount;

            UpdateMoney(-amount);

            purchaseSuccessfulSound.Play();

            //purchase went through
            return true;
        }
        //bank account declined
        else { sysScript.ErrorMessage(3); return false; }
    }

    //refund a given amount of money
    public void Refund(double amount) 
    {
        balance += amount;
        UpdateMoney(amount);
        purchaseSuccessfulSound.Play();
    }

    //calculates revenue based on distance travelled by passengers
    public double AddFareRevenue(double totalDistanceTravelled)
    {
        //revenue = TOTAL distance covered by all passengers * rate per square/ km
        double revenue = Math.Round(totalDistanceTravelled * farePerSquare, 2);
        balance += revenue;
        UpdateMoney(revenue);

        return revenue; 
    }
    public double GetBalance() { return balance; }

    //update balance on screen and run animation
    public void UpdateMoney(double difference) 
    {
        balanceText.text = balance.ToString("F2");
        textAnimation.MoneyAnimation(difference);
    }

    //returns cost of track between two node positions
    public double CalculatePriceOfTrack(Vector3 pos1, Vector3 pos2)
    {
        return ReturnInflatedCost(Math.Round(Vector3.Distance(pos1, pos2), 0) * trackCostMultiplier);
    }

    //costs increase as game progresses
    public double ReturnInflatedCost(double originalCost)
    {
        double newCost = originalCost * globalCostMultiplier;

        return newCost;
    }

    //As player expands map, costs increase by proportion.
    public void IncreaseCostsByProportion()
    {
        //increase number of times player expanded
        numberOfExpands++;
        
        //quadratic term rule: 2n^2 + 10n + 4
        double previousDistricts = 2 * Math.Pow(numberOfExpands-1,2) + 10 * (numberOfExpands-1) + 4;
        double currentDistricts = 2 * Math.Pow(numberOfExpands, 2) + 10 * (numberOfExpands) + 4;

        if(numberOfExpands > 1)
        {
            //decimal increase
            double increase = (currentDistricts - previousDistricts) / previousDistricts;

            //update global cost multiplier;
            globalCostMultiplier = globalCostMultiplier + increase; 
        }     
    }

    public double GetStationCost() { return ReturnInflatedCost(stationCost); }

    private double CalculateMaitenanceCost()
    {
        double maintenanceCost = 0;

        //add up total maintenence cost
        maintenanceCost += noOfTrackSegments * trackMaintenanceCost;
        maintenanceCost += noOfStations * stationMaintenanceCost;
        maintenanceCost += noOfTrains * trainMaintenanceCost;

        return maintenanceCost;
    }

    //returns a tuple of purchase successful and cost of maintenance
    public (bool, double) DeductMaintenanceCost()
    {
        double maintenanceCost = CalculateMaitenanceCost();

        if (maintenanceCost < balance) //will deduct money and return true if enough balance
        {
            Purchase(maintenanceCost); //deduct money
            return (true, maintenanceCost);
        }
        else {
            //if insufficient funds then system is bankrupt (game over)
            balance -= maintenanceCost;
            UpdateMoney(maintenanceCost);
            return (false, maintenanceCost); } 

    }
    public void UpdateMaintenanceCost()
    {
        maintenanceCostText.text = CalculateMaitenanceCost().ToString();
    }

}
