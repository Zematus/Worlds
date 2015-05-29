using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GuiManagerScript : MonoBehaviour {

	public CanvasGroup NonModalCanvasGroup;

	public Text MapViewButtonText;
	public Text RainfallViewButtonText;
	public Text TemperatureViewButtonText;

	public Text InfoPanelText;

	public RawImage MapImage;

	public PlanetScript PlanetScript;
	public MapScript MapScript;
	
	public FileDialogPanelScript FileDialogPanelScript;

	private bool _viewRainfall = false;
	private bool _viewTemperature = false;

	private bool _updateTexture = false;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	// Use this for initialization
	void Start () {

		UpdateMapViewButtonText ();
		UpdateRainfallViewButtonText ();
		UpdateTemperatureViewButtonText ();

		SetInfoPanelData (0, 0);

		FileDialogPanelScript.SetVisible (false);
	}
	
	// Update is called once per frame
	void Update () {
	
		if (_updateTexture) {
			_updateTexture = false;

			Manager.SetRainfallVisible (_viewRainfall);
			Manager.SetTemperatureVisible (_viewTemperature);

			Manager.RefreshTexture ();

			PlanetScript.UpdateTexture ();
			MapScript.RefreshTexture ();
		}

		if (MapImage.enabled) {
			Vector2 point;

			if (GetMapCoordinatesFromCursor (out point)) {
				SetInfoPanelData (point);
			}
		}
	}
	
	public void GenerateWorld () {

		Manager.GenerateNewWorld ();
		
		_updateTexture = true;

	}

	private void SetEnabledModalSaveDialog (bool value) {
		
		NonModalCanvasGroup.blocksRaycasts = !value;
		FileDialogPanelScript.SetVisible (value);
	}

	public void SaveWorldAs () {
		
		int seed = Manager.CurrentWorld.Seed;
		
		string worldName = "test_world";

		FileDialogPanelScript.SetDialogText ("Save World As...");
		FileDialogPanelScript.SetWorldName (worldName);
		
		FileDialogPanelScript.SetAction(() => {
			
			worldName = FileDialogPanelScript.GetWorldName ();
			
			string path = "Saves\\" + worldName + ".xml";
				Manager.SaveWorld (path);
			
			SetEnabledModalSaveDialog (false);
		});
		
		FileDialogPanelScript.SetCancelAction(() => {
			
			SetEnabledModalSaveDialog (false);
		});
		
		SetEnabledModalSaveDialog (true);
	}
	
	public void LoadWorld () {
		
		NonModalCanvasGroup.blocksRaycasts = false;
		
		FileDialogPanelScript.SetDialogText ("Load World...");
		FileDialogPanelScript.SetWorldName ("");
		FileDialogPanelScript.SetVisible (true);
		
		FileDialogPanelScript.SetAction(() => {
			
			string worldName = FileDialogPanelScript.GetWorldName ();
			
			string path = "Saves\\" + worldName + ".xml";
			Manager.LoadWorld (path);
			
			SetEnabledModalSaveDialog (false);
			
			_updateTexture = true;
		});
		
		SetEnabledModalSaveDialog (true);
	}

	public void UpdateMapView () {

		MapScript.SetVisible (!MapScript.IsVisible());

		UpdateMapViewButtonText ();
	}

	public void UpdateMapViewButtonText () {
		
		if (MapImage.enabled) {
			MapViewButtonText.text = "View World";
		} else {
			MapViewButtonText.text = "View Map";
		}
	}
	
	public void UpdateRainfallView () {

		_updateTexture = true;

		_viewRainfall = !_viewRainfall;
		
		UpdateRainfallViewButtonText ();
	}
	
	public void UpdateRainfallViewButtonText () {
		
		if (!_viewRainfall) {
			RainfallViewButtonText.text = "View Rainfall";
		} else {
			RainfallViewButtonText.text = "Hide Rainfall";
		}
	}
	
	public void UpdateTemperatureView () {
		
		_updateTexture = true;
		
		_viewTemperature = !_viewTemperature;
		
		UpdateTemperatureViewButtonText ();
	}
	
	public void UpdateTemperatureViewButtonText () {
		
		if (!_viewTemperature) {
			TemperatureViewButtonText.text = "View Temp";
		} else {
			TemperatureViewButtonText.text = "Hide Temp";
		}
	}
	
	public void SetInfoPanelData (int longitude, int latitude) {
		
		TerrainCell cell = Manager.CurrentWorld.Terrain[longitude][latitude];
		
		InfoPanelText.text = string.Format("Position: [{0},{1}]", longitude, latitude);
		InfoPanelText.text += "\nAltitude: " + cell.Altitude;
		InfoPanelText.text += "\nRainfall: " + cell.Rainfall;
		InfoPanelText.text += "\nTemperature: " + cell.Temperature;
	}
	
	public void SetInfoPanelData (Vector2 mapPosition) {

		int longitude = (int)mapPosition.x;
		int latitude = (int)mapPosition.y;

		SetInfoPanelData (longitude, latitude);
	}
	
	public bool GetMapCoordinatesFromCursor (out Vector2 point) {
		
		Rect mapImageRect = MapImage.rectTransform.rect;

		Vector3 mapMousePos = MapImage.rectTransform.InverseTransformPoint (Input.mousePosition);
		
		Vector2 mousePos = new Vector2 (mapMousePos.x, mapMousePos.y);

		if (mapImageRect.Contains (mousePos)) {

			Vector2 relPos = mousePos - mapImageRect.min;

			Vector2 uvPos = new Vector2 (relPos.x / mapImageRect.size.x, relPos.y / mapImageRect.size.y);

			uvPos += MapImage.uvRect.min;

			float worldLong = Mathf.Repeat (uvPos.x * Manager.CurrentWorld.Width, Manager.CurrentWorld.Width);
			float worldLat = uvPos.y * Manager.CurrentWorld.Height;

			point = new Vector2 (Mathf.Floor(worldLong), Mathf.Floor(worldLat));

			return true;
		}

		point = -Vector2.one;

		return false;
	}

	public void DebugEvent (BaseEventData data) {

		Debug.Log (data.ToString());
	}
	
	public void DragMap (BaseEventData data) {
		
		Rect mapImageRect = MapImage.rectTransform.rect;
		
		PointerEventData pointerData = data as PointerEventData;

		Vector2 delta = pointerData.position - _beginDragPosition;

		float uvDelta = delta.x / mapImageRect.width;

		Rect newUvRect = _beginDragMapUvRect;
		newUvRect.x -= uvDelta;

		MapImage.uvRect = newUvRect;
	}
	
	public void BeginDragMap (BaseEventData data) {
		
		PointerEventData pointerData = data as PointerEventData;

		_beginDragPosition = pointerData.position;
		_beginDragMapUvRect = MapImage.uvRect;
	}
	
	public void EndDragMap (BaseEventData data) {
	}
}
