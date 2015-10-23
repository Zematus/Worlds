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
	public TextInputDialogPanelScript MessageDialogPanelScript;
	public WorldCustomizationDialogPanelScript SetSeedDialogPanelScript;
	public WorldCustomizationDialogPanelScript CustomizeWorldDialogPanelScript;

	public PaletteScript BiomePaletteScript;
	public PaletteScript MapPaletteScript;
	
	private PlanetView _planetView = PlanetView.Biomes;
	private PlanetOverlay _planetOverlay = PlanetOverlay.Population;
	private string _planetOverlaySubtype = "None";

	private bool menusNeedUpdate = true;

	private bool _regenTextures = false;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	private bool _preparingWorld = false;
	
	private string _progressMessage = null;
	private float _progressValue = 0;

	private PostPreparationOperation _postPreparationOp = null;
	
	private const float _maxAccTime = 0.0f;
	private const int _iterationsPerRefresh = 5;

	private float _accDeltaTime = 0;
	private int _accIterations = 0;

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

			GenerateWorld ();
		}

		UpdateMapViewButtonText ();

		LoadButton.interactable = HasFilesToLoad ();

		Manager.SetBiomePalette (BiomePaletteScript.Colors);
		Manager.SetMapPalette (MapPaletteScript.Colors);

		_regenTextures = true;
	}
	
	// Update is called once per frame
	void Update () {

		UpdateMenus ();

		Manager.ExecuteTasks (100);
		
		if (_preparingWorld) {
			
			if (_progressMessage != null) ProgressDialogPanelScript.SetDialogText (_progressMessage);
			
			ProgressDialogPanelScript.SetProgress (_progressValue);
		}

		if (!Manager.WorldReady) {
			return;
		}

		bool updateTextures = false;

		if (_preparingWorld) {

			if (_postPreparationOp != null) 
				_postPreparationOp ();

			ProgressDialogPanelScript.SetVisible (false);
			ActivityDialogPanelScript.SetVisible (false);
			_preparingWorld = false;

		} else {

			_accDeltaTime += Time.deltaTime;

			if (_accDeltaTime > _maxAccTime) {

				int minDateSpan = 20;
				int lastUpdateDate = Manager.CurrentWorld.CurrentDate;

				while ((lastUpdateDate + minDateSpan) >= Manager.CurrentWorld.CurrentDate) {

					Manager.CurrentWorld.Iterate();

					updateTextures = true;

					_accDeltaTime -= _maxAccTime;
				}
				
				_accIterations++;
			}
		}
	
		if (_regenTextures) {
			_regenTextures = false;

			Manager.SetPlanetOverlay (_planetOverlay);
			Manager.SetPlanetView (_planetView);

			Manager.GenerateTextures ();

			PlanetScript.RefreshTexture ();
			MapScript.RefreshTexture ();

		} else if (updateTextures) {

			if (_accIterations >= _iterationsPerRefresh)
			{
				_accIterations -= _iterationsPerRefresh;

				if ((_planetOverlay == PlanetOverlay.Population) || 
				    (_planetOverlay == PlanetOverlay.CulturalSkill)) {
					Manager.UpdateTextures ();
				}
			}
		}

		if (MapImage.enabled) {
			UpdateInfoPanel();
		}
	}

	public void ProgressUpdate (float value, string message = null, bool reset = false) {
		
		if (reset || (value >= _progressValue)) {
			
			if (message != null) 
				_progressMessage = message;
			
			_progressValue = value;
		}
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
		
		SetSeedDialogPanelScript.SetSeedString (seed.ToString());

		SetSeedDialogPanelScript.SetVisible (true);

	}
	
	public void CancelGenerateAction () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);
	}
	
	public void CloseSeedErrorMessageAction () {
		
		MessageDialogPanelScript.SetVisible (false);

		SetGenerationSeed ();
	}
	
	public void GenerateWorld () {

		int seed = Random.Range (0, int.MaxValue);
		
		GenerateWorldInternal (seed);
	}
	
	public void GenerateWorldWithCustomSeed () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		int seed = 0;
		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		if (!int.TryParse (seedStr, out seed)) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		GenerateWorldInternal (seed);
	}
	
	public void GenerateWorldWithCustomParameters () {
		
		CustomizeWorldDialogPanelScript.SetVisible (false);
		
		Manager.TemperatureOffset = CustomizeWorldDialogPanelScript.TemperatureOffset;
		Manager.RainfallOffset = CustomizeWorldDialogPanelScript.RainfallOffset;
		Manager.SeaLevelOffset = CustomizeWorldDialogPanelScript.SeaLevelOffset;
		
		int seed = 0;
		string seedStr = CustomizeWorldDialogPanelScript.GetSeedString ();
		
		if (!int.TryParse (seedStr, out seed)) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		GenerateWorldInternal (seed);
	}
	
	private void GenerateWorldInternal (int seed) {
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Generating World...", true);
		
		_preparingWorld = true;
		
		Manager.GenerateNewWorldAsync (seed, ProgressUpdate);
		
		_postPreparationOp = () => {
			
			Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;
			
			_postPreparationOp = null;
		};
		
		_regenTextures = true;
	}
	
	public void CustomizeGeneration () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		CustomizeWorldDialogPanelScript.SetVisible (true);
		
		CustomizeWorldDialogPanelScript.SetSeedString (seedStr);
		
		CustomizeWorldDialogPanelScript.SetTemperatureOffset(Manager.TemperatureOffset);
		CustomizeWorldDialogPanelScript.SetRainfallOffset(Manager.RainfallOffset);
		CustomizeWorldDialogPanelScript.SetSeaLevelOffset(Manager.SeaLevelOffset);
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
		case PlanetOverlay.CulturalSkill: 
			planetOverlayStr = "_cultural_skill_" + _planetOverlaySubtype; 
			break;
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
		
		ProgressUpdate (0, "Loading World...", true);
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		Manager.WorldName = Path.GetFileNameWithoutExtension (path);
		
		_preparingWorld = true;
		
		_regenTextures = true;
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
		} else if (OverlayDialogPanelScript.CulturalSkillToggle.isOn) {
			SetCulturalSkillOverlay ();
		} else {
			UnsetOverlay();
		}
		
		OverlayDialogPanelScript.SetVisible (false);
	}
	
	public void UpdateMenus () {

		if (!menusNeedUpdate)
			return;

		menusNeedUpdate = false;

		OverlayDialogPanelScript.CulturalSkillToggle.isOn = false;
		OverlayDialogPanelScript.PopulationToggle.isOn = false;
		OverlayDialogPanelScript.RainfallToggle.isOn = false;
		OverlayDialogPanelScript.TemperatureToggle.isOn = false;

		switch (_planetOverlay) {
			
		case PlanetOverlay.CulturalSkill:
			OverlayDialogPanelScript.CulturalSkillToggle.isOn = true;
			break;
		
		case PlanetOverlay.Population:
			OverlayDialogPanelScript.PopulationToggle.isOn = true;
			break;
			
		case PlanetOverlay.Rainfall:
			OverlayDialogPanelScript.RainfallToggle.isOn = true;
			break;
			
		case PlanetOverlay.Temperature:
			OverlayDialogPanelScript.TemperatureToggle.isOn = true;
			break;
			
		case PlanetOverlay.None:
			break;
			
		default:
			throw new System.Exception ("Unhandled Planet Overlay type: " + _planetOverlay);
		}
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

		_regenTextures |= _planetOverlay != PlanetOverlay.Rainfall;

		_planetOverlay = PlanetOverlay.Rainfall;
	}
	
	public void SetTemperatureOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.Temperature;
		
		_planetOverlay = PlanetOverlay.Temperature;
	}
	
	public void SetPopulationOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.Population;
		
		_planetOverlay = PlanetOverlay.Population;
	}
	
	public void SetCulturalSkillOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.CulturalSkill;
		
		_planetOverlay = PlanetOverlay.CulturalSkill;
	}
	
	public void UnsetOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.None;
		
		_planetOverlay = PlanetOverlay.None;
	}
	
	public void SetBiomeView () {
		
		_regenTextures |= _planetView != PlanetView.Biomes;
		
		_planetView = PlanetView.Biomes;
		
		ViewsDialogPanelScript.SetVisible (false);
	}
	
	public void SetElevationView () {
		
		_regenTextures |= _planetView != PlanetView.Elevation;
		
		_planetView = PlanetView.Elevation;
		
		ViewsDialogPanelScript.SetVisible (false);
	}
	
	public void SetCoastlineView () {
		
		_regenTextures |= _planetView != PlanetView.Coastlines;
		
		_planetView = PlanetView.Coastlines;
		
		ViewsDialogPanelScript.SetVisible (false);
	}

	public void UpdateInfoPanel () {
		
		World world = Manager.CurrentWorld;
		
		InfoPanelText.text = "Year: " + world.CurrentDate;

		Vector2 point;
		
		if (GetMapCoordinatesFromCursor (out point)) {
			AddCellDataToInfoPanel (point);
		}
		
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nNumber of Migration Events: " + MigrateGroupEvent.EventCount;
		InfoPanelText.text += "\nMean Migration Travel Time: " + MigrateGroupEvent.MeanTravelTime;
	}
	
	public void AddCellDataToInfoPanel (int longitude, int latitude) {
		
		World world = Manager.CurrentWorld;
		
		if ((longitude < 0) || (longitude >= world.Width))
			return;
		
		if ((latitude < 0) || (latitude >= world.Height))
			return;

		InfoPanelText.text += "\n";
		
		TerrainCell cell = world.Terrain[longitude][latitude];

		world.SetObservedCell (cell);
		
		InfoPanelText.text += string.Format("\nPosition: Longitude {0}, Latitude {1}", longitude, latitude);
		InfoPanelText.text += "\nAltitude: " + cell.Altitude + " meters";
		InfoPanelText.text += "\nRainfall: " + cell.Rainfall + " mm / year";
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
		InfoPanelText.text += "\nAccessibility: " + (cell.Accessibility*100) + "%";

		int population = 0;
		int optimalPopulation = 0;
		int lastUpdateDate = 0;
		int nextUpdateDate = 0;

		float modifiedSurvivability = 0;
		float modifiedForagingCapacity = 0;

		List<CulturalSkill> cellCulturalSkills = new List<CulturalSkill> ();

		foreach (CellGroup group in cell.Groups) {
		
			population += group.Population;
			optimalPopulation += group.OptimalPopulation;

			float groupSurvivability = 0;
			float groupForagingCapacity = 0;

			group.CalculateAdaptionToCell (cell, out groupForagingCapacity, out groupSurvivability);

			modifiedSurvivability += groupSurvivability;
			modifiedForagingCapacity += groupForagingCapacity;

			lastUpdateDate = Mathf.Max(lastUpdateDate, group.LastUpdateDate);
			nextUpdateDate = Mathf.Max(nextUpdateDate, group.NextUpdateDate);

			foreach (CulturalSkill skill in group.Culture.GetSkills ()) {

				cellCulturalSkills.Add(skill);
			}
		}

		if (population > 0) {
			
			InfoPanelText.text += "\n";
			InfoPanelText.text += "\nPopulation: " + population;
			InfoPanelText.text += "\nOptimal Population: " + optimalPopulation;
			
			InfoPanelText.text += "\n";
			InfoPanelText.text += "\nModified Survivability: " + modifiedSurvivability.ToString("#.##%");
			InfoPanelText.text += "\nModified Foraging Capacity: " + modifiedForagingCapacity.ToString("#.##%");
			
			InfoPanelText.text += "\n";
			InfoPanelText.text += "\nLast Update Date: " + lastUpdateDate;
			InfoPanelText.text += "\nNext Update Date: " + nextUpdateDate;
			InfoPanelText.text += "\nTime between updates: " + (nextUpdateDate - lastUpdateDate);

			InfoPanelText.text += "\n";
			InfoPanelText.text += "\nCultural Skills";

			foreach (CulturalSkill skill in cellCulturalSkills) {
				
				InfoPanelText.text += "\n\t" + skill.Id + " - Value: " + skill.Value.ToString("0.000");
			}
		}
	}
	
	public void AddCellDataToInfoPanel (Vector2 mapPosition) {

		int longitude = (int)mapPosition.x;
		int latitude = (int)mapPosition.y;

		AddCellDataToInfoPanel (longitude, latitude);
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
