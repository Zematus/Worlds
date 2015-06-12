using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class GuiManagerScript : MonoBehaviour {

	public CanvasGroup NonModalCanvasGroup;

	public Text MapViewButtonText;
	public Text RainfallViewButtonText;
	public Text TemperatureViewButtonText;
	public Text BiomeViewButtonText;

	public Text InfoPanelText;

	public RawImage MapImage;

	public PlanetScript PlanetScript;
	public MapScript MapScript;
	
	public SaveFileDialogPanelScript SaveFileDialogPanelScript;
	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public OverlayDialogPanelScript OverlayDialogPanelScript;

	public BiomePaletteScript BiomePaletteScript;

	private bool _viewRainfall = false;
	private bool _viewTemperature = false;
	private bool _viewBiomes = false;

	private bool _updateTexture = false;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	// Use this for initialization
	void Start () {

		UpdateMapViewButtonText ();
		UpdateBiomeViewButtonText ();

		SetInfoPanelData (0, 0);

		SetEnabledModalSaveDialog (false);
		SetEnabledModalLoadDialog (false);

		Manager.SetBiomePalette (BiomePaletteScript.Colors);
	}
	
	// Update is called once per frame
	void Update () {
	
		if (_updateTexture) {
			_updateTexture = false;

			Manager.SetRainfallVisible (_viewRainfall);
			Manager.SetTemperatureVisible (_viewTemperature);
			Manager.SetBiomesVisible (_viewBiomes);

			Manager.RefreshTextures ();

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
		SaveFileDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalLoadDialog (bool value) {
		
		NonModalCanvasGroup.blocksRaycasts = !value;
		LoadFileDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalOverlayDialog (bool value) {
		
		NonModalCanvasGroup.blocksRaycasts = !value;
		OverlayDialogPanelScript.SetVisible (value);
	}

	public void SaveAction () {
		
		string worldName = SaveFileDialogPanelScript.GetWorldName ();
		
		string path = @"Saves\" + worldName + ".xml";
		Manager.SaveWorld (path);
		
		SetEnabledModalSaveDialog (false);
	}

	public void CancelSaveAction () {
		
		SetEnabledModalSaveDialog (false);
	}

	public void SaveWorldAs () {

		string worldName = "test_world";

		SaveFileDialogPanelScript.SetWorldName (worldName);
		
		SetEnabledModalSaveDialog (true);
	}
	
	public void LoadAction () {
		
		string path = LoadFileDialogPanelScript.GetFilenameToLoad();
		Manager.LoadWorld (path);
		
		SetEnabledModalLoadDialog (false);
		
		_updateTexture = true;
	}
	
	public void CancelLoadAction () {
		
		SetEnabledModalLoadDialog (false);
	}
	
	public void LoadWorld () {
		
		SetEnabledModalLoadDialog (true);
	}
	
	public void CloseOverlayMenuAction () {

		UpdateRainfallView (OverlayDialogPanelScript.RainfallToggle.isOn);
		UpdateTemperatureView (OverlayDialogPanelScript.TemperatureToggle.isOn);
		
		SetEnabledModalOverlayDialog (false);
	}
	
	public void SelectOverlays () {
		
		SetEnabledModalOverlayDialog (true);
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
	
	public void UpdateRainfallView (bool value) {

		_updateTexture |= _viewRainfall ^ value;

		_viewRainfall = value;
	}
	
	public void UpdateTemperatureView (bool value) {
		
		_updateTexture |= _viewTemperature ^ value;
		
		_viewTemperature = value;
	}
	
	public void UpdateBiomeView () {
		
		_updateTexture = true;
		
		_viewBiomes = !_viewBiomes;
		
		UpdateBiomeViewButtonText ();
	}
	
	public void UpdateBiomeViewButtonText () {
		
		if (!_viewBiomes) {
			BiomeViewButtonText.text = "View Biomes";
		} else {
			BiomeViewButtonText.text = "Hide Biomes";
		}
	}
	
	public void SetInfoPanelData (int longitude, int latitude) {
		
		TerrainCell cell = Manager.CurrentWorld.Terrain[longitude][latitude];
		
		InfoPanelText.text = string.Format("Position: [{0},{1}]", longitude, latitude);
		InfoPanelText.text += "\nAltitude: " + cell.Altitude;
		InfoPanelText.text += "\nRainfall: " + cell.Rainfall;
		InfoPanelText.text += "\nTemperature: " + cell.Temperature;
		InfoPanelText.text += "\n";

		for (int i = 0; i < cell.Biomes.Count; i++)
		{
			int percentage = (int)(cell.BiomePresences[i] * 100);
			
			InfoPanelText.text += "\nBiome: " + cell.Biomes[i].Name;
			InfoPanelText.text += " (" + percentage + "%)";
		}
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
