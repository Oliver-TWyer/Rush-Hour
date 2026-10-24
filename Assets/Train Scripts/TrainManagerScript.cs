using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TrainManagerScript : MonoBehaviour
{
    //add trains panel
    public GameObject trainManagerPanel;
    public TMP_Dropdown trainOptionsDropdown;
    public TextMeshProUGUI selectedTrainLabel;
    public Slider trainQuantitySlider;
    public TextMeshProUGUI sliderValueText;
    [SerializeField] TextMeshProUGUI costText;

    //remove trains panel
    public GameObject removeTrainsPanel;
    public Slider removeQuantitySlider;
    public TextMeshProUGUI removeValueText;

    //train model references
    public Small1970Train s1970;
    public Large1970Train l1970;
    public SmallModernTrain smodern;
    public LargeModernTrain lmodern;

    public List<Train> listOfTrainModels;

    private Line SelectedLine;
    private Train selectedTrain;

    //script references
    private SystemScript sysScript;
    private LineManager lm;
    private StatusScript statusScript;
    private BankManagerScript bank;
     
    private void Start()
    {
        sysScript = FindObjectOfType<SystemScript>();
        lm = FindObjectOfType<LineManager>();
        statusScript = FindObjectOfType<StatusScript>();
        bank = FindObjectOfType<BankManagerScript>();

        listOfTrainModels = new List<Train>();

        listOfTrainModels.Add(s1970);
        listOfTrainModels.Add(l1970);
        listOfTrainModels.Add(smodern);
        listOfTrainModels.Add(lmodern);

        SetupDropdown();
    }

    //dropdown box displaying available train models
    public void SetupDropdown()
    {
        trainOptionsDropdown.ClearOptions();

        List<string> models = new List<string>();

        models.Add("Please select a train model");
        foreach (Train t in listOfTrainModels)
        {
            models.Add($"------------<br>{t.Model}<br>${bank.ReturnInflatedCost(t.Cost)}<br>{t.MaxCapacity}pax, speed: {t.Speed}");
        }
        models.Add("");
        trainOptionsDropdown.AddOptions(models);
    }

    public void GetTrainFromDropdown()
    {
        selectedTrain = listOfTrainModels[trainOptionsDropdown.value - 1];
        selectedTrainLabel.text = selectedTrain.Model;

        //display current cost
        UpdateSliderValue();
    }

    //purchase trains and add to line
    public void AddTrainToSelectedLine()
    {
        SelectedLine = sysScript.SelectedLine;

        if(trainOptionsDropdown.value != 0) //if the user has selected a train model
        {
            if (SelectedLine.trainModel == selectedTrain || SelectedLine.trainModel == null)
            {
                if (bank.Purchase(bank.ReturnInflatedCost(CalculateCost()))) //if purchase successful then load the trains onto the line
                {
                    double spawnInterval;

                    spawnInterval = (SelectedLine.TotalJourneyTime * 2 / trainQuantitySlider.value);
                    SelectedLine.SetupTrains(selectedTrain, trainQuantitySlider.value, spawnInterval);

                    //add to total trains for maintenance costs
                    bank.noOfTrains += (int)trainQuantitySlider.value;
                    bank.UpdateMaintenanceCost();

                    UpdateSliders();

                    trainManagerPanel.SetActive(false);
                    removeTrainsPanel.SetActive(false);                  
                }
            }
            //error message if the selected train model doesn't match the exisiting one on the line
            else { sysScript.ErrorMessage(0); } 
            
        }
        //error message if the user didn't select a train model
        else { sysScript.ErrorMessage(2); }
    }

    public void UpdateSliderValue()
    {
        if (selectedTrain != null) //if they selected a train
        {
            sliderValueText.text = trainQuantitySlider.value.ToString();
            costText.text = $"Total cost: ${bank.ReturnInflatedCost(CalculateCost()).ToString()}";
        }
        else { sysScript.ErrorMessage(2); trainQuantitySlider.value = 0; } //error message if the user didn't select a train model

    }

    public double CalculateCost()
    {
        return trainQuantitySlider.value * selectedTrain.Cost;
    }
    public void UpdateRemoveSliderValue()
    {
        removeValueText.text = removeQuantitySlider.value.ToString();
    }
    public void OpenTrainManager()
    {
        trainManagerPanel.SetActive(true);
        removeTrainsPanel.SetActive(true);
        SelectedLine = sysScript.SelectedLine;

        SetupDropdown();
        UpdateSliders();
    }

    public void CloseTrainManager()
    {
        trainManagerPanel.SetActive(false);
        removeTrainsPanel.SetActive(false);
    }
    public void CloseRemoveTrainPanel()
    {
        removeTrainsPanel.SetActive(false);
    }
    public void RemoveTrains()
    {
        if (!statusScript.waveActivated) //if there is not a rush hour
        {
            SelectedLine.totalTrains -= removeQuantitySlider.value;

            for (int i = 0; i < removeQuantitySlider.value; i++)
            {
                int lastIndex = SelectedLine.TrainsOperating.Count - 1;
                Train currentTrain = SelectedLine.TrainsOperating[lastIndex];

                SelectedLine.TrainsOperating.Remove(currentTrain);
                currentTrain.DeleteTrain();

                //remove number of total trains for maintenance costs
                bank.noOfTrains--;
                bank.UpdateMaintenanceCost();
            }

            lm.UpdateTrainCount();
            CloseTrainManager();

            bank.Refund(SelectedLine.trainModel.Cost * removeQuantitySlider.value);

            if (SelectedLine.TrainsOperating.Count == 0)
            {
                SelectedLine.trainModel = null;
            }
        }
        //produce error message as trains cannot be removed during a rush hour
        else { sysScript.ErrorMessage(1); } 
        
    }

    //maximum of 12 trains can be added per line
    private void UpdateSliders()
    {
        removeQuantitySlider.maxValue = (float)SelectedLine.totalTrains;
        removeQuantitySlider.value = 0;

        trainQuantitySlider.maxValue = 12 - (float)SelectedLine.totalTrains;
        trainQuantitySlider.value = 0;
    }
}
