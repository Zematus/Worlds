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
	
	public const float MaxDeltaTimeIterations = 0.02f; // max real time to be spent on iterations on a single frame (this is the value that matters the most performance-wise)

	public Text MapViewButtonText;

	public RawImage MapImage;

	public Button LoadButton;

	public PlanetScript PlanetScript;
	public MapScript MapScript;

	public InfoTooltipScript InfoTooltipScript;

	public InfoPanelScript InfoPanelScript;
	
	public TextInputDialogPanelScript SaveFileDialogPanelScript;
	public TextInputDialogPanelScript ExportMapDialogPanelScript;
	public DecisionDialogPanelScript DecisionDialogPanelScript;
	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public SelectFactionDialogPanelScript SelectFactionDialogPanelScript;
	public OverlayDialogPanelScript OverlayDialogPanelScript;
	public DialogPanelScript ViewsDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public DialogPanelScript OptionsDialogPanelScript;
	public SettingsDialogPanelScript SettingsDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	public ImageDialogPanelScript ActivityDialogPanelScript;
	public TextInputDialogPanelScript ErrorMessageDialogPanelScript;
	public WorldCustomizationDialogPanelScript SetSeedDialogPanelScript;
	public WorldCustomizationDialogPanelScript CustomizeWorldDialogPanelScript;
	public AddPopulationDialogScript AddPopulationDialogScript;
	public FocusPanelScript FocusPanelScript;
	public GuidingPanelScript GuidingPanelScript;
	public ModalPanelScript CreditsDialogPanelScript;

	public List<ModalPanelScript> HiddenInteractionPanels = new List<ModalPanelScript> ();

	public PaletteScript BiomePaletteScript;
	public PaletteScript MapPaletteScript;
	public PaletteScript OverlayPaletteScript;

	public SelectionPanelScript SelectionPanelScript;

	public QuickTipPanelScript QuickTipPanelScript;

	public EventPanelScript EventPanelScript;
	
	public ToggleEvent OnSimulationInterrupted;
	public ToggleEvent OnSimulationPaused;

	public ToggleEvent OnFirstMaxSpeedOptionSet;
	public ToggleEvent OnLastMaxSpeedOptionSet;

	public UnityEvent MapEntitySelected;

	public UnityEvent OverlayChanged;
	
	public SpeedChangeEvent OnSimulationSpeedChanged;

//	private bool _showFocusButton = false;
//	private string _focusButtonText = "";

	private bool _eventPauseActive = false;

	private bool _pauseButtonPressed = false;

	private bool _pausingDialogActive = false;

	private bool _displayedTip_mapScroll = false;
	private bool _displayedTip_initialPopulation = false;

	private bool _mouseIsOverMap = false;

	private Vector3 _tooltipOffset = new Vector3 (0, 0);

	private Language _lastHoveredOverLanguage = null;
	private Territory _lastHoveredOverTerritory = null;
	private Region _lastHoveredOverRegion = null;
	
	private PlanetView _planetView = PlanetView.Biomes;

	private PlanetOverlay _planetOverlay = PlanetOverlay.General;

	private string _planetOverlaySubtype = "None";

	private Dictionary<PlanetOverlay, string> _planetOverlaySubtypeCache = new Dictionary<PlanetOverlay, string> ();

	private bool _displayRoutes = false;
	private bool _displayGroupActivity = false;

//	private bool _overlayMenusNeedUpdate = true;

	private bool _regenTextures = false;

	private bool _resetOverlays = true;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	private bool _backgroundProcessActive = false;
	
	private string _progressMessage = null;
	private float _progressValue = 0;

	private event PostProgressOperation _postProgressOp = null;

	private event PointerClickOperation _mapLeftClickOp = null;

	private event PointerHoverOperation _mapHoverOp = null;
	
	private const float _maxAccTime = 1.0f; // the standard length of time of a simulation cycle (in real time)

	private float _accDeltaTime = 0;
	private long _simulationDateSpan = 0;

	private bool _resolvedDecision = false;

	private int _mapUpdateCount = 0;
	private int _lastMapUpdateCount = 0;
	private float _timeSinceLastMapUpdate = 0;

	private int _topMaxSpeedLevelIndex;
	private int _selectedMaxSpeedLevelIndex;

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

        if (_debugLogStream != null)
        {
            _debugLogStream.Close();
        }
	}

	public void HandleLog(string logString, string stackTrace, LogType type)
	{
        if (_debugLogStream == null)
            return;

		_debugLogStream.WriteLine (logString);

		#if DEBUG
		_debugLogStream.Flush ();
		#endif
	}

	// Use this for initialization
	void Start () {

		Manager.LoadAppSettings (@"Worlds.settings");
		
		_topMaxSpeedLevelIndex = Speed.Levels.Length - 1;
		_selectedMaxSpeedLevelIndex = _topMaxSpeedLevelIndex;

		Manager.UpdateMainThreadReference ();
		
		SaveFileDialogPanelScript.SetVisible (false);
		ExportMapDialogPanelScript.SetVisible (false);
		DecisionDialogPanelScript.SetVisible (false);
		LoadFileDialogPanelScript.SetVisible (false);
		SelectFactionDialogPanelScript.SetVisible (false);
		OverlayDialogPanelScript.SetVisible (false);
		ViewsDialogPanelScript.SetVisible (false);
		MainMenuDialogPanelScript.SetVisible (false);
		ProgressDialogPanelScript.SetVisible (false);
		ActivityDialogPanelScript.SetVisible (false);
		OptionsDialogPanelScript.SetVisible (false);
		SetSeedDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);
		ErrorMessageDialogPanelScript.SetVisible (false);
		AddPopulationDialogScript.SetVisible (false);

		FocusPanelScript.SetVisible (false);
		GuidingPanelScript.SetVisible (false);

		QuickTipPanelScript.SetVisible (false);
		InfoTooltipScript.SetVisible (false);

		_mapLeftClickOp += ClickOp_SelectCell;
		_mapHoverOp += HoverOp_ShowCellInfoTooltip;
		
		if (!Manager.WorldIsReady) {

            //GenerateWorld(false, 407252633);
            //GenerateWorld(false, 783909167);
            //GenerateWorld(false, 1446630758);
            //GenerateWorld(false, 1788799931);
            //GenerateWorld(false, 616109363);
            //GenerateWorld(false, 1065375312);
            //GenerateWorld(false, 279552712);
            //GenerateWorld(false, 1735984055);
            GenerateWorld(false, 519520942);


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

//		UpdateOverlayMenus ();

		Manager.ExecuteTasks (100);
		
		if (_backgroundProcessActive) {
			
			if (_progressMessage != null) ProgressDialogPanelScript.SetDialogText (_progressMessage);
			
			ProgressDialogPanelScript.SetProgress (_progressValue);
		}
		
		if (!Manager.WorldIsReady) {
			return;
		}
		
		if (Manager.PerformingAsyncTask) {
			return;
		}

		if (_backgroundProcessActive) {

			ProgressDialogPanelScript.SetVisible (false);
			ActivityDialogPanelScript.SetVisible (false);

			_backgroundProcessActive = false;
			
			if (_postProgressOp != null) 
				_postProgressOp ();

			ShowHiddenInteractionPanels ();
		}

		bool simulationRunning = Manager.SimulationCanRun && Manager.SimulationRunning;

		if (simulationRunning) {

			World world = Manager.CurrentWorld;

			Speed maxSpeed = Speed.Levels [_selectedMaxSpeedLevelIndex];

			_accDeltaTime += Time.deltaTime;

			if (_accDeltaTime > _maxAccTime) {

				_accDeltaTime -= _maxAccTime;
				_simulationDateSpan = 0;
			}

			int maxSimulationDateSpan = (int)Mathf.Ceil(maxSpeed * _accDeltaTime);

			// Simulate additional iterations if we haven't reached the max amount of iterations allowed per the percentage of transpired real time during this cycle
			if (_simulationDateSpan < maxSimulationDateSpan) {

				long maxDateSpanBetweenUpdates = (int)Mathf.Ceil(maxSpeed * MaxDeltaTimeIterations);
				long lastUpdateDate = world.CurrentDate;

				long dateSpan = 0;

				float startTimeIterations = Time.realtimeSinceStartup;

				// Simulate up to the max amout of iterations allowed per frame
				while ((lastUpdateDate + maxDateSpanBetweenUpdates) > world.CurrentDate) {

					if (_resolvedDecision) {
						
						_resolvedDecision = false;

					} else {
						
						world.EvaluateEventsToHappen ();
					}

					if (world.HasDecisionsToResolve ()) {

						RequestDecisionResolution ();
						break;
					}

					dateSpan += world.Update ();

					float deltaTimeIterations = Time.realtimeSinceStartup - startTimeIterations;

					// If too much real time was spent simulating after this iteration stop simulating until the next frame
					if (deltaTimeIterations > MaxDeltaTimeIterations)
						break;
				}

				_simulationDateSpan += dateSpan;
			}

			while (world.EventMessagesLeftToShow () > 0) {

				ShowEventMessage (Manager.CurrentWorld.GetNextMessageToShow ());
			}
		}
	
		if (_regenTextures) {
			if (_resetOverlays) {
				_planetView = PlanetView.Biomes;

				#if DEBUG
				_planetOverlay = PlanetOverlay.PolityTerritory;
				#else
				_planetOverlay = PlanetOverlay.General;
				#endif
			}

			Manager.SetPlanetOverlay (_planetOverlay, _planetOverlaySubtype);
			Manager.SetPlanetView (_planetView);
			Manager.SetDisplayRoutes (_displayRoutes);
			Manager.SetDisplayGroupActivity (_displayGroupActivity);

			if (_resetOverlays) {
				OverlayChanged.Invoke ();

				_resetOverlays = false;
			}

			Manager.GenerateTextures ();

			MapScript.RefreshTexture ();

			_mapUpdateCount++;

			_regenTextures = false;

		} else {

			Manager.UpdateTextures ();

			_mapUpdateCount++;
		}

		if (MapImage.enabled) {
			UpdateInfoPanel ();
			UpdateFocusPanel ();
			UpdateGuidingPanel ();
			UpdateSelectionMenu ();
		}

		if (_mouseIsOverMap) {
			ExecuteMapHoverOp ();
		}

		ReadKeyboardInput ();
	}

	public bool CanAlterRunningStateOrSpeed () {

		return Manager.SimulationCanRun && !_pausingDialogActive;
	}

	public void ReadKeyboardInput_TimeControls () {

		if (Input.GetKeyDown (KeyCode.Space))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				PauseSimulation (Manager.SimulationRunning);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha1))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (0);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha2))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (1);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha3))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (2);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha4))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (3);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha5))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (4);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha6))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (5);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha7))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (6);
			}
		}

		if (Input.GetKeyDown (KeyCode.Alpha8))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (7);
			}
		}

		if (Input.GetKeyDown (KeyCode.KeypadPlus))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (_selectedMaxSpeedLevelIndex + 1);
			}
		}

		if (Input.GetKeyDown (KeyCode.KeypadMinus))
		{
			if (CanAlterRunningStateOrSpeed ()) {
				SetMaxSpeedLevel (_selectedMaxSpeedLevelIndex - 1);
			}
		}
	}

	public void ReadKeyboardInput_Escape () {

		if (Input.GetKeyDown (KeyCode.Escape))
		{
			if (!_backgroundProcessActive) {

				if (SaveFileDialogPanelScript.gameObject.activeInHierarchy) {

					CancelSaveAction ();

				} else if (ExportMapDialogPanelScript.gameObject.activeInHierarchy) {

					CancelExportAction ();

				} else if (LoadFileDialogPanelScript.gameObject.activeInHierarchy) {

					CancelLoadAction ();

				} else if (SelectFactionDialogPanelScript.gameObject.activeInHierarchy) {

					CancelSelectFaction ();

				} else if (OverlayDialogPanelScript.gameObject.activeInHierarchy) {

					CloseOverlayMenuAction ();

				} else if (ViewsDialogPanelScript.gameObject.activeInHierarchy) {

					CloseViewsMenuAction ();

				} else if (MainMenuDialogPanelScript.gameObject.activeInHierarchy) {

					CloseMainMenu ();

				} else if (OptionsDialogPanelScript.gameObject.activeInHierarchy) {

					CloseOptionsMenu ();

				} else if ((SetSeedDialogPanelScript.gameObject.activeInHierarchy) || 
					(CustomizeWorldDialogPanelScript.gameObject.activeInHierarchy)) {

					CancelGenerateAction ();

				} else if (ErrorMessageDialogPanelScript.gameObject.activeInHierarchy) {

					CloseErrorMessageAction ();

				} else if (AddPopulationDialogScript.gameObject.activeInHierarchy) {

					CancelPopulationPlacement ();

				} else {

					OpenMainMenu ();
				}
			}
		}
	}

	public void ReadKeyboardInput () {
		
		ReadKeyboardInput_TimeControls ();
		ReadKeyboardInput_Escape ();
	}

	public bool IsPolityOverlay (PlanetOverlay overlay) {
	
		return (overlay == PlanetOverlay.PolityCulturalActivity) ||
			(overlay == PlanetOverlay.PolityCulturalSkill) ||
			(overlay == PlanetOverlay.PolityCulturalPreference) ||
			(overlay == PlanetOverlay.PolityCulturalKnowledge) ||
			(overlay == PlanetOverlay.PolityCulturalDiscovery) ||
			(overlay == PlanetOverlay.PolityTerritory) ||
			(overlay == PlanetOverlay.PolityContacts) ||
			(overlay == PlanetOverlay.General);
	}

	public void UpdateFocusPanel () {

		Polity selectedPolity = null;
		bool isUnderFocus = false;

		if ((Manager.CurrentWorld.SelectedTerritory != null) && IsPolityOverlay(_planetOverlay)) {

			selectedPolity = Manager.CurrentWorld.SelectedTerritory.Polity;

			isUnderFocus |= (Manager.CurrentWorld.PolitiesUnderPlayerFocus.Contains (selectedPolity));
		}

		if (selectedPolity != null) {
			FocusPanelScript.SetVisible (true);

			if (isUnderFocus)
				FocusPanelScript.SetState (FocusPanelState.UnsetFocus, selectedPolity);
			else
				FocusPanelScript.SetState (FocusPanelState.SetFocus, selectedPolity);

		} else {
			FocusPanelScript.SetVisible (false);
		}
	}

	public void UpdateGuidingPanel () {

		Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

		if (guidedFaction != null) {
			GuidingPanelScript.SetVisible (true);

			GuidingPanelScript.SetState (guidedFaction);

		} else {
			GuidingPanelScript.SetVisible (false);
		}
	}

	public void SelectAndCenterOnCell (WorldPosition position) {
		
		ShiftMapToPosition (position);

		Manager.SetSelectedCell (position);

		MapEntitySelected.Invoke ();
	}

	public string GetMessageToShow (WorldEventMessage eventMessage) {

		return Manager.GetDateString (eventMessage.Date) + " - " + eventMessage.Message;
	}

	public void ShowEventMessageForPolity (WorldEventMessage eventMessage, long polityId) {

		Polity polity = Manager.CurrentWorld.GetPolity (polityId);

		if (polity != null) {
			
			WorldPosition corePosition = polity.CoreGroup.Position;

			EventPanelScript.AddEventMessage (GetMessageToShow (eventMessage), () => {

				SelectAndCenterOnCell (corePosition);

				if ((_planetOverlay != PlanetOverlay.PolityTerritory) && (_planetOverlay != PlanetOverlay.General))
					ChangePlanetOverlay (PlanetOverlay.PolityTerritory);
			});
		} else {
			
			EventPanelScript.AddEventMessage (GetMessageToShow (eventMessage));
		}
	}

	public void ShowEventMessage (WorldEventMessage eventMessage) {

		if (eventMessage is TribeSplitEventMessage) {

			TribeSplitEventMessage tribeSplitEventMessage = eventMessage as TribeSplitEventMessage;

			ShowEventMessageForPolity (eventMessage, tribeSplitEventMessage.NewTribeId);

		} else if (eventMessage is PolityFormationEventMessage) {

			PolityFormationEventMessage polityFormationEventMessage = eventMessage as PolityFormationEventMessage;

			ShowEventMessageForPolity (eventMessage, polityFormationEventMessage.PolityId);

		} else if (eventMessage is PolityEventMessage) {

			PolityEventMessage polityEventMessage = eventMessage as PolityEventMessage;

			ShowEventMessageForPolity (eventMessage, polityEventMessage.PolityId);

		} else if (eventMessage is DiscoveryEventMessage) {

			DiscoveryEventMessage discoveryEventMessage = eventMessage as DiscoveryEventMessage;

			EventPanelScript.AddEventMessage (GetMessageToShow (discoveryEventMessage), () => {

				SelectAndCenterOnCell (discoveryEventMessage.Position);

				SetPopCulturalDiscoveryOverlay (discoveryEventMessage.DiscoveryId);
			});
		} else if (eventMessage is CellEventMessage) {
		
			CellEventMessage cellEventMessage = eventMessage as CellEventMessage;

			EventPanelScript.AddEventMessage (GetMessageToShow (cellEventMessage), () => {

				SelectAndCenterOnCell (cellEventMessage.Position);
			});
		} else {

			EventPanelScript.AddEventMessage (GetMessageToShow (eventMessage));
		}
	}

	public void ProgressUpdate (float value, string message = null, bool reset = false) {
		
		if (reset || (value >= _progressValue)) {
			
			if (message != null) 
				_progressMessage = message;
			
			_progressValue = value;
		}
	}

	public void MenuUninterruptSimulation () {

		if (!_eventPauseActive) {
			InterruptSimulation (false);
		}

		ShowHiddenInteractionPanels ();
	}
	
	public void CloseMainMenu () {
		
		MainMenuDialogPanelScript.SetVisible (false);
		
		MenuUninterruptSimulation ();
	}

	public void CloseSettingsDialog () {

		SettingsDialogPanelScript.SetVisible (false);

		MenuUninterruptSimulation ();
	}

	public void CloseCreditsDialog () {

		CreditsDialogPanelScript.SetVisible (false);

		MenuUninterruptSimulation ();
	}
	
	public void CloseOptionsMenu () {
		
		OptionsDialogPanelScript.SetVisible (false);

		MenuUninterruptSimulation ();
	}
	
	public void Exit () {
		
		Application.Quit();
	}

	public void OpenSettingsDialog () {

		MainMenuDialogPanelScript.SetVisible (false);

		SettingsDialogPanelScript.FullscreenToggle.isOn = Manager.IsFullscreen;

		SettingsDialogPanelScript.SetVisible (true);

		InterruptSimulation (true);
	}

	public void OpenCreditsDialog () {

		MainMenuDialogPanelScript.SetVisible (false);

		CreditsDialogPanelScript.SetVisible (true);

		InterruptSimulation (true);
	}

	public void ToogleFullscreen (bool state) {
	
		Manager.SetFullscreen (state);
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

		MenuUninterruptSimulation ();
	}
	
	public void CloseErrorMessageAction () {
		
		ErrorMessageDialogPanelScript.SetVisible (false);

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
			
			ErrorMessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			ErrorMessageDialogPanelScript.SetVisible (true);
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
			
			ErrorMessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			ErrorMessageDialogPanelScript.SetVisible (true);
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

		_selectedMaxSpeedLevelIndex = _topMaxSpeedLevelIndex;

		SetMaxSpeedLevel (_selectedMaxSpeedLevelIndex);
		
		_postProgressOp -= PostProgressOp_GenerateWorld;
	}
	
	private void GenerateWorldInternal (int seed) {
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Generating World...", true);
		
		Manager.GenerateNewWorldAsync (seed, ProgressUpdate);

		_postProgressOp += PostProgressOp_GenerateWorld;
		
		_backgroundProcessActive = true;
		
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

		MenuUninterruptSimulation ();
		
		DisplayTip_MapScroll ();
	}

	public void ClickOp_SelectCell (Vector2 position) {

		Vector2 mapCoordinates;

		if (!GetMapCoordinatesFromPointerPosition (position, out mapCoordinates))
			return;

		int longitude = (int)mapCoordinates.x;
		int latitude = (int)mapCoordinates.y;

		Manager.SetSelectedCell (longitude, latitude);

		MapEntitySelected.Invoke ();
	}

	public void ClickOp_SelectPopulationPlacement (Vector2 position) {

		int population = AddPopulationDialogScript.Population;

		Vector2 point;
		
		if (GetMapCoordinatesFromPointerPosition (out point)) {
			if (AddPopulationGroupAtPosition (point, population)) {

				MenuUninterruptSimulation ();
				
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

		_postProgressOp += PostProgressOp_ExportAction;
		
		_backgroundProcessActive = true;
	}
	
	public void CancelExportAction () {
		
		ExportMapDialogPanelScript.SetVisible (false);

		MenuUninterruptSimulation ();
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
		case PlanetOverlay.General: 
			planetOverlayStr = "_general"; 
			break;
		case PlanetOverlay.PopDensity: 
			planetOverlayStr = "_population_density"; 
			break;
		case PlanetOverlay.FarmlandDistribution: 
			planetOverlayStr = "_farmland_distribution"; 
			break;
		case PlanetOverlay.PopCulturalPreference: 
			planetOverlayStr = "_population_cultural_preference_" + _planetOverlaySubtype; 
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
		case PlanetOverlay.FactionCoreDistance: 
			planetOverlayStr = "_faction_core_distances"; 
			break;
		case PlanetOverlay.Language: 
			planetOverlayStr = "_languages"; 
			break;
		case PlanetOverlay.PolityProminence: 
			planetOverlayStr = "_polity_prominences"; 
			break;
		case PlanetOverlay.PolityContacts: 
			planetOverlayStr = "_polity_contacts"; 
			break;
		case PlanetOverlay.PolityCulturalPreference: 
			planetOverlayStr = "_polity_cultural_preference_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.PolityCulturalActivity: 
			planetOverlayStr = "_polity_cultural_activity_" + _planetOverlaySubtype; 
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

		ExportMapDialogPanelScript.SetName (Manager.AddDateToWorldName(Manager.WorldName) + planetViewStr + planetOverlayStr);
		
		ExportMapDialogPanelScript.SetVisible (true);
	}

	public void PostProgressOp_SaveAction () {

		LoadButton.interactable = HasFilesToLoad ();
		
		_postProgressOp -= PostProgressOp_SaveAction;

		if (!_eventPauseActive) {
			InterruptSimulation (!Manager.SimulationCanRun);
		}

		ShowHiddenInteractionPanels ();
	}

	public void PostProgressOp_ExportAction () {

		_postProgressOp -= PostProgressOp_ExportAction;

		if (!_eventPauseActive) {
			InterruptSimulation (!Manager.SimulationCanRun);
		}

		ShowHiddenInteractionPanels ();
	}

	public void SaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);
		
		ActivityDialogPanelScript.SetVisible (true);
		
		ActivityDialogPanelScript.SetDialogText ("Saving World...");

		string saveName = SaveFileDialogPanelScript.GetName ();
		
		Manager.WorldName = Manager.RemoveDateFromWorldName(saveName);
		
		string path = Manager.SavePath + saveName + ".plnt";

		Manager.SaveWorldAsync (path);

		_postProgressOp += PostProgressOp_SaveAction;
		
		_backgroundProcessActive = true;
	}

	public void CancelSaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);

		MenuUninterruptSimulation ();
	}

	public void SaveWorldAs () {

		MainMenuDialogPanelScript.SetVisible (false);

		SaveFileDialogPanelScript.SetName (Manager.AddDateToWorldName(Manager.WorldName));
		
		SaveFileDialogPanelScript.SetVisible (true);

		InterruptSimulation (true);
	}

	public void GetMaxSpeedOptionFromCurrentWorld () {

		long maxSpeed = Manager.CurrentWorld.MaxTimeToSkip;

		for (int i = 0; i < Speed.Levels.Length; i++) {

			if (maxSpeed <= Speed.Levels [i]) {

				_selectedMaxSpeedLevelIndex = i;

				SetMaxSpeedLevel (_selectedMaxSpeedLevelIndex);

				break;
			}
		}
	}

	public void IncreaseMaxSpeed () {

		if (_pauseButtonPressed) {
			return;
		}
	
		if (_selectedMaxSpeedLevelIndex == _topMaxSpeedLevelIndex)
			return;

		_selectedMaxSpeedLevelIndex++;

		SetMaxSpeedLevel (_selectedMaxSpeedLevelIndex);
	}

	public void DecreaseMaxSpeed () {

		if (_pauseButtonPressed) {
			return;
		}

		if (_selectedMaxSpeedLevelIndex == 0)
			return;

		_selectedMaxSpeedLevelIndex--;

		SetMaxSpeedLevel (_selectedMaxSpeedLevelIndex);
	}

	public void SetMaxSpeedLevel (int speedLevelIndex) {

		_selectedMaxSpeedLevelIndex = Mathf.Clamp (speedLevelIndex, 0, _topMaxSpeedLevelIndex);

		OnFirstMaxSpeedOptionSet.Invoke (_pausingDialogActive || (_selectedMaxSpeedLevelIndex == 0));
		OnLastMaxSpeedOptionSet.Invoke (_pausingDialogActive || (_selectedMaxSpeedLevelIndex == _topMaxSpeedLevelIndex));

		// This is the max amount of iterations to simulate per second
		Speed selectedSpeed = Speed.Levels [_selectedMaxSpeedLevelIndex];

		// This is the max amount of iterations to simulate per frame
		int maxSpeed = (int)Mathf.Ceil(selectedSpeed * MaxDeltaTimeIterations);

		Manager.CurrentWorld.SetMaxTimeToSkip (maxSpeed);

		OnSimulationSpeedChanged.Invoke (selectedSpeed);

		ResetAccDeltaTime ();
	}

	private void ResetAccDeltaTime () {

		_accDeltaTime = 0;
		_simulationDateSpan = 0;
	}

	public void PostProgressOp_LoadAction () {

		// TODO: delete next commented line
//		ResetUIElements ();
		
		SelectionPanelScript.RemoveAllOptions ();
		
		if (!Manager.SimulationCanRun) {
			
			SetInitialPopulation ();

		} else {

			MenuUninterruptSimulation ();
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
		
		Manager.WorldName = Manager.RemoveDateFromWorldName(Path.GetFileNameWithoutExtension (path));
		
		_postProgressOp += PostProgressOp_LoadAction;
		
		_backgroundProcessActive = true;
		
		_regenTextures = true;
	}

	public void CancelLoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);

		MenuUninterruptSimulation ();
	}
	
	public void LoadWorld () {

		MainMenuDialogPanelScript.SetVisible (false);
		
		LoadFileDialogPanelScript.SetVisible (true);

		InterruptSimulation (true);
	}

	public bool AreBackgroundActivityPanelsActive () {

		GameObject[] panels = GameObject.FindGameObjectsWithTag ("BackgroundActivityPanel");

		foreach (GameObject panel in panels) {

			if (panel.activeInHierarchy) {

				return true;
			}
		}

		return false;
	}

	public bool AreMenuPanelsActive () {

		GameObject[] panels = GameObject.FindGameObjectsWithTag ("MenuPanel");

		foreach (GameObject panel in panels) {
		
			if (panel.activeInHierarchy) {
			
				return true;
			}
		}

		return false;
	}

	public bool AreInteractionPanelsActive () {

		GameObject[] panels = GameObject.FindGameObjectsWithTag ("InteractionPanel");

		foreach (GameObject panel in panels) {

			if (panel.activeInHierarchy) {

				return true;
			}
		}

		return false;
	}

	public void ShowHiddenInteractionPanels () {
	
		foreach (ModalPanelScript panel in HiddenInteractionPanels) {

			panel.SetVisible (true);
		}

		HiddenInteractionPanels.Clear ();
	}

	public void RequestDecisionResolution () {

		Decision decisionToResolve = Manager.CurrentWorld.PullDecisionToResolve ();

		DecisionDialogPanelScript.Set (decisionToResolve, _selectedMaxSpeedLevelIndex);

		if (!AreMenuPanelsActive ()) {
			
			DecisionDialogPanelScript.SetVisible (true);

		} else {

			HiddenInteractionPanels.Add (DecisionDialogPanelScript);
		}

		InterruptSimulation (true);

		_eventPauseActive = true;
	}

	public void ResolveDecision () {

		DecisionDialogPanelScript.SetVisible (false);

		int resumeSpeedLevelIndex = DecisionDialogPanelScript.ResumeSpeedLevelIndex;

		if (resumeSpeedLevelIndex == -1) {

			PauseSimulation (true);

		} else {

			_selectedMaxSpeedLevelIndex = resumeSpeedLevelIndex;

			SetMaxSpeedLevel (_selectedMaxSpeedLevelIndex);
		}

		InterruptSimulation (false);

		_eventPauseActive = false;

		_resolvedDecision = true;
	}

	public void ChangePlanetOverlayToSelected () {

		SelectionPanelScript.RemoveAllOptions ();
		SelectionPanelScript.SetVisible (false);

		if (OverlayDialogPanelScript.DontUpdateDialog)
			return;

		OverlayDialogPanelScript.ResetToggles ();

		if (OverlayDialogPanelScript.GeneralDataToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.General, false);
		} else if (OverlayDialogPanelScript.PopDensityToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PopDensity, false);
		} else if (OverlayDialogPanelScript.FarmlandToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.FarmlandDistribution, false);
		} else if (OverlayDialogPanelScript.PopCulturalPreferenceToggle.isOn) {
			SetPopCulturalPreferenceOverlay (false);
		} else if (OverlayDialogPanelScript.PopCulturalActivityToggle.isOn) {
			SetPopCulturalActivityOverlay (false);
		} else if (OverlayDialogPanelScript.PopCulturalSkillToggle.isOn) {
			SetPopCulturalSkillOverlay (false);
		} else if (OverlayDialogPanelScript.PopCulturalKnowledgeToggle.isOn) {
			SetPopCulturalKnowledgeOverlay (false);
		} else if (OverlayDialogPanelScript.PopCulturalDiscoveryToggle.isOn) {
			SetPopCulturalDiscoveryOverlay (false);
		} else if (OverlayDialogPanelScript.TerritoriesToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PolityTerritory, false);
		} else if (OverlayDialogPanelScript.DistancesToCoresToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.FactionCoreDistance, false);
		} else if (OverlayDialogPanelScript.ProminenceToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PolityProminence, false);
		} else if (OverlayDialogPanelScript.ContactsToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PolityContacts, false);
		} else if (OverlayDialogPanelScript.PolityCulturalPreferenceToggle.isOn) {
			SetPolityCulturalPreferenceOverlay (false);
		} else if (OverlayDialogPanelScript.PolityCulturalActivityToggle.isOn) {
			SetPolityCulturalActivityOverlay (false);
		} else if (OverlayDialogPanelScript.PolityCulturalSkillToggle.isOn) {
			SetPolityCulturalSkillOverlay (false);
		} else if (OverlayDialogPanelScript.PolityCulturalKnowledgeToggle.isOn) {
			SetPolityCulturalKnowledgeOverlay (false);
		} else if (OverlayDialogPanelScript.PolityCulturalDiscoveryToggle.isOn) {
			SetPolityCulturalDiscoveryOverlay (false);
		} else if (OverlayDialogPanelScript.TemperatureToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Temperature, false);
		} else if (OverlayDialogPanelScript.RainfallToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Rainfall, false);
		} else if (OverlayDialogPanelScript.ArabilityToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Arability, false);
		} else if (OverlayDialogPanelScript.RegionToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Region, false);
		} else if (OverlayDialogPanelScript.LanguageToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.Language, false);
		} else if (OverlayDialogPanelScript.PopChangeToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.PopChange, false);
		} else if (OverlayDialogPanelScript.UpdateSpanToggle.isOn) {
			ChangePlanetOverlay (PlanetOverlay.UpdateSpan, false);
		} else {
			ChangePlanetOverlay (PlanetOverlay.None, false);
		}

		SetRouteDisplayOverlay (OverlayDialogPanelScript.DisplayRoutesToggle.isOn, false);
		SetGroupActivityOverlay (OverlayDialogPanelScript.DisplayGroupActivityToggle.isOn, false);
	}
	
	public void CloseOverlayMenuAction () {
		
		OverlayDialogPanelScript.SetVisible (false);

		ShowHiddenInteractionPanels ();
	}

	public void CloseViewsMenuAction () {

		ViewsDialogPanelScript.SetVisible (false);

		ShowHiddenInteractionPanels ();
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

		InterruptSimulation (true);
	}

	public void SetSimulationSpeedStopped (bool state) {

		if (state) {
			OnSimulationSpeedChanged.Invoke (Speed.Zero);
		} else {
			OnSimulationSpeedChanged.Invoke (Speed.Levels[_selectedMaxSpeedLevelIndex]);
		}
	}

	public void MinimizeInfoText (bool state) {

		_infoTextMinimized = state;
	}

	public void PauseSimulation (bool state) {

		OnSimulationPaused.Invoke (state);

		_pauseButtonPressed = state;

		bool holdState = _pauseButtonPressed;

		HoldSimulation (holdState);
	}

	public void InterruptSimulation (bool state) {

		_pausingDialogActive = state;

		OnSimulationInterrupted.Invoke (state);

		bool holdState = _pausingDialogActive || _pauseButtonPressed;

		HoldSimulation (holdState);
	}

	private void HoldSimulation (bool state) {

		SetSimulationSpeedStopped (state);

		OnFirstMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedLevelIndex == 0));
		OnLastMaxSpeedOptionSet.Invoke (state || (_selectedMaxSpeedLevelIndex == _topMaxSpeedLevelIndex));

		Manager.InterruptSimulation (state);

		ResetAccDeltaTime ();
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

	public void SetRouteDisplayOverlay (bool value, bool invokeEvent = true) {

		_regenTextures |= _displayRoutes != value;

		_displayRoutes = value;

		if (_regenTextures) {
			Manager.SetDisplayRoutes (_displayRoutes);

			if (invokeEvent) {
				OverlayChanged.Invoke ();
			}
		}
	}

	public void SetGroupActivityOverlay (bool value, bool invokeEvent = true) {

		_regenTextures |= _displayGroupActivity != value;

		_displayGroupActivity = value;

		if (_regenTextures) {
			Manager.SetDisplayGroupActivity (_displayGroupActivity);

			if (invokeEvent) {
				OverlayChanged.Invoke ();
			}
		}
	}

	public void ChangePlanetOverlay (PlanetOverlay value, string planetOverlaySubtype, bool invokeEvent = true) {

		_regenTextures |= _planetOverlaySubtype != planetOverlaySubtype;
		_regenTextures |= _planetOverlay != value;

		if ((_planetOverlay != value) && (_planetOverlay != PlanetOverlay.None)) {

			_planetOverlaySubtypeCache[_planetOverlay] = _planetOverlaySubtype;
		}

		_planetOverlaySubtype = planetOverlaySubtype;

		_planetOverlay = value;

		if (invokeEvent) {
			Manager.SetPlanetOverlay (_planetOverlay, _planetOverlaySubtype);

			OverlayChanged.Invoke ();
		}
	}

	public void ChangePlanetOverlay (PlanetOverlay value, bool invokeEvent = true) {

		_regenTextures |= _planetOverlay != value;

		if (_regenTextures && (_planetOverlay != PlanetOverlay.None)) {

			_planetOverlaySubtypeCache[_planetOverlay] = _planetOverlaySubtype;
		}

		_planetOverlay = value;

		if (!_planetOverlaySubtypeCache.TryGetValue (_planetOverlay, out _planetOverlaySubtype)) {
		
			_planetOverlaySubtype = "None";
		}

		if (invokeEvent) {
			Manager.SetPlanetOverlay (_planetOverlay, _planetOverlaySubtype);

			OverlayChanged.Invoke ();
		}
	}

	public void SetPopCulturalPreferenceOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalPreference, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Preference:";

		foreach (CulturalPreferenceInfo preferenceInfo in Manager.CurrentWorld.CulturalPreferenceInfoList) {

			AddSelectionPanelOption (preferenceInfo.Name, preferenceInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalPreferenceOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalPreference, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Preference:";

		foreach (CulturalPreferenceInfo preferenceInfo in Manager.CurrentWorld.CulturalPreferenceInfoList) {

			AddSelectionPanelOption (preferenceInfo.Name, preferenceInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPopCulturalActivityOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalActivity, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Activity:";

		foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList) {

			AddSelectionPanelOption (activityInfo.Name, activityInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalActivityOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalActivity, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Activity:";

		foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList) {

			AddSelectionPanelOption (activityInfo.Name, activityInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}
	
	public void SetPopCulturalSkillOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalSkill, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Skill:";

		foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {

			AddSelectionPanelOption (skillInfo.Name, skillInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalSkillOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalSkill, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Skill:";

		foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {

			AddSelectionPanelOption (skillInfo.Name, skillInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}
	
	public void SetPopCulturalKnowledgeOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalKnowledge, invokeEvent);
		
		SelectionPanelScript.Title.text = "Displayed Knowledge:";
		
		foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {

			AddSelectionPanelOption (knowledgeInfo.Name, knowledgeInfo.Id);
		}
		
		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalKnowledgeOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalKnowledge, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Knowledge:";

		foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {

			AddSelectionPanelOption (knowledgeInfo.Name, knowledgeInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPopCulturalDiscoveryOverlay (string planetOverlaySubtype, bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalDiscovery, planetOverlaySubtype, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Discovery:";

		foreach (CulturalDiscovery discoveryInfo in Manager.CurrentWorld.CulturalDiscoveryInfoList) {

			AddSelectionPanelOption (discoveryInfo.Name, discoveryInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPopCulturalDiscoveryOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PopCulturalDiscovery, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Discovery:";

		foreach (CulturalDiscovery discoveryInfo in Manager.CurrentWorld.CulturalDiscoveryInfoList) {

			AddSelectionPanelOption (discoveryInfo.Name, discoveryInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void SetPolityCulturalDiscoveryOverlay (bool invokeEvent = true) {

		ChangePlanetOverlay (PlanetOverlay.PolityCulturalDiscovery, invokeEvent);

		SelectionPanelScript.Title.text = "Displayed Discovery:";

		foreach (CulturalDiscovery discoveryInfo in Manager.CurrentWorld.CulturalDiscoveryInfoList) {

			AddSelectionPanelOption (discoveryInfo.Name, discoveryInfo.Id);
		}

		SelectionPanelScript.SetVisible (true);
	}

	public void AddSelectionPanelOption (string optionName, string optionId) {

		SelectionPanelScript.AddOption (optionId, optionName, (state) => {
			if (state) {
				_planetOverlaySubtype = optionId;
			} else if (_planetOverlaySubtype == optionId) {
				_planetOverlaySubtype = "None";
			}

			_regenTextures = true;
		});

		if (_planetOverlaySubtype == optionId) {
			SelectionPanelScript.SetStateOption (optionId, true);
		}
	}

	public void UpdateSelectionMenu () {

		if (!SelectionPanelScript.IsVisible ())
			return;

		if (_planetOverlay == PlanetOverlay.PopCulturalPreference) {

			foreach (CulturalPreferenceInfo preferenceInfo in Manager.CurrentWorld.CulturalPreferenceInfoList) {

				AddSelectionPanelOption (preferenceInfo.Name, preferenceInfo.Id);
			}
		} else if (_planetOverlay == PlanetOverlay.PopCulturalActivity) {

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
		} else if (_planetOverlay == PlanetOverlay.PolityCulturalPreference) {

			foreach (CulturalPreferenceInfo preferenceInfo in Manager.CurrentWorld.CulturalPreferenceInfoList) {

				AddSelectionPanelOption (preferenceInfo.Name, preferenceInfo.Id);
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
	
	public void SetBiomeView () {
		
		_regenTextures |= _planetView != PlanetView.Biomes;
		
		_planetView = PlanetView.Biomes;
		
		ViewsDialogPanelScript.SetVisible (false);

		ShowHiddenInteractionPanels ();
	}
	
	public void SetElevationView () {
		
		_regenTextures |= _planetView != PlanetView.Elevation;
		
		_planetView = PlanetView.Elevation;
		
		ViewsDialogPanelScript.SetVisible (false);

		ShowHiddenInteractionPanels ();
	}
	
	public void SetCoastlineView () {
		
		_regenTextures |= _planetView != PlanetView.Coastlines;
		
		_planetView = PlanetView.Coastlines;
		
		ViewsDialogPanelScript.SetVisible (false);

		ShowHiddenInteractionPanels ();
	}

	public void OpenSelectFactionDialog () {

		SelectFactionDialogPanelScript.SetVisible (true);

		InterruptSimulation (true);
	}

	public void SetFactionToGuideAction() {

		SelectFactionDialogPanelScript.SetVisible (false);

		Faction faction = SelectFactionDialogPanelScript.ChosenFaction;

		if (faction != null) {
			Manager.SetGuidedFaction (faction);
		}

		MenuUninterruptSimulation ();
	}

	public void StopGuidingFaction() {

		Manager.SetGuidedFaction (null);
	}

	public void CancelSelectFaction () {

		SelectFactionDialogPanelScript.SetVisible (false);

		MenuUninterruptSimulation ();
	}

	public void SetPlayerFocusOnPolity () {
	
		Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

		if ((selectedTerritory != null) && !selectedTerritory.Polity.IsUnderPlayerFocus)
			Manager.SetFocusOnPolity (selectedTerritory.Polity);
	}

	public void UnsetPlayerFocusOnPolity () {

		Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

		if ((selectedTerritory != null) && selectedTerritory.Polity.IsUnderPlayerFocus)
			Manager.UnsetFocusOnPolity (selectedTerritory.Polity);
	}

	public void UpdateInfoPanel () {

//		_showFocusButton = false;
//		_focusButtonText = "";
		
		World world = Manager.CurrentWorld;
		
		InfoPanelScript.InfoText.text = Manager.GetDateString (world.CurrentDate);

		if (_infoTextMinimized)
			return;

		if (Manager.CurrentWorld.SelectedCell != null) {
			AddCellDataToInfoPanel (Manager.CurrentWorld.SelectedCell);
		}

		InfoPanelScript.InfoText.text += "\n";

		#if DEBUG
		InfoPanelScript.InfoText.text += "\n -- Debug Data -- ";
		
		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nNumber of Migration Events: " + MigrateGroupEvent.MigrationEventCount;
		
		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nMUPS: " + _lastMapUpdateCount;
		InfoPanelScript.InfoText.text += "\n";
		#endif

//		InfoPanelScript.ShowFocusButton (_showFocusButton);
//		InfoPanelScript.FocusButtonText.text = _focusButtonText;
	}
	
	public void AddCellDataToInfoPanel (int longitude, int latitude) {
		
		TerrainCell cell = Manager.CurrentWorld.GetCell (longitude, latitude);
		
		if (cell == null) return;

		AddCellDataToInfoPanel (cell);
	}

	public void AddCellDataToInfoPanel_Terrain (TerrainCell cell) {
		
		float cellArea = cell.Area;

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Cell Terrain Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		InfoPanelScript.InfoText.text += "\nArea: " + cellArea + " Km^2";
		InfoPanelScript.InfoText.text += "\nAltitude: " + cell.Altitude + " meters";
		InfoPanelScript.InfoText.text += "\nRainfall: " + cell.Rainfall + " mm / year";
		InfoPanelScript.InfoText.text += "\nTemperature: " + cell.Temperature + " C";
		InfoPanelScript.InfoText.text += "\n";

		for (int i = 0; i < cell.PresentBiomeNames.Count; i++) {
			float percentage = cell.BiomePresences [i];

			InfoPanelScript.InfoText.text += "\nBiome: " + cell.PresentBiomeNames [i];
			InfoPanelScript.InfoText.text += " (" + percentage.ToString ("P") + ")";
		}

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nSurvivability: " + cell.Survivability.ToString ("P");
		InfoPanelScript.InfoText.text += "\nForaging Capacity: " + cell.ForagingCapacity.ToString ("P");
		InfoPanelScript.InfoText.text += "\nAccessibility: " + cell.Accessibility.ToString ("P");
		InfoPanelScript.InfoText.text += "\nArability: " + cell.Arability.ToString ("P");
		InfoPanelScript.InfoText.text += "\n";

		Region region = cell.Region;

		if (region == null) {
			InfoPanelScript.InfoText.text += "\nCell doesn't belong to any known region";
		} else {
			InfoPanelScript.InfoText.text += "\nCell is part of Region #" + region.Id + ": " + region.Name;
		}
	}

	public void AddCellDataToInfoPanel_Region (TerrainCell cell) {

		Region region = cell.Region;

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Region Terrain Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (region == null) {
			InfoPanelScript.InfoText.text += "\nCell doesn't belong to any known region";

			return;
		} else {
			InfoPanelScript.InfoText.text += "\nRegion #" + region.Id + ": " + region.Name;
		}
		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nAttributes: ";

		bool first = true;
		foreach (RegionAttribute attr in region.Attributes) {

			if (first) {
				InfoPanelScript.InfoText.text += attr.Name;
				first = false;
			} else {
				InfoPanelScript.InfoText.text += ", " + attr.Name;
			}
		}

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nTotal Area: " + region.TotalArea + " Km^2";

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nCoast Percentage: " + region.CoastPercentage.ToString ("P");
		InfoPanelScript.InfoText.text += "\nOcean Percentage: " + region.OceanPercentage.ToString ("P");

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nAverage Altitude: " + region.AverageAltitude + " meters";
		InfoPanelScript.InfoText.text += "\nAverage Rainfall: " + region.AverageRainfall + " mm / year";
		InfoPanelScript.InfoText.text += "\nAverage Temperature: " + region.AverageTemperature + " C";
		InfoPanelScript.InfoText.text += "\n";

		InfoPanelScript.InfoText.text += "\nMin Region Altitude: " + region.MinAltitude + " meters";
		InfoPanelScript.InfoText.text += "\nMax Region Altitude: " + region.MaxAltitude + " meters";
		InfoPanelScript.InfoText.text += "\nAverage Border Altitude: " + region.AverageOuterBorderAltitude + " meters";
		InfoPanelScript.InfoText.text += "\n";

		for (int i = 0; i < region.PresentBiomeNames.Count; i++) {
			float percentage = region.BiomePresences [i];

			InfoPanelScript.InfoText.text += "\nBiome: " + region.PresentBiomeNames [i];
			InfoPanelScript.InfoText.text += " (" + percentage.ToString ("P") + ")";
		}

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nAverage Survivability: " + region.AverageSurvivability.ToString ("P");
		InfoPanelScript.InfoText.text += "\nAverage Foraging Capacity: " + region.AverageForagingCapacity.ToString ("P");
		InfoPanelScript.InfoText.text += "\nAverage Accessibility: " + region.AverageAccessibility.ToString ("P");
		InfoPanelScript.InfoText.text += "\nAverage Arability: " + region.AverageArability.ToString ("P");
	}

	public void AddCellDataToInfoPanel_FarmlandDistribution (TerrainCell cell) {

		float cellArea = cell.Area;
		float farmlandPercentage = cell.FarmlandPercentage;

		if (farmlandPercentage > 0) {

			InfoPanelScript.InfoText.text += "\n";
			InfoPanelScript.InfoText.text += "\n -- Cell Farmland Distribution Data -- ";
			InfoPanelScript.InfoText.text += "\n";

			InfoPanelScript.InfoText.text += "\nFarmland Percentage: " + farmlandPercentage.ToString ("P");
			InfoPanelScript.InfoText.text += "\n";
		}

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		if (cell.FarmlandPercentage > 0) {

			float farmlandArea = farmlandPercentage * cellArea;

			InfoPanelScript.InfoText.text += "\nFarmland Area per Pop: " + (farmlandArea / (float)population).ToString ("0.000") + " Km^2 / Pop";
		}
	}

	public void AddCellDataToInfoPanel_PopDensity (TerrainCell cell) {

		float cellArea = cell.Area;

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Population Density Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int optimalPopulation = cell.Group.OptimalPopulation;

		InfoPanelScript.InfoText.text += "\nPopulation: " + population;
		InfoPanelScript.InfoText.text += "\nPrevious Population: " + cell.Group.PreviousPopulation;
		InfoPanelScript.InfoText.text += "\nOptimal Population: " + optimalPopulation;
		InfoPanelScript.InfoText.text += "\nPop Density: " + (population / cellArea).ToString ("0.000") + " Pop / Km^2";

		float modifiedSurvivability = 0;
		float modifiedForagingCapacity = 0;

		cell.Group.CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\nModified Survivability: " + modifiedSurvivability.ToString ("P");
		InfoPanelScript.InfoText.text += "\nModified Foraging Capacity: " + modifiedForagingCapacity.ToString ("P");
	}

	public void AddCellDataToInfoPanel_Language (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Language Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		Language groupLanguage = cell.Group.Culture.Language;

		if (groupLanguage == null) {

			InfoPanelScript.InfoText.text += "\n\tNo major language spoken at location";

			return;
		}

		InfoPanelScript.InfoText.text += "\n\tPredominant language at location: " + groupLanguage.Id;
	}

	public void AddCellDataToInfoPanel_UpdateSpan (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Update Span Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		long lastUpdateDate = cell.Group.LastUpdateDate;
		long nextUpdateDate = cell.Group.NextUpdateDate;

		InfoPanelScript.InfoText.text += "\nLast Update Date: " + Manager.GetDateString(lastUpdateDate);
		InfoPanelScript.InfoText.text += "\nNext Update Date: " + Manager.GetDateString(nextUpdateDate);
		InfoPanelScript.InfoText.text += "\nTime between updates: " + Manager.GetTimeSpanString(nextUpdateDate - lastUpdateDate);
	}

	public void AddCellDataToInfoPanel_PolityProminence (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Polity Prominence Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		bool firstPolity = true;

		List<PolityProminence> polityProminences = new List<PolityProminence>(cell.Group.PolityProminences.Values);

		polityProminences.Sort ((a, b) => {
			if (a.Value > b.Value) return -1;
			if (a.Value < b.Value) return 1;
			return 0;
		});

		foreach (PolityProminence polityProminence in polityProminences) {

			Polity polity = polityProminence.Polity;
			float prominenceValue = polityProminence.Value;
			float factionCoreDistance = polityProminence.FactionCoreDistance;
			float polityCoreDistance = polityProminence.PolityCoreDistance;
			float administrativeCost = polityProminence.AdministrativeCost;

			if (prominenceValue >= 0.001) {

				if (firstPolity) {
					InfoPanelScript.InfoText.text += "\nPolities:";

					firstPolity = false;
				}

				InfoPanelScript.InfoText.text += "\n\tPolity: " + polity.Name.Text +
					"\n\t\tProminence: " + prominenceValue.ToString ("P") +
					"\n\t\tDistance to Polity Core: " + polityCoreDistance.ToString ("0.000") +
					"\n\t\tDistance to Faction Core: " + factionCoreDistance.ToString ("0.000") +
					"\n\t\tAdministrative Cost: " + administrativeCost.ToString ("0.000");
			}
		}
	}

	public void AddCellDataToInfoPanel_PolityContacts (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Contacts Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		Territory territory = cell.EncompassingTerritory;


		if (territory == null) {
			InfoPanelScript.InfoText.text += "\n\tGroup not part of a polity's territory";
			return;
		}

		Polity polity = territory.Polity;

		InfoPanelScript.InfoText.text += "\nTerritory of the " + polity.Name.Text + " " + polity.Type.ToLower ();
		InfoPanelScript.InfoText.text += "\nTranslates to: " + polity.Name.Meaning;
		InfoPanelScript.InfoText.text += "\n";

		if (polity.Contacts.Count <= 0) {

			InfoPanelScript.InfoText.text += "\nPolity has no contact with other polities...";
		} else {

			InfoPanelScript.InfoText.text += "\nPolities in contact:";
		}

		foreach (PolityContact contact in polity.Contacts.Values)
        {
			Polity contactPolity = contact.Polity;

			InfoPanelScript.InfoText.text += "\n\n\tPolity: " + contactPolity.Name.Text + " " + contactPolity.Type.ToLower ();
//			InfoPanelScript.InfoText.text += "\n\tTranslates to: " + contactPolity.Name.Meaning;

			Faction dominantFaction = contactPolity.DominantFaction;

			InfoPanelScript.InfoText.text += "\n\tDominant Faction: " + dominantFaction.Type + " " + dominantFaction.Name;
//			InfoPanelScript.InfoText.text += "\n\t\tTranslates to: " + dominantFaction.Name.Meaning;

			Agent leader = contactPolity.CurrentLeader;

			InfoPanelScript.InfoText.text += "\n\tLeader: " + leader.Name.Text;
//			InfoPanelScript.InfoText.text += "\n\tTranslates to: " + leader.Name.Meaning;

			InfoPanelScript.InfoText.text += "\n\tContact Strength: " + contact.GroupCount;
		}
	}

	public void AddCellDataToInfoPanel_General (TerrainCell cell) {
		
		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "Uninhabited land";

			return;
		}

		int cellPopulation = cell.Group.Population;

		if (cellPopulation <= 0) {

			InfoPanelScript.InfoText.text += "Group has zero population";
			Debug.LogError ("Group has zero or less population: " + cellPopulation);

			return;
		}

		Territory territory = cell.EncompassingTerritory;

		if (territory == null) {
			
			InfoPanelScript.InfoText.text += "Disorganized bands";
			InfoPanelScript.InfoText.text += "\n";
			InfoPanelScript.InfoText.text += "\n";

			InfoPanelScript.InfoText.text += cellPopulation + " inhabitants in selected cell";
		
		} else {

			Polity polity = territory.Polity;

			InfoPanelScript.InfoText.text += "Territory of the " + polity.Name.Text + " " + polity.Type.ToLower ();
			InfoPanelScript.InfoText.text += "\nTranslates to: " + polity.Name.Meaning;
			InfoPanelScript.InfoText.text += "\n";

			Agent leader = polity.CurrentLeader;

			InfoPanelScript.InfoText.text += "\nLeader: " + leader.Name.Text;
			InfoPanelScript.InfoText.text += "\nTranslates to: " + leader.Name.Meaning;
			InfoPanelScript.InfoText.text += "\nBirth Date: " + Manager.GetDateString(leader.BirthDate);
			InfoPanelScript.InfoText.text += "\nAge: " + Manager.GetTimeSpanString(leader.Age);
			InfoPanelScript.InfoText.text += "\nGender: " + ((leader.IsFemale) ? "Female" : "Male");
			InfoPanelScript.InfoText.text += "\nCharisma: " + leader.Charisma;
			InfoPanelScript.InfoText.text += "\nWisdom: " + leader.Wisdom;
			InfoPanelScript.InfoText.text += "\n";
			InfoPanelScript.InfoText.text += "\n";

			int polPopulation = (int)polity.TotalPopulation;

			if (polity.Type == Tribe.TribeType) {
				InfoPanelScript.InfoText.text += polPopulation + " tribe members";
			} else {
				InfoPanelScript.InfoText.text += polPopulation + " polity citizens";
			}

//			SetFocusButton (polity);
		}
	}

	public void AddCellDataToInfoPanel_PolityTerritory (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Territory Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		Territory territory = cell.EncompassingTerritory;


		if (territory == null) {
			InfoPanelScript.InfoText.text += "\n\tGroup not part of a polity's territory";
			return;
		}

		Polity polity = territory.Polity;

		PolityProminence pi = cell.Group.GetPolityProminence (polity);

		InfoPanelScript.InfoText.text += "\nTerritory of the " + polity.Name.Text + " " + polity.Type.ToLower ();
		InfoPanelScript.InfoText.text += "\nTranslates to: " + polity.Name.Meaning;
		InfoPanelScript.InfoText.text += "\n";

		int totalPopulation = (int)Mathf.Floor (polity.TotalPopulation);

		InfoPanelScript.InfoText.text += "\n\tPolity population: " + totalPopulation;
		InfoPanelScript.InfoText.text += "\n";

		float administrativeCost = polity.TotalAdministrativeCost;

		InfoPanelScript.InfoText.text += "\n\tAdministrative Cost: " + administrativeCost;

		Agent leader = polity.CurrentLeader;

		InfoPanelScript.InfoText.text += "\nLeader: " + leader.Name.Text;
		InfoPanelScript.InfoText.text += "\nTranslates to: " + leader.Name.Meaning;
		InfoPanelScript.InfoText.text += "\nBirth Date: " + Manager.GetDateString(leader.BirthDate);
		InfoPanelScript.InfoText.text += "\nGender: " + ((leader.IsFemale) ? "Female" : "Male");
		InfoPanelScript.InfoText.text += "\nCharisma: " + leader.Charisma;
		InfoPanelScript.InfoText.text += "\nWisdom: " + leader.Wisdom;
		InfoPanelScript.InfoText.text += "\n";

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Factions -- ";
		InfoPanelScript.InfoText.text += "\n";

		List<Faction> factions = new List<Faction> (polity.GetFactions ());

		factions.Sort ((a, b) => {
			if (a.Influence > b.Influence)
				return -1;
			if (a.Influence < b.Influence)
				return 1;

			return 0;
		});

		foreach (Faction faction in factions) {

			InfoPanelScript.InfoText.text += "\n\t" + faction.Type + " " + faction.Name;
			InfoPanelScript.InfoText.text += "\n\t\tCore: " + faction.CoreGroup.Position;
			InfoPanelScript.InfoText.text += "\n\t\tInfluence: " + faction.Influence.ToString ("P");

			Agent factionLeader = faction.CurrentLeader;

			InfoPanelScript.InfoText.text += "\n\t\tLeader: " + factionLeader.Name.Text;
			InfoPanelScript.InfoText.text += "\n\t\tTranslates to: " + factionLeader.Name.Meaning;
			InfoPanelScript.InfoText.text += "\n\t\tBirth Date: " + Manager.GetDateString(factionLeader.BirthDate);
			InfoPanelScript.InfoText.text += "\n\t\tGender: " + ((factionLeader.IsFemale) ? "Female" : "Male");
			InfoPanelScript.InfoText.text += "\n\t\tCharisma: " + factionLeader.Charisma;
			InfoPanelScript.InfoText.text += "\n\t\tWisdom: " + factionLeader.Wisdom;
			InfoPanelScript.InfoText.text += "\n";
		}

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Selected Group's Polity Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		float percentageOfPopulation = cell.Group.GetPolityProminenceValue (polity);
		int prominencedPopulation = (int)Mathf.Floor (population * percentageOfPopulation);

		float percentageOfPolity = 1;

		if (totalPopulation > 0) {
			percentageOfPolity = prominencedPopulation / (float)totalPopulation;
		}

		InfoPanelScript.InfoText.text += "\n\tProminenced population: " + prominencedPopulation;
		InfoPanelScript.InfoText.text += "\n\tPercentage of polity population: " + percentageOfPolity.ToString ("P");
		InfoPanelScript.InfoText.text += "\n\tDistance to polity core: " + pi.PolityCoreDistance.ToString ("0.000");
		InfoPanelScript.InfoText.text += "\n\tDistance to faction core: " + pi.FactionCoreDistance.ToString ("0.000");

//		SetFocusButton (polity);
	}

//	private void SetFocusButton (Polity polity) {
//
//		if (!polity.IsUnderPlayerFocus) {
//			_showFocusButton = true;
//			_focusButtonText = "Set focus on " + polity.Name.Text;
//		}
//	}

	public void AddCellDataToInfoPanel_PolityCulturalPreference (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Preference Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		PolityProminence polityProminence = cell.Group.HighestPolityProminence;

		if (polityProminence == null) {

			InfoPanelScript.InfoText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstPreference = true;

		foreach (CulturalPreference preference in polityProminence.Polity.Culture.Preferences) {

			if (firstPreference) {
				InfoPanelScript.InfoText.text += "\nPreferences:";

				firstPreference = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + preference.Name + " Preference: " + preference.Value.ToString ("P");
		}
	}

	public void AddCellDataToInfoPanel_PolityCulturalActivity (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Activity Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		PolityProminence polityProminence = cell.Group.HighestPolityProminence;

		if (polityProminence == null) {

			InfoPanelScript.InfoText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstActivity = true;

		foreach (CulturalActivity activity in polityProminence.Polity.Culture.Activities) {

			if (firstActivity) {
				InfoPanelScript.InfoText.text += "\nActivities:";

				firstActivity = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + activity.Name + " Contribution: " + activity.Contribution.ToString ("P");
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalPreference (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Preference Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		bool firstPreference = true;

		foreach (CulturalPreference preference in cell.Group.Culture.Preferences) {

			if (firstPreference) {
				InfoPanelScript.InfoText.text += "\nPreferences:";

				firstPreference = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + preference.Name + " Preference: " + preference.Value.ToString ("P");
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalActivity (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Activity Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		bool firstActivity = true;

		foreach (CulturalActivity activity in cell.Group.Culture.Activities) {

			if (firstActivity) {
				InfoPanelScript.InfoText.text += "\nActivities:";

				firstActivity = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + activity.Name + " Contribution: " + activity.Contribution.ToString ("P");
		}
	}

	public void AddCellDataToInfoPanel_PolityCulturalSkill (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Skill Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		PolityProminence polityProminence = cell.Group.HighestPolityProminence;

		if (polityProminence == null) {

			InfoPanelScript.InfoText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstSkill = true;

		foreach (CulturalSkill skill in polityProminence.Polity.Culture.Skills) {

			float skillValue = skill.Value;

			if (skillValue >= 0.001) {

				if (firstSkill) {
					InfoPanelScript.InfoText.text += "\nSkills:";

					firstSkill = false;
				}

				InfoPanelScript.InfoText.text += "\n\t" + skill.Name + " Value: " + skill.Value.ToString ("0.000");
			}
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalSkill (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Skill Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		bool firstSkill = true;

		foreach (CulturalSkill skill in cell.Group.Culture.Skills) {

			float skillValue = skill.Value;

			if (skillValue >= 0.001) {

				if (firstSkill) {
					InfoPanelScript.InfoText.text += "\nSkills:";

					firstSkill = false;
				}

				InfoPanelScript.InfoText.text += "\n\t" + skill.Name + " Value: " + skill.Value.ToString ("0.000");
			}
		}
	}

	public void AddCellDataToInfoPanel_PolityCulturalKnowledge (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Knowledge Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		PolityProminence polityProminence = cell.Group.HighestPolityProminence;

		if (polityProminence == null) {

			InfoPanelScript.InfoText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstKnowledge = true;

		foreach (CulturalKnowledge knowledge in polityProminence.Polity.Culture.Knowledges) {

			float knowledgeValue = knowledge.ScaledValue;

			if (firstKnowledge) {
				InfoPanelScript.InfoText.text += "\nKnowledges:";

				firstKnowledge = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + knowledge.Name + " Value: " + knowledgeValue.ToString ("0.000");
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalKnowledge (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Knowledge Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		bool firstKnowledge = true;

		foreach (CulturalKnowledge knowledge in cell.Group.Culture.Knowledges) {

			float knowledgeValue = knowledge.ScaledValue;

			if (firstKnowledge) {
				InfoPanelScript.InfoText.text += "\nKnowledges:";

				firstKnowledge = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + knowledge.Name + " Value: " + knowledgeValue.ToString ("0.000");
		}
	}

	public void AddCellDataToInfoPanel_PolityCulturalDiscovery (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Polity Discovery Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		PolityProminence polityProminence = cell.Group.HighestPolityProminence;

		if (polityProminence == null) {

			InfoPanelScript.InfoText.text += "\n\tGroup not part of a polity";

			return;
		}

		bool firstDiscovery = true;

		foreach (CulturalDiscovery discovery in polityProminence.Polity.Culture.Discoveries) {

			if (firstDiscovery) {
				InfoPanelScript.InfoText.text += "\nDiscoveries:";

				firstDiscovery = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + discovery.Name;
		}
	}

	public void AddCellDataToInfoPanel_PopCulturalDiscovery (TerrainCell cell) {

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += "\n -- Group Discovery Data -- ";
		InfoPanelScript.InfoText.text += "\n";

		if (cell.Group == null) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		int population = cell.Group.Population;

		if (population <= 0) {

			InfoPanelScript.InfoText.text += "\n\tNo population at location";

			return;
		}

		bool firstDiscovery = true;

		foreach (CulturalDiscovery discovery in cell.Group.Culture.Discoveries) {

			if (firstDiscovery) {
				InfoPanelScript.InfoText.text += "\nDiscoveries:";

				firstDiscovery = false;
			}

			InfoPanelScript.InfoText.text += "\n\t" + discovery.Name;
		}
	}
	
	public void AddCellDataToInfoPanel (TerrainCell cell) {

		int longitude = cell.Longitude;
		int latitude = cell.Latitude;

		InfoPanelScript.InfoText.text += "\n";
		InfoPanelScript.InfoText.text += string.Format ("\nPosition: Longitude {0}, Latitude {1}", longitude, latitude);

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


		if (_planetOverlay == PlanetOverlay.General) {

			AddCellDataToInfoPanel_General (cell);
		}

		if ((_planetOverlay == PlanetOverlay.PopDensity) || 
			(_planetOverlay == PlanetOverlay.PopChange)) {
		
			AddCellDataToInfoPanel_PopDensity (cell);
		}

		if (_planetOverlay == PlanetOverlay.UpdateSpan) {

			AddCellDataToInfoPanel_UpdateSpan (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityProminence) {

			AddCellDataToInfoPanel_PolityProminence (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityContacts) {

			AddCellDataToInfoPanel_PolityContacts (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityTerritory) {

			AddCellDataToInfoPanel_PolityTerritory (cell);
		}

		if (_planetOverlay == PlanetOverlay.FactionCoreDistance) {

			AddCellDataToInfoPanel_PolityTerritory (cell);
		}

		if (_planetOverlay == PlanetOverlay.PolityCulturalPreference) {

			AddCellDataToInfoPanel_PolityCulturalPreference (cell);
		}

		if (_planetOverlay == PlanetOverlay.PopCulturalPreference) {

			AddCellDataToInfoPanel_PopCulturalPreference (cell);
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

		if (hoveredCell == null) {
			throw new System.Exception ("Unable to get cell at [" + longitude + "," + latitude + "]");
		}

		if (IsPolityOverlay (_planetOverlay))
			ShowCellInfoToolTip_PolityTerritory (hoveredCell);
		else if (_planetOverlay == PlanetOverlay.Region)
			ShowCellInfoToolTip_Region (hoveredCell);
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

		switch (_planetOverlay) {
		case PlanetOverlay.General:
			InfoTooltipScript.DisplayTip (polity.Name.Text, tooltipPos);
			break;
		case PlanetOverlay.PolityTerritory:
			ShowCellInfoToolTip_PolityTerritory (polity, tooltipPos);
			break;
		case PlanetOverlay.PolityContacts:
			ShowCellInfoToolTip_PolityContacts (polity, tooltipPos);
			break;
		case PlanetOverlay.PolityCulturalPreference:
			ShowCellInfoToolTip_PolityCulturalPreference (polity, tooltipPos);
			break;
		case PlanetOverlay.PolityCulturalActivity:
			ShowCellInfoToolTip_PolityCulturalActivity (polity, tooltipPos);
			break;
		case PlanetOverlay.PolityCulturalSkill:
			ShowCellInfoToolTip_PolityCulturalSkill (polity, tooltipPos);
			break;
		case PlanetOverlay.PolityCulturalKnowledge:
			ShowCellInfoToolTip_PolityCulturalKnowledge (polity, tooltipPos);
			break;
		case PlanetOverlay.PolityCulturalDiscovery:
			ShowCellInfoToolTip_PolityCulturalDiscovery (polity, tooltipPos);
			break;
		default:
			InfoTooltipScript.SetVisible (false);
			break;
		}
	}

	public void ShowCellInfoToolTip_PolityTerritory (Polity polity, Vector3 position, float fadeStart = 5) {

		string text = polity.Name.Text + " " + polity.Type.ToLower () + "\n\nFaction Influences:";

		foreach (Faction faction in polity.GetFactions ()) {
		
			text += "\n " + faction.Name.Text + ": " + faction.Influence.ToString ("P");
		}

		InfoTooltipScript.DisplayTip (text, position, fadeStart);
	}

	public void ShowCellInfoToolTip_PolityContacts (Polity polity, Vector3 position, float fadeStart = 5) {

		string polityTitle = polity.Name.Text + " " + polity.Type.ToLower ();

		string text;

		Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

		float relationshipValue = 0;
		int groupCount = 0;

		if ((polity.Territory != selectedTerritory) && (selectedTerritory != null)) {

			groupCount = selectedTerritory.Polity.GetContactGroupCount (polity);
		} 

		if (groupCount > 0) {
			relationshipValue = selectedTerritory.Polity.GetRelationshipValue (polity);

			text = "Neighboring polity: " + polityTitle;

			text += "\n\nRelationship Value: " + relationshipValue.ToString ("0.000");
			text += "\n\nOverlaping groups: " + groupCount;

		} else {

			text = polityTitle;
		}

		InfoTooltipScript.DisplayTip (text, position, fadeStart);
	}

	public void ShowCellInfoToolTip_PolityCulturalPreference (Polity polity, Vector3 position, float fadeStart = 5) {

		CulturalPreference preference = polity.Culture.GetPreference (_planetOverlaySubtype);

		if (preference != null) {
			string text = preference.Name + " Preference: " + preference.Value.ToString ("0.00") + "\n\nFactions:";

			foreach (Faction faction in polity.GetFactions ()) {

				float value = faction.GetPreferenceValue (_planetOverlaySubtype);

				text += "\n " + faction.Name.Text + ": " + value.ToString ("0.00");
			}

			InfoTooltipScript.DisplayTip (text, position, fadeStart);

		} else {
			InfoTooltipScript.SetVisible (false);
		}
	}

	public void ShowCellInfoToolTip_PolityCulturalActivity (Polity polity, Vector3 position, float fadeStart = 5) {

		CulturalActivity activity = polity.Culture.GetActivity (_planetOverlaySubtype);

		if (activity != null) {
			string text = activity.Name + " Contribution: " + activity.Contribution.ToString ("P") + "\n\nFactions:";

			foreach (Faction faction in polity.GetFactions ()) {

				activity = faction.Culture.GetActivity (_planetOverlaySubtype);

				text += "\n " + faction.Name.Text + ": " + activity.Contribution.ToString ("P");
			}

			InfoTooltipScript.DisplayTip (text, position, fadeStart);
		} else {
			InfoTooltipScript.SetVisible (false);
		}
	}

	public void ShowCellInfoToolTip_PolityCulturalSkill (Polity polity, Vector3 position, float fadeStart = 5) {

		CulturalSkill skill = polity.Culture.GetSkill (_planetOverlaySubtype);

		if ((skill != null) && (skill.Value >= 0.001)) {
			string text = skill.Name + " Value: " + skill.Value.ToString ("0.000") + "\n\nFactions:";

			foreach (Faction faction in polity.GetFactions ()) {

				skill = faction.Culture.GetSkill (_planetOverlaySubtype);

				text += "\n " + faction.Name.Text + ": " + skill.Value.ToString ("0.000");
			}

			InfoTooltipScript.DisplayTip (text, position, fadeStart);
		} else {
			InfoTooltipScript.SetVisible (false);
		}
	}

	public void ShowCellInfoToolTip_PolityCulturalKnowledge (Polity polity, Vector3 position, float fadeStart = 5) {

		CulturalKnowledge knowledge = polity.Culture.GetKnowledge (_planetOverlaySubtype);

		if (knowledge != null) {
			string text = knowledge.Name + " Value: " + knowledge.ScaledValue.ToString ("0.000") + "\n\nFactions:";

			foreach (Faction faction in polity.GetFactions ()) {

				knowledge = faction.Culture.GetKnowledge (_planetOverlaySubtype);

				text += "\n " + faction.Name.Text + ": " + knowledge.ScaledValue.ToString ("0.000");
			}

			InfoTooltipScript.DisplayTip (text, position, fadeStart);
		} else {
			InfoTooltipScript.SetVisible (false);
		}
	}

	public void ShowCellInfoToolTip_PolityCulturalDiscovery (Polity polity, Vector3 position, float fadeStart = 5) {

		PolityCulturalDiscovery discovery = polity.Culture.GetDiscovery (_planetOverlaySubtype) as PolityCulturalDiscovery;

		if (discovery != null) {
			int presenceCount = discovery.PresenceCount;
			float politySize = polity.Territory.GetCells ().Count;

			float presencePercentage = Mathf.Min (1f, presenceCount / politySize);
			InfoTooltipScript.DisplayTip (discovery.Name + " Presence: " + presencePercentage.ToString ("P"), position, fadeStart);
		} else {
			InfoTooltipScript.SetVisible (false);
		}
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

	public void ShiftMapToPosition (WorldPosition mapPosition) {

		Rect mapImageRect = MapImage.rectTransform.rect;

		Vector2 normalizedMapPos = new Vector2 (mapPosition.Longitude / (float) Manager.CurrentWorld.Width, mapPosition.Latitude / (float) Manager.CurrentWorld.Height);

		Vector2 mapImagePos = normalizedMapPos - MapImage.uvRect.center;
		mapImagePos.x = Mathf.Repeat (mapImagePos.x, 1.0f);

		Rect newUvRect = MapImage.uvRect;
		newUvRect.x += mapImagePos.x;

		MapImage.uvRect = newUvRect;
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
			
			float worldLong = Mathf.Repeat (Mathf.Floor(uvPos.x * Manager.CurrentWorld.Width), Manager.CurrentWorld.Width);
			float worldLat = Mathf.Floor(uvPos.y * Manager.CurrentWorld.Height);
			
			mapPosition = new Vector2 (worldLong, worldLat);
			
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
