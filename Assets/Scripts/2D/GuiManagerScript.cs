using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public delegate void PostPreparationOperation ();

public class GuiManagerScript : MonoBehaviour {

	public Text MapViewButtonText;

	public Text InfoPanelText;

	public RawImage MapImage;

	public Button LoadButton;

	public PlanetScript PlanetScript;
	public MapScript MapScript;
	
	public SaveFileDialogPanelScript SaveFileDialogPanelScript;
	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public OverlayDialogPanelScript OverlayDialogPanelScript;
	public DialogPanelScript ViewsDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	public ActivityDialogPanelScript ActivityDialogPanelScript;

	public PaletteScript BiomePaletteScript;
	public PaletteScript MapPaletteScript;

	private bool _viewRainfall = false;
	private bool _viewTemperature = false;
	private PlanetView _planetView = PlanetView.Biomes;

	private bool _updateTexture = false;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	private string _worldName;

	private bool _preparingWorld = false;

	private PostPreparationOperation _postPreparationOp = null;

	// Use this for initialization
	void Start () {
		
		SetEnabledModalSaveDialog (false);
		SetEnabledModalLoadDialog (false);
		SetEnabledModalOverlayDialog (false);
		SetEnabledModalViewsDialog (false);
		SetEnabledModalMainMenuDialog (false);
		SetEnabledModalProgressDialog (false);
		SetEnabledModalActivityDialog (false);
		
		GenerateWorld ();

		UpdateMapViewButtonText ();

		//SetInfoPanelData (0, 0);

		LoadButton.interactable = HasFilesToLoad ();

		Manager.SetBiomePalette (BiomePaletteScript.Colors);
		Manager.SetMapPalette (MapPaletteScript.Colors);

		_updateTexture = true;
	}
	
	// Update is called once per frame
	void Update () {

		Manager.ExecuteTasks (100);

		if (!Manager.WorldReady) {
			return;
		}

		if (_preparingWorld) {

			if (_postPreparationOp != null) 
				_postPreparationOp ();

			SetEnabledModalProgressDialog (false);
			SetEnabledModalActivityDialog (false);
			_preparingWorld = false;
		}
	
		if (_updateTexture) {
			_updateTexture = false;

			Manager.SetRainfallVisible (_viewRainfall);
			Manager.SetTemperatureVisible (_viewTemperature);
			Manager.SetPlanetView (_planetView);

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

	public void ProgressUpdate (float value, string message = null) {
	
		Manager.EnqueueTask (() => {

			if (message != null) ProgressDialogPanelScript.SetDialogText (message);

			ProgressDialogPanelScript.SetProgress (value);

			return true;
		});
	}
	
	public void GenerateWorld () {

		SetEnabledModalMainMenuDialog (false);

		SetEnabledModalProgressDialog (true);

		ProgressUpdate (0, "Generating World...");

		_preparingWorld = true;

		Manager.GenerateNewWorldAsync (ProgressUpdate);

		_postPreparationOp = () => {

			_worldName = "world_" + Manager.CurrentWorld.Seed;

			_postPreparationOp = null;
		};
		
		_updateTexture = true;
	}

	private bool HasFilesToLoad () {

		string dirPath = Manager.SavePath;
		
		string[] files = Directory.GetFiles (dirPath, "*.PLNT");

		return files.Length > 0;
	}

	private void SetEnabledModalSaveDialog (bool value) {

		SaveFileDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalLoadDialog (bool value) {

		LoadFileDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalOverlayDialog (bool value) {

		OverlayDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalViewsDialog (bool value) {

		ViewsDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalMainMenuDialog (bool value) {

		MainMenuDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalProgressDialog (bool value) {
		
		ProgressDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalActivityDialog (bool value) {
		
		ActivityDialogPanelScript.SetVisible (value);
	}

	public void SaveAction () {
		
		SetEnabledModalSaveDialog (false);
		
		SetEnabledModalActivityDialog (true);
		
		ActivityDialogPanelScript.SetDialogText ("Saving World...");
		
		_worldName = SaveFileDialogPanelScript.GetWorldName ();
		
		string path = Manager.SavePath + _worldName + ".plnt";

		Manager.SaveWorldAsync (path);
		
		_preparingWorld = true;
		
		_postPreparationOp = () => {

			LoadButton.interactable = HasFilesToLoad ();
			
			_postPreparationOp = null;
		};
	}

	public void CancelSaveAction () {
		
		SetEnabledModalSaveDialog (false);
	}

	public void SaveWorldAs () {

		SetEnabledModalMainMenuDialog (false);

		SaveFileDialogPanelScript.SetWorldName (_worldName);
		
		SetEnabledModalSaveDialog (true);
	}
	
	public void LoadAction () {
		
		SetEnabledModalLoadDialog (false);
		
		SetEnabledModalProgressDialog (true);
		
		ProgressUpdate (0, "Loading World...");
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		_worldName = Path.GetFileNameWithoutExtension (path);
		
		_preparingWorld = true;
		
		_updateTexture = true;
	}
	
	public void CancelLoadAction () {
		
		SetEnabledModalLoadDialog (false);
	}
	
	public void LoadWorld () {

		SetEnabledModalMainMenuDialog (false);
		
		SetEnabledModalLoadDialog (true);

		LoadFileDialogPanelScript.SetLoadAction (LoadAction);
	}
	
	public void CloseOverlayMenuAction () {

		UpdateRainfallView (OverlayDialogPanelScript.RainfallToggle.isOn);
		UpdateTemperatureView (OverlayDialogPanelScript.TemperatureToggle.isOn);
		
		SetEnabledModalOverlayDialog (false);
	}
	
	public void SelectOverlays () {
		
		SetEnabledModalOverlayDialog (true);
	}
	
	public void SelectViews () {
		
		SetEnabledModalViewsDialog (true);
	}
	
	public void OpenMainMenu () {
		
		SetEnabledModalMainMenuDialog (true);
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
	
	public void SetBiomeView () {
		
		_updateTexture |= _planetView != PlanetView.Biomes;
		
		_planetView = PlanetView.Biomes;
		
		SetEnabledModalViewsDialog (false);
	}
	
	public void SetElevationView () {
		
		_updateTexture |= _planetView != PlanetView.Elevation;
		
		_planetView = PlanetView.Elevation;
		
		SetEnabledModalViewsDialog (false);
	}
	
	public void SetCoastlineView () {
		
		_updateTexture |= _planetView != PlanetView.Coastlines;
		
		_planetView = PlanetView.Coastlines;
		
		SetEnabledModalViewsDialog (false);
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
