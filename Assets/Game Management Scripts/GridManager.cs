using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    //Starting grid width and height defined in Unity Editor
    [SerializeField] int width, height; 

    //Prefab and object references defined in Unity Editor
    [SerializeField] District districtPrefab;
    [SerializeField] BusinessDistrict businessDistrictPrefab;
    [SerializeField] Transform camera;

    //Script references
    private SystemScript sysScript;
    private LoadingScreenScript loadingScreenScript;
    private StatusScript statusScript;
    private BankManagerScript bank;

    [SerializeField] AudioSource buttonSFX; //button sound effect

    private void Start()
    {
        //assign script references
        sysScript = FindObjectOfType<SystemScript>();
        loadingScreenScript = FindObjectOfType<LoadingScreenScript>();
        statusScript = FindObjectOfType<StatusScript>();
        bank = FindObjectOfType<BankManagerScript>();

        GenerateGrid();
    }
    void GenerateGrid() //only run at start of game
    {
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                InstantiateDistrict(x, y);               
            }
        }

        sysScript.GeneratePopulation();

        camera.transform.position = new Vector3((float)width/2 -0.5f, (float)height/2 -0.5f, -10); //set camera position

        foreach(District d in sysScript.ListOfDistricts) //create journeys between each district (see design section 8)
        {
            d.CreateJourneys();
        }
    }

    private District InstantiateDistrict(float x,float y) //instatiates district at given position
    {
        District spawnedDistrict;

        if(UnityEngine.Random.Range(1,5) == 4) // 1/4 chance that the district is a business district (see design section 3)
        {
            spawnedDistrict = Instantiate(businessDistrictPrefab, new Vector2(x, y), Quaternion.identity);
        }
        else
        {
            spawnedDistrict = Instantiate(districtPrefab, new Vector2(x, y), Quaternion.identity);
        }

        //NOT MY CODE - determines if district is offset
        bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
        spawnedDistrict.Init(isOffset); //initialises district

        return spawnedDistrict;
    }

    //loading screen when expanding map
    public void LoadExpandMap() 
    {
        buttonSFX.Play();
        StartCoroutine(ExpandMap());
    }

    public IEnumerator ExpandMap()
    {
        //ensures loading screen appears before executing following lines of code
        loadingScreenScript.OpenLoadingScreen("Map Expansion");
        yield return null;

        //checks where there is available space to expand (see design section 12.1)
        float tryX = 0, tryY = 0;
        bool spaceFound;
        List<District> ListOfDistricts = sysScript.ListOfDistricts;
        List<Vector2> availablePositions = new List<Vector2>();
        List<District> newlyCreatedDistricts = new List<District>();

        for (int i = 0; i < ListOfDistricts.Count; i++)
        {
            District currentDistrict = ListOfDistricts[i];
            spaceFound = false;
            tryX = currentDistrict.x;
            tryY = currentDistrict.y;

            //check to the left and right side of current district
            if (CheckAvailability(tryX - 1, tryY))
            {
                tryX--;
                spaceFound = true;
            }
            else if (CheckAvailability(tryX + 1, tryY))
            {
                tryX++;
                spaceFound = true;
            }

            //add the found vacant positions to list
            if (spaceFound && !availablePositions.Contains(new Vector2(tryX,tryY))) { availablePositions.Add(new Vector2(tryX, tryY)); }

            spaceFound = false;
            tryX = currentDistrict.x;
            tryY = currentDistrict.y;

            //check above and below the current district
            if (CheckAvailability(tryX, tryY + 1))
            {
                tryY++;
                spaceFound = true;
            }
            else if (CheckAvailability(tryX, tryY - 1))
            {
                tryY--;
                spaceFound = true;
            }

            //add the found vacant positions to list
            if (spaceFound && !availablePositions.Contains(new Vector2(tryX, tryY))) { availablePositions.Add(new Vector2(tryX, tryY)); }
        }

        foreach(Vector2 pos in availablePositions) //instantiate a district at the found vacant positions
        {
            District newDistrict = InstantiateDistrict(pos.x, pos.y);
            newlyCreatedDistricts.Add(newDistrict);
        }
        sysScript.GeneratePopulation(); //generate population so that citizens 'move in' to the new districts

        foreach (District d in newlyCreatedDistricts) //create journeys between each district (see design section 8)
        {
            d.CreateJourneys();
        }

        statusScript.UpdateSystemRating(); //update system rating (see design section 8.2)
        statusScript.UpdateStats(); //update population value on screen
        loadingScreenScript.CloseLoadingScreen();

        bank.IncreaseCostsByProportion(); //increase global costs as map has expanded (inflation)
    }

    private bool CheckAvailability(float x, float y)
    {
        return !sysScript.occupiedPositions.Contains(new Vector3(x, y, 0)); //returns true if space isn't occupied
    }
}

