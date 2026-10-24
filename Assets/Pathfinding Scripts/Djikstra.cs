using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Djikstra : ScriptableObject
{
    private const int MAXIMUM = 1000; //Infinity

    //distance table format: key:startNode (Node), value: (distance (int), previousNodeID (int))
    private Dictionary<Node, List<double>> distanceTable;
    private Dictionary<Node, bool> visited;
    public List<Node> ListOfNodes;
    private SystemScript sysScript;
    Node StartNode;
    Node CurrentNode;

    public void Init(List<Node> listOfNodes, Node startNode) //new instance of djikstra created for each node (station)
    {
        ListOfNodes = listOfNodes;
        StartNode = startNode;
        distanceTable = new Dictionary<Node, List<double>>();
        visited = new Dictionary<Node, bool>();

        sysScript = GameObject.FindObjectOfType<SystemScript>();

        FormatTables();
        DrawTable();
    }

    //sets all distances to maximum and previous node to -1
    private void FormatTables()
    {
        for (int i = 0; i < ListOfNodes.Count; i++)
        {
            if (StartNode.GetID() == ListOfNodes[i].GetID())
            {
                distanceTable.Add(ListOfNodes[i], new List<double> { 0, -1 }); //if start node, make distance 0
            }
            else
            {
                distanceTable.Add(ListOfNodes[i], new List<double> { MAXIMUM, -1 }); //other nodes make distance infinity
            }

            visited.Add(ListOfNodes[i], false); //for each node add dictionary: {key: node object, value: visited T/F}
        }
    }

    private void DrawTable()
    {
        double calculatedDistance;
        Line PreviousLine = null;

        //update distance table to reflect immediate adjacent nodes to start node
        DrawTableStartUp();

        Line CurrentLine = null;
        CurrentNode = StartNode;
        double journeyTime;

        while (visited.ContainsValue(false)) //while there are unvisited nodes
        {
            CurrentNode = FindNearestUnvisitedNode();
            
            foreach (Edge edge in CurrentNode.GetConnectedEdges()) //for each edge connected to the current node
            {
                if ((edge != null) && !visited[edge.endNode]) //if an edge exists and the end node isn't visited
                {
                    CurrentLine = edge.GetLine();
                    journeyTime = edge.GetJourneyTime();


                    //this part works out if there is an interchange and adds an appropriate amount of time to account for this
                    if (distanceTable[CurrentNode][1] != -1) //if there is a previous node found
                    {
                        //get the line that runs between the previous node and the current node
                        PreviousLine = FindEdge(CurrentNode, sysScript.GetNodeWithID(distanceTable[CurrentNode][1], ListOfNodes)).GetLine();

                        //if there is an interchange add 2 to the journey time (as it takes longer to interchange trains)
                        if (CurrentLine != PreviousLine) { journeyTime += 2; }
                    }

                    //calculate what the distance would be passing through this node
                    calculatedDistance = distanceTable[CurrentNode][0] + journeyTime;

                    //if the calculated distance is less than what the difference is currently...
                    if (calculatedDistance < distanceTable[edge.endNode][0])
                    {
                        //update distance table
                        distanceTable[edge.endNode][0] = calculatedDistance; //set new distance
                        distanceTable[edge.endNode][1] = CurrentNode.GetID(); //set previous node
                    }
                }
            }
            visited[CurrentNode] = true;
        }

        //LogDistanceTable();
    }

    private void DrawTableStartUp() //update table only for children of the start node
    {
        foreach (Edge edge in StartNode.GetConnectedEdges())
        {
            distanceTable[edge.endNode][0] = edge.GetJourneyTime(); //set distance
            distanceTable[edge.endNode][1] = StartNode.GetID(); //set previous node to the start node
        }

        visited[StartNode] = true; //set start node to visited
    }

    private Node FindNearestUnvisitedNode()
    {
        double currentShortestDistance = MAXIMUM;
        Node currentNearestUnvisitedNode = null;

        foreach (Node node in distanceTable.Keys) //for each node
        {
            if ((distanceTable[node][0] < currentShortestDistance) && !visited[node]) //finding the nearest node which is unvisited
            {
                currentShortestDistance = distanceTable[node][0];
                currentNearestUnvisitedNode = node;
            }
        }

        return currentNearestUnvisitedNode;
    }

    //returns a list of nodes, and list of lines taken to reach a given end node
    //(traces back through the distance table)
    public (List<Node>, List<Line>) ReturnWaypoints(Node endNode)
    {
        double CurrentNodeID = endNode.GetID();
        Node previousNode = null, currentNode;
        List<Node> waypoints = new List<Node>();
        List<Line> linesTaken = new List<Line>();

        while (CurrentNodeID != StartNode.GetID())
        {
            currentNode = sysScript.GetNodeWithID(CurrentNodeID, ListOfNodes);
            waypoints.Add(currentNode);

            if (previousNode != null)
            {
                Line currentLine = GetLineBetweenNodes(CurrentNode, previousNode);
                if (!linesTaken.Contains(currentLine))
                {
                    linesTaken.Add(currentLine);
                }
            }

            previousNode = currentNode;
            CurrentNodeID = distanceTable[currentNode][1];         
        }

        waypoints.Reverse();

        return (waypoints,linesTaken);
    }

    private Line GetLineBetweenNodes(Node s, Node e)
    {
        Line line = null;

        foreach(Edge edge in s.GetConnectedEdges())
        {
            if (edge.endNode == e)
            {
                line = edge.GetLine();
            }
        }

        return line;
    }

    public double ReturnDistance(Node endNode)
    {
        return distanceTable[endNode][0];
    }

    public Edge FindEdge(Node a, Node b)
    {
        Edge edgeToReturn = null;

        foreach(Edge edge in a.GetConnectedEdges())
        {
            if (edge.endNode == b)
            {
                edgeToReturn = edge;
            }
        }

        return edgeToReturn;
    }

    public Dictionary<Node, List<double>> GetDistanceTable()
    {
        return distanceTable;
    }

    /*public void LogDistanceTable()
    {
        UnityEngine.Debug.Log($"Distance table from starting node: {StartNode.station.District.Name}");
        foreach (var entry in distanceTable)
        {
            Node node = entry.Key;
            string nodeName = node.station.District.Name;

            //UnityEngine.Debug.Log(nodeName);
            string previousNodeName = sysScript.GetNodeWithID(entry.Value[0], ListOfNodes);.station.District.Name;
            string distance = entry.Value[1].ToString();

            UnityEngine.Debug.Log($"Node:{nodeName}, Previous:, Distance:{distance}");
        }
    }

    public void LogDistanceTable()
    {
        UnityEngine.Debug.Log($"Distance table from starting node: {StartNode.station.District.Name}");
        foreach (KeyValuePair<Node, List<double>> kvp in distanceTable)
        {
            List<double> values = kvp.Value;

            string nodeName = kvp.Key.station.District.Name;
            string previousNodeName = sysScript.GetNodeWithID(values[0], ListOfNodes).station.District.name;
            string distance = values[1].ToString();

            UnityEngine.Debug.Log($"Node:{nodeName}, Previous:{previousNodeName}, Distance:{distance}");
        }
    }*/

    public void LogDistanceTable()
    {
        foreach (KeyValuePair<Node, List<double>> pair in distanceTable)
        {
            Node previousNode = sysScript.GetNodeWithID(distanceTable[pair.Key][1], ListOfNodes);
            UnityEngine.Debug.Log($"{pair.Key.GetID()}: {pair.Value[0]}, " +
                $"{sysScript.GetNodeWithID(pair.Value[1],ListOfNodes).station.District.Name}");
        }
    }

}
