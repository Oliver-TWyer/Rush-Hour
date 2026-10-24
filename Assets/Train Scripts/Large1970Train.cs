using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Large1970Train : Train
{
    public override void Init(Line line)
    {
        Line = line;
        Model = "Large 1970s Train";
        Cost = 300;
        Speed = 2;
        MaxCapacity = 300;

        ConfigureTrain();
    }
}
