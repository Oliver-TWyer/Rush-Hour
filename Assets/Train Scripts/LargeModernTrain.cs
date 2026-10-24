using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeModernTrain : Train
{
    public override void Init(Line line)
    {
        Line = line;
        Model = "Large Modern Train";
        Cost = 2000;
        Speed = 4;
        MaxCapacity = 600;

        ConfigureTrain();
    }
}
