using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AddPopulationDialogScript : DialogPanelScript
{
    public InputField PopulationInputField;

    public int Population = 0;

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
}
