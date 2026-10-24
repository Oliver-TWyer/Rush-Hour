using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Edge : ScriptableObject
{
    public Node startNode;
    public Node endNode;
    private double journeyTime;
    private Line Line;

    public void Init(Node StartNode, Node EndNode, double JourneyTime, Line line)
    {
        startNode = StartNode;
        endNode = EndNode;
        journeyTime = JourneyTime;
        Line = line;
    }
    public double GetJourneyTime()
    {
        return journeyTime;
    }

    public Line GetLine()
    {
        return Line;
    }

    public void Delete()
    {
        startNode.RemoveEdge(this);
        endNode.RemoveEdge(this);

        Destroy(this);
    }
}
