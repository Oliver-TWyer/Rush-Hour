using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class InformationViewScript : MonoBehaviour
{
    private SystemScript sysScript;
    private StatusScript statusScript;

    //red/orange/yellow colour pallette
    [SerializeField] UnityEngine.Color color1, color2, color3, color4;

    public GameObject infoViewPanel;
    public TMP_Dropdown dropdown;
    public int currentView;
    public bool infoViewActive;
    List<string> options;
    private void Start()
    {
        sysScript = FindObjectOfType<SystemScript>();
        statusScript = FindObjectOfType<StatusScript>();

        options = new List<string> 
        { "Select", "Relative Crowdedness", "Population Density", "Job Density", "Relative Car Usage" };
        dropdown.AddOptions(options);

        infoViewActive = false;
    }
    public void Display()
    {
        infoViewPanel.SetActive(true);
        dropdown.value = 0;
    }
    public void CloseDisplay()
    {
        infoViewPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (infoViewActive)
        {
            CloseInfoView();
        }
        else
        {
            Display();
        }
    }
    public void CloseInfoView()
    {
        infoViewActive = false;
        infoViewPanel.SetActive(false);
        ResetHighlight();
        sysScript.HideToolBar();
    }

    //called every 5 seconds - refreshes information view
    public void Refresh()
    {
        InfoView(currentView);
    }

    //changes information view 
    public void Refresh(int selection)
    {
        CloseDisplay();

        if (dropdown.value != 0)
        {
            currentView = dropdown.value - 1;
            InfoView(currentView);

            if (infoViewActive)
            {
                sysScript.ResetToolBar("Information View", $"Showing {options[dropdown.value]}");
            }
            else
            {
                sysScript.ResetToolBar("Information View", $"Not enough data available for {options[dropdown.value]}!");
            }          
        }      
    }
    public void InfoView(int selection)
    { 
        List<int> Values = new List<int>();

        //add selected attribute to a list
        switch (selection)
        {
            //relative crowdedness
            case 0:
                if (statusScript.activePaxNumber > 200)
                {
                    infoViewActive = true;
                    foreach (Node n in sysScript.ListOfNodes)
                    {
                        if (n.station.waitingQueue.Count > 0)
                        {
                            Values.Add(n.station.passengersWaiting);
                        }
                    }
                }
                
                break;
            //population density
            case 1:
                infoViewActive = true;
                foreach (District d in sysScript.ListOfDistricts)
                {
                    Values.Add(d.Population);
                }
                break;
            //job density
            case 2:
                infoViewActive = true;
                foreach (District d in sysScript.ListOfDistricts)
                {
                    Values.Add(d.Employment);
                }
                break;
            //car usage
            case 3:
                if(statusScript.previousTotalCarPax > 0)
                {
                    infoViewActive = true;
                    foreach (District d in sysScript.ListOfDistricts)
                    {
                        Values.Add(d.carUsage);
                    }
                }              
                break;
        }

        //if there is enough available data
        if (infoViewActive)
        {
            //use merge sort algorithm to sort the values from smallest to largest
            Values = MergeSort(Values);

            //calculate quartiles to determine the colour:
            //higher quartiles = red, lower quartiles = yellow
            int lowerQ = Values[Values.Count / 4];
            int median = Values[Values.Count / 2];
            int upperQ = Values[(3 * Values.Count) / 4];
            int[] quartiles = { lowerQ, median, upperQ };

            //sets each district to their respective colours based on the attribute
            switch (selection)
            {
                case 0:
                    foreach (Node n in sysScript.ListOfNodes)
                    {
                        if (n.station.passengersWaiting != 0)
                        {
                            n.station.District.renderer.color = GetColour(n.station.passengersWaiting, quartiles);
                        }
                        else
                        {
                            //set to default colour if no passengers are waiting
                            n.station.District.renderer.color = n.station.District.defaultColour;
                        }
                    }
                    break;
                case 1:
                    foreach (District d in sysScript.ListOfDistricts)
                    {
                        d.renderer.color = GetColour(d.Population, quartiles);

                    }
                    break;
                case 2:
                    foreach (District d in sysScript.ListOfDistricts)
                    {
                        d.renderer.color = GetColour(d.Employment, quartiles);
                    }
                    break;
                case 3:
                    foreach (District d in sysScript.ListOfDistricts)
                    {
                        d.renderer.color = GetColour(d.carUsage, quartiles);
                    }
                    break;
            }
        }
    }

    //returns colour based on quartile values
    private UnityEngine.Color GetColour(int value, int[] quartiles)
    {
        UnityEngine.Color color;

        if (value < quartiles[0])
        {
            //yellow
            color = color4;
        }
        else if (value >= quartiles[0] && value < quartiles[1])
        {
            //orange-yellow
            color = color3;
        }
        else if (value >= quartiles[1] && value < quartiles[2])
        {
            //orange
            color = color2;
        }
        else
        {
            //red
            color = color1;
        }

        return color;
    }

    public void ResetHighlight()
    {
        foreach (District district in sysScript.ListOfDistricts)
        {
            district.ResetColour();
        }
    }

    //assisted by YouTube vid by CodingWithJohn: https://www.youtube.com/watch?v=bOk35XmHPKs
    private List<int> MergeSort(List<int> list)
    {
        List<int> left_list = null;
        List<int> right_list = null;


        //check if list has more than 1 element
        if (list.Count > 1)
        {
            int midpoint = list.Count / 2;
            left_list = list.GetRange(0, midpoint); //split list into two halves
            right_list = list.GetRange(midpoint, list.Count - midpoint);

            //recursively split the lists
            left_list = MergeSort(left_list);
            right_list = MergeSort(right_list);
        }
        else
        {
            //lists with one element are "already sorted" - no need to merge
            return list;
        }

        return Merge(left_list, right_list);
    }

    private List<int> Merge(List<int> leftList, List<int> rightList)
    {
        List<int> outputList = new List<int>();

        //track indexes as we compare elements from L and R array
        int indexL = 0,
            indexR = 0;

        //while we have not run out of elements to compare
        while (indexL < leftList.Count && indexR < rightList.Count)
        {
            if (leftList[indexL] <= rightList[indexR])
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
