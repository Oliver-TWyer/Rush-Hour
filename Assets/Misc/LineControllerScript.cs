using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineControllerScript : MonoBehaviour
{

    //used tutorial by @BLANKdev

    private LineRenderer lr;
    private Transform[] points;
    public List<Transform> wayPoints;

    void Awake()
    {
        lr = GetComponent<LineRenderer>(); //reference to the lr
    }

    public void AddWayPoint(Transform transform)
    {
        wayPoints.Add(transform);
        SetupLine(wayPoints.ToArray());

    }
    public void SetupLine(Transform[] points) //called when line is created
    {
        lr.positionCount = points.Length;
        this.points = points;

        reDraw();
    }

    void reDraw() //called when line is changed or when the line is created
    {
        for (int i = 0; i < points.Length; i++)
        {
            lr.SetPosition(i, points[i].position);
        }
    }

    public void DestroyLine()
    {
        Destroy(this);
    }


}
