using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class AdjacencyMatrix : ScriptableObject
{
    public int length;
    private List<List<Edge>> matrix;

    public void Init(List<Node> ListOfNodes)
    {
        length = ListOfNodes.Count(); //length of AM is the number of nodes
        matrix = new List<List<Edge>>(); //nested list of list of edges

        for (int i = 0; i < length; i++) //for each node in the list...
        {
            Node currentNode = ListOfNodes[i];
            List<Edge> EdgeList = new List<Edge>(); //create a new list of edges

            for (int j = 0; j < length; j++) //format the list
            {
                EdgeList.Add(null); //add empty edge...
            }

            foreach (Edge edge in currentNode.GetConnectedEdges()) //now get the edges of the current node
            {
                EdgeList[edge.endNode.GetID() - 1] = edge; //add the edge object to the edgelist
            }

            matrix.Add(EdgeList); //add the edgelist to the matrix         
        }
    }

    public void PrintMatrix()
    {
        string str = "";
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                if (matrix[i][j] != null)
                {
                    str += matrix[i][j].GetJourneyTime() + ",";
                }
                else
                {
                    str += "0,";
                }
            }
            Debug.Log(str);
            str = "";
        }
    }
    public List<List<Edge>> GetMatrix()
    {
        return matrix;
    }
}
