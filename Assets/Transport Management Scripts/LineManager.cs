using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.UI;

public class LineManager : MonoBehaviour
{
    //UI Panel references
    public TMP_InputField lineNameInputText;
    public GameObject lineManagerPanel;
    public GameObject allLinesManagerPanel;
    public GameObject confirmDeletionPanel;
    public TMP_Dropdown linesDropdown;
    public TextMeshProUGUI lineRunsBetweenText;
    public TextMeshProUGUI trainCountText;
    public TextMeshProUGUI totalPassengersText;
    public FlexibleColorPicker fcp;
    public TMP_Dropdown stationsDropdown;
    private List<string> stationNames;

    //line renderer
    [SerializeField] LineRenderer lr; 
    private Transform[] points;

    private SystemScript sysScript;
    private ViewInfoScript infoScr;

    private void Start()
    {
        sysScript = GameObject.FindObjectOfType<SystemScript>();
        infoScr = GameObject.FindObjectOfType<ViewInfoScript>();

        stationNames = new List<string>();
    }

    public void ChangeLineName()
    {
        sysScript.SelectedLine.Name = lineNameInputText.text;
    }

    //**open line manager from dropdown**
    public void OpenLMFromALM()
    {
        foreach (Line l in sysScript.ListOfLines)
        {
            if (l.Name == sysScript.ListOfLines[linesDropdown.value - 1].Name)
            {
                sysScript.SelectedLine = l;
            }
        }

        allLinesManagerPanel.SetActive(false);
        OpenLineManager();
    }
    public void OpenLineManager()
    {
        //change colour of colour selector to selected line
        fcp.SetColor(sysScript.SelectedLine.lr.startColor);

        lineNameInputText.text = sysScript.SelectedLine.Name;
        lineManagerPanel.SetActive(true);
        Display();
    }
    public void Display()
    {
        Line SelectedLine = sysScript.SelectedLine;
        stationsDropdown.ClearOptions();
        stationNames.Clear();
        stationNames.Add("Stations on this line");

        foreach (Station stn in SelectedLine.GetStations())
        {
            stationNames.Add(stn.District.Name);
        }

        stationsDropdown.AddOptions(stationNames);
        lineRunsBetweenText.text = $"runs between<br>{SelectedLine.destA.District.Name} and {SelectedLine.destB.District.Name}";

        //line usage data
        if (SelectedLine.totalPassengersCarried > 0)
        {
            totalPassengersText.text = $"By total passengers: {SelectedLine.totalPassengersCarried}" +
            $"<br>By Pax/Sq: {SelectedLine.paxPerSquare}";
        }
        else
        {
            totalPassengersText.text = "No data available yet.";
        }

        UpdateTrainCount();
    }
    public void UpdateTrainCount()
    {
        //display how many trains spawned on the line
        trainCountText.text = $"{sysScript.SelectedLine.TrainsOperating.Count}/{sysScript.SelectedLine.totalTrains} trains are running";
    }
    public void CloseLineManager()
    {
        lineManagerPanel.SetActive(false);
        allLinesManagerPanel.SetActive(false);
    }

    //dropdown box to select lines
    public void OpenAllLinesManager()
    {
        linesDropdown.ClearOptions();
        allLinesManagerPanel.SetActive(true);

        List<string> ListOfLineNames = new List<string>();
        ListOfLineNames.Add("Select line");

        foreach(Line line in sysScript.ListOfLines)
        {
            ListOfLineNames.Add(line.Name);
        }

        linesDropdown.AddOptions(ListOfLineNames);
    }

    public void CloseAllLinesManager()
    {
        allLinesManagerPanel.SetActive(false);
    }
    public void DeleteLinePressed()
    {
        lineManagerPanel.SetActive(false);
        confirmDeletionPanel.SetActive(true);
    }
    public void Return() //if user doesn't decide to delete line
    {
        confirmDeletionPanel.SetActive(false);
        lineManagerPanel.SetActive(true);
    }

    public void RedrawLinePressed()
    {
        sysScript.SelectedLine.RedrawLine();
    }

    //when redrawing a line the old line path is visible (see screenshot below)
    public void DrawOldLinePath(Transform[] points)
    {
        this.points = points;
        Draw();

        //make old line path translucent
        Color translucentColour = sysScript.SelectedLine.lr.startColor;
        translucentColour.a = 0.5f;
        lr.startColor = translucentColour;
        lr.endColor = translucentColour;
    }

    public void HideOldLinePath() { points = new Transform[0]; Draw();  }

    private void Draw()
    {
        lr.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            lr.SetPosition(i, points[i].position);
        }
    }
    public void DeleteLine()
    {
        confirmDeletionPanel.SetActive(false);
        sysScript.SelectedLine.DeleteLine();
    }

    //open station info view when selected from dropdown
    public void OpenStationFromDropdown()
    {
        District district = null;

        foreach(District d in sysScript.ListOfDistricts)
        {
            if(d.Name == stationNames[stationsDropdown.value])
            {
                district = d;
            }
        }

        if (district != null)
        {
            infoScr.selectedDistrict = district;
            infoScr.OpenStationInfo();
            lineManagerPanel.SetActive(false);
        }
    }
}
