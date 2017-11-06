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

	private float _minTemperatureOffset = -50;
	private float _maxTemperatureOffset = 50;
	
	private float _minRainfallOffset = -990f;
	private float _maxRainfallOffset = 990f;
	
	private float _minSeaLevelOffset = -10000;
	private float _maxSeaLevelOffset = 10000;

	void Awake () {

		if (TemperatureSlider != null) {
			TemperatureSlider.minValue = _minTemperatureOffset;
			TemperatureSlider.maxValue = _maxTemperatureOffset;
		}

		if (RainfallSlider != null) {
			RainfallSlider.minValue = _minRainfallOffset;
			RainfallSlider.maxValue = _maxRainfallOffset;
		}

		if (SeaLevelSlider != null) {
			SeaLevelSlider.minValue = _minSeaLevelOffset;
			SeaLevelSlider.maxValue = _maxSeaLevelOffset;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}
	
	public void SetSeedString (string seedStr) {
		
		SeedInputField.text = seedStr;
	}

	private void SetTemperatureFieldValue (float offset) {

		float avgTemp = World.AvgPossibleTemperature + offset;
		TemperatureInputField.text = avgTemp.ToString ("0.00");
	}

	private bool GetTemperatureFieldValue (out float offset) {

		float value;

		if (!float.TryParse (TemperatureInputField.text, out value)) {

			offset = 0;

			return false;
		}

		offset = value - World.AvgPossibleTemperature;

		return true;
	}
	
	public void SetTemperatureOffset (float offset) {

		TemperatureOffset = offset;
		TemperatureSlider.value = offset;

		SetTemperatureFieldValue (offset);
	}

	private void SetRainfallFieldValue (float offset) {

		float avgRainfall = World.AvgPossibleRainfall + offset;
		RainfallInputField.text = avgRainfall.ToString ();
	}

	private bool GetRainfallFieldValue (out float offset) {

		float value;

		if (!float.TryParse (RainfallInputField.text, out value)) {

			offset = 0;

			return false;
		}

		offset = value - World.AvgPossibleRainfall;

		return true;
	}
	
	public void SetRainfallOffset (float offset) {
		
		RainfallOffset = offset;
		RainfallSlider.value = offset;

		SetRainfallFieldValue (offset);
	}
	
	public void SetSeaLevelOffset (float offset) {
		
		SeaLevelOffset = offset;
		SeaLevelInputField.text = offset.ToString ();
		SeaLevelSlider.value = offset;
	}
	
	public string GetSeedString () {
		
		return SeedInputField.text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}

	public void SeedValueChange () {

		int value = 0;
	
		int.TryParse (SeedInputField.text, out value);

		SeedInputField.text = value.ToString ();
	}

	public void TemperatureValueChange (bool fromField) {

		float value = 0;

		float minOffset = _minTemperatureOffset;
		float maxOffset = _maxTemperatureOffset;

		if (fromField) {

			if (!GetTemperatureFieldValue (out value)) {
				return;
			}
		} else {

			value = TemperatureSlider.value;
		}
		
		value = Mathf.Clamp(value, minOffset, maxOffset);

		TemperatureSlider.value = value;
		TemperatureOffset = value;

		SetTemperatureFieldValue (value);
	}
	
	public void RainfallValueChange (bool fromField) {
		
		float value = 0;
		
		float minOffset = _minRainfallOffset;
		float maxOffset = _maxRainfallOffset;
		
		if (fromField) {
			
			if (!GetRainfallFieldValue (out value)) {
				return;
			}
		} else {
			
			value = RainfallSlider.value;
		}
		
		value = Mathf.Clamp(value, minOffset, maxOffset);

		RainfallSlider.value = value;
		RainfallOffset = value;

		SetRainfallFieldValue (value);
	}
	
	public void SeaLevelValueChange (bool fromField) {
		
		float value = 0;
		
		float minOffset = _minSeaLevelOffset;
		float maxOffset = _maxSeaLevelOffset;
		
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
