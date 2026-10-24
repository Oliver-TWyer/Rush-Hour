using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

//Residential district class
public class District : MonoBehaviour
{
    //References defined in Unity Editor
    [SerializeField] public Color defaultColour;
    [SerializeField] protected Color baseColour, offsetColour;
    [SerializeField] public SpriteRenderer renderer;
    [SerializeField] protected GameObject highlight;
    [SerializeField] protected Station stationPrefab;
    [SerializeField] protected GameObject DistrictNameTextPrefab;

    protected TextMeshPro nameText;

    //Script references
    protected GridManager gm;
    protected ViewInfoScript info;
    protected SystemScript sysScript;
    protected BankManagerScript bank;

    public bool stationBuilt;
    public string Name;
    public int Population;
    public int Employment;

    public float x, y;
    
    public bool residentialDistrict;
    public bool businessDistrict;

    public Station station;
    public int carUsage;

    void Start()
    {
        Name = sysScript.GetName();
        carUsage = 0;
        stationBuilt = false;

        nameText = DistrictNameTextPrefab.GetComponent<TextMeshPro>();
        nameText.text = Name;
    }
    public virtual void Init(bool isOffset)
    {
        x = transform.position.x;
        y = transform.position.y;

        gm = FindObjectOfType<GridManager>();
        info = GameObject.FindObjectOfType<ViewInfoScript>();
        sysScript = GameObject.FindObjectOfType<SystemScript>();
        bank = FindObjectOfType<BankManagerScript>();
        renderer.color = isOffset ? offsetColour : baseColour;
        defaultColour = renderer.color;

        sysScript.ListOfDistricts.Add(this);
        sysScript.ListOfResidentialDistricts.Add(this);
        sysScript.occupiedPositions.Add(this.transform.position);
    }

    public void UpdateName(string newName)
    {
        Name = newName;
        nameText.text = Name;
    }

    public void ResetColour()
    {
        renderer.color = defaultColour;
    }

    public void CreateJourneys() //(see design section 8.1)
    {
        Journey journeyObject;
        foreach(District district in sysScript.ListOfDistricts)
        {
            if (district != this && sysScript.GetJourney(this, district) ==  null)
            {
                journeyObject = ScriptableObject.CreateInstance<Journey>();
                journeyObject.Init(this, district);
                sysScript.journeys.Add((journeyObject.start, journeyObject.end), journeyObject);
                sysScript.journeys.Add((journeyObject.end, journeyObject.start), journeyObject);
            }           
        }
    }
    protected void OnMouseEnter()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            highlight.SetActive(true); //highlight if mouse hover
        }

        if (sysScript.newLineMode)
        {
            sysScript.NewLineMouseHover(this);
        }
        else if (sysScript.journeyPlanMode)
        {
            sysScript.endStationHover(this);
        }
    }

    protected void OnMouseExit()
    {
        highlight.SetActive(false);

        if (sysScript.newLineMode || sysScript.journeyPlanMode)
        {
            sysScript.ResetToolBar();
        }
    }       

    protected void OnMouseDown()
    { 
        if (sysScript.newLineMode) //(add this district if in new line mode)
        {
            List<Station> selectedLineStationList = sysScript.SelectedLine.StationsServed;

            //only add a line here after the station has been created and if the line doesn't already stop here.
            if (this.stationBuilt && !selectedLineStationList.Contains(station))
            {
                //check if the player has sufficient funds to build the track to here
                if (selectedLineStationList.Count == 0 || bank.Purchase(bank.CalculatePriceOfTrack(selectedLineStationList.Last().District.transform.position, transform.position))){ 
                    sysScript.SelectedLine.AddWayPoint(this.transform); //draw line to this station
                    sysScript.SelectedLine.SetDestinations(station); //set this station as the line's current destination

                    selectedLineStationList.Add(station); //add this district's station to the line
                    station.AddLine(sysScript.SelectedLine); //add the line to this district's station.
                    station.SetStationIcon(); //update the icon
                }
            }
        }
        else if (sysScript.journeyPlanMode)
        {
            //if in journey plan mode set this district as the start
            sysScript.startStationClick(this);
        }
        else if (!EventSystem.current.IsPointerOverGameObject())
        {
            //display info panel if clicked on
            info.Display(this);
        }
    }

    //'build' a station at this district
    public void inititaliseStation()
    {
        station = Instantiate(stationPrefab, this.transform);
        station.Init(this);

        stationBuilt = true;
    }

}
