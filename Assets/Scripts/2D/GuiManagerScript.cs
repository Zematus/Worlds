using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Profiling;
using UnityEngine.Assertions;

public delegate void PostProgressOperation();
public delegate void PointerOperation(Vector2 position);

public enum GameMode
{
    Simulator,
    Editor,
    None
}

public class GuiManagerScript : MonoBehaviour
{
    public static GuiManagerScript ManagerScript;

    public const float MaxDeltaTimeIterations = 0.02f; // max real time to be spent on iterations on a single frame (this is the value that matters the most performance-wise)

    private float LastRealTime = -1;
    private long LastDaySave = -1;

    bool MustSave
    {
        get
        {
            //Quick exit
            if (Manager.AutoSaveMode == AutoSaveMode.Deactivate)
            {
                return false;
            }
            //Initialise var
            if (LastDaySave == -1)
            {
                LastDaySave = Manager.CurrentWorld.CurrentDate;
            }
            if (LastRealTime == -1)
            {
                LastRealTime = Time.realtimeSinceStartup;
            }
            //return answer
            bool RealTimeCondition = true;
            if (Manager.AutoSaveMode != AutoSaveMode.OnGameTime)
            {
                if (Time.realtimeSinceStartup < LastRealTime + Manager.RealWorldAutoSaveInterval)
                {
                    RealTimeCondition = false;
                }
            }
            bool GameTimeCondition = true;
            if (Manager.AutoSaveMode != AutoSaveMode.OnRealWorldTime)
            {
                if (Manager.CurrentWorld.CurrentDate < (LastDaySave + Manager.AutoSaveInterval))
                {
                    GameTimeCondition = false;
                }
            }
            bool FinalTest = false;
            if (RealTimeCondition == true && GameTimeCondition == true)
            {
                FinalTest = true;
            }
            else if (RealTimeCondition == true || GameTimeCondition == true)
            {
                if (Manager.AutoSaveMode == AutoSaveMode.OnRealWorldOrGameTime)
                {
                    FinalTest = true;
                }
            }

            if (FinalTest == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void OnAutoSave()
    {
        LastDaySave = Manager.CurrentWorld.CurrentDate;
        LastRealTime = Time.realtimeSinceStartup;
        SaveFileDialogPanelScript.SetText("AutoSave");
        SaveAction();
    }

    public CanvasScaler CanvasScaler;

    public Button LoadButton;

    public GameObject FlatMapPanel;
    public GameObject GlobeMapPanel;

    public PlanetScript PlanetScript;
    public MapScript MapScript;

    public ModalActivationScript ModalActivationScript;

    public InfoTooltipScript InfoTooltipScript;

    public InfoPanelScript InfoPanelScript;

    public TextInputDialogPanelScript SaveFileDialogPanelScript;
    public TextInputDialogPanelScript ExportMapDialogPanelScript;
    public ModDecisionDialogPanelScript ModDecisionDialogPanelScript;
    public LoadFileDialogPanelScript LoadFileDialogPanelScript;
    public SelectFactionDialogPanelScript SelectFactionDialogPanelScript;
    public OverlayDialogPanelScript OverlayDialogPanelScript;
    public DialogPanelScript MainMenuDialogPanelScript;
    public DialogPanelScript OptionsDialogPanelScript;
    public DialogPanelScript ExceptionDialogPanelScript;
    public ProgressDialogPanelScript ProgressDialogPanelScript;
    public ImageDialogPanelScript ActivityDialogPanelScript;
    public DialogPanelScript ErrorMessageDialogPanelScript;
    public WorldCustomizationDialogPanelScript SetSeedDialogPanelScript;
    public AddPopulationDialogScript AddPopulationDialogScript;
    public FocusPanelScript FocusPanelScript;

    public PaletteScript BiomePaletteScript;
    public PaletteScript MapPaletteScript;
    public PaletteScript OverlayPaletteScript;

    public SelectionPanelScript SelectionPanelScript;

    public QuickTipPanelScript QuickTipPanelScript;

    public EventPanelScript EventPanelScript;

    [Range(0f, 2f)]
    public float XAxisMagic_Offset = 0.51f;
    [Range(0f, 2f)]
    public float XAxisMagic_Mult = 0.86f;
    [Range(0f, 2f)]
    public float YAxisMagic_Offset = 0.51f;
    [Range(0f, 2f)]
    public float YAxisMagic_Mult = 0.86f;

    public ToggleEvent OnSimulationInterrupted;
    public ToggleEvent OnSimulationPaused;

    public ToggleEvent OnFirstMaxSpeedOptionSet;
    public ToggleEvent OnLastMaxSpeedOptionSet;

    public UnityEvent MapEntitySelected;
    public UnityEvent OverlayChanged;
    public UnityEvent OverlaySubtypeChanged;

    public UnityEvent OpenModeSelectionDialogRequested;

    public UnityEvent EnteredEditorMode;
    public UnityEvent EnteredSimulationMode;

    public UnityEvent WorldGenerated;
    public UnityEvent WorldRegenerated;
    public UnityEvent WorldLoaded;

    public ToggleEvent EffectHandlingRequested;

    public ToggleEvent ToggledGlobeViewing;

    public SpeedChangeEvent OnSimulationSpeedChanged;

    public MessageEvent DisplayNonBlockingMessage;
    public UnityEvent HideNonBlockingMessage;

    /// <summary>
    /// Indicates that the game should not progress after finishing a parallel op
    /// because something else is pausing the simulation (resolving an action or
    /// decision, for example). Example parallel ops: save, export
    /// </summary>
    private bool _eventPauseActive = false;

    /// <summary>
    /// Indicates that the pause button has been pressed by the user to pause the
    /// simulation (as opposed to the simulation being paused by a dialog, for example)
    /// </summary>
    private bool _pauseButtonPressed = false;

    /// <summary>
    /// Indicates that something is pausing the simulation (dialog, action, etc...)
    /// </summary>
    private bool _pausingConditionActive = false;

    /// <summary>
    /// Indicates that the simulation is waiting for the user to complete a set of requests
    /// </summary>
    private bool _resolvingEffects = false;

    /// <summary>
    /// Indicates that the simulation is waiting for the user to resolve a decision
    /// </summary>
    private bool _resolvingDecision = false;

    /// <summary>
    /// Indicates that the user finished handling a request
    /// </summary>
    private bool _doneHandlingRequest = false;

    private IEffectExpression _unresolvedEffect = null;

    private bool _displayedTip_mapScroll = false;
    private bool _displayedTip_initialPopulation = false;

    private Vector3 _tooltipOffset = new Vector3(0, 0);

    private TerrainCell _lastHoveredCell = null;

    private Territory _lastHoveredOverTerritory = null;
    private Region _lastHoveredOverRegion = null;

    private PlanetView _planetView = PlanetView.Biomes;

    private PlanetOverlay _planetOverlay = PlanetOverlay.General;

    private Stack<PlanetOverlay> _tempOverlayStack =
        new Stack<PlanetOverlay>();

    private Stack<string> _tempOverlaySubtypeStack =
        new Stack<string>();

    private string _planetOverlaySubtype = Manager.NoOverlaySubtype;

    private List<PlanetOverlay> _popOverlays = new List<PlanetOverlay>()
    {
        PlanetOverlay.PopDensity,
        PlanetOverlay.FarmlandDistribution,
        PlanetOverlay.PopCulturalPreference,
        PlanetOverlay.PopCulturalActivity,
        PlanetOverlay.PopCulturalSkill,
        PlanetOverlay.PopCulturalKnowledge,
        PlanetOverlay.PopCulturalDiscovery
    };
    private int _currentPopOverlay = 0;

    private List<PlanetOverlay> _polityOverlays = new List<PlanetOverlay>()
    {
        PlanetOverlay.PolityTerritory,
        PlanetOverlay.PolityProminence,
        PlanetOverlay.PolityContacts,
        PlanetOverlay.PolityCoreRegions,
        PlanetOverlay.PolityCulturalPreference,
        PlanetOverlay.PolityCulturalActivity,
        PlanetOverlay.PolityCulturalSkill,
        PlanetOverlay.PolityCulturalKnowledge,
        PlanetOverlay.PolityCulturalDiscovery,
        PlanetOverlay.PolityAdminCost,
        PlanetOverlay.FactionCoreDistance,
        PlanetOverlay.PolityCluster,
        PlanetOverlay.ClusterAdminCost
    };
    private int _currentPolityOverlay = 0;

    private List<PlanetOverlay> _miscOverlays = new List<PlanetOverlay>()
    {
        PlanetOverlay.Temperature,
        PlanetOverlay.Rainfall,
        PlanetOverlay.DrainageBasins,
        PlanetOverlay.Arability,
        PlanetOverlay.Accessibility,
        PlanetOverlay.Hilliness,
        PlanetOverlay.BiomeTrait,
        PlanetOverlay.Layer,
        PlanetOverlay.Region,
        PlanetOverlay.Language
    };
    private int _currentMiscOverlay = 0;

    private List<PlanetOverlay> _debugOverlays = new List<PlanetOverlay>()
    {
        PlanetOverlay.PopChange,
        PlanetOverlay.UpdateSpan,
        PlanetOverlay.Migration,
        PlanetOverlay.MigrationPressure,
        PlanetOverlay.PolityMigrationPressure
    };
    private int _currentDebugOverlay = 0;

    private Dictionary<PlanetOverlay, string> _planetOverlaySubtypeCache = new Dictionary<PlanetOverlay, string>();

    private bool _displayRoutes = false;
    private bool _displayGroupActivity = false;

    private bool _regenMapTexture = false;
    private bool _regenMapOverlayTexture = false;
    private bool _regenPointerOverlayTextures = false;

    private bool _resetOverlays = true;

    private bool _backgroundProcessActive = false;

    private string _progressMessage = null;
    private float _progressValue = 0;

    private bool _hasToSetInitialPopulation = false;
    private bool _worldCouldBeSavedAfterEdit = false;

    private event PostProgressOperation _postProgressOp = null;
    private event PostProgressOperation _generateWorldPostProgressOp = null;
    private event PostProgressOperation _regenerateWorldPostProgressOp = null;
    private event PostProgressOperation _loadWorldPostProgressOp = null;

    private event PointerOperation _mapLeftClickOp = null;

    private const float _maxAccTime = 1.0f; // the standard length of time of a simulation cycle (in real time)

    private float _accDeltaTime = 0;
    private long _simulationDateSpan = 0;

    private bool _willFinishResolvingDecision = false;

    private int _mapUpdateCount = 0;
    private int _pixelUpdateCount = 0;
    private float _timeSinceLastMapUpdate = 0;

    private int _topMaxSpeedLevelIndex;
    private int _selectedMaxSpeedLevelIndex;

    private Texture2D _heightmap = null;

    private List<ModalPanelScript> _hiddenInteractionPanels = new List<ModalPanelScript>();

    private System.Exception _cachedException = null;

    private System.Action _closeErrorActionToPerform = null;

    private int _lastExLogHash = 0;

    private PointerEventData _keyboardDragTracker = new PointerEventData(EventSystem.current)
    {
        position = Vector2.zero,
        button = PointerEventData.InputButton.Right
    };

    void OnEnable()
    {
        Manager.InitializeDebugLog();

        Application.logMessageReceivedThreaded += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;

        Manager.CloseDebugLog();
    }

    public void RegisterRegenerateWorldPostProgressOp(PostProgressOperation op)
    {
        _regenerateWorldPostProgressOp += op;
    }

    public void DeregisterRegenerateWorldPostProgressOp(PostProgressOperation op)
    {
        _regenerateWorldPostProgressOp -= op;
    }

    public void RegisterGenerateWorldPostProgressOp(PostProgressOperation op)
    {
        _generateWorldPostProgressOp += op;
    }

    public void DeregisterGenerateWorldPostProgressOp(PostProgressOperation op)
    {
        _generateWorldPostProgressOp -= op;
    }

    public void RegisterLoadWorldPostProgressOp(PostProgressOperation op)
    {
        _loadWorldPostProgressOp += op;
    }

    public void DeregisterLoadWorldPostProgressOp(PostProgressOperation op)
    {
        _loadWorldPostProgressOp -= op;
    }

    private void ResetAllDialogs()
    {
        ModalActivationScript.Activate(false);

        SelectionPanelScript.RemoveAllOptions();
        SelectionPanelScript.SetVisible(false);

        ModDecisionDialogPanelScript.SetVisible(false);
        SelectFactionDialogPanelScript.SetVisible(false);
        MainMenuDialogPanelScript.SetVisible(false);
        ProgressDialogPanelScript.SetVisible(false);
        ActivityDialogPanelScript.SetVisible(false);
        OptionsDialogPanelScript.SetVisible(false);
        ErrorMessageDialogPanelScript.SetVisible(false);
        ExceptionDialogPanelScript.SetVisible(false);
        AddPopulationDialogScript.SetVisible(false);

        FocusPanelScript.SetVisible(false);

        QuickTipPanelScript.SetVisible(false);
        InfoTooltipScript.SetVisible(false);
    }

    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        if ((type == LogType.Exception) && (logString.GetHashCode() == _lastExLogHash))
        {
            // There's no need to log multiple instances of the exact same exception
            return;
        }

        Manager.HandleLog(logString, stackTrace, type);

        if (type == LogType.Exception)
        {
            _lastExLogHash = logString.GetHashCode();

            Manager.EnableLogBackup();

            Manager.EnqueueTaskAndWait(() =>
            {
                PlayerPauseSimulation(true);

                ResetAllDialogs();

                ExceptionDialogPanelScript.SetDialogText(logString);
                ExceptionDialogPanelScript.SetVisible(true);

                return true;
            });
        }
    }

    // Use this for initialization
    void Awake()
    {
        ManagerScript = this;

        try
        {
            Manager.LoadAppSettings(@"Worlds.settings");
        }
        catch (System.Exception e)
        {
            // store the exception to report it on screen as soon as possible
            _cachedException = e;
        }

    }

    /// <summary>
    /// Common calls executed after switching from from init view scene
    /// </summary>
    private void InitFromStartView()
    {
        ValidateLayersPresent();

        SetGameModeAccordingToCurrentWorld();

        SetGlobeView(false);

        SetMaxSpeedLevel(_selectedMaxSpeedLevelIndex);
    }

    // Use this for initialization
    void Start()
    {
        SetUIScaling(Manager.UIScalingEnabled);

        _topMaxSpeedLevelIndex = Speed.Levels.Length - 1;
        _selectedMaxSpeedLevelIndex = Manager.StartSpeedIndex;

        Manager.UpdateMainThreadReference();

        ResetAllDialogs();

        _mapLeftClickOp += ClickOp_SelectCell;

#if DEBUG
        if (!Manager.WorldIsReady)
        {
            //_heightmap = Manager.LoadTexture(Path.Combine("Heightmaps", "CompositeEarth_3600x1800.png"));

            //Manager.SetActiveModPaths(new string[] {
            //    Path.Combine("Mods", "Base"),
            //    Path.Combine("Mods", "WeirdBiomesMod")
            //});
            Manager.SetActiveModPaths(new string[] { Path.Combine("Mods", "Base") });

            //GenerateWorld(false, 1142453343, useHeightmap: true);
            //GenerateWorld(false, 1582997248);
            //GenerateWorld(false, 266440697);
            //GenerateWorld(false, 744563535);
            //GenerateWorld(false, 16666383);
            //GenerateWorld(false, 483016245);
            //GenerateWorld(false, 1060158945);
            //GenerateWorld(false, 1645709120);
            //GenerateWorld(false, 888101979);
            //GenerateWorld(false, 6353535);
            //GenerateWorld(false, 1137426545);
            ///GenerateWorld(false, 1277025723);
            ///GenerateWorld(false, 1602826489);
            //GenerateWorld(false, 1251521690);
            //GenerateWorld(false, 82226810);
            //GenerateWorld(false, 122520965);
            //GenerateWorld(false, 1757624864);
            //GenerateWorld(false, 1253572363);
            //GenerateWorld(false, 403265427);
            //GenerateWorld(false, 70275341);
            //GenerateWorld(false, 1788979703);
            //GenerateWorld(false, 405241319);
            //GenerateWorld(false, 2139853335);
            //GenerateWorld(false, 1119291416);
            ///GenerateWorld(false, 2028655149);
            //GenerateWorld(false, 1963172103);
            //GenerateWorld(false, 283647159);
            //GenerateWorld(false, 252308385);
            ///GenerateWorld(false, 113741282);
            //GenerateWorld(false, 92288943);
            //GenerateWorld(false, 940983664);
            //GenerateWorld(false, 1136346461);
            GenerateWorld(false, 2093914);
        }
        else
        {
            InitFromStartView();
        }
#else
        InitFromStartView();
#endif

        LoadButton.interactable = HasFilesToLoad();

        Manager.SetBiomePalette(BiomePaletteScript.Colors);
        Manager.SetMapPalette(MapPaletteScript.Colors);
        Manager.SetOverlayPalette(OverlayPaletteScript.Colors);

        _regenMapTexture = true;
        _regenMapOverlayTexture = true;
        _regenPointerOverlayTextures = true;
    }

    void OnDestroy()
    {
        Manager.SaveAppSettings(@"Worlds.settings");
    }

    // Update is called once per frame
    void Update()
    {
        if (_cachedException != null)
        {
            System.Exception ce = _cachedException;
            _cachedException = null;

            throw ce;
        }

        ReadKeyboardInput();

        if ((Manager.CurrentDevMode != DevMode.None) && Manager.WorldIsReady)
        {
            _timeSinceLastMapUpdate += Time.deltaTime;

            if (_timeSinceLastMapUpdate > 1) // Every second
            {
                Manager.LastEventsTriggeredCount = Manager.CurrentWorld.EventsTriggered;
                Manager.LastEventsEvaluatedCount = Manager.CurrentWorld.EventsEvaluated;

                foreach (KeyValuePair<string, World.EventEvalStats> pair in
                    Manager.CurrentWorld.EventEvalStatsPerType)
                {
                    Manager.LastEventEvalStatsPerType[pair.Key] = pair.Value;
                }

                Manager.CurrentWorld.EventsTriggered = 0;
                Manager.CurrentWorld.EventsEvaluated = 0;

                Manager.CurrentWorld.EventEvalStatsPerType.Clear();

                Manager.LastMapUpdateCount = _mapUpdateCount;
                _mapUpdateCount = 0;

                Manager.LastPixelUpdateCount = _pixelUpdateCount;
                _pixelUpdateCount = 0;

                _timeSinceLastMapUpdate -= 1;

                if (Manager.WorldIsReady)
                {
                    long currentDate = Manager.CurrentWorld.CurrentDate;

                    Manager.LastDevModeDateSpan = currentDate - Manager.LastDevModeUpdateDate;
                    Manager.LastDevModeUpdateDate = currentDate;
                }
                else
                {
                    Manager.LastDevModeDateSpan = 0;
                    Manager.LastDevModeUpdateDate = 0;
                }
            }
        }

        Manager.ExecuteTasks(100);

        if (_backgroundProcessActive)
        {
            if (_progressMessage != null) ProgressDialogPanelScript.SetDialogText(_progressMessage);

            ProgressDialogPanelScript.SetProgress(_progressValue);
        }

        if (!Manager.WorldIsReady)
        {
            return;
        }

        if (Manager.PerformingAsyncTask)
        {
            return;
        }

        if (_backgroundProcessActive)
        {
            ProgressDialogPanelScript.SetVisible(false);
            ActivityDialogPanelScript.SetVisible(false);

            _backgroundProcessActive = false;

            _postProgressOp?.Invoke();

            ShowHiddenInteractionPanels();
        }

        if (_doneHandlingRequest)
        {
            if (TryResolvePendingEffects())
            {
                SetResolvingEffects(false);
            }

            _doneHandlingRequest = false;
        }

        if (!_resolvingEffects)
        {
            if (_willFinishResolvingDecision)
            {
                FinishResolvingDecision();
            }

            if (!_resolvingEffects && !_resolvingDecision)
            {
                // if we are not waiting on the user, try resolving
                // other pending decisions
                TryResolvePendingDecisions();
            }
        }

        TryResolvePendingAction();

        if (!_resolvingEffects && !_resolvingDecision)
        {
            Manager.ResolvingPlayerInvolvedDecisionChain = false;
        }

#if DEBUG
        if (Manager.Debug_PauseSimRequested)
        {
            PlayerPauseSimulation(true);
            Manager.Debug_PauseSimRequested = false;
        }
#endif

        bool simulationRunning =
            Manager.SimulationCanRun &&
            (Manager.SimulationRunning || Manager.SimulationPerformingStep);

        if (simulationRunning)
        {
            Profiler.BeginSample("Perform Simulation");

            World world = Manager.CurrentWorld;

            Speed selectedSpeed = Speed.Levels[_selectedMaxSpeedLevelIndex];

            _accDeltaTime += Time.deltaTime;

            if (_accDeltaTime > _maxAccTime)
            {
                int d = (int)(_accDeltaTime / _maxAccTime);
                _accDeltaTime -= _maxAccTime * d;

                _simulationDateSpan = 0;
            }

            int maxSimulationDateSpan = (int)Mathf.Ceil(selectedSpeed * _accDeltaTime);

            // Simulate additional iterations if we haven't reached the max amount of iterations
            // allowed per the percentage of transpired real time during this cycle
            if (_simulationDateSpan < maxSimulationDateSpan)
            {
                long maxDateSpanBetweenUpdates = (int)Mathf.Ceil(selectedSpeed * MaxDeltaTimeIterations);
                long lastUpdateDate = world.CurrentDate;

                long dateSpan = 0;

                float startTimeIterations = Time.realtimeSinceStartup;

                // Simulate up to the max amout of iterations allowed per frame
                while ((lastUpdateDate + maxDateSpanBetweenUpdates) > world.CurrentDate)
                {
                    world.EvaluateEventsToHappen();

                    if (!TryResolvePendingDecisions())
                    {
                        // Stop world update if resolving the decision will
                        // require player input
                        break;
                    }

                    Profiler.BeginSample("World Update");

                    dateSpan += world.Update();

                    Profiler.EndSample(); // World Update

#if DEBUG
                    if (Manager.Debug_PauseSimRequested)
                        break;
#endif

                    float deltaTimeIterations = Time.realtimeSinceStartup - startTimeIterations;

                    // If too much real time was spent simulating after this iteration stop simulating until the next frame
                    if (deltaTimeIterations > MaxDeltaTimeIterations)
                        break;
                }

                _simulationDateSpan += dateSpan;
            }

            while (world.EventMessagesLeftToShow() > 0)
            {
                ShowEventMessage(Manager.CurrentWorld.GetNextMessageToShow());
            }

            Profiler.EndSample(); // Perform Simulation

            if (MustSave)
            {
                OnAutoSave();
            }
        }

        ExecuteMapHoverOps();

        if (_regenPointerOverlayTextures)
        {
            Manager.GeneratePointerOverlayTextures();
            MapScript.RefreshPointerOverlayTexture();

            _regenPointerOverlayTextures = false;
        }
        else
        {
            if (Manager.GameMode == GameMode.Editor)
            {
                Manager.UpdatePointerOverlayTextures();
            }
        }

        if (_regenMapTexture || _regenMapOverlayTexture)
        {
            Profiler.BeginSample("Regen Textures");

            if (_resetOverlays)
            {
                _planetView = PlanetView.Biomes;

                _planetOverlay = PlanetOverlay.None;
            }

            Profiler.BeginSample("Manager.Set*");

            Manager.SetPlanetOverlay(_planetOverlay, _planetOverlaySubtype);
            Manager.SetPlanetView(_planetView);
            Manager.SetDisplayRoutes(_displayRoutes);
            Manager.SetDisplayGroupActivity(_displayGroupActivity);

            if (_resetOverlays)
            {
                TriggerOverlayEvents();

                _resetOverlays = false;
            }
            else
            {
                OverlaySubtypeChanged.Invoke();
            }

            Profiler.EndSample(); // Manager.Set*

            Profiler.BeginSample("Manager.GenerateTextures");

            Manager.GenerateTextures(_regenMapTexture, _regenMapOverlayTexture);

            Profiler.EndSample(); // Manager.GenerateTextures

            Profiler.BeginSample("Manager.RefreshTexture");

            MapScript.RefreshTexture();
            PlanetScript.RefreshTexture();

            Profiler.EndSample(); // Manager.RefreshTexture

            if (Manager.CurrentDevMode != DevMode.None)
            {
                _pixelUpdateCount += Manager.UpdatedPixelCount;
                _mapUpdateCount++;
            }

            _regenMapTexture = false;
            _regenMapOverlayTexture = false;

            Profiler.EndSample(); // Regen Textures
        }
        else
        {
            Profiler.BeginSample("Update Textures");

            Manager.UpdateTextures();

            if (Manager.CurrentDevMode != DevMode.None)
            {
                _pixelUpdateCount += Manager.UpdatedPixelCount;
                _mapUpdateCount++;
            }

            Profiler.EndSample(); // Update Textures
        }

        InfoPanelScript.UpdateInfoPanel();
        UpdateFocusPanel();
        UpdateSelectionMenu();

        if (Manager.GameMode == GameMode.Editor)
        {
            Manager.ApplyEditorBrush();

            Manager.UpdateEditorBrushState();
        }
    }

    /// <summary>Changes the active play mode to Simulator mode.</summary>
    public void SetSimulatorMode()
    {
        Manager.GameMode = GameMode.Simulator;

        MapScript.EnablePointerOverlay(false);

        Debug.Log("Game entered history simulator mode.");

        if (_worldCouldBeSavedAfterEdit)
        {
            SaveEditedWorldBeforeStarting();
        }
        else
        {
            AttemptToSetInitialPopulation();
        }

        EnteredSimulationMode.Invoke();

#if DEBUG
        ChangePlanetOverlay(PlanetOverlay.General); // When debugging we might like to autoselect a different default overlay
#else
		ChangePlanetOverlay(PlanetOverlay.General);
#endif
    }

    public void SetEditorMode()
    {
        Manager.GameMode = GameMode.Editor;

        MapScript.EnablePointerOverlay(true);

        Debug.Log("Game entered map editor mode.");

        _worldCouldBeSavedAfterEdit = true;

        EnteredEditorMode.Invoke();

#if DEBUG
        // When debugging we might like to autoselect a different default overlay
        ChangePlanetOverlay(PlanetOverlay.None);
#else
		ChangePlanetOverlay(PlanetOverlay.None);
#endif
    }

    private bool CanAlterRunningStateOrSpeed()
    {
        return Manager.SimulationCanRun && !_pausingConditionActive;
    }

    private void PauseSimulationIfRunning()
    {
        PlayerPauseSimulation(Manager.SimulationRunning);
    }

    private void SetMaxSpeedLevelTo0()
    {
        SetMaxSpeedLevelIfNotPaused(0);
    }

    private void SetMaxSpeedLevelTo1()
    {
        SetMaxSpeedLevelIfNotPaused(1);
    }

    private void SetMaxSpeedLevelTo2()
    {
        SetMaxSpeedLevelIfNotPaused(2);
    }

    private void SetMaxSpeedLevelTo3()
    {
        SetMaxSpeedLevelIfNotPaused(3);
    }

    private void SetMaxSpeedLevelTo4()
    {
        SetMaxSpeedLevelIfNotPaused(4);
    }

    private void SetMaxSpeedLevelTo5()
    {
        SetMaxSpeedLevelIfNotPaused(5);
    }

    private void SetMaxSpeedLevelTo6()
    {
        SetMaxSpeedLevelIfNotPaused(6);
    }

    private void SetMaxSpeedLevelTo7()
    {
        SetMaxSpeedLevelIfNotPaused(7);
    }

    private void IncreaseMaxSpeedLevel()
    {
        SetMaxSpeedLevelIfNotPaused(_selectedMaxSpeedLevelIndex + 1);
    }

    private void DecreaseMaxSpeedLevel()
    {
        SetMaxSpeedLevelIfNotPaused(_selectedMaxSpeedLevelIndex - 1);
    }

    private void ReadKeyboardInput_TimeControls()
    {
        if (CanAlterRunningStateOrSpeed())
        {
            Manager.HandleKeyUp(KeyCode.Space, false, false, PauseSimulationIfRunning);
            Manager.HandleKeyUp(KeyCode.Alpha1, false, false, SetMaxSpeedLevelTo0);
            Manager.HandleKeyUp(KeyCode.Alpha2, false, false, SetMaxSpeedLevelTo1);
            Manager.HandleKeyUp(KeyCode.Alpha3, false, false, SetMaxSpeedLevelTo2);
            Manager.HandleKeyUp(KeyCode.Alpha4, false, false, SetMaxSpeedLevelTo3);
            Manager.HandleKeyUp(KeyCode.Alpha5, false, false, SetMaxSpeedLevelTo4);
            Manager.HandleKeyUp(KeyCode.Alpha6, false, false, SetMaxSpeedLevelTo5);
            Manager.HandleKeyUp(KeyCode.Alpha7, false, false, SetMaxSpeedLevelTo6);
            Manager.HandleKeyUp(KeyCode.Alpha8, false, false, SetMaxSpeedLevelTo7);
            Manager.HandleKeyUp(KeyCode.KeypadPlus, false, true, IncreaseMaxSpeedLevel);
            Manager.HandleKeyUp(KeyCode.KeypadMinus, false, true, DecreaseMaxSpeedLevel);
        }
    }

    private void HandleEscapeOp()
    {
        if (!_backgroundProcessActive)
        {
            if (SelectFactionDialogPanelScript.gameObject.activeInHierarchy)
            {
                CancelSelectFaction();
            }
            else if (MainMenuDialogPanelScript.gameObject.activeInHierarchy)
            {
                CloseMainMenu();
            }
            else if (OptionsDialogPanelScript.gameObject.activeInHierarchy)
            {
                CloseOptionsMenu();
            }
            else if (ErrorMessageDialogPanelScript.gameObject.activeInHierarchy)
            {
                CloseErrorMessageAction();
            }
            else if (!IsModalPanelActive())
            {
                OpenMainMenu();
            }
        }
    }

    private void ReadKeyboardInput_Escape()
    {
        if (_resolvingEffects)
        {
            // Do not process if waiting for the player to complete an action
            return;
        }

        Manager.HandleKeyUp(KeyCode.Escape, false, false, HandleEscapeOp, false);
    }

    public void ToogleFullscreen()
    {
        Manager.SetFullscreen(!Manager.FullScreenEnabled);
    }

    private void ReadKeyboardInput_Menus()
    {
        if (_resolvingEffects)
        {
            // Do not process if waiting for the player to complete an action
            return;
        }

        Manager.HandleKeyUp(KeyCode.X, true, false, ExportImageAs);
        Manager.HandleKeyUp(KeyCode.S, true, false, SaveWorldAs);
        Manager.HandleKeyUp(KeyCode.L, true, false, LoadWorld);
        Manager.HandleKeyUp(KeyCode.G, true, false, SetGenerationSeed);
        //Manager.HandleKeyUp(KeyCode.F, true, false, ToogleFullscreen);
    }

    private void ReadKeyboardInput_Globe()
    {
        Manager.HandleKeyUp(KeyCode.G, false, true, ToggleGlobeView);
    }

    private void SetNextView()
    {
        SetView(_planetView.GetNextEnumValue());
    }

    private void ReadKeyboardInput_MapViews()
    {
        if (_resolvingEffects)
        {
            // Do not process if waiting for the player to complete an action
            return;
        }

        Manager.HandleKeyUp(KeyCode.V, false, false, SetNextView);
    }

    private void ActivateGeneralOverlay()
    {
        ChangePlanetOverlay(PlanetOverlay.General);
    }

    private void ReadKeyboardInput_MapOverlays()
    {
        if (_resolvingEffects)
        {
            // Do not process if waiting for the player to complete an action
            return;
        }

        Manager.HandleKeyUp(KeyCode.N, false, false, DisableAllOverlays);

        if (Manager.GameMode == GameMode.Simulator)
        {
            Manager.HandleKeyUp(KeyCode.G, false, false, ActivateGeneralOverlay);
            Manager.HandleKeyUp(KeyCode.O, false, false, ActivatePopOverlay);
            Manager.HandleKeyUp(KeyCode.P, false, false, ActivatePolityOverlay);

            if (Manager.CurrentDevMode != DevMode.None)
            {
                Manager.HandleKeyUp(KeyCode.D, false, false, ActivateDebugOverlay);
            }
        }

        Manager.HandleKeyUp(KeyCode.M, false, false, ActivateMiscOverlay);
    }

    private void ReadKeyboardInput_Navigation()
    {
        //NOTE: simultaneously pressing both keys for the same direction, e.g., A + <-, will double the
        //      speed on that direction
        Manager.HandleKeyDown(KeyCode.LeftArrow, false, false, StartDraggingWithKeyboard, Direction.West);
        Manager.HandleKeyDown(KeyCode.A, false, false, StartDraggingWithKeyboard, Direction.West);
        Manager.HandleKey(KeyCode.LeftArrow, false, false, DragWithKeyboard, Direction.West);
        Manager.HandleKey(KeyCode.A, false, false, DragWithKeyboard, Direction.West);
        Manager.HandleKeyUp(KeyCode.LeftArrow, false, false, EndDragWithKeyboard, Direction.West);
        Manager.HandleKeyUp(KeyCode.A, false, false, EndDragWithKeyboard, Direction.West);

        Manager.HandleKeyDown(KeyCode.RightArrow, false, false, StartDraggingWithKeyboard, Direction.East);
        Manager.HandleKeyDown(KeyCode.D, false, false, StartDraggingWithKeyboard, Direction.East);
        Manager.HandleKey(KeyCode.RightArrow, false, false, DragWithKeyboard, Direction.East);
        Manager.HandleKey(KeyCode.D, false, false, DragWithKeyboard, Direction.East);
        Manager.HandleKeyUp(KeyCode.RightArrow, false, false, EndDragWithKeyboard, Direction.East);
        Manager.HandleKeyUp(KeyCode.D, false, false, EndDragWithKeyboard, Direction.East);

        Manager.HandleKeyDown(KeyCode.DownArrow, false, false, StartDraggingWithKeyboard, Direction.South);
        Manager.HandleKeyDown(KeyCode.S, false, false, StartDraggingWithKeyboard, Direction.South);
        Manager.HandleKey(KeyCode.DownArrow, false, false, DragWithKeyboard, Direction.South);
        Manager.HandleKey(KeyCode.S, false, false, DragWithKeyboard, Direction.South);
        Manager.HandleKeyUp(KeyCode.DownArrow, false, false, EndDragWithKeyboard, Direction.South);
        Manager.HandleKeyUp(KeyCode.S, false, false, EndDragWithKeyboard, Direction.South);

        Manager.HandleKeyDown(KeyCode.UpArrow, false, false, StartDraggingWithKeyboard, Direction.North);
        Manager.HandleKeyDown(KeyCode.W, false, false, StartDraggingWithKeyboard, Direction.North);
        Manager.HandleKey(KeyCode.UpArrow, false, false, DragWithKeyboard, Direction.North);
        Manager.HandleKey(KeyCode.W, false, false, DragWithKeyboard, Direction.North);
        Manager.HandleKeyUp(KeyCode.UpArrow, false, false, EndDragWithKeyboard, Direction.North);
        Manager.HandleKeyUp(KeyCode.W, false, false, EndDragWithKeyboard, Direction.North);
    }

    private void StartDraggingWithKeyboard(Direction direction)
    {
        _keyboardDragTracker.position = new Vector2(0, 0);
        BeginDrag(_keyboardDragTracker);
    }

    private void DragWithKeyboard(Direction direction)
    {
        float xAxisDelta = 0;
        float yAxisDelta = 0;

        switch (direction)
        {
            case Direction.North:
                yAxisDelta = -YAxisMagic_Offset;
                break;
            case Direction.South:
                yAxisDelta = YAxisMagic_Offset;
                break;
            case Direction.West:
                xAxisDelta = XAxisMagic_Offset;
                break;
            case Direction.East:
                xAxisDelta = -XAxisMagic_Offset;
                break;
            default:
                Debug.Log(string.Format("Unrecognized direction [%s] received, setting deltas to [%d, %d]", direction, xAxisDelta, yAxisDelta));
                break;
        }

        xAxisDelta = Manager.KeyboardXAxisSensitivity * xAxisDelta * XAxisMagic_Mult;
        if (Manager.KeyboardInvertXAxis)
        {
            xAxisDelta *= -1;
        }
        yAxisDelta = Manager.KeyboardYAxisSensitivity * yAxisDelta * YAxisMagic_Mult;
        if (Manager.KeyboardInvertYAxis)
        {
            yAxisDelta *= -1;
        }
        _keyboardDragTracker.position += new Vector2(xAxisDelta, yAxisDelta);
        Drag(_keyboardDragTracker);
    }

    private void EndDragWithKeyboard(Direction direction)
    {
        EndDrag(_keyboardDragTracker);
    }

    public static bool IsModalPanelActive()
    {
        return IsMenuPanelActive() || IsInteractionPanelActive();
    }

    public static bool IsMenuPanelActive()
    {
        return GameObject.FindGameObjectsWithTag("MenuPanel").Length > 0;
    }

    public static bool IsInteractionPanelActive()
    {
        return GameObject.FindGameObjectsWithTag("InteractionPanel").Length > 0;
    }

    public void HideInteractionPanel(ModalPanelScript panel)
    {
        _hiddenInteractionPanels.Add(panel);
    }

    public void ShowHiddenInteractionPanels()
    {
        if (IsMenuPanelActive())
            return; // Don't show any hidden panel if there's any menu panel still active.

        foreach (ModalPanelScript panel in _hiddenInteractionPanels)
        {
            panel.SetVisible(true);
        }

        _hiddenInteractionPanels.Clear();
    }

    private void ActivatePopOverlay()
    {
        if (_popOverlays[_currentPopOverlay] == _planetOverlay)
        {
            _currentPopOverlay = (_currentPopOverlay + 1) % _popOverlays.Count;
        }

        ChangePlanetOverlay(_popOverlays[_currentPopOverlay]);
    }

    private void SkipDevOverlaysIfNotEnabled()
    {
        if ((Manager.CurrentDevMode == DevMode.None) &&
            ((_polityOverlays[_currentPolityOverlay] == PlanetOverlay.FactionCoreDistance) ||
            (_polityOverlays[_currentPolityOverlay] == PlanetOverlay.PolityCluster) ||
            (_polityOverlays[_currentPolityOverlay] == PlanetOverlay.ClusterAdminCost)))
        {
            _currentPolityOverlay = 0;
        }
    }

    private void ActivatePolityOverlay()
    {
        if (_polityOverlays[_currentPolityOverlay] == _planetOverlay)
        {
            _currentPolityOverlay = (_currentPolityOverlay + 1) % _polityOverlays.Count;
        }

        SkipDevOverlaysIfNotEnabled();

        ChangePlanetOverlay(_polityOverlays[_currentPolityOverlay]);
    }

    private void SkipLayerOverlayIfNotPresent()
    {
        // Skip layer overlay if no layers are present in this world
        if ((!Manager.LayersPresent) &&
            (_miscOverlays[_currentMiscOverlay] == PlanetOverlay.Layer))
        {
            _currentMiscOverlay = (_currentMiscOverlay + 1) % _miscOverlays.Count;
        }
    }

    private void SkipSimulationOverlaysIfEditorMode()
    {
        if ((Manager.GameMode == GameMode.Editor) &&
            ((_miscOverlays[_currentMiscOverlay] == PlanetOverlay.Language) ||
            (_miscOverlays[_currentMiscOverlay] == PlanetOverlay.Region)))
        {
            _currentMiscOverlay = 0;
        }
    }

    private void ActivateMiscOverlay()
    {
        if (_miscOverlays[_currentMiscOverlay] == _planetOverlay)
        {
            _currentMiscOverlay = (_currentMiscOverlay + 1) % _miscOverlays.Count;
        }

        SkipLayerOverlayIfNotPresent();
        SkipSimulationOverlaysIfEditorMode();

        ChangePlanetOverlay(_miscOverlays[_currentMiscOverlay]);
    }

    private void ActivateDebugOverlay()
    {
        if (_debugOverlays[_currentDebugOverlay] == _planetOverlay)
        {
            _currentDebugOverlay = (_currentDebugOverlay + 1) % _debugOverlays.Count;
        }

        ChangePlanetOverlay(_debugOverlays[_currentDebugOverlay]);
    }

    public void SetBiomeView()
    {
        SetView(PlanetView.Biomes);
    }

    public void SetElevationView()
    {
        SetView(PlanetView.Elevation);
    }

    public void SetCoastlineView()
    {
        SetView(PlanetView.Coastlines);
    }

    private void ReadKeyboardInput()
    {
        if (_backgroundProcessActive)
        {
            // Do not process any keyboard inputs while a background process
            // (generate/load/save/export) is executing
            return;
        }

        ReadKeyboardInput_TimeControls();
        ReadKeyboardInput_Escape();
        ReadKeyboardInput_Menus();
        ReadKeyboardInput_Globe();
        ReadKeyboardInput_MapViews();
        ReadKeyboardInput_MapOverlays();
        ReadKeyboardInput_Navigation();
    }

    private bool IsPolityOverlay(PlanetOverlay overlay)
    {
        return (overlay == PlanetOverlay.PolityCulturalActivity) ||
            (overlay == PlanetOverlay.PolityCulturalSkill) ||
            (overlay == PlanetOverlay.PolityCulturalPreference) ||
            (overlay == PlanetOverlay.PolityCulturalKnowledge) ||
            (overlay == PlanetOverlay.PolityCulturalDiscovery) ||
            (overlay == PlanetOverlay.PolityAdminCost) ||
            (overlay == PlanetOverlay.PolityTerritory) ||
            (overlay == PlanetOverlay.PolityContacts) ||
            (overlay == PlanetOverlay.PolitySelection) ||
            (overlay == PlanetOverlay.PolityCoreRegions) ||
            (overlay == PlanetOverlay.General);
    }

    private void UpdateFocusPanel()
    {
        Polity selectedPolity = null;
        bool isUnderFocus = false;

        if ((Manager.CurrentWorld.SelectedTerritory != null) && IsPolityOverlay(_planetOverlay))
        {
            selectedPolity = Manager.CurrentWorld.SelectedTerritory.Polity;

            isUnderFocus |= (Manager.CurrentWorld.PolitiesUnderPlayerFocus.Contains(selectedPolity));
        }

        if (selectedPolity != null)
        {
            FocusPanelScript.SetVisible(true);

            if (isUnderFocus)
                FocusPanelScript.SetState(FocusPanelState.UnsetFocus, selectedPolity);
            else
                FocusPanelScript.SetState(FocusPanelState.SetFocus, selectedPolity);
        }
        else
        {
            FocusPanelScript.SetVisible(false);
        }
    }

    public void ZoomAndShiftMap(float scale, WorldPosition mapPosition)
    {
        PlanetScript.ZoomAndCenterCamera(Mathf.Clamp01(scale * 1.4f), mapPosition, 1);
        MapScript.ZoomAndShiftMap(scale, mapPosition, 1);
    }

    private void CenterAndZoomOnRect(RectInt rect)
    {
        float hScale = rect.width / (float)Manager.CurrentWorld.Width;
        float vScale = rect.height / (float)Manager.CurrentWorld.Height;

        float scale = Mathf.Max(hScale, vScale);

        // zoom in if the rect is very small but only parthway thru
        scale = Mathf.Clamp01(Mathf.Lerp(scale, 2f, 0.1f));

        ZoomAndShiftMap(scale, rect.center);
    }

    private void SelectAndCenterOnCell(WorldPosition position)
    {
        ZoomAndShiftMap(0.5f, position);

        Manager.SetSelectedCell(position);

        MapEntitySelected.Invoke();
    }

    private string GetMessageToShow(WorldEventMessage eventMessage)
    {
        return Manager.GetDateString(eventMessage.Date) + " - " + eventMessage.Message;
    }

    private void ShowEventMessageForPolity(WorldEventMessage eventMessage, Identifier polityId)
    {
        Polity polity = Manager.CurrentWorld.GetPolity(polityId);

        if (polity != null)
        {
            WorldPosition corePosition = polity.CoreGroup.Position;

            EventPanelScript.AddEventMessage(GetMessageToShow(eventMessage), () =>
            {
                SelectAndCenterOnCell(corePosition);

                if ((_planetOverlay != PlanetOverlay.PolityTerritory) && (_planetOverlay != PlanetOverlay.General))
                    ChangePlanetOverlay(PlanetOverlay.PolityTerritory);
            });
        }
        else
        {
            EventPanelScript.AddEventMessage(GetMessageToShow(eventMessage));
        }
    }

    private void ShowEventMessage(WorldEventMessage eventMessage)
    {
        if (eventMessage is TribeSplitEventMessage)
        {
            TribeSplitEventMessage tribeSplitEventMessage = eventMessage as TribeSplitEventMessage;

            ShowEventMessageForPolity(eventMessage, tribeSplitEventMessage.NewTribeId);
        }
        else if (eventMessage is PolityFormationEventMessage)
        {
            PolityFormationEventMessage polityFormationEventMessage = eventMessage as PolityFormationEventMessage;

            ShowEventMessageForPolity(eventMessage, polityFormationEventMessage.PolityId);
        }
        else if (eventMessage is DiscoveryEventMessage)
        {
            DiscoveryEventMessage discoveryEventMessage = eventMessage as DiscoveryEventMessage;

            EventPanelScript.AddEventMessage(GetMessageToShow(discoveryEventMessage), () =>
            {
                SelectAndCenterOnCell(discoveryEventMessage.Position);

                SetPopCulturalDiscoveryOverlay(discoveryEventMessage.Discovery.Id);
            });
        }
        else if (eventMessage is CellEventMessage)
        {
            CellEventMessage cellEventMessage = eventMessage as CellEventMessage;

            EventPanelScript.AddEventMessage(GetMessageToShow(cellEventMessage), () =>
            {
                SelectAndCenterOnCell(cellEventMessage.Position);
            });
        }
        else
        {
            EventPanelScript.AddEventMessage(GetMessageToShow(eventMessage));
        }
    }

    public void ProgressUpdate(float value, string message = null, bool reset = false)
    {
        if (reset || (value >= _progressValue))
        {
            if (message != null)
                _progressMessage = message;

            _progressValue = value;
        }
    }

    public void MenuUninterruptSimulation()
    {
        if (!_eventPauseActive)
        {
            InterruptSimulation(false);
        }
    }

    public void UninterruptSimAndShowHiddenInterPanels()
    {
        MenuUninterruptSimulation();

        ShowHiddenInteractionPanels();
    }

    public void CloseMainMenu()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        UninterruptSimAndShowHiddenInterPanels();
    }

    public void CloseOptionsMenu()
    {
        OptionsDialogPanelScript.SetVisible(false);

        UninterruptSimAndShowHiddenInterPanels();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void SetFullscreen(bool state)
    {
        Manager.SetFullscreen(state);
    }

    public void SetUIScaling(bool state)
    {
        Manager.SetUIScaling(state);

        if (state)
        {
            CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }
        else
        {
            CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
    }

    public void ToogleDevMode()
    {
        DevMode nextDevMode;

        switch (Manager.CurrentDevMode)
        {
            case DevMode.None:
                nextDevMode = DevMode.Basic;
                break;
            case DevMode.Basic:
                nextDevMode = DevMode.Advanced;
                break;
            case DevMode.Advanced:
                nextDevMode = DevMode.None;
                break;
            default:
                throw new System.Exception("Unhandled Dev Mode: " + Manager.CurrentDevMode);
        }

        Manager.CurrentDevMode = nextDevMode;

        if (nextDevMode != DevMode.None)
        {
            Manager.CurrentWorld.EventsTriggered = 0;
            Manager.CurrentWorld.EventsEvaluated = 0;

            Manager.CurrentWorld.EventEvalStatsPerType.Clear();

            _mapUpdateCount = 0;
            _pixelUpdateCount = 0;
            _timeSinceLastMapUpdate = 0;

            Manager.LastEventsTriggeredCount = 0;
            Manager.LastEventsEvaluatedCount = 0;

            Manager.LastEventEvalStatsPerType.Clear();

            Manager.LastMapUpdateCount = 0;
            Manager.LastPixelUpdateCount = 0;
            Manager.LastDevModeDateSpan = 0;

            if (Manager.WorldIsReady)
            {
                Manager.LastDevModeUpdateDate = Manager.CurrentWorld.CurrentDate;
            }
            else
            {
                Manager.LastDevModeUpdateDate = 0;
            }
        }
    }

    public void ToogleAnimationShaders(bool state)
    {
        Manager.AnimationShadersEnabled = state;

        _regenMapOverlayTexture = true;
    }

    public void SetGenerationSeed()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        int seed = Random.Range(0, int.MaxValue);

        SetSeedDialogPanelScript.SetSeed(seed);

        SetSeedDialogPanelScript.SetVisible(true);

        InterruptSimulation(true);
    }

    public void CloseErrorMessageAction()
    {
        ErrorMessageDialogPanelScript.SetVisible(false);

        _closeErrorActionToPerform?.Invoke();
        _closeErrorActionToPerform = null;
    }

    public void CloseExceptionMessageAction()
    {
        Exit();
    }

    private void PostProgressOp_RegenerateWorld()
    {
        Debug.Log("Finished regenerating world with seed: " + Manager.CurrentWorld.Seed);

        Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;

        SelectionPanelScript.RemoveAllOptions();

        _postProgressOp -= PostProgressOp_RegenerateWorld;

        _regenerateWorldPostProgressOp?.Invoke();
        WorldRegenerated.Invoke();
    }

    public void RegenerateWorldDrainage()
    {
        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    public void RegenerateWorldAltitudeScaleChange(float value)
    {
        Manager.AltitudeScale = value;

        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    public void RegenerateWorldSeaLevelOffsetChange(float value)
    {
        Manager.SeaLevelOffset = value;

        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    public void RegenerateWorldRiverLevelOffsetChange(float value)
    {
        Manager.RiverStrength = value;

        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    public void RegenerateWorldTemperatureOffsetChange(float value)
    {
        Manager.TemperatureOffset = value;

        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    public void RegenerateWorldRainfallOffsetChange(float value)
    {
        Manager.RainfallOffset = value;

        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    public void RegenerateWorldLayerFrequencyChange(string layerId, float value)
    {
        LayerSettings settings = Manager.GetLayerSettings(layerId);

        settings.Frequency = value;

        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    public void RegenerateWorldLayerNoiseInfluenceChange(string layerId, float value)
    {
        LayerSettings settings = Manager.GetLayerSettings(layerId);

        settings.SecondaryNoiseInfluence = value;

        RegenerateWorld(GenerationType.TerrainRegeneration);
    }

    private void RegenerateWorld(GenerationType type)
    {
        ProgressDialogPanelScript.SetVisible(true);

        ProgressUpdate(0, "Regenerating World...", true);

        Manager.RegenerateWorldAsync(type, ProgressUpdate);

        _postProgressOp += PostProgressOp_RegenerateWorld;

        _backgroundProcessActive = true;

        _regenMapTexture = true;
        _regenMapOverlayTexture = true;
    }

    private void GenerateWorld(bool randomSeed = true, int seed = 0, bool useHeightmap = false)
    {
        if (randomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }

        GenerateWorldInternal(seed, useHeightmap);
    }

    private void ShowErrorMessage(string message, System.Action closeErrorActionToPerform = null)
    {
        _closeErrorActionToPerform = closeErrorActionToPerform;

        ErrorMessageDialogPanelScript.SetDialogText(message);
        ErrorMessageDialogPanelScript.SetVisible(true);
    }

    public void GenerateWorldWithCustomSeed()
    {
        int seed = SetSeedDialogPanelScript.GetSeed();

        string errorMessage = "Invalid Input, please use a value between 0 and " + int.MaxValue;

        if (seed < 0)
        {
            ShowErrorMessage(errorMessage, SetGenerationSeed);
            return;
        }

        GenerateWorldInternal(seed);
    }

    private void PostProgressOp_GenerateWorld()
    {
        EventPanelScript.DestroyMessagePanels(); // We don't want to keep messages referencing previous worlds

        Debug.Log("Finished generating world with seed: " + Manager.CurrentWorld.Seed);

        string activeModStrs = string.Join(",", Manager.ActiveModPaths);

        Debug.Log("Active Mods: " + activeModStrs);

        Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;

        SelectionPanelScript.RemoveAllOptions();

        // It's safer to return to map mode after loading or generating a new world
        SetGlobeView(false);

        _hasToSetInitialPopulation = true;

        ValidateLayersPresent();
        OpenModeSelectionDialog();

        _selectedMaxSpeedLevelIndex = Manager.StartSpeedIndex;

        ResetSimulationState();

        SetMaxSpeedLevel(_selectedMaxSpeedLevelIndex);

        _postProgressOp -= PostProgressOp_GenerateWorld;

        _generateWorldPostProgressOp?.Invoke();
        WorldGenerated.Invoke();
    }

    private void GenerateWorldInternal(int seed, bool useHeightmap = false)
    {
        ResetGuiManagerState();

        ProgressDialogPanelScript.SetVisible(true);

        ProgressUpdate(0, "Generating World...", true);

        if (SetSeedDialogPanelScript.UseHeightmapToggle.isOn || useHeightmap)
        {
            Manager.GenerateNewWorldAsync(seed, _heightmap, ProgressUpdate);
        }
        else
        {
            Manager.GenerateNewWorldAsync(seed, null, ProgressUpdate);
        }

        _postProgressOp += PostProgressOp_GenerateWorld;

        _backgroundProcessActive = true;

        _regenMapTexture = true;
        _regenMapOverlayTexture = true;
        _regenPointerOverlayTextures = true;
    }

    public void SetInitialPopulationForTests()
    {
        int population = (int)Mathf.Ceil(World.StartPopulationDensity * TerrainCell.MaxArea);

        Manager.GenerateRandomHumanGroup(population);

        InterruptSimulation(false);

        DisplayTip_MapScroll();
    }

    public void OpenModeSelectionDialog()
    {
        MapScript.EnablePointerOverlay(false);

        OpenModeSelectionDialogRequested.Invoke();

        Debug.Log("Player went back to mode selection dialog.");

        InterruptSimulation(true);
    }

    public void RandomPopulationPlacement()
    {
        int population = AddPopulationDialogScript.Population;

        AddPopulationDialogScript.SetVisible(false);

        SetStartingSpeed(AddPopulationDialogScript.StartSpeedLevelIndex);

        Debug.Log(string.Format("Player chose to do random population placement of {0}...", population));

        if (population <= 0)
            return;

        Manager.GenerateRandomHumanGroup(population);

        UninterruptSimAndShowHiddenInterPanels();

        DisplayTip_MapScroll();
    }

    private void ClickOp_SelectCell(Vector2 mapPosition)
    {
        int longitude = (int)mapPosition.x;
        int latitude = (int)mapPosition.y;

        Manager.SetSelectedCell(longitude, latitude);

        MapEntitySelected.Invoke();
    }

    private void ClickOp_SelectRequestTarget(Vector2 mapPosition)
    {
        int longitude = (int)mapPosition.x;
        int latitude = (int)mapPosition.y;

        TerrainCell clickedCell = Manager.CurrentWorld.GetCell(longitude, latitude);

        if (Manager.CurrentInputRequest is RegionSelectionRequest rsRequest)
        {
            _doneHandlingRequest = TryCompleteRegionSelectionRequest(rsRequest, clickedCell);
            return;
        }
        else if (Manager.CurrentInputRequest is ContactSelectionRequest csRequest)
        {
            _doneHandlingRequest = TryCompleteContactSelectionRequest(csRequest, clickedCell);
            return;
        }
        else if (Manager.CurrentInputRequest is GroupSelectionRequest gsRequest)
        {
            _doneHandlingRequest = TryCompleteGroupSelectionRequest(gsRequest, clickedCell);
            return;
        }
        else if (Manager.CurrentInputRequest is FactionSelectionRequest fsRequest)
        {
            _doneHandlingRequest = TryCompleteFactionSelectionRequest(fsRequest, clickedCell);
            return;
        }

        throw new System.NotImplementedException(
            $"No method defined to complete input request of type: {Manager.CurrentInputRequest.GetType()}");
    }

    private void CompleteSelectRequestTargetOp()
    {
        HideNonBlockingMessage.Invoke();

        RevertTempPlanetOverlay();

        _mapLeftClickOp -= ClickOp_SelectRequestTarget;
    }

    private bool TryCompleteContactSelectionRequest(
        ContactSelectionRequest request,
        TerrainCell targetCell)
    {
        var guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't satisfy request without an active guided faction");
        }

        var guidedPolity = guidedFaction.Polity;

        var targetTerritory = targetCell.EncompassingTerritory;

        if ((targetTerritory != null) &&
            (targetTerritory.SelectionFilterType == Territory.FilterType.Selectable))
        {
            var targetPolity = targetTerritory.Polity;

            var targetContact = guidedPolity.GetContact(targetPolity);

            if (targetContact == null)
            {
                throw new System.Exception($"Unable to find contact between {guidedPolity.Id} and {targetPolity.Id}");
            }

            request.Set(targetContact);
            request.Close();

            CompleteSelectRequestTargetOp();
            return true;
        }

        return false;
    }

    private bool TryCompleteFactionSelectionRequest(
        FactionSelectionRequest request,
        TerrainCell targetCell)
    {
        var guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't satisfy request without an active guided faction");
        }

        var targetTerritory = targetCell.EncompassingTerritory;

        if ((targetTerritory != null) &&
            (targetTerritory.SelectionFilterType == Territory.FilterType.Involved))
        {
            var targetFaction = targetCell.GetClosestFaction(targetTerritory.Polity);

            if (targetFaction.SelectionFilterType == Faction.FilterType.Selectable)
            {
                request.Set(targetFaction);
                request.Close();

                CompleteSelectRequestTargetOp();
                return true;
            }
        }

        return false;
    }

    private bool TryCompleteRegionSelectionRequest(
        RegionSelectionRequest request,
        TerrainCell targetCell)
    {
        Region targetRegion = targetCell.Region;

        if ((targetRegion != null) &&
            (targetRegion.SelectionFilterType == Region.FilterType.Selectable))
        {
            request.Set(targetRegion);
            request.Close();

            CompleteSelectRequestTargetOp();
            return true;
        }

        return false;
    }

    private bool TryCompleteGroupSelectionRequest(
        GroupSelectionRequest request,
        TerrainCell targetCell)
    {
        CellGroup targetGroup = targetCell.Group;

        if ((targetGroup != null) &&
            (targetCell.SelectionFilterType == TerrainCell.FilterType.Selectable))
        {
            request.Set(targetGroup);
            request.Close();

            CompleteSelectRequestTargetOp();
            return true;
        }

        return false;
    }

    private void ClickOp_SelectPopulationPlacement(Vector2 mapPosition)
    {
        int population = AddPopulationDialogScript.Population;

        if (AddPopulationGroupAtPosition(mapPosition, population))
        {
            UninterruptSimAndShowHiddenInterPanels();

            DisplayTip_MapScroll();

            _mapLeftClickOp -= ClickOp_SelectPopulationPlacement;
        }
    }

    public void SelectPopulationPlacement()
    {
        int population = AddPopulationDialogScript.Population;

        AddPopulationDialogScript.SetVisible(false);

        SetStartingSpeed(AddPopulationDialogScript.StartSpeedLevelIndex);

        Debug.Log(string.Format("Player chose to select cell for population placement of {0}...", population));

        if (population <= 0)
            return;

        DisplayTip_InitialPopulationPlacement();

        _mapLeftClickOp += ClickOp_SelectPopulationPlacement;
    }

    private bool AddPopulationGroupAtPosition(Vector2 mapPosition, int population)
    {
        World world = Manager.CurrentWorld;

        int longitude = (int)mapPosition.x;
        int latitude = (int)mapPosition.y;

        if ((longitude < 0) || (longitude >= world.Width))
            return false;

        if ((latitude < 0) || (latitude >= world.Height))
            return false;

        TerrainCell cell = world.GetCell(longitude, latitude);

        if (cell.IsLiquidSea)
            return false;

        Manager.GenerateHumanGroup(longitude, latitude, population);

        return true;
    }

    private void DisplayTip_InitialPopulationPlacement()
    {
        if (_displayedTip_initialPopulation)
        {
            QuickTipPanelScript.SetVisible(false);
            return;
        }

        string message = "Left click on any non-ocean position in the map to place the initial population group\n";

        if (!_displayedTip_mapScroll)
        {
            message += "Right click and drag with the mouse to scroll the map left or right\n";
        }

        message += "\n(Click anywhere on this message to close)";

        QuickTipPanelScript.SetText(message);
        QuickTipPanelScript.Reset(10);

        QuickTipPanelScript.SetVisible(true);

        _displayedTip_initialPopulation = true;
        _displayedTip_mapScroll = true;
    }

    private void DisplayTip_MapScroll()
    {
        if (_displayedTip_mapScroll)
        {
            QuickTipPanelScript.SetVisible(false);
            return;
        }

        QuickTipPanelScript.SetText(
            "Right click and drag with the mouse to scroll the map left or right\n" +
            "\n(Click anywhere on this message to close)");
        QuickTipPanelScript.Reset(10);

        QuickTipPanelScript.SetVisible(true);

        _displayedTip_mapScroll = true;
    }

    private void LoadHeightmapAction()
    {
        string path = LoadFileDialogPanelScript.GetPathToLoad();
        Texture2D texture = Manager.LoadTexture(path);

        if (texture == null)
        {
            SetSeedDialogPanelScript.SetImageTexture(Path.GetFileName(path), null, TextureValidationResult.Unknown);
        }
        else
        {
            TextureValidationResult result = Manager.ValidateTexture(texture);

            SetSeedDialogPanelScript.SetImageTexture(Path.GetFileName(path), texture, result);
        }

        _heightmap = texture;

        SetSeedDialogPanelScript.SetVisible(true);
    }

    private void CancelLoadHeightmapAction()
    {
        SetSeedDialogPanelScript.SetVisible(true);
    }

    public void LoadHeightmapImage()
    {
        LoadFileDialogPanelScript.Initialize(
            "Select Heightmap Image to Load...",
            "Load",
            LoadHeightmapAction,
            CancelLoadHeightmapAction,
            Manager.HeightmapsPath,
            Manager.SupportedHeightmapFormats);

        LoadFileDialogPanelScript.SetVisible(true);
    }

    private bool HasFilesToLoad()
    {
        string dirPath = Manager.SavePath;

        string[] files = Directory.GetFiles(dirPath, "*.plnt");

        return files.Length > 0;
    }

    public void ExportMapAction()
    {
        ActivityDialogPanelScript.SetVisible(true);

        ActivityDialogPanelScript.SetDialogText("Exporting map to PNG file...");

        string imageName = ExportMapDialogPanelScript.GetText();

        string path = Path.Combine(Manager.ExportPath, imageName + ".png");

        Manager.ExportMapTextureToFileAsync(path, MapScript);

        _postProgressOp += PostProgressOp_ExportAction;

        _backgroundProcessActive = true;
    }

    public void ExportImageAs()
    {
        OptionsDialogPanelScript.SetVisible(false);

        string planetViewStr = "";

        switch (_planetView)
        {
            case PlanetView.Biomes: planetViewStr = "_biomes"; break;
            case PlanetView.Coastlines: planetViewStr = "_coastlines"; break;
            case PlanetView.Elevation: planetViewStr = "_elevation"; break;
            default: throw new System.Exception("Unexpected planet view type: " + _planetView);
        }

        string planetOverlayStr;

        switch (_planetOverlay)
        {
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
            case PlanetOverlay.PolityCluster:
                planetOverlayStr = "_polity_clusters";
                break;
            case PlanetOverlay.ClusterAdminCost:
                planetOverlayStr = "_cluster_admin_cost";
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
            case PlanetOverlay.PolityCoreRegions:
                planetOverlayStr = "_polity_core_regions";
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
            case PlanetOverlay.PolityAdminCost:
                planetOverlayStr = "_polity_admin_cost";
                break;
            case PlanetOverlay.PolitySelection:
                planetOverlayStr = "_polity_select";
                break;
            case PlanetOverlay.Temperature:
                planetOverlayStr = "_temperature";
                break;
            case PlanetOverlay.Rainfall:
                planetOverlayStr = "_rainfall";
                break;
            case PlanetOverlay.DrainageBasins:
                planetOverlayStr = "_drainage_basins";
                break;
            case PlanetOverlay.Arability:
                planetOverlayStr = "_arability";
                break;
            case PlanetOverlay.Accessibility:
                planetOverlayStr = "_accessibility";
                break;
            case PlanetOverlay.Hilliness:
                planetOverlayStr = "_hilliness";
                break;
            case PlanetOverlay.BiomeTrait:
                planetOverlayStr = "_biome_trait_" + _planetOverlaySubtype;
                break;
            case PlanetOverlay.Layer:
                planetOverlayStr = "_layer_" + _planetOverlaySubtype;
                break;
            case PlanetOverlay.Region:
                planetOverlayStr = "_region";
                break;
            case PlanetOverlay.RegionSelection:
                planetOverlayStr = "_region_select";
                break;
            case PlanetOverlay.CellSelection:
                planetOverlayStr = "_cell_select";
                break;
            case PlanetOverlay.FactionSelection:
                planetOverlayStr = "_faction_select";
                break;
            case PlanetOverlay.PopChange:
                planetOverlayStr = "_population_change";
                break;
            case PlanetOverlay.MigrationPressure:
                planetOverlayStr = "_migration_pressure";
                break;
            case PlanetOverlay.PolityMigrationPressure:
                planetOverlayStr = "_polity_migration_pressure";
                break;
            case PlanetOverlay.UpdateSpan:
                planetOverlayStr = "_update_span";
                break;
            case PlanetOverlay.Migration:
                planetOverlayStr = "_migration_event";
                break;
            default: throw new System.Exception("Unexpected planet overlay type: " + _planetOverlay);
        }

        string worldName = Manager.WorldName;

        if (Manager.GameMode == GameMode.Simulator)
        {
            worldName = Manager.AddDateToWorldName(worldName);
        }

        ExportMapDialogPanelScript.SetText(worldName + planetViewStr + planetOverlayStr);
        ExportMapDialogPanelScript.SetVisible(true);

        InterruptSimulation(true);
    }

    /// <summary>Open the intial population setup dialog if needed.</summary>
    /// <returns>
    ///   <c>true</c> if the initial population setup dialog is activated; otherwise, <c>false</c>.
    /// </returns>
    public bool AttemptToSetInitialPopulation()
    {
        if (_hasToSetInitialPopulation)
        {
            int startingPopulation = (int)Mathf.Ceil(World.StartPopulationDensity * TerrainCell.MaxArea);

            startingPopulation = Mathf.Clamp(startingPopulation, World.MinStartingPopulation, World.MaxStartingPopulation);

            AddPopulationDialogScript.InitializeAndShow(startingPopulation, _selectedMaxSpeedLevelIndex);

            _hasToSetInitialPopulation = false;

            return true;
        }

        return false;
    }

    /// <summary>Called after a save attempt is finished (save completed) or canceled.</summary>
    public void SaveAttemptCompleted()
    {
        if (Manager.GameMode == GameMode.Simulator)
        {
            _worldCouldBeSavedAfterEdit = false;

            if (AttemptToSetInitialPopulation())
            {
                return;
            }
        }

        if (!_eventPauseActive)
        {
            InterruptSimulation(!Manager.SimulationCanRun);
        }

        ShowHiddenInteractionPanels();
    }

    private void PostProgressOp_SaveAction()
    {
        Debug.Log("Finished saving world to file.");

        LoadButton.interactable = HasFilesToLoad();

        _postProgressOp -= PostProgressOp_SaveAction;

        SaveAttemptCompleted();
    }

    private void PostProgressOp_ExportAction()
    {
        Debug.Log("Finished exporting world map to .png file.");

        _postProgressOp -= PostProgressOp_ExportAction;

        if (!_eventPauseActive)
        {
            InterruptSimulation(!Manager.SimulationCanRun);
        }

        ShowHiddenInteractionPanels();
    }

    public void SaveAction()
    {
        ActivityDialogPanelScript.SetVisible(true);

        ActivityDialogPanelScript.SetDialogText("Saving World...");

        string saveName = SaveFileDialogPanelScript.GetText();

        Manager.WorldName = Manager.RemoveDateFromWorldName(saveName);

        string path = Path.Combine(Manager.SavePath, saveName + ".plnt");

        Manager.SaveWorldAsync(path);

        _postProgressOp += PostProgressOp_SaveAction;

        _backgroundProcessActive = true;
    }

    public void SaveWorldAs()
    {
        // We can't try saving a world that is not completely generated
        if (Manager.CurrentWorld.NeedsDrainageRegeneration)
            return;

        MainMenuDialogPanelScript.SetVisible(false);

        string worldName = Manager.WorldName;

        if (Manager.GameMode == GameMode.Simulator)
        {
            worldName = Manager.AddDateToWorldName(worldName);
        }

        SaveFileDialogPanelScript.SetText(worldName);
        SaveFileDialogPanelScript.SetCancelButtonText("Cancel");
        SaveFileDialogPanelScript.SetRecommendationTextVisible(false);

        SaveFileDialogPanelScript.SetVisible(true);

        InterruptSimulation(true);
    }

    public void SaveEditedWorldBeforeStarting()
    {
        SaveFileDialogPanelScript.SetText(Manager.WorldName);
        SaveFileDialogPanelScript.SetCancelButtonText("Skip");
        SaveFileDialogPanelScript.SetRecommendationTextVisible(true);

        SaveFileDialogPanelScript.SetVisible(true);
    }

    private void GetMaxSpeedOptionFromCurrentWorld()
    {
        float maxSpeed = Manager.CurrentWorld.MaxTimeToSkip / MaxDeltaTimeIterations;

        for (int i = 0; i < Speed.Levels.Length; i++)
        {
            if (maxSpeed <= Speed.Levels[i])
            {
                _selectedMaxSpeedLevelIndex = i;

                SetMaxSpeedLevel(_selectedMaxSpeedLevelIndex);

                break;
            }
        }
    }

    public void IncreaseMaxSpeed()
    {
        if (_selectedMaxSpeedLevelIndex == _topMaxSpeedLevelIndex)
            return;

        _selectedMaxSpeedLevelIndex++;

        SetMaxSpeedLevel(_selectedMaxSpeedLevelIndex);
    }

    public void DecreaseMaxSpeed()
    {
        if (_selectedMaxSpeedLevelIndex == 0)
            return;

        _selectedMaxSpeedLevelIndex--;

        SetMaxSpeedLevel(_selectedMaxSpeedLevelIndex);
    }

    /// <summary>
    /// Update Current World Speed to selected and update UI
    /// </summary>
    /// <returns>The now current speed</returns>
    private Speed SetMaxSpeedToSelected()
    {
        bool holdState = !Manager.SimulationRunning;

        OnFirstMaxSpeedOptionSet.Invoke(
            holdState || (_selectedMaxSpeedLevelIndex == 0));
        OnLastMaxSpeedOptionSet.Invoke(
            holdState || (_selectedMaxSpeedLevelIndex == _topMaxSpeedLevelIndex));

        // This is the max amount of iterations to simulate per second
        Speed selectedSpeed = Speed.Levels[_selectedMaxSpeedLevelIndex];

        // This is the max amount of iterations to simulate per frame
        int maxSpeed = (int)Mathf.Ceil(selectedSpeed * MaxDeltaTimeIterations);

        Manager.CurrentWorld.SetMaxTimeToSkip(maxSpeed);

        if (holdState)
            return Speed.Zero;

        return selectedSpeed;
    }

    /// <summary>
    /// Sets the simulation's max speed, but validates it can be done first
    /// </summary>
    /// <param name="speedLevelIndex">The speed level index to use</param>
    private void SetMaxSpeedLevelIfNotPaused(int speedLevelIndex)
    {
        if (_pausingConditionActive || _pauseButtonPressed)
        {
            return;
        }

        SetMaxSpeedLevel(speedLevelIndex);
    }

    /// <summary>
    /// Sets the simulation's max speed
    /// </summary>
    /// <param name="speedLevelIndex">The speed level index to use</param>
    private void SetMaxSpeedLevel(int speedLevelIndex)
    {
        _selectedMaxSpeedLevelIndex = Mathf.Clamp(speedLevelIndex, 0, _topMaxSpeedLevelIndex);

        OnSimulationSpeedChanged.Invoke(SetMaxSpeedToSelected());

        ResetAccDeltaTime();
    }

    private void ResetAccDeltaTime()
    {
        _accDeltaTime = 0;
        _simulationDateSpan = 0;
    }

    public void SetGameModeAccordingToCurrentWorld()
    {
        if (!Manager.SimulationCanRun)
        {
            _hasToSetInitialPopulation = true;

            OpenModeSelectionDialog();
        }
        else
        {
            UninterruptSimAndShowHiddenInterPanels();

            SetSimulatorMode();

            GetMaxSpeedOptionFromCurrentWorld();

            // Always start paused for running worlds
            PlayerPauseSimulation(true);
        }
    }

    private void ValidateLayersPresent()
    {
        Manager.LayersPresent = Layer.Layers.Count > 0;

        // Disable layer overlay option if no layers are present in this world
        OverlayDialogPanelScript.SetLayerOverlay(Manager.LayersPresent);
    }

    private void PostProgressOp_LoadAction()
    {
        EventPanelScript.DestroyMessagePanels(); // We don't want to keep messages referencing previous worlds

        Debug.Log(string.Format(
            "Finished loading world. Seed: {0}, Avg. Temperature: {1}, Avg. Rainfall: {2}, Sea Level Offset: {3}, River Level Strength: {4}, Current Date: {5}",
            Manager.CurrentWorld.Seed,
            Manager.CurrentWorld.TemperatureOffset,
            Manager.CurrentWorld.RainfallOffset,
            Manager.CurrentWorld.SeaLevelOffset,
            Manager.CurrentWorld.RiverStrength,
            Manager.GetDateString(Manager.CurrentWorld.CurrentDate)));

        string activeModStrs = string.Join(",", Manager.ActiveModPaths);

        Debug.Log("Active Mods: " + activeModStrs);

        SelectionPanelScript.RemoveAllOptions();

        // It's safer to return to map mode after loading or generating a new world
        SetGlobeView(false);

        ValidateLayersPresent();
        SetGameModeAccordingToCurrentWorld();

        ResetSimulationState();

        _postProgressOp -= PostProgressOp_LoadAction;

        _loadWorldPostProgressOp?.Invoke();
        WorldLoaded.Invoke();
    }

    private void ResetSimulationState()
    {
        _resolvingDecision = false;
        _willFinishResolvingDecision = false;
        _doneHandlingRequest = false;
        _resolvingEffects = false;
    }

    /// <summary>Resets the state of the GUI manager before loading or generating a new world.</summary>
    private void ResetGuiManagerState()
    {
        // Ignore the result of editing a world that is being discarded
        _worldCouldBeSavedAfterEdit = false;
        _hasToSetInitialPopulation = false;

        // Make sure we don't carry this incomplete left click operation
        // to the next world
        _mapLeftClickOp -= ClickOp_SelectPopulationPlacement;

        // Reset overlays to avoid showing an invalid overlay by accident
        ChangePlanetOverlay(PlanetOverlay.None, Manager.NoOverlaySubtype);

        _planetOverlaySubtypeCache.Clear();
    }


    private void LoadAction()
    {
        ResetGuiManagerState();

        ProgressDialogPanelScript.SetVisible(true);

        ProgressUpdate(0, "Loading World...", true);

        string path = LoadFileDialogPanelScript.GetPathToLoad();

        Manager.LoadWorldAsync(path, ProgressUpdate);

        Manager.WorldName = Manager.RemoveDateFromWorldName(Path.GetFileNameWithoutExtension(path));

        _postProgressOp += PostProgressOp_LoadAction;

        _backgroundProcessActive = true;

        _regenMapTexture = true;
        _regenMapOverlayTexture = true;
        _regenPointerOverlayTextures = true;
    }

    private void CancelLoadAction()
    {
        UninterruptSimAndShowHiddenInterPanels();
    }

    public void LoadWorld()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        LoadFileDialogPanelScript.Initialize(
            "Select World to Load...",
            "Load",
            LoadAction,
            CancelLoadAction,
            Manager.SavePath,
            new string[] { ".plnt" });

        LoadFileDialogPanelScript.SetVisible(true);

        InterruptSimulation(true);
    }

    private bool TryResolvePendingDecisions()
    {
        while (Manager.CurrentWorld.HasModDecisionsToResolve())
        {
            var initializer = Manager.CurrentWorld.PullModDecisionToResolve();

            var decisionToResolve = initializer.Decision;

            decisionToResolve.InitEvaluation(initializer);

            Faction targetFaction = initializer.TargetFaction;

            if (decisionToResolve.DebugPlayerGuidance &&
                !targetFaction.IsUnderPlayerGuidance &&
                (Manager.CurrentDevMode == DevMode.Advanced))
            {
                Manager.SetGuidedFaction(targetFaction);
            }

            if (targetFaction.IsUnderPlayerGuidance)
            {
                RequestModDecisionResolution(decisionToResolve);

                Manager.ResolvingPlayerInvolvedDecisionChain = true;

                return false;
            }
            else
            {
                decisionToResolve.AutoEvaluate();
            }
        }

        return true;
    }

    private void RequestModDecisionResolution(ModDecision decisionToResolve)
    {
        int currentSpeedIndex = _selectedMaxSpeedLevelIndex;

        if (_pauseButtonPressed)
        {
            currentSpeedIndex = -1;
        }

        ModDecisionDialogPanelScript.Set(decisionToResolve, currentSpeedIndex);

        if (!IsMenuPanelActive())
        {
            ModDecisionDialogPanelScript.SetVisible(true);
        }
        else
        {
            // Hide the decision dialog until all menu panels are inactive
            HideInteractionPanel(ModDecisionDialogPanelScript);
        }

        _resolvingDecision = true;

        EventStopSimulation();
    }

    private void SetResolvingEffects(bool state)
    {
        _resolvingEffects = state;

        EffectHandlingRequested.Invoke(!state);

        EventPauseSimulation(state);
    }

    /// <summary>
    /// Method called when an action, decision or effect requires the simulation
    /// to change state between running or stopped
    /// </summary>
    /// <param name="state">'true' if the simulation should pause, 'false' if the
    /// simulation should resume</param>
    private void EventPauseSimulation(bool state)
    {
        InterruptSimulation(state);

        _eventPauseActive = state;
    }

    /// <summary>
    /// Method called when an action, decision or effect requires the simulation
    /// to pause
    /// </summary>
    private void EventStopSimulation()
    {
        EventPauseSimulation(true);
    }

    /// <summary>
    /// Method called when an action, decision or effect no longer requires the
    /// simulation to be paused
    /// </summary>
    private void EventResumeSimulation()
    {
        EventPauseSimulation(false);
    }

    private bool TryResolvePendingAction()
    {
        ModAction action = Manager.CurrentWorld.PullActionToExecute();

        if (action == null)
            return true;

        Faction faction = Manager.CurrentWorld.GuidedFaction;

        if (faction == null)
        {
            ShowErrorMessage(
                "The guided faction is no longer present",
                EventResumeSimulation);

            EventStopSimulation();

            return false;
        }

        action.SetTarget(faction);

        if (!action.CanExecute())
        {
            ShowErrorMessage(
                "The requirements to perform the selected action are no longer met",
                EventResumeSimulation);

            EventStopSimulation();

            return false;
        }

        action.SetEffectsToResolve();

        if (!TryResolvePendingEffects())
        {
            SetResolvingEffects(true);

            Manager.ResolvingPlayerInvolvedDecisionChain = true;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Takes care of resolving effects queued by an action or a decision
    /// </summary>
    /// <returns>'true' if all effects have been resolved, or there are none to
    /// resolve, 'false' if there are still effects to be fully resolved</returns>
    private bool TryResolvePendingEffects()
    {
        while (true)
        {
            if (Manager.CurrentWorld.HasEffectsToResolve())
            {
                // if we still had a previous unresolved effect, continue with it,
                // otherwise replace it with the next one on the queue
                _unresolvedEffect =
                    _unresolvedEffect ?? Manager.CurrentWorld.PullEffectToResolve();
            }

            if (_unresolvedEffect == null)
                break;

            if (_unresolvedEffect.TryGetRequest(out InputRequest request))
            {
                HandleInputRequest(request);
                return false;
            }

            _unresolvedEffect.Apply();
            _unresolvedEffect = null;
        }

        return true;
    }

    private void HandleInputRequest(InputRequest request)
    {
        Manager.CurrentInputRequest = request;

        if (request is IMapEntitySelectionRequest mapRequest)
        {
            DisplayNonBlockingMessage.Invoke(mapRequest.Text.GetFormattedString());

            _mapLeftClickOp += ClickOp_SelectRequestTarget;

            CenterAndZoomOnRect(mapRequest.GetEncompassingRectangle());
        }

        if (request is RegionSelectionRequest rsRequest)
        {
            ChangePlanetOverlay(PlanetOverlay.RegionSelection, temporary: true);
            return;
        }
        else if (request is GroupSelectionRequest gsRequest)
        {
            ChangePlanetOverlay(
                PlanetOverlay.CellSelection,
                Manager.GroupProminenceOverlaySubtype,
                temporary: true);
            return;
        }
        else if (request is ContactSelectionRequest csRequest)
        {
            ChangePlanetOverlay(PlanetOverlay.PolitySelection, temporary: true);
            return;
        }
        else if (request is FactionSelectionRequest fsRequest)
        {
            ChangePlanetOverlay(PlanetOverlay.FactionSelection, temporary: true);
            return;
        }

        throw new System.NotImplementedException(
            $"No method defined to handle input request of type: {request.GetType()}");
    }

    /// <summary>
    /// Sets the starting simulation speed
    /// </summary>
    /// <param name="startSpeedLevelIndex">The index for the starting speed to use</param>
    public void SetStartingSpeed(int startSpeedLevelIndex)
    {
        Debug.Log(string.Format("Player set starting speed to {0}...", startSpeedLevelIndex));

        if (startSpeedLevelIndex == -1)
        {
            PlayerPauseSimulation(true);
        }
        else
        {
            _selectedMaxSpeedLevelIndex = Mathf.Clamp(startSpeedLevelIndex, 0, _topMaxSpeedLevelIndex);
            Manager.StartSpeedIndex = _selectedMaxSpeedLevelIndex; // set it as the new default

            SetMaxSpeedToSelected();
        }
    }

    public void ResolveModDecision()
    {
        ModDecisionDialogPanelScript.SetVisible(false);

        if (!TryResolvePendingEffects())
        {
            SetResolvingEffects(true);
        }

        _willFinishResolvingDecision = true;
    }

    public void FinishResolvingDecision()
    {
        int resumeSpeedLevelIndex = ModDecisionDialogPanelScript.ResumeSpeedLevelIndex;

        if (resumeSpeedLevelIndex == -1)
        {
            PlayerPauseSimulation(true);
        }
        else
        {
            SetMaxSpeedLevel(resumeSpeedLevelIndex);

            PlayerPauseSimulation(false);
        }

        EventResumeSimulation();

        _willFinishResolvingDecision = false;
        _resolvingDecision = false;
    }

    public void ChangePlanetOverlayToSelected()
    {
        SelectionPanelScript.RemoveAllOptions();
        SelectionPanelScript.SetVisible(false);

        if (OverlayDialogPanelScript.DontUpdateDialog)
            return;

        OverlayDialogPanelScript.ResetToggles();

        if (OverlayDialogPanelScript.GeneralDataToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.General, false);
        }
        else if (OverlayDialogPanelScript.PopDensityToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PopDensity, false);
        }
        else if (OverlayDialogPanelScript.FarmlandToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.FarmlandDistribution, false);
        }
        else if (OverlayDialogPanelScript.PopCulturalPreferenceToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PopCulturalPreference, false);
        }
        else if (OverlayDialogPanelScript.PopCulturalActivityToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PopCulturalActivity, false);
        }
        else if (OverlayDialogPanelScript.PopCulturalSkillToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PopCulturalSkill, false);
        }
        else if (OverlayDialogPanelScript.PopCulturalKnowledgeToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PopCulturalKnowledge, false);
        }
        else if (OverlayDialogPanelScript.PopCulturalDiscoveryToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PopCulturalDiscovery, false);
        }
        else if (OverlayDialogPanelScript.TerritoriesToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityTerritory, false);
        }
        else if (OverlayDialogPanelScript.PolityClustersToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityCluster, false);
        }
        else if (OverlayDialogPanelScript.ClusterAdminCostToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.ClusterAdminCost, false);
        }
        else if (OverlayDialogPanelScript.DistancesToCoresToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.FactionCoreDistance, false);
        }
        else if (OverlayDialogPanelScript.ProminenceToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityProminence, false);
        }
        else if (OverlayDialogPanelScript.ContactsToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityContacts, false);
        }
        else if (OverlayDialogPanelScript.CoreRegionsToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityCoreRegions, false);
        }
        else if (OverlayDialogPanelScript.PolityCulturalPreferenceToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityCulturalPreference, false);
        }
        else if (OverlayDialogPanelScript.PolityCulturalActivityToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityCulturalActivity, false);
        }
        else if (OverlayDialogPanelScript.PolityCulturalSkillToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityCulturalSkill, false);
        }
        else if (OverlayDialogPanelScript.PolityCulturalKnowledgeToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityCulturalKnowledge, false);
        }
        else if (OverlayDialogPanelScript.PolityCulturalDiscoveryToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityCulturalDiscovery, false);
        }
        else if (OverlayDialogPanelScript.PolityAdminCostToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityAdminCost, false);
        }
        else if (OverlayDialogPanelScript.TemperatureToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Temperature, false);
        }
        else if (OverlayDialogPanelScript.RainfallToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Rainfall, false);
        }
        else if (OverlayDialogPanelScript.DrainageBasinsToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.DrainageBasins, false);
        }
        else if (OverlayDialogPanelScript.ArabilityToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Arability, false);
        }
        else if (OverlayDialogPanelScript.AccessibilityToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Accessibility, false);
        }
        else if (OverlayDialogPanelScript.HillinessToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Hilliness, false);
        }
        else if (OverlayDialogPanelScript.BiomeTraitToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.BiomeTrait, false);
        }
        else if (OverlayDialogPanelScript.LayerToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Layer, false);
        }
        else if (OverlayDialogPanelScript.RegionToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Region, false);
        }
        else if (OverlayDialogPanelScript.LanguageToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Language, false);
        }
        else if (OverlayDialogPanelScript.PopChangeToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PopChange, false);
        }
        else if (OverlayDialogPanelScript.UpdateSpanToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.UpdateSpan, false);
        }
        else if (OverlayDialogPanelScript.MigrationToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.Migration, false);
        }
        else if (OverlayDialogPanelScript.MigrationPressureToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.MigrationPressure, false);
        }
        else if (OverlayDialogPanelScript.PolityMigrationPressureToggle.isOn)
        {
            ChangePlanetOverlay(PlanetOverlay.PolityMigrationPressure, false);
        }
        else
        {
            ChangePlanetOverlay(PlanetOverlay.None, false);
        }

        SetRouteDisplayOverlay(OverlayDialogPanelScript.DisplayRoutesToggle.isOn, false);
        SetGroupActivityOverlay(OverlayDialogPanelScript.DisplayGroupActivityToggle.isOn, false);
    }

    public void OpenMainMenu()
    {
        MainMenuDialogPanelScript.SetVisible(true);

        InterruptSimulation(true);
    }

    public void OpenOptionsMenu()
    {
        OptionsDialogPanelScript.SetVisible(true);

        InterruptSimulation(true);
    }

    public void SetSimulationSpeedStopped(bool state)
    {
        if (state)
        {
            OnSimulationSpeedChanged.Invoke(Speed.Zero);
        }
        else
        {
            OnSimulationSpeedChanged.Invoke(Speed.Levels[_selectedMaxSpeedLevelIndex]);
        }
    }

    /// <summary>
    /// Method called when the player requests the simulation to change state between
    /// running / paused
    /// </summary>
    /// <param name="state">'true' if the player requests the simulation to stop
    /// 'false' if the player requests the simulation to continue</param>
    public void PlayerPauseSimulation(bool state)
    {
        OnSimulationPaused.Invoke(state);

        _pauseButtonPressed = state;

        bool holdState = _pauseButtonPressed;

        HoldSimulation(holdState);
    }

    public void InterruptSimulation(bool state)
    {
        _pausingConditionActive = state;

        OnSimulationInterrupted.Invoke(state);

        bool holdState = _pausingConditionActive || _pauseButtonPressed;

        HoldSimulation(holdState);
    }

    private void HoldSimulation(bool state)
    {
        SetSimulationSpeedStopped(state);

        OnFirstMaxSpeedOptionSet.Invoke(state || (_selectedMaxSpeedLevelIndex == 0));
        OnLastMaxSpeedOptionSet.Invoke(state || (_selectedMaxSpeedLevelIndex == _topMaxSpeedLevelIndex));

        Manager.InterruptSimulation(state);

        ResetAccDeltaTime();
    }

    /// <summary>
    /// Sets or resets the globe (3D) view
    /// </summary>
    /// <param name="state">'true' if globe view should be set, otherwise 'false'</param>
    private void SetGlobeView(bool state)
    {
        Manager.ViewingGlobe = state;

        MapScript.SetVisible(!state);
        PlanetScript.SetVisible(state);

        if (state)
        {
            MapScript.transform.SetParent(GlobeMapPanel.transform);
        }
        else
        {
            MapScript.transform.SetParent(FlatMapPanel.transform);
        }

        ToggledGlobeViewing.Invoke(state);
    }

    /// <summary>
    /// Toggles between map view (2D) and globe view (3D)
    /// </summary>
    public void ToggleGlobeView()
    {
        if (Manager.EditorBrushIsActive)
            return; // Do not allow map projection switching while brush is active

        SetGlobeView(!Manager.ViewingGlobe);
    }

    public void SetRouteDisplayOverlay(bool value)
    {
        SetRouteDisplayOverlay(value, true);
    }

    public void SetRouteDisplayOverlay(bool value, bool invokeEvent)
    {
        _regenMapOverlayTexture |= _displayRoutes != value;

        _displayRoutes = value;

        if (_regenMapOverlayTexture)
        {
            Manager.SetDisplayRoutes(_displayRoutes);

            if (invokeEvent)
            {
                TriggerOverlayEvents();

                _resetOverlays = false;
            }
        }
    }

    public void SetGroupActivityOverlay(bool value)
    {
        SetGroupActivityOverlay(value, true);
    }

    public void SetGroupActivityOverlay(bool value, bool invokeEvent)
    {
        _regenMapOverlayTexture |= _displayGroupActivity != value;

        _displayGroupActivity = value;

        if (_regenMapOverlayTexture)
        {
            Manager.SetDisplayGroupActivity(_displayGroupActivity);

            if (invokeEvent)
            {
                TriggerOverlayEvents();

                _resetOverlays = false;
            }
        }
    }

    public void ChangeToTemperatureOverlayFromEditorToolbar(bool state)
    {
        if (state)
        {
            ChangePlanetOverlay(PlanetOverlay.Temperature);
        }
        else
        {
            ChangePlanetOverlay(PlanetOverlay.None);
        }
    }

    public void ChangeToRainfallOverlayFromEditorToolbar(bool state)
    {
        if (state)
        {
            ChangePlanetOverlay(PlanetOverlay.Rainfall);
        }
        else
        {
            ChangePlanetOverlay(PlanetOverlay.None);
        }
    }

    public void ChangeToLayerOverlayFromEditorToolbar(bool state)
    {
        if (state)
        {
            ChangePlanetOverlay(PlanetOverlay.Layer);
        }
        else
        {
            ChangePlanetOverlay(PlanetOverlay.None);
        }
    }

    public void DisableAllOverlays()
    {
        ChangePlanetOverlay(PlanetOverlay.None);
    }

    public void SetCurrentOverlayIndexInGroup(PlanetOverlay overlay)
    {
        int index = _popOverlays.IndexOf(overlay);

        if (index != -1)
        {
            _currentPopOverlay = index;
            return;
        }

        index = _polityOverlays.IndexOf(overlay);

        if (index != -1)
        {
            _currentPolityOverlay = index;
            return;
        }

        index = _miscOverlays.IndexOf(overlay);

        if (index != -1)
        {
            _currentMiscOverlay = index;
            return;
        }

        index = _debugOverlays.IndexOf(overlay);

        if (index != -1)
        {
            _currentDebugOverlay = index;
            return;
        }
    }

    private void RevertTempPlanetOverlay()
    {
        if (_tempOverlayStack.Count <= 0)
        {
            throw new System.Exception("Temp overlay stack is empty");
        }

        PlanetOverlay prevOverlay = _tempOverlayStack.Pop();
        string prevOverlaySubtype = _tempOverlaySubtypeStack.Pop();

        ChangePlanetOverlay(prevOverlay, prevOverlaySubtype);
    }

    private void ChangePlanetOverlay(
        PlanetOverlay overlay,
        string overlaySubtype,
        bool invokeEvent = true,
        bool temporary = false)
    {
        if (temporary)
        {
            _tempOverlayStack.Push(_planetOverlay);
            _tempOverlaySubtypeStack.Push(_planetOverlaySubtype);
        }

        _regenMapOverlayTexture |= _planetOverlaySubtype != overlaySubtype;
        _regenMapOverlayTexture |= _planetOverlay != overlay;

        if ((_planetOverlay != overlay) && (_planetOverlay != PlanetOverlay.None))
        {
            _planetOverlaySubtypeCache[_planetOverlay] = _planetOverlaySubtype;
        }

        _planetOverlaySubtype = overlaySubtype;

        _planetOverlay = overlay;

        SetCurrentOverlayIndexInGroup(overlay);

        if (invokeEvent)
        {
            Manager.SetPlanetOverlay(_planetOverlay, _planetOverlaySubtype);

            TriggerOverlayEvents();

            _resetOverlays = false;
        }

        HandleOverlayWithSubtypes(overlay);
    }

    public void TriggerOverlayEvents()
    {
        OverlayChanged.Invoke();
        OverlaySubtypeChanged.Invoke();
    }

    private void ChangePlanetOverlay(
        PlanetOverlay overlay,
        bool invokeEvent = true,
        bool temporary = false)
    {
        if (!_planetOverlaySubtypeCache.TryGetValue(overlay, out string currentOverlaySubtype))
        {
            currentOverlaySubtype = Manager.NoOverlaySubtype;
        }

        ChangePlanetOverlay(overlay, currentOverlaySubtype, invokeEvent, temporary);
    }

    private void HandleOverlayWithSubtypes(PlanetOverlay value)
    {
        switch (value)
        {
            case PlanetOverlay.PopCulturalPreference:
                HandleCulturalPreferenceOverlay();
                break;

            case PlanetOverlay.PolityCulturalPreference:
                HandleCulturalPreferenceOverlay();
                break;

            case PlanetOverlay.PopCulturalActivity:
                HandleCulturalActivityOverlay();
                break;

            case PlanetOverlay.PolityCulturalActivity:
                HandleCulturalActivityOverlay();
                break;

            case PlanetOverlay.PopCulturalSkill:
                HandleCulturalSkillOverlay();
                break;

            case PlanetOverlay.PolityCulturalSkill:
                HandleCulturalSkillOverlay();
                break;

            case PlanetOverlay.PopCulturalKnowledge:
                HandleCulturalKnowledgeOverlay();
                break;

            case PlanetOverlay.PolityCulturalKnowledge:
                HandleCulturalKnowledgeOverlay();
                break;

            case PlanetOverlay.PopCulturalDiscovery:
                HandleCulturalDiscoveryOverlay();
                break;

            case PlanetOverlay.PolityCulturalDiscovery:
                HandleCulturalDiscoveryOverlay();
                break;

            case PlanetOverlay.Layer:
                HandleLayerOverlay();
                break;

            case PlanetOverlay.BiomeTrait:
                HandleBiomeTraitOverlay();
                break;
        }
    }

    private void HandleCulturalPreferenceOverlay()
    {
        SelectionPanelScript.Title.text = "Preferences";

        foreach (CulturalPreferenceInfo preferenceInfo in Manager.CurrentWorld.CulturalPreferenceInfoList)
        {
            AddSelectionPanelOption(preferenceInfo.Name, preferenceInfo.Id);
        }

        SelectionPanelScript.SetVisible(true);
    }

    private void HandleCulturalActivityOverlay()
    {
        SelectionPanelScript.Title.text = "Activities";

        foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList)
        {
            AddSelectionPanelOption(activityInfo.Name, activityInfo.Id);
        }

        SelectionPanelScript.SetVisible(true);
    }

    private void HandleCulturalSkillOverlay()
    {
        SelectionPanelScript.Title.text = "Skills";

        foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList)
        {
            AddSelectionPanelOption(skillInfo.Name, skillInfo.Id);
        }

        SelectionPanelScript.SetVisible(true);
    }

    private void HandleCulturalKnowledgeOverlay()
    {
        SelectionPanelScript.Title.text = "Knowledges";

        foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList)
        {
            AddSelectionPanelOption(knowledgeInfo.Name, knowledgeInfo.Id);
        }

        SelectionPanelScript.SetVisible(true);
    }

    private void HandleCulturalDiscoveryOverlay()
    {
        SelectionPanelScript.Title.text = "Discoveries";

        foreach (var discovery in Manager.CurrentWorld.ExistingDiscoveries.Values)
        {
            AddSelectionPanelOption(discovery.Name, discovery.Id);
        }

        SelectionPanelScript.SetVisible(true);
    }

    private void HandleLayerOverlay()
    {
        SelectionPanelScript.Title.text = "Layers";

        foreach (Layer layer in Layer.Layers.Values)
        {
            AddSelectionPanelOption(layer.Name, layer.Id);
        }

        SelectionPanelScript.SetVisible(true);
    }

    private void HandleBiomeTraitOverlay()
    {
        SelectionPanelScript.Title.text = "Biome Traits";

        foreach (string trait in Biome.AllTraits)
        {
            AddSelectionPanelOption(trait, trait);
        }

        SelectionPanelScript.SetVisible(true);
    }

    private void SetPopCulturalDiscoveryOverlay(string planetOverlaySubtype, bool invokeEvent = true)
    {
        ChangePlanetOverlay(PlanetOverlay.PopCulturalDiscovery, planetOverlaySubtype, invokeEvent);
    }

    private void AddSelectionPanelOption(string optionName, string optionId)
    {
        SelectionPanelScript.AddOption(optionId, optionName, (state) =>
        {
            if (state)
            {
                _planetOverlaySubtype = optionId;
            }
            else if (_planetOverlaySubtype == optionId)
            {
                _planetOverlaySubtype = Manager.NoOverlaySubtype;
            }

            _regenMapOverlayTexture = true;
        });

        if (_planetOverlaySubtype == optionId)
        {
            SelectionPanelScript.SetStateOption(optionId, true);
        }
    }

    private void UpdateSelectionMenu()
    {
        if (!SelectionPanelScript.IsVisible())
            return;

        if (_planetOverlay == PlanetOverlay.PopCulturalPreference)
        {
            foreach (CulturalPreferenceInfo preferenceInfo in Manager.CurrentWorld.CulturalPreferenceInfoList)
            {
                AddSelectionPanelOption(preferenceInfo.Name, preferenceInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PopCulturalActivity)
        {
            foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList)
            {
                AddSelectionPanelOption(activityInfo.Name, activityInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PopCulturalSkill)
        {
            foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList)
            {
                AddSelectionPanelOption(skillInfo.Name, skillInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PopCulturalKnowledge)
        {
            foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList)
            {
                AddSelectionPanelOption(knowledgeInfo.Name, knowledgeInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PopCulturalDiscovery)
        {
            foreach (var discovery in Manager.CurrentWorld.ExistingDiscoveries.Values)
            {
                AddSelectionPanelOption(discovery.Name, discovery.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PolityCulturalPreference)
        {
            foreach (CulturalPreferenceInfo preferenceInfo in Manager.CurrentWorld.CulturalPreferenceInfoList)
            {
                AddSelectionPanelOption(preferenceInfo.Name, preferenceInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PolityCulturalActivity)
        {
            foreach (CulturalActivityInfo activityInfo in Manager.CurrentWorld.CulturalActivityInfoList)
            {
                AddSelectionPanelOption(activityInfo.Name, activityInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PolityCulturalSkill)
        {
            foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList)
            {
                AddSelectionPanelOption(skillInfo.Name, skillInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PolityCulturalKnowledge)
        {
            foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList)
            {
                AddSelectionPanelOption(knowledgeInfo.Name, knowledgeInfo.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.PolityCulturalDiscovery)
        {
            foreach (var discovery in Manager.CurrentWorld.ExistingDiscoveries.Values)
            {
                AddSelectionPanelOption(discovery.Name, discovery.Id);
            }
        }
        else if (_planetOverlay == PlanetOverlay.Layer)
        {
            foreach (Layer layer in Layer.Layers.Values)
            {
                AddSelectionPanelOption(layer.Name, layer.Id);
            }
        }
    }

    public void SetView(PlanetView planetView)
    {
        _regenMapTexture |= _planetView != planetView;

        _planetView = planetView;
    }

    public void OpenSelectFactionDialog()
    {
        SelectFactionDialogPanelScript.SetVisible(true);

        InterruptSimulation(true);
    }

    /// <summary>
    /// Sets the faction to be guided by the player
    /// </summary>
    public void SetFactionToGuide()
    {
        SelectFactionDialogPanelScript.SetVisible(false);

        Faction faction = SelectFactionDialogPanelScript.ChosenFaction;

        if (faction != null)
        {
            Manager.SetGuidedFaction(faction);
        }

        UninterruptSimAndShowHiddenInterPanels();
    }

    public void StopGuidingFaction()
    {
        Manager.SetGuidedFaction(null);
    }

    public void CancelSelectFaction()
    {
        SelectFactionDialogPanelScript.SetVisible(false);

        UninterruptSimAndShowHiddenInterPanels();
    }

    public void SetPlayerFocusOnPolity()
    {
        Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

        if ((selectedTerritory != null) && !selectedTerritory.Polity.IsUnderPlayerFocus)
            Manager.SetFocusOnPolity(selectedTerritory.Polity);
    }

    public void UnsetPlayerFocusOnPolity()
    {
        Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

        if ((selectedTerritory != null) && selectedTerritory.Polity.IsUnderPlayerFocus)
            Manager.UnsetFocusOnPolity(selectedTerritory.Polity);
    }

    private bool TryGetMapCoordinatesFromPointerPosition(Vector2 pointerPosition, out Vector2 mapPosition, bool allowWrap = false)
    {
        if (Manager.ViewingGlobe)
        {
            return PlanetScript.TryGetMapCoordinatesFromPointerPosition(pointerPosition, out mapPosition);
        }
        else
        {
            return MapScript.TryGetMapCoordinatesFromPointerPosition(pointerPosition, out mapPosition, allowWrap);
        }
    }

    private TerrainCell GetCellFromPointer(Vector2 position, bool allowWrap)
    {
        Vector2 mapCoordinates;

        if (!TryGetMapCoordinatesFromPointerPosition(position, out mapCoordinates, allowWrap))
            return null;

        int longitude = (int)mapCoordinates.x;
        int latitude = (int)mapCoordinates.y;

        TerrainCell cell = Manager.CurrentWorld.GetCell(longitude, latitude);

        if (cell == null)
        {
            throw new System.Exception("Unable to get cell at [" + longitude + "," + latitude + "]");
        }

        return cell;
    }

    private Vector3 GetScreenPositionFromMapCoordinates(WorldPosition mapPosition)
    {
        if (Manager.ViewingGlobe)
        {
            return PlanetScript.GetScreenPositionFromMapCoordinates(mapPosition);
        }
        else
        {
            return MapScript.GetScreenPositionFromMapCoordinates(mapPosition);
        }
    }

    private void ShowCellInfoToolTip_Territory(TerrainCell cell)
    {
        if (cell.EncompassingTerritory == _lastHoveredOverTerritory)
            return;

        _lastHoveredOverTerritory = cell.EncompassingTerritory;

        if (_lastHoveredOverTerritory == null)
        {
            InfoTooltipScript.SetVisible(false);
            return;
        }

        Polity polity = _lastHoveredOverTerritory.Polity;

        if (polity == null)
        {
            throw new System.Exception("Polity can't be null");
        }

        Vector3 tooltipPos = GetScreenPositionFromMapCoordinates(polity.CoreGroup.Cell.Position) + _tooltipOffset;

        if (polity.Name == null)
        {
            throw new System.Exception("Polity.Name can't be null");
        }

        if (polity.Name.Text == null)
        {
            throw new System.Exception("polity.Name.Text can't be null");
        }

        switch (_planetOverlay)
        {
            case PlanetOverlay.General:
                InfoTooltipScript.DisplayTip(polity.Name.Text, tooltipPos);
                break;
            case PlanetOverlay.PolityTerritory:
                ShowCellInfoToolTip_PolityTerritory(polity, tooltipPos);
                break;
            case PlanetOverlay.PolityAdminCost:
                ShowCellInfoToolTip_PolityAdminCost(polity, tooltipPos);
                break;
            case PlanetOverlay.PolityContacts:
                ShowCellInfoToolTip_PolityContacts(polity, tooltipPos);
                break;
            case PlanetOverlay.PolitySelection:
                ShowCellInfoToolTip_PolityContacts(polity, tooltipPos);
                break;
            case PlanetOverlay.PolityCulturalPreference:
                ShowCellInfoToolTip_PolityCulturalPreference(polity, tooltipPos);
                break;
            case PlanetOverlay.PolityCulturalActivity:
                ShowCellInfoToolTip_PolityCulturalActivity(polity, tooltipPos);
                break;
            case PlanetOverlay.PolityCulturalSkill:
                ShowCellInfoToolTip_PolityCulturalSkill(polity, tooltipPos);
                break;
            case PlanetOverlay.PolityCulturalKnowledge:
                ShowCellInfoToolTip_PolityCulturalKnowledge(polity, tooltipPos);
                break;
            case PlanetOverlay.PolityCulturalDiscovery:
                ShowCellInfoToolTip_PolityCulturalDiscovery(polity, tooltipPos);
                break;
            default:
                InfoTooltipScript.SetVisible(false);
                break;
        }
    }

    private void ShowCellInfoToolTip_PolityTerritory(Polity polity, Vector3 position, float fadeStart = 5)
    {
        string text = polity.Name.Text + " " + polity.TypeStr.ToLower() + "\n\nFaction Influences:";

        foreach (Faction faction in polity.GetFactions())
        {
            text += "\n " + faction.Name.Text + ": " + faction.Influence.ToString("P");
        }

        InfoTooltipScript.DisplayTip(text, position, fadeStart);
    }

    private void ShowCellInfoToolTip_PolityContacts(Polity polity, Vector3 position, float fadeStart = 5)
    {
        string polityTitle = polity.Name.Text + " " + polity.TypeStr.ToLower();

        string text;

        Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

        float strength = 0;

        if ((polity.Territory != selectedTerritory) && (selectedTerritory != null))
        {
            strength = selectedTerritory.Polity.GetContactStrength(polity);
        }

        if (strength > 0)
        {
            float relationshipValue = selectedTerritory.Polity.GetRelationshipValue(polity);

            text = "Neighboring polity: " + polityTitle;

            text += "\n\nRelationship Value: " + relationshipValue.ToString("0.000");
            text += "\n\nContact Strength: " + strength;

        }
        else
        {
            text = polityTitle;
        }

        InfoTooltipScript.DisplayTip(text, position, fadeStart);
    }

    private void ShowCellInfoToolTip_PolityCulturalPreference(Polity polity, Vector3 position, float fadeStart = 5)
    {
        CulturalPreference preference = polity.Culture.GetPreference(_planetOverlaySubtype);

        if (preference != null)
        {
            string text = preference.Name + " Preference: " + preference.Value.ToString("0.00") + "\n\nFactions:";

            foreach (Faction faction in polity.GetFactions())
            {
                float value = faction.Culture.GetPreferenceValue(_planetOverlaySubtype);

                text += "\n " + faction.Name.Text + ": " + value.ToString("0.00");
            }

            InfoTooltipScript.DisplayTip(text, position, fadeStart);

        }
        else
        {
            InfoTooltipScript.SetVisible(false);
        }
    }

    private void ShowCellInfoToolTip_PolityAdminCost(Polity polity, Vector3 position, float fadeStart = 5)
    {
        float value = polity.TotalAdministrativeCost;

        string text = $"Total Admin Cost: {value:0.00}\n\nFaction Admin Loads:";

        foreach (Faction faction in polity.GetFactions())
        {
            value = faction.AdministrativeLoad;

            text += $"\n {faction.Name.Text}: {value:0.00}";
        }

        InfoTooltipScript.DisplayTip(text, position, fadeStart);
    }

    private void ShowCellInfoToolTip_PolityCulturalActivity(Polity polity, Vector3 position, float fadeStart = 5)
    {
        CulturalActivity activity = polity.Culture.GetActivity(_planetOverlaySubtype);

        if (activity != null)
        {
            string text = activity.Name + " Contribution: " +
                activity.Contribution.ToString("P") + "\n\nFactions:";

            foreach (Faction faction in polity.GetFactions())
            {
                float activityContribution =
                    faction.Culture.GetActivityContribution(_planetOverlaySubtype);

                text += "\n " + faction.Name.Text + ": " + activityContribution.ToString("P");
            }

            InfoTooltipScript.DisplayTip(text, position, fadeStart);
        }
        else
        {
            InfoTooltipScript.SetVisible(false);
        }
    }

    private void ShowCellInfoToolTip_PolityCulturalSkill(Polity polity, Vector3 position, float fadeStart = 5)
    {
        CulturalSkill skill = polity.Culture.GetSkill(_planetOverlaySubtype);

        if ((skill != null) && (skill.Value >= 0.001))
        {
            string text = skill.Name + " Value: " + skill.Value.ToString("0.000") + "\n\nFactions:";

            foreach (Faction faction in polity.GetFactions())
            {
                skill = faction.Culture.GetSkill(_planetOverlaySubtype);

                text += "\n " + faction.Name.Text + ": " + skill.Value.ToString("0.000");
            }

            InfoTooltipScript.DisplayTip(text, position, fadeStart);
        }
        else
        {
            InfoTooltipScript.SetVisible(false);
        }
    }

    private void ShowCellInfoToolTip_PolityCulturalKnowledge(Polity polity, Vector3 position, float fadeStart = 5)
    {
        CulturalKnowledge knowledge = polity.Culture.GetKnowledge(_planetOverlaySubtype);

        if (knowledge != null)
        {
            string text = knowledge.Name + " Value: " + knowledge.Value.ToString("0.000") + "\n\nFactions:";

            foreach (Faction faction in polity.GetFactions())
            {
                faction.Culture.TryGetKnowledgeValue(_planetOverlaySubtype, out var value);

                text += $"\n {faction.Name.Text}: {value:0.000)}";
            }

            InfoTooltipScript.DisplayTip(text, position, fadeStart);
        }
        else
        {
            InfoTooltipScript.SetVisible(false);
        }
    }

    private void ShowCellInfoToolTip_PolityCulturalDiscovery(Polity polity, Vector3 position, float fadeStart = 5)
    {
        var discovery = polity.Culture.GetDiscovery(_planetOverlaySubtype);

        if (discovery != null)
        {
            InfoTooltipScript.DisplayTip(discovery.Name + " is present", position, fadeStart);
        }
        else
        {
            InfoTooltipScript.SetVisible(false);
        }
    }

    private void ShowCellInfoToolTip_LastHoveredRegion()
    {
        var cellPosition = _lastHoveredOverRegion.GetMostCenteredCell().Position;

        Vector3 tooltipPos = GetScreenPositionFromMapCoordinates(cellPosition) + _tooltipOffset;

        InfoTooltipScript.DisplayTip(_lastHoveredOverRegion.Name.Text, tooltipPos);
    }

    private void ShowCellInfoToolTip_Region(TerrainCell cell)
    {
        if (cell.Region == _lastHoveredOverRegion)
            return;

        _lastHoveredOverRegion = cell.Region;

        if (_lastHoveredOverRegion == null)
        {
            InfoTooltipScript.SetVisible(false);
            return;
        }

        ShowCellInfoToolTip_LastHoveredRegion();
    }

    private void ShowCellInfoToolTip_RegionSelection(TerrainCell cell)
    {
        if (cell.Region == _lastHoveredOverRegion)
            return;

        _lastHoveredOverRegion = cell.Region;

        if ((_lastHoveredOverRegion == null) ||
            (_lastHoveredOverRegion.SelectionFilterType != Region.FilterType.Selectable))
        {
            InfoTooltipScript.SetVisible(false);
            return;
        }

        ShowCellInfoToolTip_LastHoveredRegion();
    }

    private void ShowCellInfoToolTip_FactionSelection(TerrainCell cell)
    {
        if (cell.Group == null)
        {
            InfoTooltipScript.SetVisible(false);
            return;
        }

        Faction firstFaction = null;
        string factionNames = "";

        foreach (var faction in cell.GetClosestFactions())
        {
            if ((faction.SelectionFilterType != Faction.FilterType.Selectable))
            {
                continue;
            }

            if (firstFaction == null)
            {
                firstFaction = faction;
                factionNames = faction.Name.Text;
            }
            else
            {
                factionNames += $"\n{faction.Name.Text}";
            }
        }

        if (firstFaction == null)
        {
            InfoTooltipScript.SetVisible(false);
            return;
        }

        WorldPosition cellPosition = firstFaction.CoreGroup.Position;

        Vector3 tooltipPos = GetScreenPositionFromMapCoordinates(cellPosition) + _tooltipOffset;

        InfoTooltipScript.DisplayTip(factionNames, tooltipPos);
    }

    private int ComparePromValuesDescending(PolityProminence a, PolityProminence b)
    {
        if (a.Value > b.Value) return -1;
        if (a.Value < b.Value) return 1;
        return 0;
    }

    private void ShowCellInfoToolTip_GroupProminenceSelection(TerrainCell cell)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction ??
            throw new System.Exception("Can't show tooltip without an active guided faction");

        if (!(Manager.CurrentInputRequest is GroupSelectionRequest))
            throw new System.Exception("Can't show tooltip without an group selection request");

        if ((_lastHoveredCell == null) ||
            (_lastHoveredCell.SelectionFilterType != TerrainCell.FilterType.Selectable))
        {
            InfoTooltipScript.SetVisible(false);
            return;
        }

        Vector3 tooltipPos = GetScreenPositionFromMapCoordinates(cell.Position) + _tooltipOffset;

        CellGroup group = cell.Group ??
            throw new System.Exception($"Terrain cell {cell.Position} has no group");

        string text = "Prominence Values:";

        var prominences = new List<PolityProminence>(group.GetPolityProminences());
        prominences.Sort(ComparePromValuesDescending);

        foreach (var prominence in prominences)
        {
            text += $"\n\t{prominence.Polity.Name.Text}: {prominence.Value:0.000}";
        }

        InfoTooltipScript.DisplayTip(text, tooltipPos);
    }

    public void BeginDrag(BaseEventData data)
    {
        if (Manager.ViewingGlobe)
        {
            PlanetScript.BeginDrag(data);
        }
        else
        {
            MapScript.BeginDrag(data);
        }
    }

    public void Drag(BaseEventData data)
    {
        if (Manager.ViewingGlobe)
        {
            PlanetScript.Drag(data);
        }
        else
        {
            MapScript.Drag(data);
        }
    }

    public void EndDrag(BaseEventData data)
    {
        if (Manager.ViewingGlobe)
        {
            PlanetScript.EndDrag(data);
        }
        else
        {
            MapScript.EndDrag(data);
        }
    }

    public void Scroll(BaseEventData data)
    {
        if (Manager.ViewingGlobe)
        {
            PlanetScript.Scroll(data);
        }
        else
        {
            MapScript.Scroll(data);
        }
    }

    public void SelectCellOnMap(BaseEventData data)
    {
        if ((Manager.GameMode == GameMode.Editor) && Manager.EditorBrushIsVisible)
            return;

        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button != PointerEventData.InputButton.Left)
            return;

        Vector2 mapPosition;

        if (!TryGetMapCoordinatesFromPointerPosition(pointerData.position, out mapPosition))
            return;

        if (_mapLeftClickOp != null)
        {
            _mapLeftClickOp(mapPosition);
        }
    }

    private void ExecuteMapHoverOps()
    {
        if (!Manager.PointerIsOverMap && !Manager.EditorBrushIsActive)
        {
            _lastHoveredCell = null;
            Manager.EditorBrushTargetCell = null;

            return;
        }

        TerrainCell hoveredCell = GetCellFromPointer(Input.mousePosition, Manager.EditorBrushIsActive);

        if (hoveredCell != _lastHoveredCell)
        {
            _lastHoveredCell = hoveredCell;
            Manager.EditorBrushTargetCell = hoveredCell;
        }

        Manager.SetHoveredCell(hoveredCell);

        if (hoveredCell == null)
            return;

        if (IsPolityOverlay(_planetOverlay))
            ShowCellInfoToolTip_Territory(hoveredCell);
        else if (_planetOverlay == PlanetOverlay.Region)
            ShowCellInfoToolTip_Region(hoveredCell);
        else if (_planetOverlay == PlanetOverlay.RegionSelection)
            ShowCellInfoToolTip_RegionSelection(hoveredCell);
        else if (_planetOverlay == PlanetOverlay.FactionSelection)
            ShowCellInfoToolTip_FactionSelection(hoveredCell);
        else if (_planetOverlay == PlanetOverlay.CellSelection)
        {
            if (_planetOverlaySubtype == Manager.GroupProminenceOverlaySubtype)
                ShowCellInfoToolTip_GroupProminenceSelection(hoveredCell);
        }
    }
}
