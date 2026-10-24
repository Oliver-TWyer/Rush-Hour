using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private LineControllerScript line;

    // Start is called before the first frame update
    void Start()
    {
        line.SetupLine(points);
    }

}
