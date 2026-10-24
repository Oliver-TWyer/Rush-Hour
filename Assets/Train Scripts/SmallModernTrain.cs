using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallModernTrain : Train
{
    public override void Init(Line line)
    {
        Line = line;
        Model = "Small Modern Train";
        Cost = 1000;
        Speed = 4;
        MaxCapacity = 300;

        ConfigureTrain();
    }
}