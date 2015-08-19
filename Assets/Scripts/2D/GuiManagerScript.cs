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
	
	public TextInputDialogPanelScript SaveFileDialogPanelScript;
	public TextInputDialogPanelScript ExportMapDialogPanelScript;
	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public OverlayDialogPanelScript OverlayDialogPanelScript;
	public DialogPanelScript ViewsDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public DialogPanelScript OptionsDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	public ActivityDialogPanelScript ActivityDialogPanelScript;
	public TextInputDialogPanelScript SetSeedDialogPanelScript;
	public TextInputDialogPanelScript MessageDialogPanelScript;

	public PaletteScript BiomePaletteScript;
	public PaletteScript MapPaletteScript;
	
	private PlanetView _planetView = PlanetView.Biomes;
	private PlanetOverlay _planetOverlay = PlanetOverlay.None;

	private bool _updateTexture = false;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	private bool _preparingWorld = false;

	private PostPreparationOperation _postPreparationOp = null;

	// Use this for initialization
	void Start () {

		Manager.UpdateMainThreadReference ();
		
		SaveFileDialogPanelScript.SetVisible (false);
		ExportMapDialogPanelScript.SetVisible (false);
		LoadFileDialogPanelScript.SetVisible (false);
		OverlayDialogPanelScript.SetVisible (false);
		ViewsDialogPanelScript.SetVisible (false);
		MainMenuDialogPanelScript.SetVisible (false);
		ProgressDialogPanelScript.SetVisible (false);
		ActivityDialogPanelScript.SetVisible (false);
		OptionsDialogPanelScript.SetVisible (false);
		SetSeedDialogPanelScript.SetVisible (false);
		MessageDialogPanelScript.SetVisible (false);
		
		if (!Manager.WorldReady) {

			GenerateWorld (false);
		}

		UpdateMapViewButtonText ();

		LoadButton.interactable = HasFilesToLoad ();

		Manager.SetBiomePalette (BiomePaletteScript.Colors);
		Manager.SetMapPalette (MapPaletteScript.Colors);

		_updateTexture = true;
	}

	private const float maxAccTime = 1;
	private float accDeltaTime = 0;
	
	// Update is called once per frame
	void Update () {

		Manager.ExecuteTasks (100);

		if (!Manager.WorldReady) {
			return;
		}

		if (_preparingWorld) {

			if (_postPreparationOp != null) 
				_postPreparationOp ();

			ProgressDialogPanelScript.SetVisible (false);
			ActivityDialogPanelScript.SetVisible (false);
			_preparingWorld = false;

		} else {

			accDeltaTime += Time.deltaTime;

			if (accDeltaTime > maxAccTime) {

				Manager.CurrentWorld.Iterate();

				accDeltaTime -= maxAccTime;
			}
		}
	
		if (_updateTexture) {
			_updateTexture = false;

			Manager.SetPlanetOverlay (_planetOverlay);
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
	
	public void CloseMainMenu () {
		
		MainMenuDialogPanelScript.SetVisible (false);
	}
	
	public void CloseOptionsMenu () {
		
		OptionsDialogPanelScript.SetVisible (false);
	}
	
	public void Exit () {
		
		Application.Quit();
	}

	public void SetGenerationSeed () {
		
		MainMenuDialogPanelScript.SetVisible (false);

		int seed = Random.Range (0, int.MaxValue);
		
		SetSeedDialogPanelScript.SetName (seed.ToString());

		SetSeedDialogPanelScript.SetVisible (true);

	}
	
	public void CancelGenerateAction () {
		
		SetSeedDialogPanelScript.SetVisible (false);
	}
	
	public void CloseSeedErrorMessageAction () {
		
		MessageDialogPanelScript.SetVisible (false);

		SetGenerationSeed ();
	}
	
	public void GenerateWorld (bool getSeedInput = true) {
		
		SetSeedDialogPanelScript.SetVisible (false);

		int seed = Random.Range (0, int.MaxValue);

		if (getSeedInput) {
			string seedStr = SetSeedDialogPanelScript.GetName ();

			if (!int.TryParse (seedStr, out seed)) {

				MessageDialogPanelScript.SetVisible (true);
				return;
			}

			if (seed < 0) {

				MessageDialogPanelScript.SetVisible (true);
				return;
			}
		}

		ProgressDialogPanelScript.SetVisible (true);

		ProgressUpdate (0, "Generating World...");

		_preparingWorld = true;

		Manager.GenerateNewWorldAsync (seed, ProgressUpdate);

		_postPreparationOp = () => {

			Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;

			_postPreparationOp = null;
		};
		
		_updateTexture = true;
	}

	private bool HasFilesToLoad () {

		string dirPath = Manager.SavePath;
		
		string[] files = Directory.GetFiles (dirPath, "*.PLNT");

		return files.Length > 0;
	}
	
	public void ExportMapAction () {
		
		ExportMapDialogPanelScript.SetVisible (false);
		
		ActivityDialogPanelScript.SetVisible (true);
		
		ActivityDialogPanelScript.SetDialogText ("Exporting map to PNG file...");
		
		string imageName = ExportMapDialogPanelScript.GetName ();
		
		string path = Manager.ExportPath + imageName + ".png";
		
		Manager.ExportMapTextureToFileAsync (path, MapImage.uvRect);
		
		_preparingWorld = true;
	}
	
	public void CancelExportAction () {
		
		ExportMapDialogPanelScript.SetVisible (false);
	}
	
	public void ExportImageAs () {
		
		OptionsDialogPanelScript.SetVisible (false);
		
		string planetViewStr = "";
		
		switch (_planetView) {
		case PlanetView.Biomes: planetViewStr = "_biomes"; break;
		case PlanetView.Coastlines: planetViewStr = "_coastlines"; break;
		case PlanetView.Elevation: planetViewStr = "_elevation"; break;
		default: throw new System.Exception("Unexpected planet view type: " + _planetView);
		}
		
		string planetOverlayStr;
		
		switch (_planetOverlay) {
		case PlanetOverlay.None: planetOverlayStr = ""; break;
		case PlanetOverlay.Rainfall: planetOverlayStr = "_rainfall"; break;
		case PlanetOverlay.Temperature: planetOverlayStr = "_temperature"; break;
		case PlanetOverlay.Population: planetOverlayStr = "_population"; break;
		default: throw new System.Exception("Unexpected planet overlay type: " + _planetOverlay);
		}

		ExportMapDialogPanelScript.SetName (Manager.WorldName + planetViewStr + planetOverlayStr);
		
		ExportMapDialogPanelScript.SetVisible (true);
	}

	public void SaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);
		
		ActivityDialogPanelScript.SetVisible (true);
		
		ActivityDialogPanelScript.SetDialogText ("Saving World...");
		
		Manager.WorldName = SaveFileDialogPanelScript.GetName ();
		
		string path = Manager.SavePath + Manager.WorldName + ".plnt";

		Manager.SaveWorldAsync (path);
		
		_preparingWorld = true;
		
		_postPreparationOp = () => {

			LoadButton.interactable = HasFilesToLoad ();
			
			_postPreparationOp = null;
		};
	}

	public void CancelSaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);
	}

	public void SaveWorldAs () {

		MainMenuDialogPanelScript.SetVisible (false);

		SaveFileDialogPanelScript.SetName (Manager.WorldName);
		
		SaveFileDialogPanelScript.SetVisible (true);
	}
	
	public void LoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Loading World...");
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		Manager.WorldName = Path.GetFileNameWithoutExtension (path);
		
		_preparingWorld = true;
		
		_updateTexture = true;
	}
	
	public void CancelLoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
	}
	
	public void LoadWorld () {

		MainMenuDialogPanelScript.SetVisible (false);
		
		LoadFileDialogPanelScript.SetVisible (true);

		LoadFileDialogPanelScript.SetLoadAction (LoadAction);
	}
	
	public void CloseOverlayMenuAction () {

		if (OverlayDialogPanelScript.RainfallToggle.isOn) {
			SetRainfallOverlay ();
		} else if (OverlayDialogPanelScript.TemperatureToggle.isOn) {
			SetTemperatureOverlay ();
		} else if (OverlayDialogPanelScript.PopulationToggle.isOn) {
			SetPopulationOverlay ();
		} else {
			UnsetOverlay();
		}
		
		OverlayDialogPanelScript.SetVisible (false);
	}
	
	public void SelectOverlays () {
		
		OverlayDialogPanelScript.SetVisible (true);
	}
	
	public void SelectViews () {
		
		ViewsDialogPanelScript.SetVisible (true);
	}
	
	public void OpenMainMenu () {
		
		MainMenuDialogPanelScript.SetVisible (true);
	}

	public void OpenOptionsMenu () {
		
		OptionsDialogPanelScript.SetVisible (true);
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
	
	public void SetRainfallOverlay () {

		_updateTexture |= _planetOverlay != PlanetOverlay.Rainfall;

		_planetOverlay = PlanetOverlay.Rainfall;
	}
	
	public void SetTemperatureOverlay () {
		
		_updateTexture |= _planetOverlay != PlanetOverlay.Temperature;
		
		_planetOverlay = PlanetOverlay.Temperature;
	}
	
	public void SetPopulationOverlay () {
		
		_updateTexture |= _planetOverlay != PlanetOverlay.Population;
		
		_planetOverlay = PlanetOverlay.Population;
	}
	
	public void UnsetOverlay () {
		
		_updateTexture |= _planetOverlay != PlanetOverlay.None;
		
		_planetOverlay = PlanetOverlay.None;
	}
	
	public void SetBiomeView () {
		
		_updateTexture |= _planetView != PlanetView.Biomes;
		
		_planetView = PlanetView.Biomes;
		
		ViewsDialogPanelScript.SetVisible (false);
	}
	
	public void SetElevationView () {
		
		_updateTexture |= _planetView != PlanetView.Elevation;
		
		_planetView = PlanetView.Elevation;
		
		ViewsDialogPanelScript.SetVisible (false);
	}
	
	public void SetCoastlineView () {
		
		_updateTexture |= _planetView != PlanetView.Coastlines;
		
		_planetView = PlanetView.Coastlines;
		
		ViewsDialogPanelScript.SetVisible (false);
	}
	
	public void SetInfoPanelData (int longitude, int latitude) {

		if ((longitude < 0) || (longitude >= Manager.CurrentWorld.Width))
			return;
		
		if ((latitude < 0) || (latitude >= Manager.CurrentWorld.Height))
			return;

		World world = Manager.CurrentWorld;

		InfoPanelText.text = "Year: " + world.CurrentDate;
		InfoPanelText.text += "\n";
		
		TerrainCell cell = world.Terrain[longitude][latitude];
		
		InfoPanelText.text += string.Format("\nPosition: Longitude {0}, Latitude {1}", longitude, latitude);
		InfoPanelText.text += "\nAltitude: " + cell.Altitude + " meters";
		InfoPanelText.text += "\nRainfall: " + cell.Rainfall + " mm/month";
		InfoPanelText.text += "\nTemperature: " + cell.Temperature + " C";
		InfoPanelText.text += "\n";

		for (int i = 0; i < cell.PresentBiomeNames.Count; i++)
		{
			int percentage = (int)(cell.BiomePresences[i] * 100);
			
			InfoPanelText.text += "\nBiome: " + cell.PresentBiomeNames[i];
			InfoPanelText.text += " (" + percentage + "%)";
		}

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nSurvivability: " + (cell.Survivability*100) + "%";
		InfoPanelText.text += "\nForaging Capacity: " + (cell.ForagingCapacity*100) + "%";
		InfoPanelText.text += "\nMax Forage Possible: " + cell.MaxForage + " Units";

		int population = 0;

		foreach (HumanGroup group in cell.HumanGroups) {
		
			population += group.Population;
		}

		if (population > 0) {
			
			InfoPanelText.text += "\n";
			InfoPanelText.text += "\nPopulation: " + population;
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
