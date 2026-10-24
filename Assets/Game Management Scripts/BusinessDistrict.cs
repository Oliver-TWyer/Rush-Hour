using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BusinessDistrict : District
{
    [SerializeField] Color businessColour;

    //overridden initialisation method
    public override void Init(bool isOffset)
    {
        x = transform.position.x;
        y = transform.position.y;

        gm = FindObjectOfType<GridManager>();
        info = GameObject.FindObjectOfType<ViewInfoScript>();
        sysScript = GameObject.FindObjectOfType<SystemScript>();
        bank = FindObjectOfType<BankManagerScript>();
        renderer = GetComponent<SpriteRenderer>();
        renderer.color = isOffset ? offsetColour : baseColour; //if offset, set colour
        defaultColour = renderer.color;

        Name = sysScript.GetName();
        carUsage = 0;
        stationBuilt = false;

        sysScript.ListOfBusinessDistricts.Add(this);
        sysScript.ListOfDistricts.Add(this);
        sysScript.occupiedPositions.Add(this.transform.position);

        nameText = DistrictNameTextPrefab.GetComponent<TextMeshPro>();
        nameText.text = Name;
        nameText.color = businessColour; //business district name label will appear green
    }
}
