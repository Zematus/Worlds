using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public delegate void PostProgressOperation ();

public delegate void MouseClickOperation (Vector2 position);

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
	public AddPopulationDialogScript AddPopulationDialogScript;

	public PaletteScript BiomePaletteScript;
	public PaletteScript MapPaletteScript;
	public PaletteScript OverlayPaletteScript;

	public SelectionPanelScript SelectionPanelScript;

	public QuickTipPanelScript QuickTipPanelScript;
	
	public ToggleEvent OnSimulationInterrupted;

	public ToggleEvent OnFirstMaxSpeedOptionSet;
	public ToggleEvent OnLastMaxSpeedOptionSet;
	
	public SpeedChangeEvent OnSimulationSpeedChanged;

	private bool _simulationGuiPause = false;
	private bool _simulationGuiInterruption = false;

	private bool _displayedTip_mapScroll = false;
	private bool _displayedTip_initialPopulation = false;
	
	private PlanetView _planetView = PlanetView.Biomes;

	#if DEBUG
	private PlanetOverlay _planetOverlay = PlanetOverlay.UpdateSpan;
	#else
	private PlanetOverlay _planetOverlay = PlanetOverlay.PopDensity;
	#endif

	private string _planetOverlaySubtype = "None";

	private Dictionary<PlanetOverlay, string> _planetOverlaySubtypeCache = new Dictionary<PlanetOverlay, string> ();

	private bool _displayRoutes = false;

	private bool _menusNeedUpdate = true;

	private bool _regenTextures = false;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	private bool _displayProgressDialogs = false;
	
	private string _progressMessage = null;
	private float _progressValue = 0;

	private event PostProgressOperation _postProgressOp = null;

	private event MouseClickOperation _mapLeftClickOp = null;

	private TerrainCell _previousSelectedCell = null;
	private TerrainCell _selectedCell = null;
	
	private const float _maxAccTime = 1.0f;
	private const float _maxDeltaTimeIterations = 0.02f;

	private float _accDeltaTime = 0;
	private int _simulationDateSpan = 0;

	private int _mapUpdateCount = 0;
	private int _lastMapUpdateCount = 0;
	private float _timeSinceLastMapUpdate = 0;

	private Speed[] _maxSpeedOptions = new Speed[] {
		Speed.Slowest, 
		Speed.Slow, 
		Speed.Normal, 
		Speed.Fast, 
		Speed.Fastest
	};

	private int _lastMaxSpeedOptionIndex;
	private int _selectedMaxSpeedOptionIndex;

	// Use this for initialization
	void Start () {

		Manager.LoadAppSettings (@"Worlds.settings");
		
		_lastMaxSpeedOptionIndex = _maxSpeedOptions.Length - 1;
		_selectedMaxSpeedOptionIndex = _lastMaxSpeedOptionIndex;

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
		CustomizeWorldDialogPanelScript.SetVisible (false);
		MessageDialogPanelScript.SetVisible (false);
		AddPopulationDialogScript.SetVisible (false);

		QuickTipPanelScript.SetVisible (false);

		_mapLeftClickOp += ClickOp_SelectCell;
		
		if (!Manager.WorldReady) {

			GenerateWorld (false, 407252633);
		} else if (!Manager.SimulationCanRun) {

			SetInitialPopulation ();
		} else {

			DisplayTip_MapScroll ();
		}

		UpdateMapViewButtonText ();

		LoadButton.interactable = HasFilesToLoad ();

		Manager.SetBiomePalette (BiomePaletteScript.Colors);
		Manager.SetMapPalette (MapPaletteScript.Colors);
		Manager.SetOverlayPalette (OverlayPaletteScript.Colors);

		_regenTextures = true;
	}

	void OnDestroy () {

		Manager.SaveAppSettings (@"Worlds.settings");
	}
	
	// Update is called once per frame
	void Update () {

		_timeSinceLastMapUpdate += Time.deltaTime;

		if (_timeSinceLastMapUpdate > 1) {
		
			_lastMapUpdateCount = _mapUpdateCount;
			_mapUpdateCount = 0;

			_timeSinceLastMapUpdate -= 1;
		}

		UpdateMenus ();

		Manager.ExecuteTasks (100);
		
		if (_displayProgressDialogs) {
			
			if (_progressMessage != null) ProgressDialogPanelScript.SetDialogText (_progressMessage);
			
			ProgressDialogPanelScript.SetProgress (_progressValue);
		}
		
		if (!Manager.WorldReady) {
			return;
		}
		
		if (Manager.PerformingAsyncTask) {
			return;
		}

		if (_displayProgressDialogs) {

			ProgressDialogPanelScript.SetVisible (false);
			ActivityDialogPanelScript.SetVisible (false);
			_displayProgressDialogs = false;
			
			if (_postProgressOp != null) 
				_postProgressOp ();
		}
		
		bool updateTextures = false;

		if (Manager.SimulationCanRun && Manager.SimulationRunning) {

			Speed maxSpeed = _maxSpeedOptions [_selectedMaxSpeedOptionIndex];

			_accDeltaTime += Time.deltaTime;

			if (_accDeltaTime > _maxAccTime) {

				_accDeltaTime -= _maxAccTime;
				_simulationDateSpan = 0;
			}

			if (_simulationDateSpan < maxSpeed) {

				int minDateSpan = CellGroup.GenerationTime * 1000;
				int lastUpdateDate = Manager.CurrentWorld.CurrentDate;

				float startTimeIterations = Time.realtimeSinceStartup;

				int maxDateSpanBetweenUpdates = (int)Mathf.Ceil(maxSpeed * _maxDeltaTimeIterations);

				int dateSpan = 0;

				while ((lastUpdateDate + minDateSpan) >= Manager.CurrentWorld.CurrentDate) {

					dateSpan += Manager.CurrentWorld.Iterate ();

					float deltaTimeIterations = Time.realtimeSinceStartup - startTimeIterations;

					if (dateSpan >= maxDateSpanBetweenUpdates)
						break;

					if (deltaTimeIterations > _maxDeltaTimeIterations)
						break;
				}

				_simulationDateSpan += dateSpan;

				updateTextures = true;
			}
		}
	
		if (_regenTextures) {
			_regenTextures = false;

			Manager.SetPlanetOverlay (_planetOverlay, _planetOverlaySubtype);
			Manager.SetPlanetView (_planetView);
			Manager.SetDisplayRoutes (_displayRoutes);

			Manager.GenerateTextures ();

			//PlanetScript.RefreshTexture ();
			MapScript.RefreshTexture ();

			_mapUpdateCount++;

		} else if (updateTextures) {

			if ((_planetOverlay == PlanetOverlay.PopDensity) ||
				(_planetOverlay == PlanetOverlay.FarmlandDistribution) ||
				(_planetOverlay == PlanetOverlay.PopCulturalActivity) ||
				(_planetOverlay == PlanetOverlay.PopCulturalSkill) ||
				(_planetOverlay == PlanetOverlay.PopCulturalKnowledge) ||
				(_planetOverlay == PlanetOverlay.PopCulturalDiscovery) ||
				(_planetOverlay == PlanetOverlay.PolityTerritory) ||
				(_planetOverlay == PlanetOverlay.PolityInfluence) ||
				(_planetOverlay == PlanetOverlay.PolityCulturalActivity) ||
				(_planetOverlay == PlanetOverlay.PolityCulturalKnowledge) ||
				(_planetOverlay == PlanetOverlay.PolityCulturalSkill) ||
				(_planetOverlay == PlanetOverlay.PolityCulturalDiscovery) ||
				(_planetOverlay == PlanetOverlay.PopChange) ||
				(_planetOverlay == PlanetOverlay.UpdateSpan)) {
				Manager.UpdateTextures ();

				_mapUpdateCount++;
			}
		}

		DisplaySelectedCellOverlay ();

		if (MapImage.enabled) {
			UpdateInfoPanel();
			UpdateSelectionMenu();
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
		
		InterruptSimulation (false);
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
		
		InterruptSimulation (true);

	}
	
	public void CancelGenerateAction () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);
		
		InterruptSimulation (false);
	}
	
	public void CloseSeedErrorMessageAction () {
		
		MessageDialogPanelScript.SetVisible (false);

		SetGenerationSeed ();
	}
	
	public void GenerateWorld (bool randomSeed = true, int seed = 0) {

		if (randomSeed) {
			seed = Random.Range (0, int.MaxValue);
		}
		
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

	private void PostProgressOp_GenerateWorld () {

		ResetUIElements ();
		
		Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;
		
		SelectionPanelScript.RemoveAllOptions ();
		
		SetInitialPopulation();

		_selectedMaxSpeedOptionIndex = _lastMaxSpeedOptionIndex;

		SetMaxSpeedOption (_selectedMaxSpeedOptionIndex);
		
		_postProgressOp -= PostProgressOp_GenerateWorld;
	}
	
	private void GenerateWorldInternal (int seed) {
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Generating World...", true);
		
		Manager.GenerateNewWorldAsync (seed, ProgressUpdate);

		_postProgressOp += PostProgressOp_GenerateWorld;
		
		_displayProgressDialogs = true;
		
		_regenTextures = true;
	}

	private void ResetUIElements () {
	
		_selectedCell = null;
	}

	private void SetInitialPopulation () {

		AddPopulationDialogScript.SetDialogText ("Add Initial Population Group");

		int defaultPopulationValue = (int)Mathf.Ceil (World.StartPopulationDensity * TerrainCell.MaxArea);

		defaultPopulationValue = Mathf.Clamp (defaultPopulationValue, World.MinStartingPopulation, World.MaxStartingPopulation);

		AddPopulationDialogScript.SetPopulationValue (defaultPopulationValue);
	
		AddPopulationDialogScript.SetVisible (true);
		
		InterruptSimulation (true);
	}

	public void CancelPopulationPlacement () {
		
		AddPopulationDialogScript.SetVisible (false);

		DisplayTip_MapScroll ();
	}
	
	public void RandomPopulationPlacement () {

		int population = AddPopulationDialogScript.Population;
		
		AddPopulationDialogScript.SetVisible (false);

		if (population <= 0)
			return;

		Manager.GenerateRandomHumanGroup (population);
		
		InterruptSimulation (false);
		
		DisplayTip_MapScroll ();
	}

	public void ClickOp_SelectCell (Vector2 position) {

		Vector2 mapCoordinates;

		if (!GetMapCoordinatesFromMousePosition (position, out mapCoordinates))
			return;

		int longitude = (int)mapCoordinates.x;
		int latitude = (int)mapCoordinates.y;

		TerrainCell selectedCell = Manager.CurrentWorld.GetCell (longitude, latitude);

		_previousSelectedCell = _selectedCell;

		if (_previousSelectedCell != null) {
			_previousSelectedCell.IsSelected = false;
			_previousSelectedCell.World.SelectedCell = null;
		}

		if (_previousSelectedCell == selectedCell) {
			_selectedCell = null;
		} else {
			_selectedCell = selectedCell;
			selectedCell.IsSelected = true;
			selectedCell.World.SelectedCell = selectedCell;
		}

	}

	public void ClickOp_SelectPopulationPlacement (Vector2 position) {

		int population = AddPopulationDialogScript.Population;

		Vector2 point;
		
		if (GetMapCoordinatesFromMousePosition (out point)) {
			if (AddPopulationGroupAtPosition (point, population)) {
				
				InterruptSimulation (false);
				
				DisplayTip_MapScroll();

				_mapLeftClickOp -= ClickOp_SelectPopulationPlacement;
			}
		}
	}
	
	public void SelectPopulationPlacement () {
		
		int population = AddPopulationDialogScript.Population;
		
		AddPopulationDialogScript.SetVisible (false);
		
		if (population <= 0)
			return;

		DisplayTip_InitialPopulationPlacement ();

		_mapLeftClickOp += ClickOp_SelectPopulationPlacement;
	}
	
	public bool AddPopulationGroupAtPosition (Vector2 mapPosition, int population) {

		World world = Manager.CurrentWorld;
		
		int longitude = (int)mapPosition.x;
		int latitude = (int)mapPosition.y;
		
		if ((longitude < 0) || (longitude >= world.Width))
			return false;
		
		if ((latitude < 0) || (latitude >= world.Height))
			return false;

		TerrainCell cell = world.GetCell (longitude, latitude);

		if (cell.Altitude <= Biome.Ocean.MaxAltitude)
			return false;

		Manager.GenerateHumanGroup (longitude, latitude, population);

		return true;
	}

	public void DisplayTip_InitialPopulationPlacement () {
		
		if (_displayedTip_initialPopulation) {
			
			QuickTipPanelScript.SetVisible (false);
			return;
		}

		string message = "Left click on any non-ocean position in the map to place the initial population group\n";

		if (!_displayedTip_mapScroll) {
		
			message += "Right click and drag with the mouse to scroll the map left or right\n";
		}

		message += "\n(Click anywhere on this message to close)";
	
		QuickTipPanelScript.SetText (message);
		QuickTipPanelScript.Reset (10);

		QuickTipPanelScript.SetVisible (true);
		
		_displayedTip_initialPopulation = true;
		_displayedTip_mapScroll = true;
	}
	
	public void DisplayTip_MapScroll () {

		if (_displayedTip_mapScroll) {
			
			QuickTipPanelScript.SetVisible (false);
			return;
		}
		
		QuickTipPanelScript.SetText (
			"Right click and drag with the mouse to scroll the map left or right\n" +
			"\n(Click anywhere on this message to close)");
		QuickTipPanelScript.Reset (10);
		
		QuickTipPanelScript.SetVisible (true);

		_displayedTip_mapScroll = true;
	}
	
	public void CustomizeGeneration () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		CustomizeWorldDialogPanelScript.SetVisible (true);
		
		CustomizeWorldDialogPanelScript.SetSeedString (seedStr);
		
		CustomizeWorldDialogPanelScript.SetTemperatureOffset(Manager.TemperatureOffset);
		CustomizeWorldDialogPanelScript.SetRainfallOffset(Manager.RainfallOffset);
		CustomizeWorldDialogPanelScript.SetSeaLevelOffset(Manager.SeaLevelOffset);
		
		InterruptSimulation (true);
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
		
		_displayProgressDialogs = true;
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
		case PlanetOverlay.PopDensity: 
			planetOverlayStr = "_population_density"; 
			break;
		case PlanetOverlay.FarmlandDistribution: 
			planetOverlayStr = "_farmland_distribution"; 
			break;
		case PlanetOverlay.PopCulturalActivity: 
			planetOverlayStr = "_population_cultural_activity_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PopCulturalSkill: 
			planetOverlayStr = "_population_cultural_skill_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PopCulturalKnowledge: 
			planetOverlayStr = "_population_cultural_knowledge_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PopCulturalDiscovery: 
			planetOverlayStr = "_population_cultural_discovery_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PolityTerritory: 
			planetOverlayStr = "_polity_territories"; 
			break;
		case PlanetOverlay.PolityInfluence: 
			planetOverlayStr = "_polity_influences"; 
			break;
		case PlanetOverlay.PolityCulturalActivity: 
			planetOverlayStr = "_polity_cultural_activitiy_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PolityCulturalSkill: 
			planetOverlayStr = "_polity_cultural_skill_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PolityCulturalKnowledge: 
			planetOverlayStr = "_polity_cultural_knowledge_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PolityCulturalDiscovery: 
			planetOverlayStr = "_polity_cultural_discovery_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.Temperature: 
			planetOverlayStr = "_temperature"; 
			break;
		case PlanetOverlay.Rainfall: 
			planetOverlayStr = "_rainfall"; 
			break;
		case PlanetOverlay.Arability: 
			planetOverlayStr = "_arability"; 
			break;
		case PlanetOverlay.PopChange: 
			planetOverlayStr = "_population_change"; 
			break;
		case PlanetOverlay.UpdateSpan: 
			planetOverlayStr = "_update_span"; 
			break;
		default: throw new System.Exception("Unexpected planet overlay type: " + _planetOverlay);
		}

		ExportMapDialogPanelScript.SetName (Manager.WorldName + planetViewStr + planetOverlayStr);
		
		ExportMapDialogPanelScript.SetVisible (true);
	}

	public void PostProgressOp_SaveAction () {

		LoadButton.interactable = HasFilesToLoad ();
		
		_postProgressOp -= PostProgressOp_SaveAction;
	}

	public void SaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);
		
		ActivityDialogPanelScript.SetVisible (true);
		
		ActivityDialogPanelScript.SetDialogText ("Saving World...");
		
		Manager.WorldName = SaveFileDialogPanelScript.GetName ();
		
		string path = Manager.SavePath + Manager.WorldName + ".plnt";

		Manager.SaveWorldAsync (path);

		_postProgressOp += PostProgressOp_SaveAction;
		
		_displayProgressDialogs = true;
	}

	public void CancelSaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);
		
		InterruptSimulation (false);
	}

	public void SaveWorldAs () {

		MainMenuDialogPanelScript.SetVisible (false);

		SaveFileDialogPanelScript.SetName (Manager.WorldName);
		
		SaveFileDialogPanelScript.SetVisible (true);

		InterruptSimulation (true);
	}

	public void GetMaxSpeedOptionFromCurrentWorld () {

		int maxSpeed = Manager.CurrentWorld.MaxYearsToSkip;

		for (int i = 0; i < _maxSpeedOptions.Length; i++) {

			if (maxSpeed <= _maxSpeedOptions [i]) {

				_selectedMaxSpeedOptionIndex = i;

				SetMaxSpeedOption (_selectedMaxSpeedOptionIndex);

				break;
			}
		}
	}

	public void IncreaseMaxSpeed () {

		if (_simulationGuiPause) {
			return;
		}
	
		if (_selectedMaxSpeedOptionIndex == _lastMaxSpeedOptionIndex)
			return;

		_selectedMaxSpeedOptionIndex++;

		SetMaxSpeedOption (_selectedMaxSpeedOptionIndex);
	}

	public void DecreaseMaxSpeed () {

		if (_simulationGuiPause) {
			return;
		}

		if (_selectedMaxSpeedOptionIndex == 0)
			return;

		_selectedMaxSpeedOptionIndex--;

		SetMaxSpeedOption (_selectedMaxSpeedOptionIndex);
	}

	public void SetMaxSpeedOption (int speedOptionIndex) {

		_selectedMaxSpeedOptionIndex = speedOptionIndex;

		OnFirstMaxSpeedOptionSet.Invoke (_simulationGuiInterruption || (_selectedMaxSpeedOptionIndex == 0));
		OnLastMaxSpeedOptionSet.Invoke (_simulationGuiInterruption || (_selectedMaxSpeedOptionIndex == _lastMaxSpeedOptionIndex));

		Speed selectedSpeed = _maxSpeedOptions [speedOptionIndex];

		Manager.CurrentWorld.SetMaxYearsToSkip (selectedSpeed);

		OnSimulationSpeedChanged.Invoke (selectedSpeed);
	}

	public void PostProgressOp_LoadAction () {

		ResetUIElements ();
		
		SelectionPanelScript.RemoveAllOptions ();
		
		if (!Manager.SimulationCanRun) {
			
			SetInitialPopulation ();

		} else {

			InterruptSimulation (false);
		}

		GetMaxSpeedOptionFromCurrentWorld ();
		
		_postProgressOp -= PostProgressOp_LoadAction;
	}
	
	public void LoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Loading World...", true);
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		Manager.WorldName = Path.GetFileNameWithoutExtension (path);
		
		_postProgressOp += PostProgressOp_LoadAction;
		
		_displayProgressDialogs = true;
		
		_regenTextures = true;
	}
	
	public void CancelLoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		InterruptSimulation (false);
	}
	
	public void LoadWorld () {

		MainMenuDialogPanelScript.SetVisible (false);
		
		LoadFileDialogPanelScript.SetVisible (true);

		LoadFileDialogPanelScript.SetLoadAction (LoadAction);

		InterruptSimulation (true);
	}

	public void ChangePlanetOverlayToSelected () {

		SelectionPanelScript.RemoveAllOptions ();
		SelectionPanelScript.SetVisible (false);

		if (OverlayDialogPanelScript.PopDensityToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PopDensity);
		} else if (OverlayDialogPanelScript.FarmlandToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.FarmlandDistribution);
		} else if (OverlayDialogPanelScript.PopCulturalActivityToggle.isOn) {
			SetPopCulturalActivityOverlay ();
		} else if (OverlayDialogPanelScript.PopCulturalSkillToggle.isOn) {
			SetPopCulturalSkillOverlay ();
		} else if (OverlayDialogPanelScript.PopCulturalKnowledgeToggle.isOn) {
			SetPopCulturalKnowledgeOverlay ();
		} else if (OverlayDialogPanelScript.PopCulturalDiscoveryToggle.isOn) {
			SetPopCulturalDiscoveryOverlay ();
		} else if (OverlayDialogPanelScript.TerritoriesToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PolityTerritory);
		} else if (OverlayDialogPanelScript.InfluenceToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PolityInfluence);
		} else if (OverlayDialogPanelScript.PolityCulturalActivityToggle.isOn) {
			SetPolityCulturalActivityOverlay ();
		} else if (OverlayDialogPanelScript.PolityCulturalSkillToggle.isOn) {
			SetPolityCulturalSkillOverlay ();
		} else if (OverlayDialogPanelScript.PolityCulturalKnowledgeToggle.isOn) {
			SetPolityCulturalKnowledgeOverlay ();
		} else if (OverlayDialogPanelScript.PolityCulturalDiscoveryToggle.isOn) {
			SetPolityCulturalDiscoveryOverlay ();
		} else if (OverlayDialogPanelScript.TemperatureToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Temperature);
		} else if (OverlayDialogPanelScript.RainfallToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Rainfall);
		} else if (OverlayDialogPanelScript.ArabilityToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Arability);
		} else if (OverlayDialogPanelScript.PopChangeToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PopChange);
		} else if (OverlayDialogPanelScript.UpdateSpanToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.UpdateSpan);
		} else {
			UnsetOverlay();
		}

		SetRouteDisplayOverlay (OverlayDialogPanelScript.DisplayRoutesToggle.isOn);
	}
	
	public void CloseOverlayMenuAction () {

		ChangePlanetOverlayToSelected ();
		
		OverlayDialogPanelScript.SetVisible (false);
	}
	
	public void UpdateMenus () {

		if (!_menusNeedUpdate)
			return;

		_menusNeedUpdate = false;

		OverlayDialogPanelScript.PopDataToggle.isOn = false;
		OverlayDialogPanelScript.PolityDataToggle.isOn = false;
		OverlayDialogPanelScript.MiscDataToggle.isOn = false;
		OverlayDialogPanelScript.DebugDataToggle.isOn = false;

		OverlayDialogPanelScript.PopDensityToggle.isOn = false;
		OverlayDialogPanelScript.FarmlandToggle.isOn = false;
		OverlayDialogPanelScript.PopCulturalActivityToggle.isOn = false;
		OverlayDialogPanelScript.PopCulturalSkillToggle.isOn = false;
		OverlayDialogPanelScript.PopCulturalKnowledgeToggle.isOn = false;
		OverlayDialogPanelScript.PopCulturalDiscoveryToggle.isOn = false;

		OverlayDialogPanelScript.TerritoriesToggle.isOn = false;
		OverlayDialogPanelScript.InfluenceToggle.isOn = false;
		OverlayDialogPanelScript.PolityCulturalActivityToggle.isOn = false;
		OverlayDialogPanelScript.PolityCulturalSkillToggle.isOn = false;
		OverlayDialogPanelScript.PolityCulturalKnowledgeToggle.isOn = false;
		OverlayDialogPanelScript.PolityCulturalDiscoveryToggle.isOn = false;

		OverlayDialogPanelScript.TemperatureToggle.isOn = false;
		OverlayDialogPanelScript.RainfallToggle.isOn = false;
		OverlayDialogPanelScript.ArabilityToggle.isOn = false;

		OverlayDialogPanelScript.PopChangeToggle.isOn = false;
		OverlayDialogPanelScript.UpdateSpanToggle.isOn = false;

		OverlayDialogPanelScript.DisplayRoutesToggle.isOn = false;
		
		SelectionPanelScript.SetVisible (false);

		switch (_planetOverlay) {

		case PlanetOverlay.PopDensity:
			OverlayDialogPanelScript.PopDensityToggle.isOn = true;
			OverlayDialogPanelScript.PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.FarmlandDistribution:
			OverlayDialogPanelScript.FarmlandToggle.isOn = true;
			OverlayDialogPanelScript.PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopCulturalActivity:
			OverlayDialogPanelScript.PopCulturalActivityToggle.isOn = true;
			OverlayDialogPanelScript.PopDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;

		case PlanetOverlay.PopCulturalSkill:
			OverlayDialogPanelScript.PopCulturalSkillToggle.isOn = true;
			OverlayDialogPanelScript.PopDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;
			
		case PlanetOverlay.PopCulturalKnowledge:
			OverlayDialogPanelScript.PopCulturalKnowledgeToggle.isOn = true;
			OverlayDialogPanelScript.PopDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;

		case PlanetOverlay.PopCulturalDiscovery:
			OverlayDialogPanelScript.PopCulturalDiscoveryToggle.isOn = true;
			OverlayDialogPanelScript.PopDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;

		case PlanetOverlay.PolityTerritory:
			OverlayDialogPanelScript.TerritoriesToggle.isOn = true;
			OverlayDialogPanelScript.PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityInfluence:
			OverlayDialogPanelScript.InfluenceToggle.isOn = true;
			OverlayDialogPanelScript.PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityCulturalActivity:
			OverlayDialogPanelScript.PolityCulturalActivityToggle.isOn = true;
			OverlayDialogPanelScript.PolityDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;

		case PlanetOverlay.PolityCulturalSkill:
			OverlayDialogPanelScript.PolityCulturalSkillToggle.isOn = true;
			OverlayDialogPanelScript.PolityDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;

		case PlanetOverlay.PolityCulturalKnowledge:
			OverlayDialogPanelScript.PolityCulturalKnowledgeToggle.isOn = true;
			OverlayDialogPanelScript.PolityDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;

		case PlanetOverlay.PolityCulturalDiscovery:
			OverlayDialogPanelScript.PolityCulturalDiscoveryToggle.isOn = true;
			OverlayDialogPanelScript.PolityDataToggle.isOn = true;
			SelectionPanelScript.SetVisible (true);
			break;

		case PlanetOverlay.Temperature:
			OverlayDialogPanelScript.TemperatureToggle.isOn = true;
			OverlayDialogPanelScript.MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.Rainfall:
			OverlayDialogPanelScript.RainfallToggle.isOn = true;
			OverlayDialogPanelScript.MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.Arability:
			OverlayDialogPanelScript.ArabilityToggle.isOn = true;
			OverlayDialogPanelScript.MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopChange:
			OverlayDialogPanelScript.PopChangeToggle.isOn = true;
			OverlayDialogPanelScript.DebugDataToggle.isOn = true;
			break;

		case PlanetOverlay.UpdateSpan:
			OverlayDialogPanelScript.UpdateSpanToggle.isOn = true;
			OverlayDialogPanelScript.DebugDataToggle.isOn = true;
			break;
			
		case PlanetOverlay.None:
			break;
			
		default:
			throw new System.Exception ("Unhandled Planet Overlay type: " + _planetOverlay);
		}

		OverlayDialogPanelScript.DisplayRoutesToggle.isOn = _displayRoutes;
	}
	
	public void SelectOverlays () {
		
		OverlayDialogPanelScript.SetVisible (true);
	}
	
	public void SelectViews () {
		
		ViewsDialogPanelScript.SetVisible (true);
	}
	
	public void OpenMainMenu () {
		
		MainMenuDialogPanelScript.SetVisible (true);
		
		InterruptSimulation (true);
	}

	public void OpenOptionsMenu () {
		
		OptionsDialogPanelScript.SetVisible (true);
	}

	public void SetSimulationSpeedStopped (bool state) {

		if (state) {
			OnSimulationSpeedChanged.Invoke (Speed.Stopped);
		} else {
			OnSimulationSpeedChanged.Invoke (_maxSpeedOptions[_selectedMaxSpeedOptionIndex]);
		}
	}

	public void PauseSimulation (bool state) {

		SetSimulationSpeedStopped (state);

		OnFirstMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == 0));
		OnLastMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == _lastMaxSpeedOptionIndex));

		_simulationGuiPause = state;

		Manager.InterruptSimulation (state);
	}

	public void InterruptSimulation (bool state) {
		
		SetSimulationSpeedStopped (state);

		OnSimulationInterrupted.Invoke (state);
		OnFirstMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == 0));
		OnLastMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == _lastMaxSpeedOptionIndex));

		_simulationGuiInterruption = state;

		Manager.InterruptSimulation (state || _simulationGuiPause);
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

	public void SetRouteDisplayOverlay (bool value) {

		_regenTextures |= _displayRoutes != value;

		_displayRoutes = value;
	}

	public void ChangePlanetOverlay (PlanetOverlay value) {

		_regenTextures |= _planetOverlay != value;

		if (_regenTextures && (_planetOverlay != PlanetOverlay.None)) {

			_planetOverlaySubtypeCache[_planetOverlay] = _planetOverlaySubtype;
		}

		_planetOverlay = value;

		if (!_planetOverlaySubtypeCache.TryGetValue (_planetOverlay, out _planetOverlaySubtype)) {
		
			_planetOverlaySubtype = "None";
		}
	}

	public void SetPopCulturalActivityOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalActivity);

		SelectionPanelScript.Title.text = "Displayed Activity:";

		foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList) {

			AddSelectionPanelOption (activityInfo.Name, activityInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalActivityOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalActivity);

		SelectionPanelScript.Title.text = "Displayed Activity:";

		foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList) {

			AddSelectionPanelOption (activityInfo.Name, activityInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}
	
	public void SetPopCulturalSkillOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalSkill);

		SelectionPanelScript.Title.text = "Displayed Skill:";

		foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {

			AddSelectionPanelOption (skillInfo.Name, skillInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalSkillOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalSkill);

		SelectionPanelScript.Title.text = "Displayed Skill:";

		foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {

			AddSelectionPanelOption (skillInfo.Name, skillInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}
	
	public void SetPopCulturalKnowledgeOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalKnowledge);
		
		SelectionPanelScript.Title.text = "Displayed Knowledge:";
		
		foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {

			AddSelectionPanelOption (knowledgeInfo.Name, knowledgeInfo.Id);
		}
		
		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalKnowledgeOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalKnowledge);

		SelectionPanelScript.Title.text = "Displayed Knowledge:";

		foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {

			AddSelectionPanelOption (knowledgeInfo.Name, knowledgeInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPopCulturalDiscoveryOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalDiscovery);

		SelectionPanelScript.Title.text = "Displayed Discovery:";

		foreach (CulturalDiscovery discoveryInfo in Manager.CurrentWorld.CulturalDiscoveryInfoList) {

			AddSelectionPanelOption (discoveryInfo.Name, discoveryInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalDiscoveryOverlay () {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalDiscovery);

		SelectionPanelScript.Title.text = "Displayed Discovery:";

		foreach (CulturalDiscovery discoveryInfo in Manager.CurrentWorld.CulturalDiscoveryInfoList) {

			AddSelectionPanelOption (discoveryInfo.Name, discoveryInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void AddSelectionPanelOption (string optionName, string optionId) {

		SelectionPanelScript.AddOption (optionName, (state) => {
			if (state) {
				_planetOverlaySubtype = optionId;
			} else if (_planetOverlaySubtype == optionId) {
				_planetOverlaySubtype = "None";
			}

			_regenTextures = true;
		});

		if (_planetOverlaySubtype == optionId) {
			SelectionPanelScript.SetStateOption (optionName, true);
		}
	}

	public void UpdateSelectionMenu () {

		if (!SelectionPanelScript.IsVisible ())
			return;

		if (_planetOverlay == PlanetOverlay.PopCulturalActivity) {

			foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList) {

				AddSelectionPanelOption (activityInfo.Name, activityInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PopCulturalSkill) {
			
			foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {

				AddSelectionPanelOption (skillInfo.Name, skillInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PopCulturalKnowledge) {
			
			foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {

				AddSelectionPanelOption (knowledgeInfo.Name, knowledgeInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PopCulturalDiscovery) {

			foreach (CulturalDiscovery discoveryInfo in Manager.CurrentWorld.CulturalDiscoveryInfoList) {

				AddSelectionPanelOption (discoveryInfo.Name, discoveryInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PolityCulturalActivity) {

			foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList) {

				AddSelectionPanelOption (activityInfo.Name, activityInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PolityCulturalSkill) {

			foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {

				AddSelectionPanelOption (skillInfo.Name, skillInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PolityCulturalKnowledge) {

			foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {

				AddSelectionPanelOption (knowledgeInfo.Name, knowledgeInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PolityCulturalDiscovery) {

			foreach (CulturalDiscovery discoveryInfo in Manager.CurrentWorld.CulturalDiscoveryInfoList) {

				AddSelectionPanelOption (discoveryInfo.Name, discoveryInfo.Id);
			}
		}
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

	public void DisplaySelectedCellOverlay () {

		if (_previousSelectedCell == _selectedCell)
			return;

		if (_previousSelectedCell != null) {
			Manager.DisplayCellData (_previousSelectedCell, false);
		}
	
		if (_selectedCell == null)
			return;

		Manager.DisplayCellData (_selectedCell, true);
	}

	public void UpdateInfoPanel () {
		
		World world = Manager.CurrentWorld;
		
		InfoPanelText.text = "Year: " + world.CurrentDate;

		if (_selectedCell != null) {
			AddCellDataToInfoPanel (_selectedCell);
		}

		#if DEBUG
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nNumber of Events: " + WorldEvent.EventCount;

//		float meanTravelTime = 0;
//
//		if (MigrateGroupEvent.MigrationEventCount > 0)
//			meanTravelTime = MigrateGroupEvent.TotalTravelTime / MigrateGroupEvent.MigrationEventCount;
		
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nNumber of Migration Events: " + MigrateGroupEvent.MigrationEventCount;
//		InfoPanelText.text += "\nMean Migration Travel Time: " + meanTravelTime.ToString("0.0");
		
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nMUPS: " + _lastMapUpdateCount;
		#endif
	}
	
	public void AddCellDataToInfoPanel (int longitude, int latitude) {
		
		TerrainCell cell = Manager.CurrentWorld.GetCell (longitude, latitude);
		
		if (cell == null) return;

		AddCellDataToInfoPanel (cell);
	}
	
	public void AddCellDataToInfoPanel (TerrainCell cell) {
		
		World world = Manager.CurrentWorld;

		int longitude = cell.Longitude;
		int latitude = cell.Latitude;
		
		world.SetObservedCell (cell);

		InfoPanelText.text += "\n";

		float cellArea = cell.Area;
		
		InfoPanelText.text += string.Format("\nPosition: Longitude {0}, Latitude {1}", longitude, latitude);
		InfoPanelText.text += "\nArea: " + cellArea + " Km^2";
		InfoPanelText.text += "\nAltitude: " + cell.Altitude + " meters";
		InfoPanelText.text += "\nRainfall: " + cell.Rainfall + " mm / year";
		InfoPanelText.text += "\nTemperature: " + cell.Temperature + " C";
		InfoPanelText.text += "\n";

		for (int i = 0; i < cell.PresentBiomeNames.Count; i++)
		{
			float percentage = cell.BiomePresences[i];
			
			InfoPanelText.text += "\nBiome: " + cell.PresentBiomeNames[i];
			InfoPanelText.text += " (" + percentage.ToString ("P") + ")";
		}

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nSurvivability: " + cell.Survivability.ToString("P");
		InfoPanelText.text += "\nForaging Capacity: " + cell.ForagingCapacity.ToString("P");
		InfoPanelText.text += "\nAccessibility: " + cell.Accessibility.ToString("P");
		InfoPanelText.text += "\nArability: " + cell.Arability.ToString("P");

		float farmlandPercentage = cell.FarmlandPercentage;

		if (farmlandPercentage > 0) {

			InfoPanelText.text += "\n";
			InfoPanelText.text += "\nFarmland Percentage: " + farmlandPercentage.ToString ("P");
		}

		int population = 0;
		int optimalPopulation = 0;
		int lastUpdateDate = 0;
		int nextUpdateDate = 0;

		if (cell.Group != null) {
		
			float modifiedSurvivability = 0;
			float modifiedForagingCapacity = 0;

			population = cell.Group.Population;
			optimalPopulation = cell.Group.OptimalPopulation;
			lastUpdateDate = cell.Group.LastUpdateDate;
			nextUpdateDate = cell.Group.NextUpdateDate;
		
			cell.Group.CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

			if (population > 0) {
				
				InfoPanelText.text += "\n";
				InfoPanelText.text += "\nPrevious Population: " + cell.Group.PreviousPopulation;
				InfoPanelText.text += "\nPopulation: " + population;
				InfoPanelText.text += "\nOptimal Population: " + optimalPopulation;
				InfoPanelText.text += "\nPop Density: " + (population / cellArea).ToString ("0.000") + " Pop / Km^2";

				if (cell.FarmlandPercentage > 0) {

					float farmlandArea = farmlandPercentage * cellArea;

					InfoPanelText.text += "\nFarmland Area per Pop: " + (farmlandArea / (float)population).ToString ("0.000") + " Km^2 / Pop";
				}
				
				InfoPanelText.text += "\n";
				InfoPanelText.text += "\nModified Survivability: " + modifiedSurvivability.ToString ("P");
				InfoPanelText.text += "\nModified Foraging Capacity: " + modifiedForagingCapacity.ToString ("P");
				
				InfoPanelText.text += "\n";
				InfoPanelText.text += "\nLast Update Date: " + lastUpdateDate;
				InfoPanelText.text += "\nNext Update Date: " + nextUpdateDate;
				InfoPanelText.text += "\nTime between updates: " + (nextUpdateDate - lastUpdateDate);

				bool firstPolity = true;

				foreach (PolityInfluence polityInfluence in cell.Group.GetPolityInfluences ()) {

					Polity polity = polityInfluence.Polity;
					float influenceValue = polityInfluence.Value;

					if (influenceValue >= 0.001) {

						if (firstPolity) {
							InfoPanelText.text += "\n";
							InfoPanelText.text += "\nPolities";

							firstPolity = false;
						}

						InfoPanelText.text += "\n\tPolity[" + polity.Id + "] - Influence: " + influenceValue.ToString ("P");
					}
				}

				bool firstActivity = true;

				foreach (CellCulturalActivity activity in cell.Group.Culture.Activities) {

					float activityContribution = activity.Contribution;

					if (activityContribution >= 0.001) {

						if (firstActivity) {
							InfoPanelText.text += "\n";
							InfoPanelText.text += "\nActivities";

							firstActivity = false;
						}

						InfoPanelText.text += "\n\t" + activity.Id + " - Contribution: " + activity.Contribution.ToString ("P");
					}
				}

				bool firstSkill = true;

				foreach (CellCulturalSkill skill in cell.Group.Culture.Skills) {

					float skillValue = skill.Value;

					if (skillValue >= 0.001) {

						if (firstSkill) {
							InfoPanelText.text += "\n";
							InfoPanelText.text += "\nSkills";

							firstSkill = false;
						}

						InfoPanelText.text += "\n\t" + skill.Id + " - Value: " + skill.Value.ToString ("0.000");
					}
				}

				bool firstKnowledge = true;
				
				foreach (CellCulturalKnowledge knowledge in cell.Group.Culture.Knowledges) {
					
					float knowledgeValue = knowledge.Value;
					
					if (knowledgeValue >= 0.001) {
						
						if (firstKnowledge) {
							InfoPanelText.text += "\n";
							InfoPanelText.text += "\nKnowledges";
							
							firstKnowledge = false;
						}

						InfoPanelText.text += "\n\t" + knowledge.Id + " - Value: " + knowledge.Value.ToString ("0.000");
					}
				}
				
				bool firstDiscovery = true;
				
				foreach (CellCulturalDiscovery discovery in cell.Group.Culture.Discoveries) {

					if (firstDiscovery) {
						InfoPanelText.text += "\n";
						InfoPanelText.text += "\nDiscoveries";
						
						firstDiscovery = false;
					}
					
					InfoPanelText.text += "\n\t" + discovery.Id;
				}
			}
		}
	}
	
	public void AddCellDataToInfoPanel (Vector2 mapPosition) {

		int longitude = (int)mapPosition.x;
		int latitude = (int)mapPosition.y;

		AddCellDataToInfoPanel (longitude, latitude);
	}

	public bool GetMapCoordinatesFromMousePosition (Vector2 mousePosition, out Vector2 mapPosition) {

		Rect mapImageRect = MapImage.rectTransform.rect;
		
		Vector3 positionOverMapRect3D = MapImage.rectTransform.InverseTransformPoint (mousePosition);
		
		Vector2 positionOverMapRect = new Vector2 (positionOverMapRect3D.x, positionOverMapRect3D.y);
		
		if (mapImageRect.Contains (positionOverMapRect)) {
			
			Vector2 relPos = positionOverMapRect - mapImageRect.min;
			
			Vector2 uvPos = new Vector2 (relPos.x / mapImageRect.size.x, relPos.y / mapImageRect.size.y);
			
			uvPos += MapImage.uvRect.min;
			
			float worldLong = Mathf.Repeat (uvPos.x * Manager.CurrentWorld.Width, Manager.CurrentWorld.Width);
			float worldLat = uvPos.y * Manager.CurrentWorld.Height;
			
			mapPosition = new Vector2 (Mathf.Floor(worldLong), Mathf.Floor(worldLat));
			
			return true;
		}
		
		mapPosition = -Vector2.one;
		
		return false;
	}
	
	public bool GetMapCoordinatesFromMousePosition (out Vector2 mapPosition) {

		return GetMapCoordinatesFromMousePosition (Input.mousePosition, out mapPosition);
	}
	
	public void DragMap (BaseEventData data) {
		
		Rect mapImageRect = MapImage.rectTransform.rect;
		
		PointerEventData pointerData = data as PointerEventData;
		
		if (pointerData.button != PointerEventData.InputButton.Right)
			return;

		Vector2 delta = pointerData.position - _beginDragPosition;

		float uvDelta = delta.x / mapImageRect.width;

		Rect newUvRect = _beginDragMapUvRect;
		newUvRect.x -= uvDelta;

		MapImage.uvRect = newUvRect;
	}
	
	public void BeginDragMap (BaseEventData data) {
		
		PointerEventData pointerData = data as PointerEventData;

		if (pointerData.button != PointerEventData.InputButton.Right)
			return;

		_beginDragPosition = pointerData.position;
		_beginDragMapUvRect = MapImage.uvRect;
	}
	
	public void EndDragMap (BaseEventData data) {
	}
	
	public void SelectCellOnMap (BaseEventData data) {
		
		PointerEventData pointerData = data as PointerEventData;
		
		if (pointerData.button != PointerEventData.InputButton.Left)
			return;

		if (_mapLeftClickOp != null) {
		
			_mapLeftClickOp (pointerData.position);
		}
	}
}
