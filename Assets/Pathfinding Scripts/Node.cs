using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : ScriptableObject
{
    private int nodeID;

    private List<Edge> ConnectedEdges;
    public Station station;

    private SystemScript sysScript;

    //nodes are created at the same time a station is built
    public void Init(int NodeID, Station Station)
    {
        sysScript = GameObject.FindObjectOfType<SystemScript>();

        nodeID = NodeID;

        ConnectedEdges = new List<Edge>();    
        station = Station;

        sysScript.ListOfNodes.Add(this);
    }

    //edges represent train tracks, and have a line attribute
    public void AddEdge(Node endNode, double journeyTime, Line line)
    {
        Edge edge = ScriptableObject.CreateInstance<Edge>();
        edge.Init(this, endNode, journeyTime, line);
        ConnectedEdges.Add(edge); //add a new edge to this node

        //add a new edge to the endNode (as this graph is undirected we can use the same distance)
        endNode.AddReturnEdge(this, journeyTime, line); 

        sysScript.ListOfEdges.Add(edge);
    }
    public void AddReturnEdge(Node endNode, double journeyTime, Line line)
    {
        Edge edge = ScriptableObject.CreateInstance<Edge>();
        edge.Init(this, endNode, journeyTime, line);
        ConnectedEdges.Add(edge);

        sysScript.ListOfEdges.Add(edge);
    }

    public void RemoveEdge(Edge edge)
    {
        ConnectedEdges.Remove(edge);
    }

    public int GetID()
    {
        return nodeID;
    }

    public List<Edge> GetConnectedEdges()
    {
        return ConnectedEdges;
    }

    public Node GetNode()
    {
        return this;
    }

    public string GetName()
    {
        return station.District.Name;
    }
}
