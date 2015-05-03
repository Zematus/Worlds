using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GuiManagerScript : MonoBehaviour {

	public Text MapViewButtonText;

	public RawImage MapImage;

	// Use this for initialization
	void Start () {
		
		UpdateMapViewButtonText();
	}
	
	// Update is called once per frame
	void Update () {
	
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
}
