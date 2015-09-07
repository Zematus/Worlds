using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class WorldCustomizationDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DialogText;
	public InputField SeedInputField;

	public InputField TemperatureInputField;
	public Slider TemperatureSlider;
	public float TemperatureOffset = 0;
	
	public InputField RainfallInputField;
	public Slider RainfallSlider;
	public float RainfallOffset = 0;
	
	public InputField SeaLevelInputField;
	public Slider SeaLevelSlider;
	public float SeaLevelOffset = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}
	
	public void SetSeedStr (string seedStr) {
		
		SeedInputField.text = seedStr;
	}
	
	public string GetSeedString () {
		
		return SeedInputField.text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}

	public void TemperatureValueChange (bool fromField) {

		float value = 0;

		float minOffset = -30;
		float maxOffset = 30;

		if (fromField) {

			if (!float.TryParse (TemperatureInputField.text, out value)) {
				return;
			}
		} else {

			value = TemperatureSlider.value;
		}
		
		value = Mathf.Clamp(value, minOffset, maxOffset);

		TemperatureInputField.text = value.ToString();
		TemperatureSlider.value = value;
		TemperatureOffset = value;
	}
	
	public void RainfallValueChange (bool fromField) {
		
		float value = 0;
		
		float minOffset = -5000;
		float maxOffset = 5000;
		
		if (fromField) {
			
			if (!float.TryParse (RainfallInputField.text, out value)) {
				return;
			}
		} else {
			
			value = RainfallSlider.value;
		}
		
		value = Mathf.Clamp(value, minOffset, maxOffset);
		
		RainfallInputField.text = value.ToString();
		RainfallSlider.value = value;
		RainfallOffset = value;
	}
	
	public void SeaLevelValueChange (bool fromField) {
		
		float value = 0;
		
		float minOffset = -5000;
		float maxOffset = 5000;
		
		if (fromField) {
			
			if (!float.TryParse (SeaLevelInputField.text, out value)) {
				return;
			}
		} else {
			
			value = SeaLevelSlider.value;
		}
		
		value = Mathf.Clamp(value, minOffset, maxOffset);
		
		SeaLevelInputField.text = value.ToString();
		SeaLevelSlider.value = value;
		SeaLevelOffset = value;
	}
}
