using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Small1970Train : Train
{
    public override void Init(Line line)
    {
        Line = line;
        Model = "Small 1970s Train";
        Cost = 100;
        Speed = 1;
        MaxCapacity = 100;

        ConfigureTrain();
    }
}

