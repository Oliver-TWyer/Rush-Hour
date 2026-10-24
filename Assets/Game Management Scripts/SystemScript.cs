using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class SystemScript : MonoBehaviour
{
    //Script references
    private InformationViewScript infoView;
    private ErrorMessageScript errorMsgScript;
    private StatusScript statusScript;
    private BankManagerScript bank;
    private MenuManagerScript menuManager;
    
    //Lists of game objects
    private List<string> ListOfStnNames = new List<string>();
    public List<District> ListOfDistricts = new List<District>();
    public List<District> ListOfResidentialDistricts = new List<District>();
    public List<District> ListOfBusinessDistricts = new List<District>();
    public List<Citizen> ListOfCitizens;
    public List<Vector3> occupiedPositions;
    //List of journeys
    public Dictionary<(District, District), Journey> journeys;

    //journey plan mode
    public GameObject journeyPlannerPanel;
    public bool journeyPlanMode;

    public bool infoViewMode;

    //new line mode
    public bool newLineMode;
    public Line linePrefab;
    public Line SelectedLine;
    private int nextAvailableLineName;
    public List<Line> ListOfLines;


    //tool bar panel
    public GameObject toolbarPanel;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI instructionsText;
    private List<string> factory = new List<string>();
    public Color dangerColour, safetyColour, standardColour;

    //menu bar panel
    public TextMeshProUGUI newLineButtonText;
    [SerializeField] Slider gameTimeSlider;

    //line manager panel
    private LineManager lineManager;
    public FlexibleColorPicker fcp;

    //list of district names
    private List<string> ListOfNames;

    //pathfinding 
    public List<Node> ListOfNodes;
    public List<Edge> ListOfEdges;
    private int nextAvailableID;
    private List<Node> visited;
    List<Node> accessibleNodes;
    Node startNode, endNode;
    Djikstra djk;

    public Line journeyLine;

    //audio sources
    [SerializeField] AudioSource completeLineSFX;
    [SerializeField] AudioSource stationClickedSFX;
    [SerializeField] AudioSource mainMusicAudio;
    [SerializeField] Slider volumeSlider;

    public Color color1, color2, color3, color4;

    float timer;
    private double journeyPlannerTimer;

    private void Start()
    {
        lineManager = GameObject.FindGameObjectWithTag("lm").GetComponent<LineManager>();
        errorMsgScript = GameObject.FindObjectOfType<ErrorMessageScript>();
        bank = FindObjectOfType<BankManagerScript>();
        statusScript = FindObjectOfType<StatusScript>();
        menuManager = FindObjectOfType<MenuManagerScript>();
        infoView = FindObjectOfType<InformationViewScript>();

        newLineMode = false;
        journeyPlanMode = false;

        nextAvailableID = 1;
        nextAvailableLineName = 1;

        ListOfNames = new List<string>();
        ListOfLines = new List<Line>();
        visited = new List<Node>();
        accessibleNodes = new List<Node>();
        ListOfEdges = new List<Edge>();      
        occupiedPositions = new List<Vector3>();
        journeys = new Dictionary<(District, District), Journey>();

        timer = 0;

        //reads file 
        ConvertNamesToList();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuManager.EscKeyPressed(); //opens menu
        }

        //if the user right clicks in new line mode, the last piece of track is destroyed and refunded
        if (Input.GetMouseButtonDown(1) && newLineMode)
        { SelectedLine.RefundTrackFromLastDestination(); } 

        //timer for journey planner line (flashes every second)
        //(see design section 4.2)
        if (journeyPlanMode && journeyLine != null)
        {
            if(journeyPlannerTimer < 1)
            {
                journeyPlannerTimer += Time.deltaTime;
            }
            else 
            { ChangeJourneyLineColour();
              journeyPlannerTimer = 0; }
        }
     
    }

    public void GeneratePopulation()
    {
        //creates 1000 citizen instances at every map expansion
        for (int i = 0; i < 1000; i++)
        {
            Citizen c = ScriptableObject.CreateInstance<Citizen>();
            c.Init();
            ListOfCitizens.Add(c);
        }
    }

    //creates a node for a station
    public Node AddStation(Station stn)
    {
        Node createdNode = ScriptableObject.CreateInstance<Node>();

        createdNode.Init(nextAvailableID, stn);
        nextAvailableID++;

        return createdNode;
    }

    //reads list of district names file and converts to a list
    public void ConvertNamesToList()
    {
        string path = "names.txt";

        using (StreamReader sr = new StreamReader(path))
        {
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                ListOfNames.Add(line);
            }
        }
    }

    //returns a randomly selected name
    public string GetName()
    {
        int index = UnityEngine.Random.Range(0, ListOfNames.Count);

        return ListOfNames[index];
    }

    public void ToggleNewLine()
    {
        if (newLineMode)
        {
            ExitNewLineMode();
        }
        else
        {
            EnterNewLineMode();
        }
    }

    public void EnterNewLineMode() //when the new line button is created
    {
        newLineButtonText.text = "Complete Line";
        newLineMode = true; //when in new line mode, each district that is clicked on is added to the line until newlinemode is false

        //instantiate the line prefab so it can be drawn on screen
        var newLine = Instantiate(linePrefab, new Vector2(transform.position.x, transform.position.y), Quaternion.identity); 
        SelectedLine = newLine; //the current selected line is the object we just created

        SelectedLine.Name = $"Line {nextAvailableLineName}"; //default line name
        nextAvailableLineName++;

        Color randomColour = UnityEngine.Random.ColorHSV(); //generate a random colour for the line

        //change colour of line renderer
        SelectedLine.lr.startColor = randomColour;
        SelectedLine.lr.endColor = randomColour;

        //UI
        ResetToolBar("New Line Mode", "Click on the districts you would like to add to this line");
    }

    //called when mouse cursor over district
    public void NewLineMouseHover(District district) 
    {
        if (district.stationBuilt && !SelectedLine.StationsServed.Contains(district.station))
        {
            //if the user has already clicked on the start station
            if (!SelectedLine.StationsServed.Contains(district.station) && SelectedLine.StationsServed.Count > 0)
            {
                UpdateToolBar($"Add station: {district.Name}<br>Cost:" +
                    $" ${bank.CalculatePriceOfTrack(SelectedLine.StationsServed.Last().District.transform.position, district.transform.position)}", safetyColour);
            }
            else { UpdateToolBar($"Add station: {district.Name}", safetyColour); } //when showing start station preview
        }
        else if (SelectedLine.StationsServed.Contains(district.station))
        {
            UpdateToolBar("Line already stops at this station!", dangerColour);
        }
        else
        {
            UpdateToolBar("Station not built yet!", dangerColour);
        }
    }

    public void UpdateLineColour()
    {
        //fcp is the colour selector
        SelectedLine.lr.startColor = fcp.color;
        SelectedLine.lr.endColor = fcp.color;

        //update colour of stations and trains that run on the line
        foreach(Station s in SelectedLine.StationsServed)
        {
            s.UpdateColour();
        }
        foreach (Train t in SelectedLine.GetTrains())
        {
            t.SetTrainLineColour();
        }
    }
    public void ExitNewLineMode()
    {
        newLineButtonText.text = "Create New Line";
        HideToolBar();

        completeLineSFX.Play(); // play a sound effect

        if (SelectedLine.StationsServed.Count != 0)
        {
            lineManager.OpenLineManager();

            //create edges between stations on the line and add to graph
            SelectedLine.CreateEdges();
        }
        else
        {
            //if the user didn't add any stations
            nextAvailableLineName--;
            Destroy(SelectedLine);
        }

        newLineMode = false;

        //update djikstra distance table for all nodes
        foreach (Node n in ListOfNodes)
        {
            n.station.UpdateDjikstra();
        }

        //update rating after a new line is built
        UpdateRatings();

        lineManager.HideOldLinePath();
    }

    public void HideToolBar()
    {
        toolbarPanel.SetActive(false);
    }

    public void ChangeMainMusicVolume()
    {
        mainMusicAudio.volume = volumeSlider.value; //update main music volume to slider value
    }

    //tool bar methods

    //add mode with instructions
    public void ResetToolBar(string mode, string instructions)
    {
        //set default text
        factory.Clear();

        factory.Add(mode);
        factory.Add(instructions);

        toolbarPanel.SetActive(true);

        modeText.text = mode;
        instructionsText.text = instructions;
        instructionsText.color = standardColour;
    }

    //reset toolbar to default text
    public void ResetToolBar()
    {
        instructionsText.color = standardColour;

        modeText.text = factory[0];
        instructionsText.text = factory[1];
    }

    //update instructions on tool bar
    public void UpdateToolBar(string instructions, Color colour)
    {
        toolbarPanel.SetActive(true);

        instructionsText.color = colour;
        instructionsText.text = instructions;
    }

    public void CancelButtonPressed()
    {
        if (newLineMode)
        {
            ExitNewLineMode();
        }
        else if (journeyPlanMode)
        {
            JourneyPlanMode();
        }
        else if (infoView.infoViewActive)
        {
            infoView.CloseInfoView();
        }

        HideToolBar();
    }

    //returns a journey object between two stations
    public Journey GetJourney(District start, District end)
    {
        if (journeys.ContainsKey((start, end)))
        {
            return journeys[(start, end)];
        }
        else if (journeys.ContainsKey((end, start)))
        {
            return journeys[(end, start)];
        }
        else { return null; }
    }

    //journey planner methods
    public void JourneyPlanMode()
    {
        if (journeyPlanMode)
        {
            journeyPlanMode = false;
            HideToolBar();
            DestroyJourneyLine();
            startNode = null;
            endNode = null;
        }
        else
        {
            infoView.CloseInfoView();
            ResetToolBar("Journey Planner Mode", "Click on any station to set as the start, hover over the end station to show route");
            journeyPlanMode = true;
        }
    }

    //draws line between array of points
    public void DrawJourneyLine(Transform[] points)
    {
        journeyLine = Instantiate(linePrefab, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
        journeyLine.lr.startColor = new Color(1, 1, 1, 1);
        journeyLine.lr.endColor = new Color(1, 1, 1, 1);
        journeyLine.lr.startWidth = 0.1f;
        journeyLine.lr.endWidth = 0.1f;
        journeyLine.lr.sortingOrder = 20;

        journeyLine.SetupLine(points);
    }

    //alternates between 100% alpha value and 0% (flashing effect: see design section 4.2)
    public void ChangeJourneyLineColour()
    {
        if (journeyLine.lr.startColor.a == 1)
        {
            journeyLine.lr.startColor = new Color(1, 1, 1, 0);
            journeyLine.lr.endColor = new Color(1, 1, 1, 0);
        }
        else
        {
            journeyLine.lr.startColor = new Color(1, 1, 1, 1);
            journeyLine.lr.endColor = new Color(1, 1, 1, 1);
        }
    }

    //removes journey planner line from screen
    public void DestroyJourneyLine()
    {
        if (journeyLine != null)
        {
            journeyLine.DestroyLine();
        }
    }
    public void startStationClick(District district)
    {
        startNode = district.station.GetNode();

        //testing debug log distance table
        //startNode.station.djikstra.LogDistanceTable();
        //stationClickedSFX.Play(); //play sound effect
    }
    public void endStationHover(District district)
    {
        if (startNode != null && district != startNode.station.District && district.stationBuilt)
        {
            DestroyJourneyLine();
            Journey journey = null;
            endNode = district.station.GetNode();

            //get journey between start and end node
            journey = GetJourney(startNode.station.District, endNode.station.District);

            if (journey.pathFound)
            {
                //list of points that the journey passes through
                List<Transform> points = new List<Transform>();

                UpdateToolBar($"Showing route between {journey.start.Name} and {journey.end.Name}<br>Rating ~ {journey.rating}/10", safetyColour);

                foreach (Node n in journey.waypoints)
                {
                    points.Add(n.station.District.transform);
                }

                //draw journey on screen
                DrawJourneyLine(points.ToArray());
            }
            else if (startNode == endNode || startNode == null || endNode == null)
            {
                ResetToolBar();
            }
            else //print error message if no route between start node and end node
            {
                UpdateToolBar("No route between these stations!", dangerColour);
            }
        }
    }

    public void UpdateRatings()
    {
        //updates journey ratings and overall system rating
        foreach (Journey journey in journeys.Values) { journey.UpdateJourney(); }
        statusScript.UpdateSystemRating();
    }

    //graph traversal

    //returns list of all possible nodes that can be accessed from a given start node (in case of an unconnected graph)
    public List<Node> InitialiseSearch(Node start) 
    {
        visited.Clear();
        Search(start);

        return visited;
    }
    public void Search(Node current)
    {
        visited.Add(current);
        foreach (Edge edge in current.GetConnectedEdges())
        {
            if (!visited.Contains(edge.endNode))
            {
                Search(edge.endNode);
            }
        }
    }

    //returns a node with a given ID
    public Node GetNodeWithID(double id, List<Node> nodes)
    { 
        Node NodeToReturn = null;

        foreach (Node node in nodes)
        {
            if (node.GetID() == id)
            {
                NodeToReturn = node;
            }
        }
        return NodeToReturn;
    }

    //returns a node with a given name
    public Node GetNodeWithName(string name, List<Node> nodes)
    {
        Node NodeToReturn = null;

        foreach (Node node in nodes)
        {
            if (node.station.District.Name == name)
            {
                NodeToReturn = node;
            }
        }
        return NodeToReturn;
    }

    //determines if citizen should take train over car
    //based on journey rating (see design section 8)
    public bool MakeDecision(double rating)
    {
        int num = UnityEngine.Random.Range(0,10); //random between(0,10)

        if (num <= rating) { return true; }
        else { return false; }
    }

    public void UnlockAll()
    {
        foreach(District d in ListOfDistricts)
        {
            if (!d.stationBuilt)
            {
                d.inititaliseStation();
            }         
        }
    }

    //displays error message from error code (see design section 6.5)
    public void ErrorMessage(int errorCode)
    {
        errorMsgScript.ErrorMessage(errorCode);
    }

    //speed up or slow down game
    public void UpdateGameTime()
    {
        Time.timeScale = gameTimeSlider.value;
    }

    public bool CheckValidString(string str)
    {
        //remove some special chars
        string validCharacters = @"[^a-zA-Z0-9&/']";

        if (str.Length > 25 || Regex.IsMatch(str, validCharacters))
        {
            ErrorMessage(7);
            return false;
        }
        else { return true; }
    }
}