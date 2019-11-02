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

    /// <summary>Handler for changes to the population input field.</summary>
    public void PopulationValueChange()
    {
        int.TryParse(PopulationInputField.text, out int value);

        SetPopulationValue(value);
    }

    /// <summary>Sets the initial value to display int the input field.</summary>
    /// <param name="value">The population value to set on the field</param>
    public void SetPopulationValue(int value)
    {
        value = Mathf.Clamp(value, World.MinStartingPopulation, World.MaxStartingPopulation);

        Population = value;

        PopulationInputField.text = value.ToString();
    }

    /// <summary>Handler for cancelling the add population operation.</summary>
    public void CancelOperation()
    {
        SetVisible(false);

        OperationCanceled.Invoke();
    }

    /// <summary>Initializes the dialog and shows it on screen.</summary>
    public void InitializeAndShow()
    {
        int defaultPopulationValue = (int)Mathf.Ceil(World.StartPopulationDensity * TerrainCell.MaxArea);

        defaultPopulationValue = Mathf.Clamp(defaultPopulationValue, World.MinStartingPopulation, World.MaxStartingPopulation);

        SetPopulationValue(defaultPopulationValue);

        SetVisible(true);
    }

    /// <summary>Handles keyboard shortcuts for this dialog.</summary>
    private void ReadKeyboardInput()
    {
        Manager.HandleKeyUp(KeyCode.Escape, false, false, CancelOperation);
    }
}
