using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GuiManagerScript : MonoBehaviour {

	public Text MapViewButtonText;
	public Text RainfallViewButtonText;

	public RawImage MapImage;

	public PlanetScript PlanetScript;
	public MapScript MapScript;

	private bool _viewRainfall = false;
	private bool _updateTexture = false;

	// Use this for initialization
	void Start () {
		
		UpdateMapViewButtonText();
		UpdateRainfallViewButtonText();
	}
	
	// Update is called once per frame
	void Update () {
	
		if (_updateTexture)
		{
			_updateTexture = false;

			Manager.SetRainfallVisible(_viewRainfall);
			Manager.RefreshTexture();

			PlanetScript.UpdateTexture();
			MapScript.UpdateTexture();
		}
	}

	public void UpdateMapView () {
	
		MapImage.enabled = !MapImage.enabled;

		UpdateMapViewButtonText();
	}

	public void UpdateMapViewButtonText () {
		
		if (MapImage.enabled)
		{
			MapViewButtonText.text = "View World";
		}
		else
		{
			MapViewButtonText.text = "View Map";
		}
	}
	
	public void UpdateRainfallView () {

		_updateTexture = true;

		_viewRainfall = !_viewRainfall;
		
		UpdateRainfallViewButtonText();
	}
	
	public void UpdateRainfallViewButtonText () {
		
		if (!_viewRainfall)
		{
			RainfallViewButtonText.text = "View Rainfall";
		}
		else
		{
			RainfallViewButtonText.text = "Hide Rainfall";
		}
	}
}
