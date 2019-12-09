using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class AddPopulationDialogScript : ModalPanelScript
{
    public InputField PopulationInputField;

    public int Population = 0;

    public UnityEvent OperationCanceled;

    public Text SpeedButtonText;

    public int StartSpeedLevelIndex;

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

    /// <summary>
    /// Initializes the population placement dialog and shows it on screen.
    /// </summary>
    /// <param name="startingPopulation">The default initial population to display on the dialog</param>
    /// <param name="startingSpeedIndex">The default initial speed to display on the dialog</param>
    public void InitializeAndShow(int startingPopulation, int startingSpeedIndex)
    {
        SetPopulationValue(startingPopulation);

        StartSpeedLevelIndex = startingSpeedIndex;

        SpeedButtonText.text = Speed.Levels[startingSpeedIndex];

        SetVisible(true);
    }

    /// <summary>Handles keyboard shortcuts for this dialog.</summary>
    private void ReadKeyboardInput()
    {
        Manager.HandleKeyUp(KeyCode.Escape, false, false, CancelOperation);
    }

    /// <summary>
    /// Increase the speed level displayed on the starting speed button
    /// </summary>
    public void SelectNextSpeedLevel()
    {
        StartSpeedLevelIndex++;

        if (StartSpeedLevelIndex == Speed.Levels.Length)
        {
            StartSpeedLevelIndex = -1;

            SpeedButtonText.text = Speed.Zero;
        }
        else
        {
            SpeedButtonText.text = Speed.Levels[StartSpeedLevelIndex];
        }
    }
}
