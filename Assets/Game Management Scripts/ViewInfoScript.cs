using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Text.RegularExpressions;

public class ViewInfoScript : MonoBehaviour
{
    public GameObject panel;
    public GameObject stationManagerPanel;

    public TextMeshProUGUI stationManagerButtonText;
    public TextMeshProUGUI districtTypeText;
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI employmentText;

    public TMP_InputField nameInputField;

    public TextMeshProUGUI stationNameText;
    public TextMeshProUGUI currentPaxText;
    public TMP_Dropdown linesDropdown;
    private List<string> lineNames;

    public District selectedDistrict;

    private BankManagerScript bank;
    private SystemScript sysScript;
    private LineManager lm;

    private void Start()
    {
        sysScript = FindObjectOfType<SystemScript>();
        bank = FindObjectOfType<BankManagerScript>();
        sysScript = FindObjectOfType<SystemScript>();
        lm = FindObjectOfType<LineManager>();
    }

    //displays information for selected district
    public void Display(District district)
    {
        selectedDistrict = district;

        panel.SetActive(true);
        populationText.text = selectedDistrict.Population.ToString();
        employmentText.text = selectedDistrict.Employment.ToString();
        lineNames = new List<string>();

        nameInputField.text = selectedDistrict.Name;


        if (selectedDistrict.stationBuilt)
        {
            stationManagerButtonText.text = "Manage Station";
        }
        else
        {
            stationManagerButtonText.text = "Build Station";
        }

        if(district.GetType() == typeof(BusinessDistrict))
        {
            districtTypeText.text = "Business District";
        }
        else { districtTypeText.text = "Residential District"; }
    }

    public void ChangeDistrictName()
    {
        string textInput = nameInputField.text;

        if (sysScript.CheckValidString(textInput))
        {
            selectedDistrict.UpdateName(textInput);
        }
        else
        {
            //revert changes if invalid name
            nameInputField.text = selectedDistrict.Name;
        }
    }

    public void CloseDisplay()
    {
        panel.SetActive(false);
    }

    public void OpenStationInfo()
    {
        panel.SetActive(false);

        if (selectedDistrict.stationBuilt)
        {
            stationManagerPanel.SetActive(true);
            InitStationManager();
        }
    }

    public void InitStationManager() //present info about the district's station
    {
        Station stn = selectedDistrict.station;

        stationNameText.text = $"{selectedDistrict.Name} Station";
        currentPaxText.text = $"Waiting Passengers: {stn.passengersWaiting.ToString()}";

        //dropdown box of lines at this station
        lineNames = new List<string>();

        foreach (Line line in stn.GetLines())
        {
            lineNames.Add(line.Name);
        }

        if (lineNames.Count > 0) { lineNames.Insert(0, "Lines serving this station..."); }
        else { lineNames.Insert(0, "No lines currently serve this station..."); }

        linesDropdown.ClearOptions();
        linesDropdown.AddOptions(lineNames);
        
    }

    public void CloseStationInfo()
    {
        stationManagerPanel.SetActive(false);
    }

    //build a station or manage current station
    public void Station()
    {
        if(selectedDistrict.stationBuilt)
        {
            stationManagerPanel.SetActive(true);
            InitStationManager();
        }
        else //if station not built yet
        {
            if (bank.Purchase(bank.GetStationCost()))
            {
                selectedDistrict.inititaliseStation();
            }
        }

        StationButtonUnHover();
        CloseDisplay();
    }

    public void StationButtonHover()
    {
        if (!selectedDistrict.stationBuilt)
        {
            sysScript.ResetToolBar("Construction cost: ", "");
            sysScript.UpdateToolBar($"${bank.GetStationCost()}", sysScript.safetyColour);
        }
    }
    public void StationButtonUnHover()
    {
        sysScript.HideToolBar();
    }

    //open line manager if selected from dropdown
    public void OpenLineFromLinesDropdown()
    {
        Line line = null;

        foreach(Line l in sysScript.ListOfLines)
        {
            if (l.Name == lineNames[linesDropdown.value])
            {
                line = l;
            }
        }

        if(line != null)
        {
            sysScript.SelectedLine = line;
            stationManagerPanel.SetActive(false);
            lm.OpenLineManager();
        }
    }
}
