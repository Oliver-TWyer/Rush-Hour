using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Data;
using Unity.VisualScripting;
using UnityEngine.UI;

public class StatusScript : MonoBehaviour
{
    //script references
    private SystemScript sysScript;
    private BankManagerScript bank;
    private FloatingMessageScript floatingMessageScript;
    private GridManager gridManager;
    private GameOverScript gameOver;
    private InformationViewScript informationViewScript;

    public List<CarPassenger> activeCarJourneys;
    public TextMeshProUGUI populationText, employmentText, activePaxText,
        activeCarPaxText, systemRatingText;
    public int population, employment;
    public List<Passenger> activePax;
    private double timer;
    private DateTime time;
    private List<CarPassenger> finishedCarJourneys;
    public bool waveActivated;
    public TextMeshProUGUI statusText;
    private Color standardColour;
    public int abortedBecauseOfLongJourneyTime;

    //statistics
    public double totalPax, totalCarPax, totalDistanceTravelled, revenue;
    public double previousTotalPax, previousTotalCarPax;
    public double activePaxNumber;
    public double averageRating;

    [SerializeField] TMP_Dropdown linesTotalPaxDropdown;
    [SerializeField] TMP_Dropdown linesPaxPerSqDropdown;

    [SerializeField] Button expandMapButton;

    //rush hour summary panel
    public GameObject WaveSummaryPanel;
    public TextMeshProUGUI trainJourneysText, carJourneysText, abortedJourneysText, 
        airPollutionText, revenueText, maintenanceText, profitText;

    [SerializeField] AudioSource rushHourCompleteSFX;


    private void Start()
    {
        sysScript = FindObjectOfType<SystemScript>();
        bank = FindObjectOfType<BankManagerScript>();
        floatingMessageScript = FindObjectOfType<FloatingMessageScript>();
        gridManager = FindObjectOfType<GridManager>();
        gameOver = FindObjectOfType<GameOverScript>();
        informationViewScript = FindObjectOfType<InformationViewScript>();

        activePax = new List<Passenger>();
        activePaxNumber = 0;
        activeCarJourneys = new List<CarPassenger>();
        finishedCarJourneys = new List<CarPassenger>();
        population = 0; employment = 0;
        timer = 0;
        waveActivated = false;

        standardColour = sysScript.standardColour;
        abortedBecauseOfLongJourneyTime = 0;
        averageRating = 0;

        UpdateStats();
    }
    private void Update()
    {
        time = DateTime.Now;

        //check if car passengers have completed their journeys
        foreach(CarPassenger cpsg in activeCarJourneys)
        { 
            if (time > cpsg.finishTime)
            {
                finishedCarJourneys.Add(cpsg);
            }
        }
       
        foreach(CarPassenger cpsg in finishedCarJourneys)
        {
            activeCarJourneys.Remove(cpsg);
        }
        finishedCarJourneys.Clear();

        //update on screen data every 0.5 secondd
        if (timer < 0.5)
        {
            timer += Time.deltaTime;
        }
        else
        {
            activePaxText.text = activePaxNumber.ToString();
            activeCarPaxText.text = activeCarJourneys.Count.ToString();

            if (informationViewScript.infoViewActive)
            {
                informationViewScript.Refresh();
            }
            
            timer = 0;
        }

        //if rush hour has been completed (everyone has completed their journeys)
        if(waveActivated && activePaxNumber == 0 && activeCarJourneys.Count == 0)
        {
            UpdateStatus("Activate a rush hour so citizens can board the trains");
            OpenWaveSummary();

            abortedBecauseOfLongJourneyTime = 0;
            waveActivated = false;
        }
    }

    //activate rush hour
    public void ActivateWave()
    {
        previousTotalPax = totalPax;
        previousTotalCarPax = totalCarPax;

        waveActivated = true;

        //store start statistics for the wave summary
        totalPax = activePaxNumber;
        totalCarPax = activeCarJourneys.Count;
    }

    //get fare revenue
    public void GetFareRevenue()
    {
        revenue = bank.AddFareRevenue(totalDistanceTravelled);
        revenueText.text = $"${revenue}";

        totalDistanceTravelled = 0;
    }

    //update status message of system
    public void UpdateStatus(string text, Color colour)
    {
        statusText.text = text;
        statusText.color = colour;
    }
    public void UpdateStatus(string text)
    {
        statusText.text = text;
        statusText.color = standardColour;
    }

    //rush hour summary
    //displays info such as total passengers, fare revenue
    //total car usage etc
    public void OpenWaveSummary()
    {
        WaveSummaryPanel.SetActive(true);
        rushHourCompleteSFX.Play();

        if (previousTotalPax != 0 && totalPax > previousTotalPax)
        {
            trainJourneysText.text = $"{totalPax.ToString()} (+{Math.Round(((totalPax - previousTotalPax) / previousTotalPax) * 100, 0)}%)";
        }
        else { trainJourneysText.text = totalPax.ToString(); }

        if (previousTotalCarPax != 0 && totalCarPax < previousTotalCarPax)
        {
            carJourneysText.text = $"{totalCarPax.ToString()} ({Math.Round(((totalCarPax - previousTotalCarPax) / previousTotalCarPax) * 100, 0)}%)";
        }
        else { carJourneysText.text = totalCarPax.ToString(); }

        abortedJourneysText.text = abortedBecauseOfLongJourneyTime.ToString();

        foreach (Line l in sysScript.ListOfLines)
        {
            l.paxPerSquare = Math.Round(l.totalPassengersCarried / sysScript.journeys[(l.destA.District, l.destB.District)].trainLineDistance, 1);
        }

        //ordered list of lines by total passengers, and per square km

        List<Line> lineUsageByPassengers = MergeSort(sysScript.ListOfLines, "total");
        List<Line> lineUsageByPassengersPerSq = MergeSort(sysScript.ListOfLines, "perSquare");

        List<string> lineUsageByPassengersStr = new List<string>();
        List<string> lineUsageByPassengersPerSqStr = new List<string>();

        int rank = 1;
        for (int i = sysScript.ListOfLines.Count - 1; i >= 0; i--)
        {
            Line byPax = lineUsageByPassengers[i];
            Line byPerSquare = lineUsageByPassengersPerSq[i];
            lineUsageByPassengersStr.Add($"{rank}: {byPax.Name} - {byPax.totalPassengersCarried}");
            lineUsageByPassengersPerSqStr.Add($"{rank}: {byPerSquare.Name} - {byPerSquare.paxPerSquare}");

            rank++;
        }

        linesTotalPaxDropdown.ClearOptions();
        linesPaxPerSqDropdown.ClearOptions();

        linesTotalPaxDropdown.AddOptions(lineUsageByPassengersStr);
        linesPaxPerSqDropdown.AddOptions(lineUsageByPassengersPerSqStr);

        GetFareRevenue();

        //deduct cost of maintenance from balance and check for bankrupc
        (bool, double) maintenance = bank.DeductMaintenanceCost();

        //bank returns false if there is NOT enough money to cover maintenance
        if (!maintenance.Item1)
        {
            profitText.color = new Color32(135, 20, 0, 255);

            gameOver.OpenGameOver();
        }

        maintenanceText.text = ($"${maintenance.Item2.ToString()}");
        profitText.text = ($"£{(revenue - maintenance.Item2).ToString()}");
    }

    public void CloseWaveSummary()
    {
        WaveSummaryPanel.SetActive(false);
    }

    //system rating is based on the ratings of journeys between stations
    //(see design section 8.2)
    public void UpdateSystemRating()
    {
        int totalTrainJourneys = 0, totalCarJourneys = 0, totalJourneys;
        double trainRatingN = 0;

        totalJourneys = sysScript.journeys.Values.Count;

        //calculate mean average of journey ratings
        foreach (Journey j in sysScript.journeys.Values)
        {
            if (j.pathFound) { totalTrainJourneys++; trainRatingN += j.rating; }
            else { totalCarJourneys++; }
        }
        //rating stays between 0 and 10, rounded to 1.d.p
        averageRating = Math.Round(Math.Clamp((trainRatingN / totalJourneys) * 1.5,0,10),1);

        systemRatingText.text = $"{averageRating.ToString()}/10";

        //map can be expanded if the system rating >= 8
        if(averageRating >= 8) { expandMapButton.interactable = true; }
        else { expandMapButton.interactable = false; }
    }

    public void UpdateStats()
    {
        population = 0; employment = 0;

        foreach(District d in sysScript.ListOfDistricts)
        {
            population += d.Population;
            employment += d.Employment;
        }

        populationText.text = population.ToString();

        UpdateSystemRating();

    }

    public void ExpandMap()
    {
        gridManager.LoadExpandMap();
    }

    public void TryExpand()
    {
        if(averageRating > 8)
        {
            floatingMessageScript.ShowMessage("Expand the city to serve more districts by train!");
        }
        else
        {
            floatingMessageScript.ShowMessage("System must have a higher rating before expansion");
        }
    }

    public void MouseExitedButton()
    {
        floatingMessageScript.HideMessage();
    }

    //assisted by YouTube vid by CodingWithJohn: https://www.youtube.com/watch?v=bOk35XmHPKs

    //merge sort is used to order lines by their usage
    //I personalised it so it can either order the lines by their total usage
    //Or there usage per sq/km (i.e. their usage density)
    private List<Line> MergeSort(List<Line> list, string sortBy)
    {
        List<Line> left_list = null;
        List<Line> right_list = null;

        //check if list has more than 1 element
        if (list.Count > 1)
        {
            //split list into two halves
            int midpoint = list.Count / 2;
            left_list = list.GetRange(0, midpoint);
            right_list = list.GetRange(midpoint, list.Count - midpoint);

            //recursively split the lists
            left_list = MergeSort(left_list, sortBy);
            right_list = MergeSort(right_list, sortBy);
        }
        else
        {
            //lists with one element are "already sorted" - no need to merge
            return list;
        }

        //merge based on the selected attribute
        if (sortBy == "total")
        {
            return MergeByTotal(left_list, right_list);
        }
        else if (sortBy == "perSquare")
        {
            return MergeByPerSquare(left_list, right_list);
        }

        return null;
    }

    private List<Line> MergeByTotal(List<Line> leftList, List<Line> rightList)
    {
        List<Line> outputList = new List<Line>();

        //track indexes as we compare elements from L and R array
        int indexL = 0,
            indexR = 0;

        //while we have not run out of elements to compare
        while (indexL < leftList.Count && indexR < rightList.Count)
        {
            if (leftList[indexL].totalPassengersCarried <= rightList[indexR].totalPassengersCarried)
            {
                outputList.Add(leftList[indexL]);
                indexL++;
            }
            else
            {
                outputList.Add(rightList[indexR]);
                indexR++;
            }
        }

        while (indexL < leftList.Count)
        {
            outputList.Add(leftList[indexL]);
            indexL++;
        }

        while (indexR < rightList.Count)
        {
            outputList.Add(rightList[indexR]);
            indexR++;
        }

        return outputList;
    }

    private List<Line> MergeByPerSquare(List<Line> leftList, List<Line> rightList)
    {
        List<Line> outputList = new List<Line>();

        //track indexes as we compare elements from L and R array
        int indexL = 0,
            indexR = 0;

        //while we have not run out of elements to compare
        while (indexL < leftList.Count && indexR < rightList.Count)
        {
            if (leftList[indexL].paxPerSquare <= rightList[indexR].paxPerSquare)
            {
                outputList.Add(leftList[indexL]);
                indexL++;
            }
            else
            {
                outputList.Add(rightList[indexR]);
                indexR++;
            }
        }

        while (indexL < leftList.Count)
        {
            outputList.Add(leftList[indexL]);
            indexL++;
        }

        while (indexR < rightList.Count)
        {
            outputList.Add(rightList[indexR]);
            indexR++;
        }

        return outputList;
    }
}



