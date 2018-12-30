using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class AddPopulationDialogScript : ModalPanelScript
{
    public InputField PopulationInputField;

    public int Population = 0;

    public UnityEvent OperationCanceled;
    
    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    public void PopulationValueChange()
    {
        int value = 0;

        int.TryParse(PopulationInputField.text, out value);

        SetPopulationValue(value);
    }

    public void SetPopulationValue(int value)
    {
        value = Mathf.Clamp(value, World.MinStartingPopulation, World.MaxStartingPopulation);

        Population = value;

        PopulationInputField.text = value.ToString();
    }

    public void CancelOperation()
    {
        SetVisible(false);

        OperationCanceled.Invoke();
    }

    public void InitializeAndShow()
    {
        int defaultPopulationValue = (int)Mathf.Ceil(World.StartPopulationDensity * TerrainCell.MaxArea);

        defaultPopulationValue = Mathf.Clamp(defaultPopulationValue, World.MinStartingPopulation, World.MaxStartingPopulation);

        SetPopulationValue(defaultPopulationValue);

        SetVisible(true);
    }

    public void ReadKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelOperation();
        }
    }
}
