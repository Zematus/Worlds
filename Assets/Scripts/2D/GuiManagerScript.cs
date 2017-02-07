using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public delegate void PostProgressOperation ();

public delegate void PointerClickOperation (Vector2 position);

public delegate void PointerHoverOperation (Vector2 position);

public class GuiManagerScript : MonoBehaviour {

	public Text MapViewButtonText;

	public Text InfoPanelText;

	public RawImage MapImage;

	public Button LoadButton;

	public PlanetScript PlanetScript;
	public MapScript MapScript;

	public InfoTooltipScript InfoTooltipScript;
	
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

	private bool _mouseIsOverMap = false;

	private Vector3 _tooltipOffset = new Vector3 (0, 0);

	private Territory _lastHoveredOverTerritory = null;
	private Region _lastHoveredOverRegion = null;
	
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

	private bool _resetOverlays = true;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	private bool _displayProgressDialogs = false;
	
	private string _progressMessage = null;
	private float _progressValue = 0;

	private event PostProgressOperation _postProgressOp = null;

	private event PointerClickOperation _mapLeftClickOp = null;

	private event PointerHoverOperation _mapHoverOp = null;
	
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

	private bool _infoTextMinimized = false;

	private StreamWriter _debugLogStream;

	void OnEnable()
	{
		string filename = @".\debug.log";

		if (File.Exists (filename)) {
		
			File.Delete (filename);
		}

		_debugLogStream = File.CreateText(filename);

		Application.logMessageReceivedThreaded += HandleLog;

		if (Debug.isDebugBuild) {
			Debug.Log ("Executing debug build...");
		} else {
			Debug.Log ("Executing release build...");
		}
	}

	void OnDisable()
	{
		Application.logMessageReceivedThreaded -= HandleLog;

		_debugLogStream.Close ();
	}

	public void HandleLog(string logString, string stackTrace, LogType type)
	{
		_debugLogStream.WriteLine (logString);
	}

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
		InfoTooltipScript.SetVisible (false);

		_mapLeftClickOp += ClickOp_SelectCell;
		_mapHoverOp += HoverOp_ShowCellInfoTooltip;
		
		if (!Manager.WorldReady) {

			//GenerateWorld (false, 407252633);
			GenerateWorld (false, 783909167);
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

		bool simulationState = Manager.SimulationCanRun && Manager.SimulationRunning;

		InterruptSimulation (!simulationState && !_simulationGuiPause);

		if (simulationState) {

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

			if (_resetOverlays) {	
				_resetOverlays = false;

				_planetView = PlanetView.Biomes;

				#if DEBUG
				_planetOverlay = PlanetOverlay.UpdateSpan;
				#else
				_planetOverlay = PlanetOverlay.PopDensity;
				#endif
			}

			Manager.SetPlanetOverlay (_planetOverlay, _planetOverlaySubtype);
			Manager.SetPlanetView (_planetView);
			Manager.SetDisplayRoutes (_displayRoutes);

			Manager.GenerateTextures ();

			//PlanetScript.RefreshTexture ();
			MapScript.RefreshTexture ();

			_mapUpdateCount++;

		} else if (updateTextures) {

			Manager.UpdateTextures ();

			_mapUpdateCount++;
		}

		if (MapImage.enabled) {
			UpdateInfoPanel();
			UpdateSelectionMenu();
		}

		if (_mouseIsOverMap) {

			ExecuteMapHoverOp ();
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

		// TODO: delete next line
//		ResetUIElements ();
		
		Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;
		
		SelectionPanelScript.RemoveAllOptions ();

		SetInitialPopulation ();

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

	// TODO: delete function
//	private void ResetUIElements () {
//	
//		_selectedCell = null;
//	}

	public void SetInitialPopulationForTests () {

		int population = (int)Mathf.Ceil (World.StartPopulationDensity * TerrainCell.MaxArea);

		Manager.GenerateRandomHumanGroup (population);

		InterruptSimulation (false);

		DisplayTip_MapScroll ();
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

		if (!GetMapCoordinatesFromPointerPosition (position, out mapCoordinates))
			return;

		int longitude = (int)mapCoordinates.x;
		int latitude = (int)mapCoordinates.y;

		TerrainCell selectedCell = Manager.CurrentWorld.GetCell (longitude, latitude);

		Manager.SetSelectedCell (selectedCell);
	}

	public void ClickOp_SelectPopulationPlacement (Vector2 position) {

		int population = AddPopulationDialogScript.Population;

		Vector2 point;
		
		if (GetMapCoordinatesFromPointerPosition (out point)) {
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
		case PlanetOverlay.Language: 
			planetOverlayStr = "_languages"; 
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
		case PlanetOverlay.Region: 
			planetOverlayStr = "_region"; 
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

		// TODO: delete next commented line
//		ResetUIElements ();
		
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
		} else if (OverlayDialogPanelScript.RegionToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Region);
		} else if (OverlayDialogPanelScript.LanguageToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Language);
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
		OverlayDialogPanelScript.RegionToggle.isOn = false;
		OverlayDialogPanelScript.LanguageToggle.isOn = false;

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

		case PlanetOverlay.Region:
			OverlayDialogPanelScript.RegionToggle.isOn = true;
			OverlayDialogPanelScript.MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.Language:
			OverlayDialogPanelScript.LanguageToggle.isOn = true;
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

	public void MinimizeInfoText (bool state) {

		_infoTextMinimized = state;
	}

	public void PauseSimulation (bool state) {

		SetSimulationSpeedStopped (state);

		OnFirstMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == 0));
		OnLastMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == _lastMaxSpeedOptionIndex));

		_simulationGuiPause = state;

		Manager.InterruptSimulation (state);
	}

	public void InterruptSimulation (bool state) {

		SetPauseGui (state);

		Manager.InterruptSimulation (state || _simulationGuiPause);
	}

	public void SetPauseGui (bool state) {

		SetSimulationSpeedStopped (state);

		OnSimulationInterrupted.Invoke (state);
		OnFirstMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == 0));
		OnLastMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedOptionIndex == _lastMaxSpeedOptionIndex));

		_simulationGuiInterruption = state;
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

	public void UpdateInfoPanel () {
		
		World world = Manager.CurrentWorld;
		
		InfoPanelText.text = "Year: " + world.CurrentDate;

		if (_infoTextMinimized)
			return;

		if (Manager.CurrentWorld.SelectedCell != null) {
			AddCellDataToInfoPanel (Manager.CurrentWorld.SelectedCell);
		}

		InfoPanelText.text += "\n";

		#if DEBUG
		InfoPanelText.text += "\n -- Debug Data -- ";
//		InfoPanelText.text += "\n";
//		InfoPanelText.text += "\nNumber of Events: " + WorldEvent.EventCount;

//		float meanTravelTime = 0;
//
//		if (MigrateGroupEvent.MigrationEventCount > 0)
//			meanTravelTime = MigrateGroupEvent.TotalTravelTime / MigrateGroupEvent.MigrationEventCount;
		
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nNumber of Migration Events: " + MigrateGroupEvent.MigrationEventCount;
//		InfoPanelText.text += "\nMean Migration Travel Time: " + meanTravelTime.ToString("0.0");
		
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nMUPS: " + _lastMapUpdateCount;
		InfoPanelText.text += "\n";
		#endif
	}
	
	public void AddCellDataToInfoPanel (int longitude, int latitude) {
		
		TerrainCell cell = Manager.CurrentWorld.GetCell (longitude, latitude);
		
		if (cell == null) return;

		AddCellDataToInfoPanel (cell);
	}

	public void AddCellDataToInfoPanel_Terrain (TerrainCell cell) {
		
		float cellArea = cell.Area;

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Cell Terrain Data -- ";
		InfoPanelText.text += "\n";

		InfoPanelText.text += "\nArea: " + cellArea + " Km^2";
		InfoPanelText.text += "\nAltitude: " + cell.Altitude + " meters";
		InfoPanelText.text += "\nRainfall: " + cell.Rainfall + " mm / year";
		InfoPanelText.text += "\nTemperature: " + cell.Temperature + " C";
		InfoPanelText.text += "\n";

		for (int i = 0; i < cell.PresentBiomeNames.Count; i++) {
			float percentage = cell.BiomePresences [i];

			InfoPanelText.text += "\nBiome: " + cell.PresentBiomeNames [i];
			InfoPanelText.text += " (" + percentage.ToString ("P") + ")";
		}

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nSurvivability: " + cell.Survivability.ToString ("P");
		InfoPanelText.text += "\nForaging Capacity: " + cell.ForagingCapacity.ToString ("P");
		InfoPanelText.text += "\nAccessibility: " + cell.Accessibility.ToString ("P");
		InfoPanelText.text += "\nArability: " + cell.Arability.ToString ("P");
		InfoPanelText.text += "\n";

		Region region = cell.Region;

		if (region == null) {
			InfoPanelText.text += "\nCell doesn't belong to any known region";
		} else {
			InfoPanelText.text += "\nCell is part of Region #" + region.Id + ": " + region.Name;
		}
	}

	public void AddCellDataToInfoPanel_Region (TerrainCell cell) {

		Region region = cell.Region;

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Region Terrain Data -- ";
		InfoPanelText.text += "\n";

		if (region == null) {
			InfoPanelText.text += "\nCell doesn't belong to any known region";

			return;
		} else {
			InfoPanelText.text += "\nRegion #" + region.Id + ": " + region.Name;
		}
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nAttributes: ";

		bool first = true;
		foreach (RegionAttribute attr in region.Attributes) {

			if (first) {
				InfoPanelText.text += attr.Name;
				first = false;
			} else {
				InfoPanelText.text += ", " + attr.Name;
			}
		}

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nTotal Area: " + region.TotalArea + " Km^2";

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nCoast Percentage: " + region.CoastPercentage.ToString ("P");
		InfoPanelText.text += "\nOcean Percentage: " + region.OceanPercentage.ToString ("P");

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nAverage Altitude: " + region.AverageAltitude + " meters";
		InfoPanelText.text += "\nAverage Rainfall: " + region.AverageRainfall + " mm / year";
		InfoPanelText.text += "\nAverage Temperature: " + region.AverageTemperature + " C";
		InfoPanelText.text += "\n";

		InfoPanelText.text += "\nMin Region Altitude: " + region.MinAltitude + " meters";
		InfoPanelText.text += "\nMax Region Altitude: " + region.MaxAltitude + " meters";
		InfoPanelText.text += "\nAverage Border Altitude: " + region.AverageOuterBorderAltitude + " meters";
		InfoPanelText.text += "\n";

		for (int i = 0; i < region.PresentBiomeNames.Count; i++) {
			float percentage = region.BiomePresences [i];

			InfoPanelText.text += "\nBiome: " + region.PresentBiomeNames [i];
			InfoPanelText.text += " (" + percentage.ToString ("P") + ")";
		}

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nAverage Survivability: " + region.AverageSurvivability.ToString ("P");
		InfoPanelText.text += "\nAverage Foraging Capacity: " + region.AverageForagingCapacity.ToString ("P");
		InfoPanelText.text += "\nAverage Accessibility: " + region.AverageAccessibility.ToString ("P");
		InfoPanelText.text += "\nAverage Arability: " + region.AverageArability.ToString ("P");
	}

	public void AddCellDataToInfoPanel_FarmlandDistribution (TerrainCell cell) {

		float cellArea = cell.Area;
		float farmlandPercentage = cell.FarmlandPercentage;

		if (farmlandPercentage > 0) {

			InfoPanelText.text += "\n";
			InfoPanelText.text += "\n -- Cell Farmland Distribution Data -- ";
			InfoPanelText.text += "\n";

			InfoPanelText.text += "\nFarmland Percentage: " + farmlandPercentage.ToString ("P");
			InfoPanelText.text += "\n";
		}

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		if (cell.FarmlandPercentage > 0) {

			float farmlandArea = farmlandPercentage * cellArea;

			InfoPanelText.text += "\nFarmland Area per Pop: " + (farmlandArea / (float)population).ToString ("0.000") + " Km^2 / Pop";
		}
	}

	public void AddCellDataToInfoPanel_PopDensity (TerrainCell cell) {

		float cellArea = cell.Area;

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Population Density Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int optimalPopulation = cell.Group.OptimalPopulation;

		InfoPanelText.text += "\nPopulation: " + population;
		InfoPanelText.text += "\nPrevious Population: " + cell.Group.PreviousPopulation;
		InfoPanelText.text += "\nOptimal Population: " + optimalPopulation;
		InfoPanelText.text += "\nPop Density: " + (population / cellArea).ToString ("0.000") + " Pop / Km^2";

		float modifiedSurvivability = 0;
		float modifiedForagingCapacity = 0;

		cell.Group.CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nModified Survivability: " + modifiedSurvivability.ToString ("P");
		InfoPanelText.text += "\nModified Foraging Capacity: " + modifiedForagingCapacity.ToString ("P");
	}

	public void AddCellDataToInfoPanel_Language (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Language Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		Language groupLanguage = cell.Group.Culture.Language;

		if (groupLanguage == null) {

			InfoPanelText.text += "\n\tNo major language spoken at location";

			return;
		}

		InfoPanelText.text += "\n\tPredominant language at location: " + groupLanguage.Id;
	}

	public void AddCellDataToInfoPanel_UpdateSpan (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Update Span Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int lastUpdateDate = cell.Group.LastUpdateDate;
		int nextUpdateDate = cell.Group.NextUpdateDate;

		InfoPanelText.text += "\nLast Update Date: " + lastUpdateDate;
		InfoPanelText.text += "\nNext Update Date: " + nextUpdateDate;
		InfoPanelText.text += "\nTime between updates: " + (nextUpdateDate - lastUpdateDate);
	}

	public void AddCellDataToInfoPanel_PolityInfluence (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Polity Influence Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		bool firstPolity = true;

		foreach (PolityInfluence polityInfluence in cell.Group.GetPolityInfluences ()) {

			Polity polity = polityInfluence.Polity;
			float influenceValue = polityInfluence.Value;
			float coreDistance = polityInfluence.CoreDistance;
			float administrativeCost = polityInfluence.AdiministrativeCost;

			if (influenceValue >= 0.001) {

				if (firstPolity) {
					InfoPanelText.text += "\nPolities:";

					firstPolity = false;
				}

				InfoPanelText.text += "\n\tPolity #" + polity.Id + ":" +
					"\n\t\tInfluence: " + influenceValue.ToString ("P") +
					"\n\t\tDistance to Core: " + coreDistance.ToString ("0.000") +
					"\n\t\tAdministrative Cost: " + administrativeCost.ToString ("0.000");
			}
		}
	}

	public void AddCellDataToInfoPanel_PolityTerritory (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Polity Territory Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		Territory territory = cell.EncompassingTerritory;


		if (territory == null) {
			InfoPanelText.text += "\n\tGroup not part of a polity's territory";
			return;
		}

		Polity polity = territory.Polity;

		InfoPanelText.text += "\n\tTerritory of " + polity.Type + " " + polity.Name + " (#" + polity.Id +")";
		InfoPanelText.text += "\n";

		int totalPopulation = (int)Mathf.Floor(polity.TotalPopulation);

		InfoPanelText.text += "\n\tPolity population: " + totalPopulation;
		InfoPanelText.text += "\n";

		float administrativeCost = polity.TotalAdministrativeCost;

		InfoPanelText.text += "\n\tAdministrative Cost: " + administrativeCost;

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Polity Factions -- ";
		InfoPanelText.text += "\n";

		foreach (Faction faction in polity.GetFactions ()) {

			InfoPanelText.text += "\n\t" + faction.Type + " " + faction.Name;
			InfoPanelText.text += "\n\t\tProminence: " + faction.Prominence.ToString ("P");
		}

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Selected Group's Polity Data -- ";
		InfoPanelText.text += "\n";

		float percentageOfPopulation = cell.Group.GetPolityInfluenceValue (polity);
		int influencedPopulation = (int)Mathf.Floor(population * percentageOfPopulation);

		float percentageOfPolity = 1;

		if (totalPopulation > 0) {
			percentageOfPolity = influencedPopulation / (float)totalPopulation;
		}

		InfoPanelText.text += "\n\tInfluenced population: " + influencedPopulation;
		InfoPanelText.text += "\n\tPercentage of polity population: " + percentageOfPolity.ToString ("P");
	}

	public void AddCellDataToInfoPanel_PolityCulturalActivity (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Polity Activity Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		PolityInfluence polityInfluence = cell.Group.HighestPolityInfluence;

		if (polityInfluence == null) {

			InfoPanelText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstActivity = true;

		foreach (CulturalActivity activity in polityInfluence.Polity.Culture.Activities) {

			float activityContribution = activity.Contribution;

			if (activityContribution >= 0.001) {

				if (firstActivity) {
					InfoPanelText.text += "\nActivities:";

					firstActivity = false;
				}

				InfoPanelText.text += "\n\t" + activity.Id + " - Contribution: " + activity.Contribution.ToString ("P");
			}
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalActivity (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Activity Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		bool firstActivity = true;

		foreach (CulturalActivity activity in cell.Group.Culture.Activities) {

			float activityContribution = activity.Contribution;

			if (activityContribution >= 0.001) {

				if (firstActivity) {
					InfoPanelText.text += "\nActivities:";

					firstActivity = false;
				}

				InfoPanelText.text += "\n\t" + activity.Id + " - Contribution: " + activity.Contribution.ToString ("P");
			}
		}
	}

	public void AddCellDataToInfoPanel_PolityCulturalSkill (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Polity Skill Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		PolityInfluence polityInfluence = cell.Group.HighestPolityInfluence;

		if (polityInfluence == null) {

			InfoPanelText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstSkill = true;

		foreach (CulturalSkill skill in polityInfluence.Polity.Culture.Skills) {

			float skillValue = skill.Value;

			if (skillValue >= 0.001) {

				if (firstSkill) {
					InfoPanelText.text += "\nSkills:";

					firstSkill = false;
				}

				InfoPanelText.text += "\n\t" + skill.Id + " - Value: " + skill.Value.ToString ("0.000");
			}
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalSkill (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Skill Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		bool firstSkill = true;

		foreach (CulturalSkill skill in cell.Group.Culture.Skills) {

			float skillValue = skill.Value;

			if (skillValue >= 0.001) {

				if (firstSkill) {
					InfoPanelText.text += "\nSkills:";

					firstSkill = false;
				}

				InfoPanelText.text += "\n\t" + skill.Id + " - Value: " + skill.Value.ToString ("0.000");
			}
		}
	}

	public void AddCellDataToInfoPanel_PolityCulturalKnowledge (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Polity Knowledge Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		PolityInfluence polityInfluence = cell.Group.HighestPolityInfluence;

		if (polityInfluence == null) {

			InfoPanelText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstKnowledge = true;

		foreach (CulturalKnowledge knowledge in polityInfluence.Polity.Culture.Knowledges) {

			float knowledgeValue = knowledge.ScaledValue;

			if (firstKnowledge) {
				InfoPanelText.text += "\nKnowledges:";

				firstKnowledge = false;
			}

			InfoPanelText.text += "\n\t" + knowledge.Id + " - Value: " + knowledgeValue.ToString ("0.000");
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalKnowledge (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Knowledge Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		bool firstKnowledge = true;

		foreach (CulturalKnowledge knowledge in cell.Group.Culture.Knowledges) {

			float knowledgeValue = knowledge.ScaledValue;

			if (firstKnowledge) {
				InfoPanelText.text += "\nKnowledges:";

				firstKnowledge = false;
			}

			InfoPanelText.text += "\n\t" + knowledge.Id + " - Value: " + knowledgeValue.ToString ("0.000");
		}
	}

	public void AddCellDataToInfoPanel_PolityCulturalDiscovery (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Polity Discovery Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		PolityInfluence polityInfluence = cell.Group.HighestPolityInfluence;

		if (polityInfluence == null) {

			InfoPanelText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstDiscovery = true;

		foreach (CulturalDiscovery discovery in polityInfluence.Polity.Culture.Discoveries) {

			if (firstDiscovery) {
				InfoPanelText.text += "\nDiscoveries:";

				firstDiscovery = false;
			}

			InfoPanelText.text += "\n\t" + discovery.Id;
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalDiscovery (TerrainCell cell) {

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\n -- Group Discovery Data -- ";
		InfoPanelText.text += "\n";

		if (cell.Group == null) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelText.text += "\n\tNo population at location";

			return;
		}

		bool firstDiscovery = true;

		foreach (CulturalDiscovery discovery in cell.Group.Culture.Discoveries) {

			if (firstDiscovery) {
				InfoPanelText.text += "\nDiscoveries:";

				firstDiscovery = false;
			}

			InfoPanelText.text += "\n\t" + discovery.Id;
		}
	}
	
	public void AddCellDataToInfoPanel (TerrainCell cell) {

		int longitude = cell.Longitude;
		int latitude = cell.Latitude;

		InfoPanelText.text += "\n";
		InfoPanelText.text += string.Format ("\nPosition: Longitude {0}, Latitude {1}", longitude, latitude);

		if ((_planetOverlay == PlanetOverlay.None) || 
			(_planetOverlay == PlanetOverlay.Rainfall) || 
			(_planetOverlay == PlanetOverlay.Arability) || 
			(_planetOverlay == PlanetOverlay.Temperature)) {

			AddCellDataToInfoPanel_Terrain (cell);
		}

		if (_planetOverlay == PlanetOverlay.Region) {

			AddCellDataToInfoPanel_Region (cell);
		}

		if (_planetOverlay == PlanetOverlay.Language) {

			AddCellDataToInfoPanel_Language (cell);
		}

		if (_planetOverlay == PlanetOverlay.FarmlandDistribution) {

			AddCellDataToInfoPanel_FarmlandDistribution (cell);
		}

		if ((_planetOverlay == PlanetOverlay.PopDensity) || 
			(_planetOverlay == PlanetOverlay.PopChange)) {
		
			AddCellDataToInfoPanel_PopDensity (cell);
		}

		if (_planetOverlay == PlanetOverlay.UpdateSpan) {

			AddCellDataToInfoPanel_UpdateSpan (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityInfluence) {

			AddCellDataToInfoPanel_PolityInfluence (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityTerritory) {

			AddCellDataToInfoPanel_PolityTerritory (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityCulturalActivity) {

			AddCellDataToInfoPanel_PolityCulturalActivity (cell);
		}

		if (_planetOverlay == PlanetOverlay.PopCulturalActivity) {

			AddCellDataToInfoPanel_PopCulturalActivity (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityCulturalSkill) {

			AddCellDataToInfoPanel_PolityCulturalSkill (cell);
		}

		if (_planetOverlay == PlanetOverlay.PopCulturalSkill) {

			AddCellDataToInfoPanel_PopCulturalSkill (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityCulturalKnowledge) {

			AddCellDataToInfoPanel_PolityCulturalKnowledge (cell);
		}

		if (_planetOverlay == PlanetOverlay.PopCulturalKnowledge) {

			AddCellDataToInfoPanel_PopCulturalKnowledge (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityCulturalDiscovery) {

			AddCellDataToInfoPanel_PolityCulturalDiscovery (cell);
		}

		if (_planetOverlay == PlanetOverlay.PopCulturalDiscovery) {

			AddCellDataToInfoPanel_PopCulturalDiscovery (cell);
		}
	}
	
	public void AddCellDataToInfoPanel (Vector2 mapPosition) {

		int longitude = (int)mapPosition.x;
		int latitude = (int)mapPosition.y;

		AddCellDataToInfoPanel (longitude, latitude);
	}

	public void HoverOp_ShowCellInfoTooltip (Vector2 position) {

		Vector2 mapCoordinates;

		if (!GetMapCoordinatesFromPointerPosition (position, out mapCoordinates))
			return;

		int longitude = (int)mapCoordinates.x;
		int latitude = (int)mapCoordinates.y;

		TerrainCell hoveredCell = Manager.CurrentWorld.GetCell (longitude, latitude);

		switch (_planetOverlay) {

		case PlanetOverlay.PolityTerritory:

			ShowCellInfoToolTip_PolityTerritory (hoveredCell);
			break;

		case PlanetOverlay.Region:

			ShowCellInfoToolTip_Region (hoveredCell);
			break;
		}
	}

	public void ShowCellInfoToolTip_PolityTerritory (TerrainCell cell) {

		if (cell.EncompassingTerritory == _lastHoveredOverTerritory)
			return;

		_lastHoveredOverTerritory = cell.EncompassingTerritory;

		if (_lastHoveredOverTerritory == null) {
		
			InfoTooltipScript.SetVisible (false);
			return;
		}

		Polity polity = _lastHoveredOverTerritory.Polity;

		if (polity == null) {

			throw new System.Exception ("Polity can't be null");
		}

		Vector3 tooltipPos = GetScreenPositionFromMapCoordinates(polity.CoreGroup.Cell.Position) + _tooltipOffset;

		if (polity.Name == null) {

			throw new System.Exception ("Polity.Name can't be null");

		} 

		if (polity.Name.Text == null) {

			throw new System.Exception ("polity.Name.Text can't be null");
		}

		InfoTooltipScript.DisplayTip (polity.Name.Text, tooltipPos);
	}

	public void ShowCellInfoToolTip_Region (TerrainCell cell) {

		if (cell.Region == _lastHoveredOverRegion)
			return;

		_lastHoveredOverRegion = cell.Region;

		if (_lastHoveredOverRegion == null) {

			InfoTooltipScript.SetVisible (false);
			return;
		}

		WorldPosition regionCenterCellPosition = _lastHoveredOverRegion.GetMostCenteredCell ().Position;

		Vector3 tooltipPos = GetScreenPositionFromMapCoordinates(regionCenterCellPosition) + _tooltipOffset;

		InfoTooltipScript.DisplayTip (_lastHoveredOverRegion.Name.Text, tooltipPos);
	}

	public Vector3 GetScreenPositionFromMapCoordinates (WorldPosition mapPosition) {

		Rect mapImageRect = MapImage.rectTransform.rect;

		Vector2 normalizedMapPos = new Vector2 (mapPosition.Longitude / (float) Manager.CurrentWorld.Width, mapPosition.Latitude / (float) Manager.CurrentWorld.Height);

		Vector2 mapImagePos = normalizedMapPos - MapImage.uvRect.min;
		mapImagePos.x = Mathf.Repeat (mapImagePos.x, 1.0f);

		mapImagePos.Scale (mapImageRect.size);

		return MapImage.rectTransform.TransformPoint (mapImagePos + mapImageRect.min);
	}

	public bool GetMapCoordinatesFromPointerPosition (Vector2 pointerPosition, out Vector2 mapPosition) {

		Rect mapImageRect = MapImage.rectTransform.rect;
		
		Vector3 positionOverMapRect3D = MapImage.rectTransform.InverseTransformPoint (pointerPosition);
		
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
	
	public bool GetMapCoordinatesFromPointerPosition (out Vector2 mapPosition) {

		return GetMapCoordinatesFromPointerPosition (Input.mousePosition, out mapPosition);
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

	public void ExecuteMapHoverOp () {

		if (_mapHoverOp != null) {

			_mapHoverOp (Input.mousePosition);
		}
	}

	public void PointerEntersMap (BaseEventData data) {

		_mouseIsOverMap = true;
	}

	public void PointerExitsMap (BaseEventData data) {

		_mouseIsOverMap = false;
	}
}
