using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AddPopulationDialogScript : DialogPanelScript {

	public InputField PopulationInputField;

	public int Population = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void PopulationValueChange () {
		
		int value = 0;
		
		int.TryParse (PopulationInputField.text, out value);

		SetPopulationValue (value);
	}

	public void SetPopulationValue (int value) {
		
		value = Mathf.Clamp (value, World.MinStartingPopulation, World.MaxStartingPopulation);
		
		Population = value;
		
		PopulationInputField.text = value.ToString ();
	}
}
