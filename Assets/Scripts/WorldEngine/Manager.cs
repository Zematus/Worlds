using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using UnityEngine.Profiling;

public enum EditorBrushType
{
    Altitude,
    Temperature,
    Rainfall,
    Layer,
    None
}

public enum TextureValidationResult
{
    Ok,
    NotMinimumRequiredDimensions,
    InvalidColorPallete,
    Unknown
}

public enum PlanetView
{
    Elevation = 0,
    Biomes = 1,
    Coastlines = 2
}

public enum PlanetOverlay
{
    None,
    General,
    PopDensity,
    FarmlandDistribution,
    PopCulturalPreference,
    PopCulturalActivity,
    PopCulturalSkill,
    PopCulturalKnowledge,
    PopCulturalDiscovery,
    PolityTerritory,
    FactionCoreDistance,
    PolityProminence,
    PolityContacts,
    PolityCoreRegions,
    PolityCulturalPreference,
    PolityCulturalActivity,
    PolityCulturalSkill,
    PolityCulturalKnowledge,
    PolityCulturalDiscovery,
    PolityAdminCost,
    PolitySelection,
    FactionSelection,
    Temperature,
    Rainfall,
    DrainageBasins,
    Arability,
    Accessibility,
    Hilliness,
    BiomeTrait,
    Layer,
    Region,
    RegionSelection,
    CellSelection,
    Language,
    PopChange,
    MigrationPressure,
    PolityMigrationPressure,
    UpdateSpan,
    Migration,
    PolityCluster,
    ClusterAdminCost
}

public enum OverlayColorId
{
    None = -1,
    Arability = 0,
    Farmland = 1,
    GeneralDensitySubOptimal = 2,
    GeneralDensityOptimal = 3,
    Territory = 4,
    TerritoryBorder = 5,
    SelectedTerritory = 6,
    ContactedTerritoryGood = 7,
    ContactedTerritoryBad = 8,
    LowValue = 9,
    MedValue = 10,
    HighValue = 11,
    ActiveRoute = 12,
    InactiveRoute = 13,
    RiverBasins = 14,
}

public enum DevMode
{
    None = 0,
    Basic = 1,
    Advanced = 2
}

public enum HighlightMode
{
    None = 0x0,
    OnSelectedCell = 0x1,
    OnSelectedCollection = 0x2,
    OnSelected = 0x3,
    OnHoveredCell = 0x4,
    OnHoveredCollection = 0x8,
    OnHovered = 0xC
}

public enum AutoSaveMode
{
    Deactivate,
    OnRealWorldTime,
    OnGameTime,
    OnRealWorldOrGameTime,
    OnRealWorldAndGameTime
};

public class Manager
{
#if DEBUG
    public delegate void RegisterDebugEventDelegate(string eventType, object data);

    public static bool Debug_IsLoadedWorld = false;

    public static RegisterDebugEventDelegate RegisterDebugEvent = null;

    public class Debug_TracingData
    {
        public Identifier GroupId;
        public Identifier PolityId;
        public Identifier FactionId;
        public Identifier ClusterId;
        public Identifier RegionId;
        public int Longitude;
        public int Latitude;
        public int Priority;
        public long LastSaveDate;
    }

    public static Debug_TracingData TracingData = new Manager.Debug_TracingData();

    public static bool TrackGenRandomCallers = false;
#endif

    //	public static bool RecordingEnabled = false;

    //	public static IRecorder Recorder = DefaultRecorder.Default;

    public const string NoOverlaySubtype = "none";
    public const string GroupProminenceOverlaySubtype = "prominence";

    public const int WorldWidth = 400;
    public const int WorldHeight = 200;
    public const long PosIdOffset = long.MaxValue / (WorldWidth * WorldHeight);

    public const float BrushStrengthFactor_Base = 0.04f;
    public const float BrushStrengthFactor_Altitude = 0.5f;
    public const float BrushStrengthFactor_Rainfall = 0.25f;
    public const float BrushStrengthFactor_Temperature = 0.25f;
    public const float BrushStrengthFactor_Layer = 0.25f;

    public const float BrushNoiseRadiusFactor = 200;

    public const string DefaultModPath = "Mods";

    public const float StageProgressIncFromLoading = 0.1f;

    public const int MaxEditorBrushRadius = 25;
    public const int MinEditorBrushRadius = 1;

    //AutoSave variable and function
    public static AutoSaveMode AutoSaveMode = AutoSaveMode.Deactivate;
    public static float RealWorldAutoSaveInterval = 600f; //600f = every 10 minutes
    public static long AutoSaveInterval = 365000000; //365000000 = every one millon year

    public static bool LayersPresent = false;

    public static float LastStageProgress = 0;

    public static EventManagerScript EventManager = null;

    public static Thread MainThread { get; private set; }

    public static string SavePath { get; private set; }
    public static string HeightmapsPath { get; private set; }
    public static string ExportPath { get; private set; }

    public static string[] SupportedHeightmapFormats =
        { ".PSD", ".TIFF", ".JPG", ".TGA", ".PNG", ".BMP", ".PICT" };

    public static string WorldName { get; set; }

    public static HashSet<TerrainCell> HighlightedCells { get; private set; }
    public static HashSet<TerrainCell> UpdatedCells { get; private set; }
    public static HashSet<TerrainCell> TerrainUpdatedCells { get; private set; }

    public static TerrainCell EditorBrushTargetCell = null;
    public static int EditorBrushRadius = MaxEditorBrushRadius;
    public static float EditorBrushStrength = 0.25f;
    public static float EditorBrushNoise = 0.0f;
    public static bool EditorBrushIsVisible = false;
    public static bool EditorBrushIsActive = false;
    public static bool EditorBrushIsFlattenModeIsActive = false;

    public static BrushAction ActiveEditorBrushAction = null;

    public static EditorBrushType EditorBrushType = EditorBrushType.None;

    public static bool PointerIsOverMap = false;

    public static int UpdatedPixelCount = 0;

    public static int PixelToCellRatio = 4;

    public static int StartSpeedIndex = 7;
    public static float AltitudeScale = World.DefaultAltitudeScale;
    public static float SeaLevelOffset = 0;
    public static float RiverStrength = World.DefaultRiverStrength;
    public static float TemperatureOffset = World.AvgPossibleTemperature;
    public static float RainfallOffset = World.AvgPossibleRainfall;

    public static bool DisplayMigrationTaggedGroup = false;

    public static bool DisplayDebugTaggedGroups = false;

    public static World WorldBeingLoaded = null;

    public static bool FullScreenEnabled = false;
    public static bool UIScalingEnabled = false;
    public static bool AnimationShadersEnabled = true;

    public static DevMode CurrentDevMode = DevMode.None;

    public static List<string> ActiveModPaths = new List<string>() { Path.Combine(@"Mods", "Base") };
    public static bool ModsAlreadyLoaded = false;

    public static Dictionary<string, LayerSettings> LayerSettings =
        new Dictionary<string, LayerSettings>();

    public static GameMode GameMode = GameMode.None;

    public static bool ViewingGlobe = false;

    public static int LastEventsTriggeredCount = 0;
    public static int LastEventsEvaluatedCount = 0;

    public static Dictionary<string, World.EventEvalStats> LastEventEvalStatsPerType =
        new Dictionary<string, World.EventEvalStats>();

    public static int LastMapUpdateCount = 0;
    public static int LastPixelUpdateCount = 0;
    public static long LastDevModeDateSpan = 0;
    public static long LastDevModeUpdateDate = 0;

    public static long LastTextureUpdateDate = 0;

    public static bool DisableShortcuts = false;

    public static InputRequest CurrentInputRequest = null;

#if DEBUG
    public static bool Debug_PauseSimRequested = false;
    public static bool Debug_BreakRequested = false;
    public static Identifier Debug_IdentifierOfInterest = "147918939:7115485649982034428";
    public static Identifier Debug_IdentifierOfInterest2 = "97371564:7301682472976039088";
    public static Identifier Debug_IdentifierOfInterest3 = "111147064:7417435791841258610";
    public static Identifier Debug_IdentifierOfInterest4;

    public static bool Debug_Flag1 = false;
    public static bool Debug_Flag2 = false;
#endif

    public static bool PerformingAsyncTask { get; private set; }
    public static bool SimulationRunning { get; private set; }
    public static bool WorldIsReady { get; private set; }

    public static long CurrentMaxUpdateSpan = 0;
    public static float CurrentMaxAdminCost = 0;

    public static int CurrentDiscoveryUid = 0;

    private static bool _isLoadReady = false;

    private static string _debugLogFilename = "debug";
    private static string _debugLogExt = ".log";
    private static StreamWriter _debugLogStream = null;
    private static bool _backupDebugLog = false;

    private static HashSet<TerrainCell> _lastUpdatedCells = new HashSet<TerrainCell>();

    private static int _resolutionWidthWindowed = 1600;
    private static int _resolutionHeightWindowed = 900;

    private static bool _resolutionInitialized = false;

    private static CellUpdateType _observableUpdateTypes = CellUpdateType.None;
    private static CellUpdateSubType _observableUpdateSubTypes = CellUpdateSubType.None;

    private static Manager _manager = new Manager();

    private static PlanetView _planetView = PlanetView.Biomes;
    private static PlanetOverlay _planetOverlay = PlanetOverlay.None;
    private static string _planetOverlaySubtype = NoOverlaySubtype;

    private static HighlightMode _highlightMode = HighlightMode.None;

    private delegate bool FilterCollectionDelegate(ICellSet getter);

    private static FilterCollectionDelegate _filterHighlightCollection = null;

    private static readonly List<Color> _biomePalette = new List<Color>();
    private static readonly List<Color> _mapPalette = new List<Color>();
    private static readonly List<Color> _overlayPalette = new List<Color>();

    private static int _totalLoadTicks = 0;
    private static int _loadTicks = 0;

    private static bool _displayRoutes = false;
    private static bool _displayGroupActivity = false;
    private static bool _displayGroupActivityWasEnabled = false;

    private static Color _brushColor = new Color(1, 1, 1, 0.1f);
    private static Color _brushBorderColor = new Color(1, 1, 1, 0.25f);
    private static Color _transparentColor = new Color(1, 1, 1, 0.0f);

    private static TerrainCell _lastEditorBrushTargetCell = null;
    private static int _lastEditorBrushRadius = EditorBrushRadius;
    private static bool _editorBrushWasVisible = EditorBrushIsVisible;

    private static Stack<EditorAction> _undoableEditorActions = new Stack<EditorAction>();
    private static Stack<EditorAction> _redoableEditorActions = new Stack<EditorAction>();

    private static event System.Action _onUndoStackUpdate;
    private static event System.Action _onRedoStackUpdate;

    private static bool _undoAndRedoBlocked = false;

    private static ProgressCastDelegate _progressCastMethod = null;

    private static bool _simulationStep = false;

    private World _currentWorld = null;

    private Texture2D _currentMapTexture = null;
    private Texture2D _currentMapOverlayTexture = null;
    private Texture2D _currentMapActivityTexture = null;
    private Texture2D _currentMapOverlayShaderInfoTexture = null;

    private Texture2D _pointerOverlayTexture = null;

    private Color32[] _currentMapTextureColors = null;
    private Color32[] _currentMapOverlayTextureColors = null;
    private Color32[] _currentMapOverlayShaderInfoColor = null;
    private Color32[] _currentMapActivityTextureColors = null;

    private Color32[] _pointerOverlayTextureColors = null;

    private float?[,] _currentCellSlants;

    private Queue<IManagerTask> _taskQueue = new Queue<IManagerTask>();

    public XmlAttributeOverrides AttributeOverrides { get; private set; }

    public static bool SimulationCanRun
    {
        get
        {
            bool canRun = (_manager._currentWorld.CellGroupCount > 0);

            return canRun;
        }
    }

    public static PlanetOverlay PlanetOverlay => _planetOverlay;

    public static string PlanetOverlaySubtype => _planetOverlaySubtype;

    public static bool DisplayRoutes => _displayRoutes;

    public static bool DisplayGroupActivity => _displayGroupActivity;

    public static int UndoableEditorActionsCount => _undoableEditorActions.Count;
    public static int RedoableEditorActionsCount => _redoableEditorActions.Count;

    public static bool SimulationPerformingStep => _simulationStep;

    public static void UpdateMainThreadReference()
    {
        MainThread = Thread.CurrentThread;
    }

    private Manager()
    {
        InitializeSavePath();
        InitializeHeightmapsPath();
        InitializeExportPath();

        AttributeOverrides = GenerateAttributeOverrides();

        HighlightedCells = new HashSet<TerrainCell>();
        UpdatedCells = new HashSet<TerrainCell>();
        TerrainUpdatedCells = new HashSet<TerrainCell>();

        /// static initalizations

        Tribe.GenerateTribeNounVariations();
    }

    private static bool CanHandleKeyInput(bool requireCtrl, bool requireShift, bool canDisable)
    {
        if (requireCtrl != (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            return false;

        if (requireShift != (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            return false;

        if (canDisable && DisableShortcuts)
            return false;

        return true;
    }

    public static void SetToPerformSimulationStep(bool state)
    {
        _simulationStep = state;
    }

    public static void HandleKeyUp(
        KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action, bool canDisable = true)
    {
        if (!Input.GetKeyUp(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift, canDisable))
            return;

        action.Invoke();
    }

    public static void HandleKey(
        KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action, bool canDisable = true)
    {
        if (!Input.GetKey(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift, canDisable))
            return;

        action.Invoke();
    }

    public static void HandleKeyDown(
        KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action, bool canDisable = true)
    {
        if (!Input.GetKeyDown(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift, canDisable))
            return;

        action.Invoke();
    }

    public static void ResetLayerSettings()
    {
        LayerSettings.Clear();
    }

    public static void SetLayerSettings(List<LayerSettings> layerSettings)
    {
        ResetLayerSettings();

        foreach (LayerSettings settings in layerSettings)
        {
            LayerSettings.Add(settings.Id, settings);
        }
    }

    public static LayerSettings GetLayerSettings(string layerId)
    {
        LayerSettings settings;

        if (!LayerSettings.TryGetValue(layerId, out settings))
        {
            settings = new LayerSettings(Layer.Layers[layerId]);
            LayerSettings.Add(layerId, settings);
        }

        return settings;
    }

    public static void BlockUndoAndRedo(bool state)
    {
        _undoAndRedoBlocked = state;
    }

    public static void RegisterUndoStackUpdateOp(System.Action op)
    {
        _onUndoStackUpdate += op;
    }

    public static void DeregisterUndoStackUpdateOp(System.Action op)
    {
        _onUndoStackUpdate -= op;
    }

    public static void RegisterRedoStackUpdateOp(System.Action op)
    {
        _onRedoStackUpdate += op;
    }

    public static void DeregisterRedoStackUpdateOp(System.Action op)
    {
        _onRedoStackUpdate -= op;
    }

    public static void UndoEditorAction()
    {
        if (_undoAndRedoBlocked || EditorBrushIsActive)
            return;

        if (_undoableEditorActions.Count <= 0)
            return;

        EditorAction action = PopUndoableAction();

        action.Undo();

        PushRedoableAction(action);
    }

    public static void RedoEditorAction()
    {
        if (_undoAndRedoBlocked || EditorBrushIsActive)
            return;

        if (_redoableEditorActions.Count <= 0)
            return;

        EditorAction action = PopRedoableAction();

        action.Do();

        PushUndoableAction(action);
    }

    public static void PerformEditorAction(EditorAction editorAction)
    {
        editorAction.Do();

        PushUndoableAction(editorAction);
        ResetRedoableActionsStack();
    }

    public static void ResetActionStacks()
    {
        ResetUndoableActionsStack();
        ResetRedoableActionsStack();
    }

    public static void PushUndoableAction(EditorAction action)
    {
        _undoableEditorActions.Push(action);

        // The world needs drainage regen if the pushed action is a brush action
        CurrentWorld.NeedsDrainageRegeneration |= action is BrushAction;

        _onUndoStackUpdate?.Invoke();
    }

    public static void PushRedoableAction(EditorAction action)
    {
        _redoableEditorActions.Push(action);

        _onRedoStackUpdate?.Invoke();
    }

    public static EditorAction PopUndoableAction()
    {
        EditorAction action = _undoableEditorActions.Pop();

        // The world needs drainage regen if both the popped action 
        // and the new action at top of the stack are brush actions
        CurrentWorld.NeedsDrainageRegeneration =
            (_undoableEditorActions.Count > 0) &&
            (action is BrushAction) &&
            (_undoableEditorActions.Peek() is BrushAction);

        _onUndoStackUpdate?.Invoke();

        return action;
    }

    public static EditorAction PopRedoableAction()
    {
        EditorAction action = _redoableEditorActions.Pop();

        _onRedoStackUpdate?.Invoke();

        return action;
    }

    public static void ResetUndoableActionsStack()
    {
        _undoableEditorActions.Clear();

        if (_onUndoStackUpdate != null)
        {
            _onUndoStackUpdate.Invoke();
        }
    }

    public static void ResetRedoableActionsStack()
    {
        _redoableEditorActions.Clear();

        if (_onRedoStackUpdate != null)
        {
            _onRedoStackUpdate.Invoke();
        }
    }

    public static void InitializeDebugLog()
    {
        if (_debugLogStream != null)
            return;

        string filename = _debugLogFilename + _debugLogExt;

        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        _debugLogStream = File.CreateText(filename);

        string buildType;

        if (Debug.isDebugBuild)
        {
            buildType = "Debug";
        }
        else
        {
            buildType = "Release";
        }

        _debugLogStream.WriteLine("Running Worlds " + Application.version +
            " (" + Application.platform + " " + buildType + ")...");
        _debugLogStream.Flush();
    }

    public static void CloseDebugLog()
    {
        if (_debugLogStream == null)
            return;

        _debugLogStream.Close();

        _debugLogStream = null;

        if (_backupDebugLog)
        {
            string logFilename = _debugLogFilename + _debugLogExt;
            string backupFilename = _debugLogFilename + System.DateTime.Now.ToString("_dd_MM_yyyy_hh_mm_ss") + _debugLogExt;
            File.Copy(logFilename, backupFilename);
        }
    }

    public static void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (_debugLogStream == null)
            return;

        string worldInfoStr = "";

        if (CurrentWorld != null)
        {
            worldInfoStr += $"[Date: {CurrentWorld.CurrentDate}] - ";
        }

        logString = logString.Replace("\n", "\n\t");
        _debugLogStream.WriteLine(worldInfoStr + logString);

        if (type == LogType.Exception)
        {
            stackTrace = stackTrace.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\r", "\n").Replace("\n", "\n\t");
            _debugLogStream.WriteLine("\t" + stackTrace);
        }

        _debugLogStream.Flush();
    }

    public static void EnableLogBackup()
    {
        _backupDebugLog = true;
    }

    private void InitializeSavePath()
    {
        string path = Path.GetFullPath(@"Saves");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        SavePath = path;
    }

    private void InitializeHeightmapsPath()
    {
        string path = Path.GetFullPath(@"Heightmaps");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        HeightmapsPath = path;
    }

    private void InitializeExportPath()
    {
        string path = Path.GetFullPath(@"Images");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        ExportPath = path;
    }

    public static string GetDateString(long date)
    {
        if (date < 0)
        {
            return "Unknown";
        }

        long year = date / World.YearLength;
        int day = (int)(date % World.YearLength);

        return string.Format("Year {0}, Day {1}", year, day);
    }

    public static string GetTimeSpanString(long timespan)
    {
        long years = timespan / World.YearLength;
        int days = (int)(timespan % World.YearLength);

        return string.Format("{0} years, {1} days", years, days);
    }

    public static long GetDateNumber(long years, int days)
    {
        return years * World.YearLength + days;
    }

    public static string AddDateToWorldName(string worldName)
    {
        long year = CurrentWorld.CurrentDate / World.YearLength;
        int day = (int)(CurrentWorld.CurrentDate % World.YearLength);

        return worldName + "_date_" + string.Format("{0}_{1}", year, day);
    }

    public static string RemoveDateFromWorldName(string worldName)
    {
        int dateIndex = worldName.LastIndexOf("_date_", System.StringComparison.Ordinal);

        if (dateIndex > 0)
        {
            return worldName.Substring(0, dateIndex);
        }

        return worldName;
    }

    public static void SetFullscreen(bool state)
    {
        FullScreenEnabled = state;

        if (state)
        {
            Resolution currentResolution = Screen.currentResolution;

            Screen.SetResolution(currentResolution.width, currentResolution.height, state);
        }
        else
        {
            Screen.SetResolution(_resolutionWidthWindowed, _resolutionHeightWindowed, state);
        }
    }

    public static void SetUIScaling(bool state)
    {
        UIScalingEnabled = state;
    }

    public static void InitializeScreen()
    {
        if (_resolutionInitialized)
            return;

        SetFullscreen(FullScreenEnabled);
        SetUIScaling(UIScalingEnabled);

        _resolutionInitialized = true;
    }

    public static void InterruptSimulation(bool state)
    {
        SimulationRunning = !state;
    }

    public static void ExecuteTasks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!ExecuteNextTask()) break;
        }
    }

    public static bool ExecuteNextTask()
    {
        IManagerTask task;

        lock (_manager._taskQueue)
        {
            if (_manager._taskQueue.Count <= 0)
                return false;

            task = _manager._taskQueue.Dequeue();
        }

        task.Execute();

        return true;
    }

    public static ManagerTask<T> EnqueueTask<T>(ManagerTaskDelegate<T> taskDelegate)
    {
        ManagerTask<T> task = new ManagerTask<T>(taskDelegate);

        if (MainThread == Thread.CurrentThread)
        {
            task.Execute();
        }
        else
        {
            lock (_manager._taskQueue)
            {
                _manager._taskQueue.Enqueue(task);
            }
        }

        return task;
    }

    public static T EnqueueTaskAndWait<T>(ManagerTaskDelegate<T> taskDelegate)
    {
        return EnqueueTask(taskDelegate).Result;
    }

    public static ManagerTask EnqueueTask(ManagerTaskDelegate taskDelegate)
    {
        ManagerTask task = new ManagerTask(taskDelegate);

        if (MainThread == Thread.CurrentThread)
        {
            task.Execute();
        }
        else
        {
            lock (_manager._taskQueue)
            {
                _manager._taskQueue.Enqueue(task);
            }
        }

        return task;
    }

    public static void EnqueueTaskAndWait(ManagerTaskDelegate taskDelegate)
    {
        EnqueueTask(taskDelegate).Wait();
    }

    public static void SetBiomePalette(IEnumerable<Color> colors)
    {
        _biomePalette.Clear();
        _biomePalette.AddRange(colors);
    }

    public static void SetMapPalette(IEnumerable<Color> colors)
    {
        _mapPalette.Clear();
        _mapPalette.AddRange(colors);
    }

    public static void SetOverlayPalette(IEnumerable<Color> colors)
    {
        _overlayPalette.Clear();
        _overlayPalette.AddRange(colors);
    }

    public static World CurrentWorld
    {
        get
        {
            return _manager._currentWorld;
        }
    }

    public static Texture2D CurrentMapTexture
    {
        get
        {
            return _manager._currentMapTexture;
        }
    }

    public static Texture2D CurrentMapOverlayTexture
    {
        get
        {
            return _manager._currentMapOverlayTexture;
        }
    }

    public static Texture2D CurrentMapActivityTexture
    {
        get
        {
            return _manager._currentMapActivityTexture;
        }
    }

    public static Texture2D CurrentMapOverlayShaderInfoTexture
    {
        get
        {
            return _manager._currentMapOverlayShaderInfoTexture;
        }
    }

    public static Texture2D PointerOverlayTexture
    {
        get
        {
            return _manager._pointerOverlayTexture;
        }
    }

    public static Vector2 GetUVFromMapCoordinates(Vector2 mapPosition)
    {
        return new Vector2(mapPosition.x / CurrentWorld.Width, mapPosition.y / CurrentWorld.Height);
    }

    /// <summary>Generates a texture based on the current map and overlay and exports it to an image file.</summary>
    /// <param name="path">The target path and file to write the image to.</param>
    /// <param name="mapScript">The map script to use to generate the texture.</param>
    public static void ExportMapTextureToFile(string path, MapScript mapScript)
    {
        Texture2D mapTexture = _manager._currentMapTexture;
        Texture2D overlayTexture = _manager._currentMapOverlayTexture;
        Texture2D exportTexture = null;

        // Enqueue (and wait for) the operations that need to be executed in the 3D engine's main thread
        EnqueueTaskAndWait(() =>
        {
            int width = mapTexture.width;
            int height = mapTexture.height;

            exportTexture = new Texture2D(
                    width,
                    height,
                    mapTexture.format,
                    false);

            mapScript.RenderToTexture2D(exportTexture);
        });

        ManagerTask<byte[]> bytes = EnqueueTask(() => exportTexture.EncodeToPNG());

        File.WriteAllBytes(path, bytes);

        EnqueueTaskAndWait(() =>
        {
            Object.Destroy(exportTexture);
            return true;
        });
    }

    /// <summary>Initializes a job to start an export map texture to image operation</summary>
    /// <param name="path">The target path and file to write the image to.</param>
    /// <param name="mapScript">The map script to use to generate the texture.</param>
    /// <param name="progressCastMethod">handler for tracking the job's progress.</param>
    public static void ExportMapTextureToFileAsync(string path, MapScript mapScript, ProgressCastDelegate progressCastMethod = null)
    {
        SimulationRunning = false;
        PerformingAsyncTask = true;

        _progressCastMethod = progressCastMethod;

        if (_progressCastMethod == null)
        {
            _progressCastMethod = (value, message, reset) => { };
        }

        Debug.Log("Trying to export world map to .png file: " + Path.GetFileName(path));

        // Launch the thread job that will handle the async export task
        ThreadPool.QueueUserWorkItem(state =>
        {
            try
            {
                ExportMapTextureToFile(path, mapScript);
            }
            catch (System.Exception e)
            {
                // To display the exception on screen we need to queue a task on the rendering engine's main thread
                // and rethrow the exception inside that task
                EnqueueTaskAndWait(() =>
                {
                    throw new System.Exception("Unhandled exception in ExportMapTextureToFile with path: " + path, e);
                });
            }

            PerformingAsyncTask = false;
            SimulationRunning = true;
        });
    }

    public static void GeneratePointerOverlayTextures()
    {
        GeneratePointerOverlayTextureFromWorld(CurrentWorld);
    }

    public static void GenerateTextures(bool doMapTexture, bool doOverlayMapTexture)
    {
        if (CurrentDevMode != DevMode.None)
        {
            UpdatedPixelCount = 0;
        }

        if (doMapTexture)
        {
            GenerateMapTextureFromWorld(CurrentWorld);
        }

        if (doOverlayMapTexture)
        {
            GenerateMapOverlayTextureFromWorld(CurrentWorld);
            GenerateMapActivityTextureFromWorld(CurrentWorld);

            if (Manager.AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
            {
                GenerateMapOverlayShaderInfoTextureFromWorld(CurrentWorld);
            }
        }

        ResetUpdatedAndHighlightedCells();
    }

    public static bool ValidUpdateTypeAndSubtype(CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (_displayRoutes && ((updateType & CellUpdateType.Route) != CellUpdateType.None))
            return true;

        if ((_observableUpdateTypes & updateType) == CellUpdateType.None)
            return false;

        if ((_observableUpdateSubTypes & updateSubType) == CellUpdateSubType.None)
            return false;

        if (_planetOverlay == PlanetOverlay.General)
        {
            if (((updateType & CellUpdateType.Territory) != CellUpdateType.None) &&
                ((updateSubType & CellUpdateSubType.MembershipAndCore) != CellUpdateSubType.None))
            {
                return true;
            }
            else if (((updateType & CellUpdateType.Group) != CellUpdateType.None) &&
                ((updateSubType & CellUpdateSubType.Culture) != CellUpdateSubType.None))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    // Only use this function if ValidUpdateTypeAndSubtype has already been called
    public static void AddUpdatedCell(TerrainCell cell)
    {
        UpdatedCells.Add(cell);
    }

    public static void AddUpdatedCell(TerrainCell cell, CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        UpdatedCells.Add(cell);

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.Add(cell);
        }

        if (CellShouldBeHighlighted(cell))
        {
            HighlightedCells.Add(cell);
        }
    }

    public static void AddUpdatedCells(Polity polity, CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        AddUpdatedCells(polity.Territory, updateType, updateSubType);
    }

    public static void AddUpdatedCells(Territory territory, CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        ICollection<TerrainCell> cells = territory.GetCells();

        UpdatedCells.UnionWith(cells);

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.UnionWith(cells);
        }

        if (TerritoryShouldBeHighlighted(territory))
        {
            HighlightedCells.UnionWith(cells);
        }
    }

    public static void AddUpdatedCells(Region region, CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        ICollection<TerrainCell> cells = region.GetCells();

        UpdatedCells.UnionWith(cells);

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.UnionWith(cells);
        }

        if (RegionShouldBeHighlighted(region))
        {
            HighlightedCells.UnionWith(cells);
        }
    }

    /// <summary>
    /// Adds a cell that has just been selected to the list of cells to highlight on the map
    /// </summary>
    /// <param name="cell">the selected cell</param>
    private static void AddSelectedCellToHighlight(
        TerrainCell cell)
    {
        if ((_highlightMode & HighlightMode.OnSelectedCell) == HighlightMode.OnSelectedCell)
        {
            HighlightedCells.Add(cell);

            // Add to updated cells to make sure that it gets displayed correctly
            UpdatedCells.Add(cell);
        }
    }

    /// <summary>
    /// Adds a cell that has just been hovered to the list of cells to highlight on the map
    /// </summary>
    /// <param name="cell">the hovered cell</param>
    private static void AddHoveredCellToHighlight(
        TerrainCell cell)
    {
        if ((_highlightMode & HighlightMode.OnHoveredCell) == HighlightMode.OnHoveredCell)
        {
            HighlightedCells.Add(cell);

            // Add to updated cells to make sure that it gets displayed correctly
            UpdatedCells.Add(cell);
        }
    }

    /// <summary>
    /// Adds a collection of cells that have just been selected to the list of cells to
    /// highlight on the map
    /// </summary>
    /// <param name="cellsGetter">the cell collection getter</param>
    private static void AddSelectedCellsToHighlight(
        ICellSet cellsGetter)
    {
        if ((_highlightMode & HighlightMode.OnSelectedCollection) != HighlightMode.OnSelectedCollection)
            return;

        bool passedFilter = _filterHighlightCollection?.Invoke(cellsGetter) ?? true;

        if (!passedFilter)
            return;

        ICollection<TerrainCell> cells = cellsGetter.GetCells();

        HighlightedCells.UnionWith(cells);

        // Add to updated cells to make sure that it gets displayed correctly
        UpdatedCells.UnionWith(cells);
    }

    /// <summary>
    /// Adds a collection of cells that have just been hovered to the list of cells to
    /// highlight on the map
    /// </summary>
    /// <param name="cellsGetter">the cell collection getter</param>
    private static void AddHoveredCellsToHighlight(
        ICellSet cellsGetter)
    {
        if ((_highlightMode & HighlightMode.OnHoveredCollection) != HighlightMode.OnHoveredCollection)
            return;

        bool passedFilter = _filterHighlightCollection?.Invoke(cellsGetter) ?? true;

        if (!passedFilter)
            return;

        ICollection<TerrainCell> cells = cellsGetter.GetCells();

        HighlightedCells.UnionWith(cells);

        // Add to updated cells to make sure that it gets displayed correctly
        UpdatedCells.UnionWith(cells);
    }

    public static void GenerateRandomHumanGroup(int initialPopulation)
    {
        World world = _manager._currentWorld;

        if (_progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _progressCastMethod;
        }

        world.GenerateRandomHumanGroups(1, initialPopulation);
    }

    public static void GenerateHumanGroup(int longitude, int latitude, int initialPopulation)
    {
        World world = _manager._currentWorld;

        if (_progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _progressCastMethod;
        }

        world.GenerateHumanGroup(longitude, latitude, initialPopulation);
    }

    public static void SetActiveModPaths(ICollection<string> paths)
    {
        ActiveModPaths.Clear();

        foreach (string path in paths)
        {
            // Make sure mod paths are always parseable regardless of system
            string[] splitPath = path.Split('/', '\\');
            ActiveModPaths.Add(Path.Combine(splitPath));
        }

        ActiveModPaths.Sort(System.StringComparer.Ordinal); // order mods alphabetically before load

        ModsAlreadyLoaded = false;
    }

    public static void GenerateNewWorld(int seed, Texture2D heightmap)
    {
        WorldIsReady = false;

        LastStageProgress = 0;

        ProgressCastDelegate progressCastMethod;

        if (_progressCastMethod == null)
            progressCastMethod = (value, message, reset) => { };
        else
            progressCastMethod = _progressCastMethod;

        TryLoadActiveMods();

        progressCastMethod(LastStageProgress, "Generating World...");

        World world = new World(WorldWidth, WorldHeight, seed)
        {
            ProgressCastMethod = progressCastMethod,
            ModPaths = new List<string>(ActiveModPaths),
            LayerSettings = new List<LayerSettings>(LayerSettings.Values)
        };

        world.StartInitialization(LastStageProgress, 1.0f);
        world.Generate(heightmap);
        world.FinishInitialization();

        _manager._currentWorld = world;
        _manager._currentCellSlants = new float?[world.Width, world.Height];

        CurrentMaxUpdateSpan = 0;
        CurrentMaxAdminCost = 0;

        WorldIsReady = true;

        ForceWorldCleanup();
    }

    public static void GenerateNewWorldAsync(int seed, Texture2D heightmap = null, ProgressCastDelegate progressCastMethod = null)
    {
        SimulationRunning = false;
        PerformingAsyncTask = true;

        _progressCastMethod = progressCastMethod;

        if (_progressCastMethod == null)
        {
            _progressCastMethod = (value, message, reset) => { };
        }

        Debug.Log(string.Format("Trying to generate world with seed: {0}, Altitude Scale: {1}, Sea Level Offset: {2}, River Strength: {3}, Avg. Temperature: {4}, Avg. Rainfall: {5}",
            seed, AltitudeScale, SeaLevelOffset, RiverStrength, TemperatureOffset, RainfallOffset));

        string activeModStrs = string.Join(",", ActiveModPaths);

        Debug.Log("Active Mods: " + activeModStrs);

        ThreadPool.QueueUserWorkItem(state =>
        {
            try
            {
                GenerateNewWorld(seed, heightmap);
            }
            catch (System.Exception e)
            {
                EnqueueTaskAndWait(() =>
                {
                    throw new System.Exception("Unhandled exception in GenerateNewWorld with seed: " + seed, e);
                });
            }

            PerformingAsyncTask = false;
            SimulationRunning = true;
        });
    }

    public static void RegenerateWorld(GenerationType type)
    {
        WorldIsReady = false;

        World world = _manager._currentWorld;

        if (_progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _progressCastMethod;
        }

        world.StartReinitialization(0f, 1.0f);
        world.Regenerate(type);
        world.FinishInitialization();

        _manager._currentCellSlants = new float?[world.Width, world.Height];

        CurrentMaxUpdateSpan = 0;
        CurrentMaxAdminCost = 0;

        WorldIsReady = true;

        ForceWorldCleanup();
    }

    public static void RegenerateWorldAsync(GenerationType type, ProgressCastDelegate progressCastMethod = null)
    {
        SimulationRunning = false;
        PerformingAsyncTask = true;

        _progressCastMethod = progressCastMethod;

        if (_progressCastMethod == null)
        {
            _progressCastMethod = (value, message, reset) => { };
        }

        Debug.Log(string.Format("Trying to regenerate world with seed: {0}, Altitude Scale: {1}, Sea Level Offset: {2}, River Strength: {3}, Avg. Temperature: {4}, Avg. Rainfall: {5}",
            _manager._currentWorld.Seed, AltitudeScale, SeaLevelOffset, RiverStrength, TemperatureOffset, RainfallOffset));

        ThreadPool.QueueUserWorkItem(state =>
        {
            try
            {
                RegenerateWorld(type);
            }
            catch (System.Exception e)
            {
                EnqueueTaskAndWait(() =>
                {
                    throw new System.Exception("Unhandled exception in RegenerateWorld", e);
                });
            }

            PerformingAsyncTask = false;
            SimulationRunning = true;
        });
    }

    public static void SaveAppSettings(string path)
    {
        AppSettings settings = new AppSettings();

        settings.Put();

        XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, settings);
        }
    }

    public static void LoadAppSettings(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));

        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            AppSettings settings = serializer.Deserialize(stream) as AppSettings;

            settings.Take();
        }
    }

    public static void SaveWorld(string path)
    {
        _manager._currentWorld.Synchronize();

        XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, _manager._currentWorld);
        }
    }

    public static void SaveWorldAsync(string path, ProgressCastDelegate progressCastMethod = null)
    {
        SimulationRunning = false;
        PerformingAsyncTask = true;

        _progressCastMethod = progressCastMethod;

        if (_progressCastMethod == null)
        {
            _progressCastMethod = (value, message, reset) => { };
        }

        Debug.Log("Trying to save world to file: " + Path.GetFileName(path));

        ThreadPool.QueueUserWorkItem(state =>
        {
            try
            {
                SaveWorld(path);
            }
            catch (System.Exception e)
            {
                EnqueueTaskAndWait(() =>
                {
                    throw new System.Exception("Unhandled exception in SaveWorld with path: " + path, e);
                });
            }

            PerformingAsyncTask = false;
            SimulationRunning = true;
        });
    }

    // NOTE: Make sure there are no outside references to the world object stored in _manager._currentWorld, otherwise it is pointless to call this...
    // WARNING: Don't abuse this function call.
    private static void ForceWorldCleanup()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    private static void TryLoadActiveMods()
    {
        if (!ModsAlreadyLoaded)
        {
            LoadMods(ActiveModPaths);
            ModsAlreadyLoaded = true;
        }
    }

    public static void LoadWorld(string path)
    {
        WorldIsReady = false;

        LastStageProgress = 0;

        ProgressCastDelegate progressCastMethod;

        if (_progressCastMethod == null)
            progressCastMethod = (value, message, reset) => { };
        else
            progressCastMethod = _progressCastMethod;

        ResetWorldLoadTrack();

        World world;

        XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);

        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            world = serializer.Deserialize(stream) as World;
        }

        if (_progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _progressCastMethod;
        }

        LastStageProgress = StageProgressIncFromLoading;

        SetActiveModPaths(world.ModPaths);
        SetLayerSettings(world.LayerSettings);

        TryLoadActiveMods();

        progressCastMethod(LastStageProgress, "Loading World...");

        AltitudeScale = world.AltitudeScale;
        SeaLevelOffset = world.SeaLevelOffset;
        RiverStrength = world.RiverStrength;
        RainfallOffset = world.RainfallOffset;
        TemperatureOffset = world.TemperatureOffset;

        float progressBeforeFinalizing = 0.4f + LastStageProgress;

        world.StartInitialization(LastStageProgress, progressBeforeFinalizing, true);
        world.GenerateTerrain(GenerationType.TerrainNormal, null);
        world.FinishInitialization();

        progressCastMethod(progressBeforeFinalizing, "Finalizing...");

        world.FinalizeLoad(progressBeforeFinalizing, 1.0f, _progressCastMethod);

        _manager._currentWorld = world;
        _manager._currentCellSlants = new float?[world.Width, world.Height];

        CurrentMaxUpdateSpan = 0;
        CurrentMaxAdminCost = 0;

        WorldBeingLoaded = null;

        WorldIsReady = true;

        ForceWorldCleanup();
    }

    /// <summary>Initializes a job to start loading a world save file.</summary>
    /// <param name="path">The target path and file to load the world from.</param>
    /// <param name="progressCastMethod">handler for tracking the job's progress.</param>
    public static void LoadWorldAsync(string path, ProgressCastDelegate progressCastMethod = null)
    {
#if DEBUG
        Debug_IsLoadedWorld = true;
#endif

        SimulationRunning = false;
        PerformingAsyncTask = true;

        _progressCastMethod = progressCastMethod;

        if (_progressCastMethod == null)
        {
            _progressCastMethod = (value, message, reset) => { };
        }

        Debug.Log("Trying to load world from file: " + Path.GetFileName(path));

        // Launch the thread job that will handle the async load task
        ThreadPool.QueueUserWorkItem(state =>
        {
            try
            {
                LoadWorld(path);
            }
            catch (System.Exception e)
            {
                // To display the exception on screen we need to queue a task on the rendering engine's main thread
                // and rethrow the exception inside that task
                EnqueueTaskAndWait(() =>
                {
                    throw new System.Exception("Unhandled exception in LoadWorld with path: " + path, e);
                });
            }

            PerformingAsyncTask = false;
            SimulationRunning = true;
        });
    }

    public static void ResetWorldLoadTrack()
    {
        _isLoadReady = false;
    }

    public static void InitializeWorldLoadTrack()
    {
        _isLoadReady = true;

        _totalLoadTicks = WorldBeingLoaded.SerializedEventCount;
        _totalLoadTicks += WorldBeingLoaded.CellGroupCount;
        _totalLoadTicks += WorldBeingLoaded.TerrainCellAlterationListCount;

        _loadTicks = 0;
    }

    public static void UpdateWorldLoadTrackEventCount()
    {
        if (!_isLoadReady)
            InitializeWorldLoadTrack();

        _loadTicks += 1;

        float value = LastStageProgress + (StageProgressIncFromLoading * _loadTicks / (float)_totalLoadTicks);

        _progressCastMethod?.Invoke(Mathf.Min(1, value));
    }

    private static void SetObservableUpdateTypes(PlanetOverlay overlay, string planetOverlaySubtype = NoOverlaySubtype)
    {
        if ((overlay == PlanetOverlay.None) ||
            (overlay == PlanetOverlay.Arability) ||
            (overlay == PlanetOverlay.Accessibility) ||
            (overlay == PlanetOverlay.Hilliness) ||
            (overlay == PlanetOverlay.BiomeTrait) ||
            (overlay == PlanetOverlay.Layer) ||
            (overlay == PlanetOverlay.Rainfall) ||
            (overlay == PlanetOverlay.DrainageBasins) ||
            (overlay == PlanetOverlay.Temperature) ||
            (overlay == PlanetOverlay.FarmlandDistribution) ||
            (overlay == PlanetOverlay.CellSelection))
        {
            _observableUpdateTypes = CellUpdateType.Cell;
        }
        else if (
            (overlay == PlanetOverlay.Region) ||
            (overlay == PlanetOverlay.RegionSelection))
        {
            _observableUpdateTypes = CellUpdateType.Region;
        }
        else if ((overlay == PlanetOverlay.PolityCluster) ||
            (overlay == PlanetOverlay.ClusterAdminCost))
        {
            _observableUpdateTypes = CellUpdateType.Cluster;
        }
        else if (overlay == PlanetOverlay.Language)
        {
            _observableUpdateTypes = CellUpdateType.Language;
        }
        else if ((overlay == PlanetOverlay.PolityTerritory) ||
            (overlay == PlanetOverlay.PolityContacts) ||
            (overlay == PlanetOverlay.PolityCoreRegions) ||
            (overlay == PlanetOverlay.PolityCulturalPreference) ||
            (overlay == PlanetOverlay.PolityCulturalActivity) ||
            (overlay == PlanetOverlay.PolityCulturalDiscovery) ||
            (overlay == PlanetOverlay.PolityCulturalKnowledge) ||
            (overlay == PlanetOverlay.PolityCulturalSkill) ||
            (overlay == PlanetOverlay.PolityAdminCost) ||
            (overlay == PlanetOverlay.PolitySelection) ||
            (overlay == PlanetOverlay.FactionSelection))
        {
            _observableUpdateTypes = CellUpdateType.Territory;
        }
        else if (overlay == PlanetOverlay.General)
        {
            _observableUpdateTypes = CellUpdateType.Group | CellUpdateType.Territory;
        }
        else
        {
            _observableUpdateTypes = CellUpdateType.Group;
        }
    }

    private static void SetObservableUpdateSubtypes(PlanetOverlay overlay, string planetOverlaySubtype = NoOverlaySubtype)
    {
        if ((overlay == PlanetOverlay.None) ||
            (overlay == PlanetOverlay.Arability) ||
            (overlay == PlanetOverlay.Accessibility) ||
            (overlay == PlanetOverlay.Hilliness) ||
            (overlay == PlanetOverlay.BiomeTrait) ||
            (overlay == PlanetOverlay.Layer) ||
            (overlay == PlanetOverlay.Rainfall) ||
            (overlay == PlanetOverlay.DrainageBasins) ||
            (overlay == PlanetOverlay.Temperature) ||
            (overlay == PlanetOverlay.FarmlandDistribution))
        {
            _observableUpdateSubTypes = CellUpdateSubType.Terrain;
        }
        else if (
            (overlay == PlanetOverlay.Region) ||
            (overlay == PlanetOverlay.PolityCoreRegions) ||
            (overlay == PlanetOverlay.RegionSelection) ||
            (overlay == PlanetOverlay.PolityCluster) ||
            (overlay == PlanetOverlay.Language))
        {
            _observableUpdateSubTypes = CellUpdateSubType.Membership;
        }
        else if (overlay == PlanetOverlay.PolityTerritory)
        {
            _observableUpdateSubTypes = CellUpdateSubType.MembershipAndCore;
        }
        else if (overlay == PlanetOverlay.FactionCoreDistance)
        {
            _observableUpdateSubTypes = CellUpdateSubType.Membership | CellUpdateSubType.CoreDistance;
        }
        else if (
            (overlay == PlanetOverlay.PolityContacts) ||
            (overlay == PlanetOverlay.PolitySelection))
        {
            _observableUpdateSubTypes = CellUpdateSubType.Membership | CellUpdateSubType.Relationship;
        }
        else if (overlay == PlanetOverlay.General)
        {
            _observableUpdateSubTypes = CellUpdateSubType.MembershipAndCore | CellUpdateSubType.Culture;
        }
        else if ((overlay == PlanetOverlay.PolityCulturalPreference) ||
            (overlay == PlanetOverlay.PolityCulturalActivity) ||
            (overlay == PlanetOverlay.PolityCulturalDiscovery) ||
            (overlay == PlanetOverlay.PolityCulturalKnowledge) ||
            (overlay == PlanetOverlay.PolityCulturalSkill))
        {
            _observableUpdateSubTypes = CellUpdateSubType.Membership | CellUpdateSubType.Culture;
        }
        else
        {
            _observableUpdateSubTypes = CellUpdateSubType.All;
        }
    }

    private static bool FilterSelectableRegion(ICellSet getter)
    {
        if ((getter is Region region) &&
            (region.SelectionFilterType == Region.FilterType.Selectable))
        {
            return true;
        }

        return false;
    }

    private static bool FilterSelectableTerritory(ICellSet getter)
    {
        if ((getter is Territory territory) &&
            (territory.SelectionFilterType == Territory.FilterType.Selectable))
        {
            return true;
        }

        return false;
    }

    private static bool FilterSelectableFaction(ICellSet getter)
    {
        if ((getter is Faction faction) &&
            (faction.SelectionFilterType == Faction.FilterType.Selectable))
        {
            return true;
        }

        return false;
    }

    private static void SetOverlayHighlightMode(PlanetOverlay overlay)
    {
        _filterHighlightCollection = null;

        if (overlay == PlanetOverlay.RegionSelection)
        {
            _highlightMode = HighlightMode.OnHoveredCollection;
            _filterHighlightCollection = FilterSelectableRegion;
        }
        else if (overlay == PlanetOverlay.PolitySelection)
        {
            _highlightMode = HighlightMode.OnHoveredCollection;
            _filterHighlightCollection = FilterSelectableTerritory;
        }
        else if (overlay == PlanetOverlay.FactionSelection)
        {
            _highlightMode = HighlightMode.OnHoveredCollection;
            _filterHighlightCollection = FilterSelectableFaction;
        }
        else if (overlay == PlanetOverlay.CellSelection)
        {
            _highlightMode = HighlightMode.OnHoveredCell;
        }
        else if ((overlay == PlanetOverlay.PolityContacts) ||
            (overlay == PlanetOverlay.PolityCoreRegions))
        {
            _highlightMode = HighlightMode.OnSelectedCell;
        }
        else
        {
            _highlightMode = HighlightMode.OnSelected;
        }
    }

    public static void AddedCoreRegion(Polity polity, Region region)
    {
        if ((_planetOverlay == PlanetOverlay.PolityCoreRegions) &&
            (CurrentWorld.SelectedTerritory == polity.Territory))
        {
            UpdatedCells.UnionWith(region.GetCells());
            region.SelectionFilterType = Region.FilterType.Core;
        }
    }

    public static void RemovedCoreRegion(Polity polity, Region region)
    {
        if ((_planetOverlay == PlanetOverlay.PolityCoreRegions) &&
            (CurrentWorld.SelectedTerritory == polity.Territory))
        {
            UpdatedCells.UnionWith(region.GetCells());
            region.SelectionFilterType = Region.FilterType.None;
        }
    }

    public static void SetPlanetOverlay(PlanetOverlay overlay, string planetOverlaySubtype = "None")
    {
        SetOverlayHighlightMode(overlay);

        SetObservableUpdateTypes(overlay, planetOverlaySubtype);
        SetObservableUpdateSubtypes(overlay, planetOverlaySubtype);

        if (overlay != _planetOverlay)
        {
            if (CurrentWorld.SelectedTerritory != null)
            {
                if (_planetOverlay == PlanetOverlay.PolityCoreRegions)
                {
                    foreach (Region region in CurrentWorld.SelectedTerritory.Polity.CoreRegions)
                    {
                        region.SelectionFilterType = Region.FilterType.None;
                    }
                }
                else if (overlay == PlanetOverlay.PolityCoreRegions)
                {
                    foreach (Region region in CurrentWorld.SelectedTerritory.Polity.CoreRegions)
                    {
                        region.SelectionFilterType = Region.FilterType.Core;
                    }
                }
            }
        }

        _planetOverlay = overlay;
        _planetOverlaySubtype = planetOverlaySubtype;
    }

    public static void SetDisplayRoutes(bool value)
    {
        if (value)
            _observableUpdateTypes |= CellUpdateType.Route;
        else
            _observableUpdateTypes &= ~CellUpdateType.Route;

        _displayRoutes = value;
    }

    public static void SetDisplayGroupActivity(bool value)
    {
        _displayGroupActivity = value;
    }

    public static void SetPlanetView(PlanetView value)
    {
        _planetView = value;
    }

    public static void SetSelectedCell(int longitude, int latitude)
    {
        SetSelectedCell(CurrentWorld.GetCell(longitude, latitude));
    }

    public static void SetSelectedCell(WorldPosition position)
    {
        SetSelectedCell(CurrentWorld.GetCell(position));
    }

    public static void SetSelectedRegion(Region region)
    {
        if (CurrentWorld.SelectedRegion != null)
        {
            AddSelectedCellsToHighlight(CurrentWorld.SelectedRegion);

            CurrentWorld.SelectedRegion.IsSelected = false;
            CurrentWorld.SelectedRegion = null;
        }

        if (region != null)
        {
            CurrentWorld.SelectedRegion = region;
            region.IsSelected = true;

            AddSelectedCellsToHighlight(region);
        }
    }

    public static void SetHoveredRegion(Region region)
    {
        if (CurrentWorld.HoveredRegion != null)
        {
            AddHoveredCellsToHighlight(CurrentWorld.HoveredRegion);

            CurrentWorld.HoveredRegion.IsHovered = false;
            CurrentWorld.HoveredRegion = null;
        }

        if (region != null)
        {
            CurrentWorld.HoveredRegion = region;
            region.IsHovered = true;

            AddHoveredCellsToHighlight(region);
        }
    }

    private static void SetSelectedTerritory_HandleUpdate(Territory territory, Region.FilterType type)
    {
        if (_planetOverlay == PlanetOverlay.PolityContacts)
        {
            // Add to updated cells to make sure that it gets displayed correctly
            UpdatedCells.UnionWith(territory.GetCells());

            foreach (PolityContact contact in territory.Polity.GetContacts())
            {
                UpdatedCells.UnionWith(contact.NeighborPolity.Territory.GetCells());
            }
        }
        else if (_planetOverlay == PlanetOverlay.PolityCoreRegions)
        {
            // Add to updated cells to make sure that it gets displayed correctly
            UpdatedCells.UnionWith(territory.GetCells());

            foreach (Region region in territory.Polity.CoreRegions)
            {
                UpdatedCells.UnionWith(region.GetCells());
                region.SelectionFilterType = type;
            }
        }
    }

    public static void SetSelectedTerritory(Territory territory)
    {
        if (CurrentWorld.SelectedTerritory != null)
        {
            AddSelectedCellsToHighlight(CurrentWorld.SelectedTerritory);

            SetSelectedTerritory_HandleUpdate(CurrentWorld.SelectedTerritory, Region.FilterType.None);

            CurrentWorld.SelectedTerritory.IsSelected = false;
            CurrentWorld.SelectedTerritory = null;
        }

        if (territory != null)
        {
            CurrentWorld.SelectedTerritory = territory;
            territory.IsSelected = true;

            AddSelectedCellsToHighlight(territory);

            SetSelectedTerritory_HandleUpdate(territory, Region.FilterType.Core);
        }

        if ((_planetOverlay == PlanetOverlay.PolityCluster) ||
            (_planetOverlay == PlanetOverlay.ClusterAdminCost))
        {
            foreach (var polity in CurrentWorld.GetActivePolities())
            {
                AddSelectedCellsToHighlight(polity.Territory);
            }
        }
    }

    public static void SetHoveredTerritory(Territory territory)
    {
        if (CurrentWorld.HoveredTerritory != null)
        {
            AddHoveredCellsToHighlight(CurrentWorld.HoveredTerritory);

            CurrentWorld.HoveredTerritory.IsHovered = false;
            CurrentWorld.HoveredTerritory = null;
        }

        if (territory != null)
        {
            CurrentWorld.HoveredTerritory = territory;
            territory.IsHovered = true;

            AddHoveredCellsToHighlight(territory);
        }
    }

    public static void SetHoveredFaction(Faction faction)
    {
        if (CurrentWorld.HoveredFaction != null)
        {
            AddHoveredCellsToHighlight(CurrentWorld.HoveredFaction);

            CurrentWorld.HoveredFaction.IsHovered = false;
            CurrentWorld.HoveredFaction = null;
        }

        if (faction != null)
        {
            CurrentWorld.HoveredFaction = faction;
            faction.IsHovered = true;

            AddHoveredCellsToHighlight(faction);
        }
    }

    public static void SetSelectedCell(TerrainCell cell)
    {
        if (CurrentWorld.SelectedCell != null)
        {
            AddSelectedCellToHighlight(CurrentWorld.SelectedCell);

            CurrentWorld.SelectedCell.IsSelected = false;
            CurrentWorld.SelectedCell = null;
        }

        if (cell == null)
            return;

        CurrentWorld.SelectedCell = cell;
        cell.IsSelected = true;

        AddSelectedCellToHighlight(cell);

        SetSelectedRegion(cell.Region);
        SetSelectedTerritory(cell.EncompassingTerritory);
    }

    public static void SetHoveredCell(TerrainCell cell)
    {
        if (CurrentWorld.HoveredCell != null)
        {
            AddHoveredCellToHighlight(CurrentWorld.HoveredCell);

            CurrentWorld.HoveredCell.IsHovered = false;
            CurrentWorld.HoveredCell = null;
        }

        Region region = null;
        Territory territory = null;
        Faction faction = null;

        if (cell != null)
        {
            CurrentWorld.HoveredCell = cell;
            cell.IsHovered = true;

            AddHoveredCellToHighlight(cell);

            region = cell.Region;
            territory = cell.EncompassingTerritory;
            faction = cell.GetMostProminentClosestFaction();
        }

        SetHoveredRegion(region);
        SetHoveredTerritory(territory);
        SetHoveredFaction(faction);
    }

    public static void SetFocusOnPolity(Polity polity)
    {
        if (polity == null)
            return;

        if (CurrentWorld.PolitiesUnderPlayerFocus.Contains(polity))
            return;

        polity.SetUnderPlayerFocus(true);
        CurrentWorld.PolitiesUnderPlayerFocus.Add(polity);
    }

    public static void UnsetFocusOnPolity(Polity polity)
    {
        if (polity == null)
            return;

        if (!CurrentWorld.PolitiesUnderPlayerFocus.Contains(polity))
            return;

        polity.SetUnderPlayerFocus(false);
        CurrentWorld.PolitiesUnderPlayerFocus.Remove(polity);
    }

    public static void SetGuidedFaction(Faction faction)
    {
        if (CurrentWorld.GuidedFaction == faction)
            return;

        if (CurrentWorld.GuidedFaction != null)
        {
            CurrentWorld.GuidedFaction.SetUnderPlayerGuidance(false);
        }

        CurrentWorld.GuidedFaction = faction;

        if (faction != null)
        {
            faction.SetUnderPlayerGuidance(true);

            EventManager.GuidedFactionSet.Invoke(true);
        }
        else
        {
            EventManager.GuidedFactionSet.Invoke(false);
        }
    }

    public static void ResetUpdatedAndHighlightedCells()
    {
        _lastUpdatedCells.Clear();
        _lastUpdatedCells.UnionWith(UpdatedCells);

        UpdatedCells.Clear();
        TerrainUpdatedCells.Clear();
        HighlightedCells.Clear();
    }

    public static void UpdatePointerOverlayTextures()
    {
        if (_editorBrushWasVisible || EditorBrushIsVisible)
        {
            if ((_lastEditorBrushTargetCell != EditorBrushTargetCell) ||
                (_lastEditorBrushRadius != EditorBrushRadius) ||
                (_editorBrushWasVisible != EditorBrushIsVisible))
            {
                UpdatePointerOverlayTextureColors(_manager._pointerOverlayTextureColors);

                PointerOverlayTexture.SetPixels32(_manager._pointerOverlayTextureColors);

                PointerOverlayTexture.Apply();
            }
        }
    }

    public static void ApplyEditorBrush()
    {
        if (EditorBrushIsVisible && EditorBrushIsActive &&
            (EditorBrushType != EditorBrushType.None) &&
            (EditorBrushTargetCell != null))
        {
            int sizeX = CurrentWorld.Width;
            int sizeY = CurrentWorld.Height;

            int centerX = EditorBrushTargetCell.Longitude;
            int centerY = EditorBrushTargetCell.Latitude;

            float fRadius = EditorBrushRadius - 0.1f;

            int startJ = centerY - EditorBrushRadius + 1;
            int endJ = centerY + EditorBrushRadius;

            for (int j = startJ; j < endJ; j++)
            {
                int mJ = j;
                int mOffsetI = 0;

                if (mJ < 0) // do a polar wrap around the y-axis when near a pole
                {
                    mJ = -mJ - 1;
                    mOffsetI = sizeX / 2;
                }
                else if (mJ >= sizeY)
                {
                    mJ = (2 * sizeY) - mJ - 1;
                    mOffsetI = sizeX / 2;
                }

                int jDiff = j - centerY;
                int iRadius = (int)MathUtility.GetComponent(fRadius, jDiff);

                int offsetI = centerX - iRadius;
                mOffsetI = (mOffsetI + offsetI + sizeX) % sizeX; // make sure the brush wraps around the x-axis and account for radial y-axis wraps
                int iDiameter = 1 + (iRadius * 2);

                for (int uI = 0; uI < iDiameter; uI++)
                {
                    int mI = (uI + mOffsetI) % sizeX;
                    int i = uI + offsetI;

                    int iDiff = i - centerX;
                    float dist = MathUtility.GetMagnitude(iDiff, jDiff);
                    float distFactor = dist / EditorBrushRadius;

                    if (EditorBrushIsFlattenModeIsActive)
                        ApplyEditorBrushFlatten(mI, mJ, distFactor);
                    else
                        ApplyEditorBrush(mI, mJ, distFactor);
                }
            }

            if (EditorBrushType == EditorBrushType.Altitude)
            {
                CurrentWorld.FinishTerrainGenerationForModifiedCells();
            }
        }
    }

    private static void ApplyEditorBrush(int longitude, int latitude, float distanceFactor)
    {
        switch (EditorBrushType)
        {
            case EditorBrushType.Altitude:
                ApplyEditorBrush_Altitude(longitude, latitude, distanceFactor);
                break;
            case EditorBrushType.Temperature:
                ApplyEditorBrush_Temperature(longitude, latitude, distanceFactor);
                break;
            case EditorBrushType.Rainfall:
                ApplyEditorBrush_Rainfall(longitude, latitude, distanceFactor);
                break;
            case EditorBrushType.Layer:
                ApplyEditorBrush_Layer(longitude, latitude, distanceFactor);
                break;
            default:
                throw new System.Exception("Unhandled Editor Brush Type: " + EditorBrushType);
        }
    }

    private static void ApplyEditorBrushFlatten(int longitude, int latitude, float distanceFactor)
    {
        switch (EditorBrushType)
        {
            case EditorBrushType.Altitude:
                ApplyEditorBrushFlatten_Altitude(longitude, latitude, distanceFactor);
                break;
            case EditorBrushType.Temperature:
                ApplyEditorBrushFlatten_Temperature(longitude, latitude, distanceFactor);
                break;
            case EditorBrushType.Rainfall:
                ApplyEditorBrushFlatten_Rainfall(longitude, latitude, distanceFactor);
                break;
            case EditorBrushType.Layer:
                ApplyEditorBrushFlatten_Layer(longitude, latitude, distanceFactor);
                break;
            default:
                throw new System.Exception("Unhandled Editor Brush Type: " + EditorBrushType);
        }
    }

    public static void ResetSlantsAround(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            _manager._currentCellSlants[nCell.Longitude, nCell.Latitude] = null;
        }

        _manager._currentCellSlants[cell.Longitude, cell.Latitude] = null;
    }

    /// <summary>
    /// Tests if the selected editor brush can be activated.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if the brush can be activated. Otherwise, <c>false</c>.
    /// </returns>
    public static bool CanActivateBrush()
    {
        if (EditorBrushType == EditorBrushType.Layer)
        {
            return Layer.IsValidLayerId(_planetOverlaySubtype);
        }

        return true;
    }

    /// <summary>
    /// Tells if the manager should activate the selected editor brush or not.
    /// </summary>
    /// <param name="state">
    /// Indicates if the brush should be activated.
    /// </param>
    public static void ActivateEditorBrush(bool state)
    {
        bool useLayerBrush = EditorBrushType == EditorBrushType.Layer;

        EditorBrushIsActive = state;

        if (state)
        {
            // Applying any kind of brush will make it necessary to do drainage regeneration
            CurrentWorld.NeedsDrainageRegeneration = true;

            if (useLayerBrush)
            {
                ActiveEditorBrushAction = new LayerBrushAction(_planetOverlaySubtype);
            }
            else
            {
                ActiveEditorBrushAction = new AlterationBrushAction();
            }
        }
        else if (ActiveEditorBrushAction != null)
        {
            ////
            // TODO: Figure out how to make drainage regen work
            //
            //CurrentWorld.PerformTerrainAlterationDrainageRegen(); // do a test drainage expansion to find all cells that need to be recalculated. changes will not persist
            //
            //CurrentWorld.GenerateDrainageBasins(true); // redo drainage expansion to modify terrain
            //CurrentWorld.GenerateDrainageBasins(false); // final proper drainage expansion with already modified terrain
            //
            //CurrentWorld.FinalizeTerrainAlterationDrainageRegen();
            ////

            ActiveEditorBrushAction.FinalizeCellModifications();

            PushUndoableAction(ActiveEditorBrushAction);
            ResetRedoableActionsStack();

            ActiveEditorBrushAction = null;
        }
    }

    private static void ApplyEditorBrush_Altitude(int longitude, int latitude, float distanceFactor)
    {
        float strength = EditorBrushStrength / AltitudeScale;
        float noiseRadius = BrushNoiseRadiusFactor / (float)EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Altitude *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = strength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellAltitude(cell, valueOffset, EditorBrushNoise, noiseRadius);

        ResetSlantsAround(cell);
    }

    private static void ApplyEditorBrushFlatten_Altitude(int longitude, int latitude, float distanceFactor)
    {
        float strength = EditorBrushStrength / AltitudeScale;
        int sampleRadius = 1;

        float strToValue = BrushStrengthFactor_Altitude *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffsetFactor = strength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        TerrainCell cellNorth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude - sampleRadius);
        TerrainCell cellEast = CurrentWorld.GetCellWithSphericalWrap(longitude + sampleRadius, latitude);
        TerrainCell cellSouth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude + sampleRadius);
        TerrainCell cellWest = CurrentWorld.GetCellWithSphericalWrap(longitude - sampleRadius, latitude);

        float targetValue =
            (cellNorth.BaseAltitudeValue +
            cellEast.BaseAltitudeValue +
            cellSouth.BaseAltitudeValue +
            cellWest.BaseAltitudeValue) / 4f;

        float currentValue = cell.BaseAltitudeValue;
        float valueOffset = (targetValue - currentValue) * valueOffsetFactor;

        CurrentWorld.ModifyCellAltitude(cell, valueOffset);

        ResetSlantsAround(cell);
    }

    private static void ApplyEditorBrush_Temperature(int longitude, int latitude, float distanceFactor)
    {
        float noiseRadius = BrushNoiseRadiusFactor / (float)EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Temperature *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellTemperature(cell, valueOffset, EditorBrushNoise, noiseRadius);
    }

    private static void ApplyEditorBrushFlatten_Temperature(int longitude, int latitude, float distanceFactor)
    {
        int sampleRadius = 1;

        float strToValue = BrushStrengthFactor_Temperature *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffsetFactor = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        TerrainCell cellNorth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude - sampleRadius);
        TerrainCell cellEast = CurrentWorld.GetCellWithSphericalWrap(longitude + sampleRadius, latitude);
        TerrainCell cellSouth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude + sampleRadius);
        TerrainCell cellWest = CurrentWorld.GetCellWithSphericalWrap(longitude - sampleRadius, latitude);

        float targetValue =
            (cellNorth.BaseTemperatureValue + cellNorth.BaseTemperatureOffset +
            cellEast.BaseTemperatureValue + cellEast.BaseTemperatureOffset +
            cellSouth.BaseTemperatureValue + cellSouth.BaseTemperatureOffset +
            cellWest.BaseTemperatureValue + cellWest.BaseTemperatureOffset) / 4f;

        float currentValue = cell.BaseTemperatureValue + cell.BaseTemperatureOffset;
        float valueOffset = (targetValue - currentValue) * valueOffsetFactor;

        CurrentWorld.ModifyCellTemperature(cell, valueOffset);
    }

    private static void ApplyEditorBrush_Layer(int longitude, int latitude, float distanceFactor)
    {
        if (!Layer.IsValidLayerId(_planetOverlaySubtype))
        {
            throw new System.Exception("Not a recognized layer Id: " + _planetOverlaySubtype);
        }

        float noiseRadius = BrushNoiseRadiusFactor / (float)EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Layer *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellLayerData(cell, valueOffset, _planetOverlaySubtype, EditorBrushNoise, noiseRadius);
    }

    private static void ApplyEditorBrushFlatten_Layer(int longitude, int latitude, float distanceFactor)
    {
        if (!Layer.IsValidLayerId(_planetOverlaySubtype))
        {
            throw new System.Exception("Not a recognized layer Id: " + _planetOverlaySubtype);
        }

        int sampleRadius = 1;

        float strToValue = BrushStrengthFactor_Layer *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffsetFactor = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        TerrainCell cellNorth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude - sampleRadius);
        TerrainCell cellEast = CurrentWorld.GetCellWithSphericalWrap(longitude + sampleRadius, latitude);
        TerrainCell cellSouth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude + sampleRadius);
        TerrainCell cellWest = CurrentWorld.GetCellWithSphericalWrap(longitude - sampleRadius, latitude);

        float targetValue =
            (cellNorth.GetLayerValue(_planetOverlaySubtype) +
            cellEast.GetLayerValue(_planetOverlaySubtype) +
            cellSouth.GetLayerValue(_planetOverlaySubtype) +
            cellWest.GetLayerValue(_planetOverlaySubtype)) / 4f;

        float currentValue = cell.GetLayerValue(_planetOverlaySubtype);
        float valueOffset = (targetValue - currentValue) * valueOffsetFactor;

        CurrentWorld.ModifyCellLayerData(cell, valueOffset, _planetOverlaySubtype);
    }

    private static void ApplyEditorBrush_Rainfall(int longitude, int latitude, float distanceFactor)
    {
        float noiseRadius = BrushNoiseRadiusFactor / (float)EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Rainfall *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellRainfall(cell, valueOffset, EditorBrushNoise, noiseRadius);
    }

    private static void ApplyEditorBrushFlatten_Rainfall(int longitude, int latitude, float distanceFactor)
    {
        int sampleRadius = 1;

        float strToValue = BrushStrengthFactor_Rainfall *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffsetFactor = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        TerrainCell cellNorth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude - sampleRadius);
        TerrainCell cellEast = CurrentWorld.GetCellWithSphericalWrap(longitude + sampleRadius, latitude);
        TerrainCell cellSouth = CurrentWorld.GetCellWithSphericalWrap(longitude, latitude + sampleRadius);
        TerrainCell cellWest = CurrentWorld.GetCellWithSphericalWrap(longitude - sampleRadius, latitude);

        float targetValue =
            (cellNorth.BaseRainfallValue + cellNorth.BaseRainfallOffset +
            cellEast.BaseRainfallValue + cellEast.BaseRainfallOffset +
            cellSouth.BaseRainfallValue + cellSouth.BaseRainfallOffset +
            cellWest.BaseRainfallValue + cellWest.BaseRainfallOffset) / 4f;

        float currentValue = cell.BaseRainfallValue + cell.BaseRainfallOffset;
        float valueOffset = (targetValue - currentValue) * valueOffsetFactor;

        CurrentWorld.ModifyCellRainfall(cell, valueOffset);
    }

    public static void UpdateEditorBrushState()
    {
        _lastEditorBrushTargetCell = EditorBrushTargetCell;
        _lastEditorBrushRadius = EditorBrushRadius;
        _editorBrushWasVisible = EditorBrushIsVisible;
    }

    public static void UpdateTextures()
    {
        if (CurrentDevMode != DevMode.None)
        {
            UpdatedPixelCount = 0;
        }

        UpdateMapTextureColors();
        UpdateMapOverlayTextureColors();
        UpdateMapActivityTextureColors();

        if (Manager.AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            UpdateMapOverlayShaderTextureColors();
        }

        CurrentMapTexture.SetPixels32(_manager._currentMapTextureColors);
        CurrentMapOverlayTexture.SetPixels32(_manager._currentMapOverlayTextureColors);
        CurrentMapActivityTexture.SetPixels32(_manager._currentMapActivityTextureColors);

        if (Manager.AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            CurrentMapOverlayShaderInfoTexture.SetPixels32(_manager._currentMapOverlayShaderInfoColor);
        }

        CurrentMapTexture.Apply();
        CurrentMapOverlayTexture.Apply();
        CurrentMapActivityTexture.Apply();

        if (Manager.AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            CurrentMapOverlayShaderInfoTexture.Apply();
        }

        ResetUpdatedAndHighlightedCells();

        LastTextureUpdateDate = CurrentWorld.CurrentDate;
    }

    public static void UpdatePointerOverlayTextureColors(Color32[] textureColors)
    {
        if (_editorBrushWasVisible && (_lastEditorBrushTargetCell != null))
        {
            UpdatePointerOverlayTextureColorsFromBrush(textureColors, _lastEditorBrushTargetCell, _lastEditorBrushRadius, true);
        }

        if (EditorBrushIsVisible && (EditorBrushTargetCell != null))
        {
            UpdatePointerOverlayTextureColorsFromBrush(textureColors, EditorBrushTargetCell, EditorBrushRadius);
        }
    }

    public static void UpdateMapTextureColors()
    {
        Color32[] textureColors = _manager._currentMapTextureColors;

        foreach (TerrainCell cell in TerrainUpdatedCells)
        {
            UpdateMapTextureColorsFromCell(textureColors, cell);
        }
    }

    public static void UpdateMapOverlayTextureColors()
    {
        Color32[] textureColors = _manager._currentMapOverlayTextureColors;

        foreach (TerrainCell cell in _lastUpdatedCells)
        {
            if (UpdatedCells.Contains(cell))
                continue;

            UpdateMapOverlayTextureColorsFromCell(textureColors, cell);
        }

        foreach (TerrainCell cell in UpdatedCells)
        {
            UpdateMapOverlayTextureColorsFromCell(textureColors, cell);
        }
    }

    public static void UpdateMapActivityTextureColors()
    {
        Color32[] textureColors = _manager._currentMapActivityTextureColors;

        if (_displayGroupActivityWasEnabled)
        {
            foreach (TerrainCell cell in _lastUpdatedCells)
            {
                if (UpdatedCells.Contains(cell))
                    continue;

                if (HighlightedCells.Contains(cell))
                    continue;

                UpdateMapActivityTextureColorsFromCell(textureColors, cell);
            }
        }

        _displayGroupActivityWasEnabled = _displayGroupActivity;

        if (_displayRoutes || _displayGroupActivity)
        {
            foreach (TerrainCell cell in UpdatedCells)
            {
                if (HighlightedCells.Contains(cell))
                    continue;

                UpdateMapActivityTextureColorsFromCell(textureColors, cell, _displayGroupActivity);
            }
        }

        foreach (TerrainCell cell in HighlightedCells)
        {
            UpdateMapActivityTextureColorsFromCell(textureColors, cell);
        }
    }

    public static void UpdateMapOverlayShaderTextureColors()
    {
        Color32[] overlayShaderInfoColors = _manager._currentMapOverlayShaderInfoColor;

        foreach (TerrainCell cell in UpdatedCells)
        {
            UpdateMapOverlayShaderTextureColorsFromCell(overlayShaderInfoColors, cell);
        }
    }

    public static bool CellShouldBeHighlighted(TerrainCell cell)
    {
        if (((_highlightMode & HighlightMode.OnSelectedCell) == HighlightMode.OnSelectedCell) &&
            cell.IsSelected)
        {
            return true;
        }

        if (((_highlightMode & HighlightMode.OnHoveredCell) == HighlightMode.OnHoveredCell) &&
            cell.IsHovered)
        {
            return true;
        }

        if (((_observableUpdateTypes & CellUpdateType.Region) == CellUpdateType.Region) &&
            (cell.Region != null))
        {
            if (((_highlightMode & HighlightMode.OnSelectedCollection) ==
                HighlightMode.OnSelectedCollection) && cell.Region.IsSelected &&
                (_filterHighlightCollection?.Invoke(cell.Region) ?? true))
            {
                return true;
            }

            if (((_highlightMode & HighlightMode.OnHoveredCollection) ==
                HighlightMode.OnHoveredCollection) && cell.Region.IsHovered &&
                (_filterHighlightCollection?.Invoke(cell.Region) ?? true))
            {
                return true;
            }
        }

        if (((_observableUpdateTypes & CellUpdateType.Territory) == CellUpdateType.Territory) &&
            (cell.EncompassingTerritory != null))
        {
            if (((_highlightMode & HighlightMode.OnSelectedCollection) ==
                HighlightMode.OnSelectedCollection) && cell.EncompassingTerritory.IsSelected &&
                (_filterHighlightCollection?.Invoke(cell.EncompassingTerritory) ?? true))
            {
                return true;
            }

            if (((_highlightMode & HighlightMode.OnHoveredCollection) == 
                HighlightMode.OnHoveredCollection) && cell.EncompassingTerritory.IsHovered &&
                (_filterHighlightCollection?.Invoke(cell.EncompassingTerritory) ?? true))
            {
                return true;
            }

            var faction = cell.GetClosestFaction(cell.EncompassingTerritory.Polity);

            if (faction != null)
            {
                if (((_highlightMode & HighlightMode.OnHoveredCollection) ==
                    HighlightMode.OnHoveredCollection) && faction.IsHovered &&
                    (_filterHighlightCollection?.Invoke(faction) ?? true))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool TerritoryShouldBeHighlighted(Territory territory)
    {
        if (((_highlightMode & HighlightMode.OnSelectedCollection) == HighlightMode.OnSelectedCollection) &&
            territory.IsSelected)
        {
            return true;
        }

        if (((_highlightMode & HighlightMode.OnHoveredCollection) == HighlightMode.OnHoveredCollection) &&
            territory.IsHovered)
        {
            return true;
        }

        return false;
    }

    public static bool RegionShouldBeHighlighted(Region region)
    {
        if (((_highlightMode & HighlightMode.OnSelectedCollection) == HighlightMode.OnSelectedCollection) &&
            region.IsSelected)
        {
            return true;
        }

        if (((_highlightMode & HighlightMode.OnHoveredCollection) == HighlightMode.OnHoveredCollection) &&
            region.IsHovered)
        {
            return true;
        }

        return false;
    }

    public static void UpdatePointerOverlayTextureColorsFromBrush(Color32[] textureColors, TerrainCell centerCell, int radius, bool erase = false)
    {
        World world = centerCell.World;

        int sizeX = world.Width;
        int sizeY = world.Height;

        int r = PixelToCellRatio;

        int centerX = centerCell.Longitude;
        int centerY = centerCell.Latitude;

        float fRadius = radius - 0.1f;

        int startJ = centerY - radius + 1;
        int endJ = centerY + radius;

        for (int j = startJ; j < endJ; j++)
        {
            int mJ = j;
            int mOffsetI = 0;

            if (mJ < 0) // do a polar wrap around the y-axis when near a pole
            {
                mJ = -mJ - 1;
                mOffsetI = sizeX / 2;
            }
            else if (mJ >= sizeY)
            {
                mJ = (2 * sizeY) - mJ - 1;
                mOffsetI = sizeX / 2;
            }

            int jDiff = j - centerY;
            int iRadius = (int)MathUtility.GetComponent(fRadius, jDiff);

            int offsetI = centerX - iRadius;
            mOffsetI = (mOffsetI + offsetI + sizeX) % sizeX; // make sure the brush wraps around the x-axis and account for radial y-axis wraps
            int iDiameter = 1 + (iRadius * 2);

            for (int uI = 0; uI < iDiameter; uI++)
            {
                int mI = (uI + mOffsetI) % sizeX;
                int i = uI + offsetI;

                int iDiff = i - centerX;
                float dist = MathUtility.GetMagnitude(iDiff, jDiff);

                bool isBorder = (dist < radius) && (dist > (radius - 1.1f));

                for (int m = 0; m < r; m++)
                {
                    for (int n = 0; n < r; n++)
                    {
                        int offsetY = sizeX * r * (mJ * r + n);
                        int offsetX = mI * r + m;

                        if (erase)
                            textureColors[offsetY + offsetX] = _transparentColor;
                        else if (isBorder)
                            textureColors[offsetY + offsetX] = _brushBorderColor;
                        else
                            textureColors[offsetY + offsetX] = _brushColor;
                    }
                }
            }
        }
    }

    public static void UpdateMapTextureColorsFromCell(Color32[] textureColors, TerrainCell cell)
    {
        World world = cell.World;

        int sizeX = world.Width;

        int r = PixelToCellRatio;

        int i = cell.Longitude;
        int j = cell.Latitude;

        Color cellColor = GenerateColorFromTerrainCell(cell);

        for (int m = 0; m < r; m++)
        {
            for (int n = 0; n < r; n++)
            {
                int offsetY = sizeX * r * (j * r + n);
                int offsetX = i * r + m;

                textureColors[offsetY + offsetX] = cellColor;

                if (CurrentDevMode != DevMode.None)
                {
                    UpdatedPixelCount++;
                }
            }
        }
    }

    public static void UpdateMapOverlayTextureColorsFromCell(Color32[] textureColors, TerrainCell cell)
    {
        World world = cell.World;

        int sizeX = world.Width;

        int r = PixelToCellRatio;

        int i = cell.Longitude;
        int j = cell.Latitude;

        Color cellColor = GenerateOverlayColorFromTerrainCell(cell);

        for (int m = 0; m < r; m++)
        {
            for (int n = 0; n < r; n++)
            {
                int offsetY = sizeX * r * (j * r + n);
                int offsetX = i * r + m;

                textureColors[offsetY + offsetX] = cellColor;

                if (CurrentDevMode != DevMode.None)
                {
                    UpdatedPixelCount++;
                }
            }
        }
    }

    public static void UpdateMapActivityTextureColorsFromCell(Color32[] textureColors, TerrainCell cell, bool displayActivityCells = false)
    {
        World world = cell.World;

        int sizeX = world.Width;

        int r = PixelToCellRatio;

        int i = cell.Longitude;
        int j = cell.Latitude;

        Color cellColor = GenerateActivityColorFromTerrainCell(cell, displayActivityCells);

        for (int m = 0; m < r; m++)
        {
            for (int n = 0; n < r; n++)
            {
                int offsetY = sizeX * r * (j * r + n);
                int offsetX = i * r + m;

                textureColors[offsetY + offsetX] = cellColor;

                if (CurrentDevMode != DevMode.None)
                {
                    UpdatedPixelCount++;
                }
            }
        }
    }

    public static void UpdateMapOverlayShaderTextureColorsFromCell(Color32[] textureColors, TerrainCell cell)
    {
        World world = cell.World;

        int sizeX = world.Width;

        int r = PixelToCellRatio;

        int i = cell.Longitude;
        int j = cell.Latitude;

        Color cellColor = GenerateOverlayShaderInfoFromTerrainCell(cell);

        for (int m = 0; m < r; m++)
        {
            for (int n = 0; n < r; n++)
            {
                int offsetY = sizeX * r * (j * r + n);
                int offsetX = i * r + m;

                textureColors[offsetY + offsetX] = cellColor;

                if (CurrentDevMode != DevMode.None)
                {
                    UpdatedPixelCount++;
                }
            }
        }
    }

    public static Texture2D GeneratePointerOverlayTextureFromWorld(World world)
    {
        int sizeX = world.Width;
        int sizeY = world.Height;

        int r = PixelToCellRatio;

        Color32[] textureColors = new Color32[sizeX * sizeY * r * r];

        Texture2D texture = new Texture2D(sizeX * r, sizeY * r, TextureFormat.ARGB32, false);

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                Color cellColor = _transparentColor;

                for (int m = 0; m < r; m++)
                {
                    for (int n = 0; n < r; n++)
                    {
                        int offsetY = sizeX * r * (j * r + n);
                        int offsetX = i * r + m;

                        textureColors[offsetY + offsetX] = cellColor;
                    }
                }
            }
        }

        texture.SetPixels32(textureColors);

        texture.Apply();

        _manager._pointerOverlayTextureColors = textureColors;
        _manager._pointerOverlayTexture = texture;

        return texture;
    }

    private delegate Color GenerateColorFromTerrainCellDelegate(TerrainCell cell);

    private static void GenerateTextureColorsFromWorld(World world, GenerateColorFromTerrainCellDelegate generateColorFromTerrainCell, out Color32[] textureColors, out Texture2D texture)
    {
        int sizeX = world.Width;
        int sizeY = world.Height;

        int r = PixelToCellRatio;

        textureColors = new Color32[sizeX * sizeY * r * r];

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                Color cellColor = generateColorFromTerrainCell(world.TerrainCells[i][j]);

                for (int m = 0; m < r; m++)
                {
                    for (int n = 0; n < r; n++)
                    {
                        int offsetY = sizeX * r * (j * r + n);
                        int offsetX = i * r + m;

                        textureColors[offsetY + offsetX] = cellColor;

                        if (CurrentDevMode != DevMode.None)
                        {
                            UpdatedPixelCount++;
                        }
                    }
                }
            }
        }

        texture = new Texture2D(sizeX * r, sizeY * r, TextureFormat.ARGB32, false);

        texture.SetPixels32(textureColors);

        texture.Apply();
    }

    public static void GenerateMapTextureFromWorld(World world)
    {
        GenerateTextureColorsFromWorld(world, GenerateColorFromTerrainCell, out Color32[] textureColors, out Texture2D texture);

        _manager._currentMapTextureColors = textureColors;
        _manager._currentMapTexture = texture;
    }

    public static void GenerateMapOverlayTextureFromWorld(World world)
    {
        GenerateTextureColorsFromWorld(world, GenerateOverlayColorFromTerrainCell, out Color32[] textureColors, out Texture2D texture);

        _manager._currentMapOverlayTextureColors = textureColors;
        _manager._currentMapOverlayTexture = texture;
    }

    public static void GenerateMapActivityTextureFromWorld(World world)
    {
        GenerateTextureColorsFromWorld(world, GenerateActivityColorFromTerrainCell, out Color32[] textureColors, out Texture2D texture);

        _manager._currentMapActivityTextureColors = textureColors;
        _manager._currentMapActivityTexture = texture;
    }

    public static void GenerateMapOverlayShaderInfoTextureFromWorld(World world)
    {
        GenerateTextureColorsFromWorld(world, GenerateOverlayShaderInfoFromTerrainCell, out Color32[] textureColors, out Texture2D texture);

        _manager._currentMapOverlayShaderInfoColor = textureColors;
        _manager._currentMapOverlayShaderInfoTexture = texture;
    }

    private static float GetSlant(TerrainCell cell)
    {
        if (_manager._currentCellSlants[cell.Longitude, cell.Latitude] != null)
        {
            return _manager._currentCellSlants[cell.Longitude, cell.Latitude].Value;
        }

        Dictionary<Direction, TerrainCell> neighbors = cell.Neighbors;

        float wAltitude = 0;
        float eAltitude = 0;

        int c = 0;
        if (neighbors.TryGetValue(Direction.West, out TerrainCell nCell))
        {
            wAltitude += nCell.Altitude;
            c++;
        }

        if (neighbors.TryGetValue(Direction.Southwest, out nCell))
        {
            wAltitude += nCell.Altitude;
            c++;
        }

        if (neighbors.TryGetValue(Direction.South, out nCell))
        {
            wAltitude += nCell.Altitude;
            c++;
        }

        wAltitude /= (float)c;

        c = 0;

        if (neighbors.TryGetValue(Direction.East, out nCell))
        {
            eAltitude += nCell.Altitude;
            c++;
        }

        if (neighbors.TryGetValue(Direction.Northeast, out nCell))
        {
            eAltitude += nCell.Altitude;
            c++;
        }

        if (neighbors.TryGetValue(Direction.North, out nCell))
        {
            eAltitude += nCell.Altitude;
            c++;
        }

        eAltitude /= (float)c;

        float value = wAltitude - eAltitude;

        _manager._currentCellSlants[cell.Longitude, cell.Latitude] = value;

        return value;
    }

    public static bool ResolvingPlayerInvolvedDecisionChain { get; set; }

    private static bool IsCoastWater(TerrainCell cell)
    {
        if (cell.WaterBiomePresence < 0.5f)
            return false;

        return cell.IsPartOfCoastline;
    }

    private static bool IsCoastLand(TerrainCell cell)
    {
        if (cell.WaterBiomePresence >= 0.5f)
            return false;

        return cell.IsPartOfCoastline;
    }

    private static Color GenerateColorFromTerrainCell(TerrainCell cell)
    {
        Color color;

        switch (_planetView)
        {
            case PlanetView.Biomes:
                color = GenerateBiomeColor(cell);
                break;

            case PlanetView.Elevation:
                color = GenerateAltitudeContourColor(cell);
                break;

            case PlanetView.Coastlines:
                color = GenerateCoastlineColor(cell);
                break;

            default:
                throw new System.Exception("Unsupported Planet View Type");
        }

        color.a = 1;

        return color;
    }

    private static Color GenerateOverlayColorFromTerrainCell(TerrainCell cell)
    {
        Color color = _transparentColor;

        if (cell == null)
        {
            throw new System.NullReferenceException("cell is null");
        }

        int? maxPopulation = null;

        if (CurrentWorld.MostPopulousGroup != null)
        {
            maxPopulation = CurrentWorld.MostPopulousGroup.Population;
        }

        switch (_planetOverlay)
        {
            case PlanetOverlay.None:
                break;

            case PlanetOverlay.General:
                color = SetGeneralOverlayColor(cell, color, maxPopulation);
                break;

            case PlanetOverlay.PopDensity:
                color = SetPopulationDensityOverlayColor(cell, color, maxPopulation);
                break;

            case PlanetOverlay.FarmlandDistribution:
                color = SetFarmlandOverlayColor(cell, color);
                break;

            case PlanetOverlay.PopCulturalPreference:
                color = SetPopCulturalPreferenceOverlayColor(cell, color);
                break;

            case PlanetOverlay.PopCulturalActivity:
                color = SetPopCulturalActivityOverlayColor(cell, color);
                break;

            case PlanetOverlay.PopCulturalSkill:
                color = SetPopCulturalSkillOverlayColor(cell, color);
                break;

            case PlanetOverlay.PopCulturalKnowledge:
                color = SetPopCulturalKnowledgeOverlayColor(cell, color);
                break;

            case PlanetOverlay.PopCulturalDiscovery:
                color = SetPopCulturalDiscoveryOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityTerritory:
                color = SetPolityTerritoryOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityAdminCost:
                color = SetPolityAdminCostOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityCluster:
                color = SetPolityClusterOverlayColor(cell, color);
                break;

            case PlanetOverlay.ClusterAdminCost:
                color = SetClusterAdminCostOverlayColor(cell, color);
                break;

            case PlanetOverlay.FactionCoreDistance:
                color = SetFactionCoreDistanceOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityProminence:
                color = SetPolityProminenceOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityContacts:
                color = SetPolityContactsOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityCoreRegions:
                color = SetPolityCoreRegionsOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityCulturalPreference:
                color = SetPolityCulturalPreferenceOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityCulturalActivity:
                color = SetPolityCulturalActivityOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityCulturalSkill:
                color = SetPolityCulturalSkillOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityCulturalKnowledge:
                color = SetPolityCulturalKnowledgeOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityCulturalDiscovery:
                color = SetPolityCulturalDiscoveryOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolitySelection:
                color = SetPolitySelectionOverlayColor(cell, color);
                break;

            case PlanetOverlay.Temperature:
                color = SetTemperatureOverlayColor(cell, color);
                break;

            case PlanetOverlay.Rainfall:
                color = SetRainfallOverlayColor(cell, color);
                break;

            case PlanetOverlay.DrainageBasins:
                color = SetDrainageBasinOverlayColor(cell, color);
                break;

            case PlanetOverlay.Arability:
                color = SetArabilityOverlayColor(cell, color);
                break;

            case PlanetOverlay.Accessibility:
                color = SetAccessibilityOverlayColor(cell, color);
                break;

            case PlanetOverlay.Hilliness:
                color = SetHillinessOverlayColor(cell, color);
                break;

            case PlanetOverlay.BiomeTrait:
                color = SetBiomeTraitPresenceOverlayColor(cell, color);
                break;

            case PlanetOverlay.Layer:
                color = SetLayerOverlayColor(cell, color);
                break;

            case PlanetOverlay.Region:
                color = SetRegionOverlayColor(cell, color);
                break;

            case PlanetOverlay.RegionSelection:
                color = SetRegionSelectionOverlayColor(cell, color);
                break;

            case PlanetOverlay.FactionSelection:
                color = SetFactionSelectionOverlayColor(cell, color);
                break;

            case PlanetOverlay.CellSelection:
                if (_planetOverlaySubtype == GroupProminenceOverlaySubtype)
                    color = SetGroupSelectionByProminenceOverlayColor(cell, color);
                else
                    throw new System.Exception(
                        $"Unsupported Planet Overlay Subtype: {_planetOverlaySubtype}");
                break;

            case PlanetOverlay.Language:
                color = SetLanguageOverlayColor(cell, color);
                break;

            case PlanetOverlay.PopChange:
                color = SetPopulationChangeOverlayColor(cell, color);
                break;

            case PlanetOverlay.MigrationPressure:
                color = SetMigrationPressureOverlayColor(cell, color);
                break;

            case PlanetOverlay.PolityMigrationPressure:
                color = SetPolityMigrationPressureOverlayColor(cell, color);
                break;

            case PlanetOverlay.UpdateSpan:
                color = SetUpdateSpanOverlayColor(cell, color);
                break;

            case PlanetOverlay.Migration:
                color = SetMigrationOverlayColor(cell, color);
                break;

            default:
                throw new System.Exception(
                    $"Unsupported Planet Overlay Type: {_planetOverlay}");
        }

        return color;
    }

    private static Color GenerateActivityColorFromTerrainCell(TerrainCell cell)
    {
        return GenerateActivityColorFromTerrainCell(cell, false);
    }

    private static Color GenerateActivityColorFromTerrainCell(TerrainCell cell, bool displayActivityCells)
    {
        if (_displayRoutes && cell.HasCrossingRoutes)
        {
            float maxRouteChance = 0;

            bool hasUsedRoute = false;

            foreach (Route route in cell.CrossingRoutes)
            {
                CellGroup travelGroup = route.FirstCell.Group;

                if (travelGroup != null)
                {
                    float travelFactor = travelGroup.SeaTravelFactor;

                    float routeLength = route.Length;
                    float routeLengthFactor = Mathf.Pow(routeLength, 2);

                    float successChance = travelFactor / (travelFactor + routeLengthFactor);

                    maxRouteChance = Mathf.Max(maxRouteChance, successChance);
                }

                hasUsedRoute |= route.Used;
            }

            if (maxRouteChance > 0)
            {
                float alpha = MathUtility.ToPseudoLogaritmicScale01(maxRouteChance, 1f);

                Color color = GetOverlayColor(OverlayColorId.InactiveRoute);
                color.a = alpha * 0.5f;

                if (hasUsedRoute)
                {
                    color = GetOverlayColor(OverlayColorId.ActiveRoute);
                    color.a = alpha;
                }

                return color;
            }
        }

        if (CellShouldBeHighlighted(cell))
        {
            return Color.white * 0.65f;
        }
        else if (displayActivityCells)
        {
            return Color.white * 0.5f;
        }

        return _transparentColor;
    }

    private static Color GenerateOverlayShaderInfoFromTerrainCell(TerrainCell cell)
    {
        Color color = _transparentColor;

        switch (_planetOverlay)
        {
            case PlanetOverlay.DrainageBasins:
                color = SetDrainageBasinOverlayShaderInfoColor(cell);
                break;
        }

        return color;
    }

    private static Color GenerateCoastlineColor(TerrainCell cell)
    {
        if (_mapPalette.Count == 0)
        {
            return Color.black;
        }

        if (IsCoastWater(cell))
        {
            return _mapPalette[2];
        }

        if (!cell.IsBelowSeaLevel)
        {
            if (cell.WaterBiomePresence >= 0.5f)
                return _mapPalette[3];

            float slant = GetSlant(cell);
            float altDiff = World.MaxPossibleAltitude - World.MinPossibleAltitude;
            altDiff /= 2f;

            float slantFactor = 1;
            if (altDiff > 0)
            {
                slantFactor = 0.75f * slant / altDiff;

                if (slantFactor > 1)
                    slantFactor = 1;
            }

            slantFactor = Mathf.Min(1, -(20 * slantFactor));

            if (slantFactor > 0.1f)
            {
                return (_mapPalette[4] * slantFactor) + (_mapPalette[1] * (1 - slantFactor));
            }

            return _mapPalette[1];
        }

        return _mapPalette[0];
    }

    private static Color GenerateAltitudeColor(float altitude)
    {
        float value;

        if (altitude < 0)
        {
            value = (2 - altitude / World.MinPossibleAltitude - Manager.SeaLevelOffset) / 2f;

            Color color1 = Color.blue;

            return new Color(color1.r * value, color1.g * value, color1.b * value);
        }

        value = (1 + altitude / (World.MaxPossibleAltitude - Manager.SeaLevelOffset)) / 2f;

        Color color2 = new Color(1f, 0.6f, 0);

        return new Color(color2.r * value, color2.g * value, color2.b * value);
    }

    private static Color GenerateBiomeColor(TerrainCell cell)
    {
        float slant = GetSlant(cell);
        //float altDiff = CurrentWorld.MaxAltitude - CurrentWorld.MinAltitude;
        float altDiff = World.MaxPossibleAltitude - World.MinPossibleAltitude;
        altDiff /= 2f;

        float slantFactor = 1;
        if (altDiff > 0)
        {
            slantFactor = 0.75f * slant / altDiff;

            if (slantFactor > 1)
                slantFactor = 1;
        }

        slantFactor = Mathf.Min(1f, (4f + (10f * slantFactor)) / 5f);

        float altitudeFactor = Mathf.Min(1f, (0.5f + ((cell.Altitude - CurrentWorld.MinAltitude) / altDiff)) / 1.5f);

        Color color = Color.black;

        if (_biomePalette.Count == 0)
        {
            return color;
        }

        for (int i = 0; i < cell.PresentBiomeIds.Count; i++)
        {
            string biomeId = cell.PresentBiomeIds[i];

            Biome biome = Biome.Biomes[biomeId];

            float biomeRelPresence = cell.BiomePresences[i];
            Color biomeColor = biome.Color;

            color.r += biomeColor.r * biomeRelPresence;
            color.g += biomeColor.g * biomeRelPresence;
            color.b += biomeColor.b * biomeRelPresence;
        }

        if (cell.FarmlandPercentage > 0)
        {
            color = color * (1 - cell.FarmlandPercentage) + GetOverlayColor(OverlayColorId.Farmland) * cell.FarmlandPercentage;
        }

        return color * slantFactor * altitudeFactor;
    }

    private static bool IsRegionBorder(Region region, TerrainCell cell)
    {
        return region.IsInnerBorderCell(cell);
    }

    private static Color SetRegionOverlayColor(TerrainCell cell, Color color)
    {
        Region region = cell.Region;

        if (region != null)
        {
            Color regionColor = GenerateColorFromId(region.Id);

            Biome mostPresentBiome = Biome.Biomes[region.BiomeWithMostPresence];
            regionColor = mostPresentBiome.Color * 0.85f + regionColor * 0.15f;

            bool isRegionBorder = IsRegionBorder(region, cell);

            if (!isRegionBorder)
            {
                regionColor /= 1.5f;
            }

            regionColor.a = 0.5f;

            color = regionColor;
        }

        return color;
    }

    private static Color SetRegionSelectionOverlayColor(TerrainCell cell, Color color)
    {
        Faction guidedFaction = CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't generate overlay without an active guided faction");
        }

        Region region = cell.Region;

        if ((region != null) &&
            (region.SelectionFilterType != Region.FilterType.None))
        {
            Color regionColor = GenerateColorFromId(region.Id);

            Biome mostPresentBiome = Biome.Biomes[region.BiomeWithMostPresence];
            regionColor = mostPresentBiome.Color * 0.85f + regionColor * 0.15f;

            bool isRegionBorder = IsRegionBorder(region, cell);

            if (region.SelectionFilterType == Region.FilterType.Core)
            {
                regionColor = (0.4f * regionColor) + 0.6f * Color.blue;
            }
            else if (region.SelectionFilterType == Region.FilterType.Selectable)
            {
                regionColor = (0.4f * regionColor) + 0.6f * Color.cyan;
            }

            if (!isRegionBorder)
            {
                regionColor /= 1.5f;
            }

            regionColor.a = 0.5f;

            color = regionColor;
        }

        return color;
    }

    private static Color SetPolitySelectionOverlayColor(TerrainCell cell, Color color)
    {
        var territory = cell.EncompassingTerritory;

        if ((territory != null) &&
            (territory.SelectionFilterType != Territory.FilterType.None))
        {
            Color polityColor = GenerateColorFromId(territory.Polity.Id);

            bool isTerritoryBorder = IsTerritoryBorder(territory, cell);

            if (territory.SelectionFilterType == Territory.FilterType.Core)
            {
                polityColor = (0.2f * polityColor) + 0.8f * Color.blue;
            }
            else if (territory.SelectionFilterType == Territory.FilterType.Selectable)
            {
                polityColor = (0.2f * polityColor) + 0.8f * Color.cyan;
            }

            if (!isTerritoryBorder)
            {
                polityColor /= 1.5f;
            }

            polityColor.a = 0.5f;

            color = polityColor;
        }

        return color;
    }

    private static Color SetFactionSelectionOverlayColor(TerrainCell cell, Color color)
    {
        var territory = cell.EncompassingTerritory;

        if ((territory != null) &&
            (territory.SelectionFilterType != Territory.FilterType.None))
        {
            bool isTerritoryBorder = IsTerritoryBorder(territory, cell);

            Faction faction = cell.GetClosestFaction(territory.Polity);

            bool isSelectableFaction = false;

            if ((faction != null) && 
                ((faction.SelectionFilterType == Faction.FilterType.Selectable) ||
                (faction.SelectionFilterType == Faction.FilterType.Related)))
            {
                color = GenerateColorFromId(faction.Id);

                if (faction.SelectionFilterType == Faction.FilterType.Related)
                {
                    color = (0.3f * color) + 0.7f * Color.yellow;
                }
                else if (faction.SelectionFilterType == Faction.FilterType.Selectable)
                {
                    color = (0.3f * color) + 0.7f * Color.cyan;
                    isSelectableFaction = true;
                }
            }
            else
            {
                color = GenerateColorFromId(territory.Polity.Id);

                if (territory.SelectionFilterType == Territory.FilterType.Core)
                {
                    color = (0.3f * color) + 0.7f * Color.blue;
                }
                else if (territory.SelectionFilterType == Territory.FilterType.Involved)
                {
                    color = (0.3f * color) + 0.7f * Color.yellow;
                }
            }

            if (!isTerritoryBorder)
            {
                color /= 1.5f;
            }

            if (isSelectableFaction)
            {
                color.a = 0.5f;
            }
            else
            {
                color.a = 0.2f;
            }
        }

        return color;
    }

    private static Color SetGroupSelectionByProminenceOverlayColor(TerrainCell cell, Color color)
    {
        if (CurrentWorld.GuidedFaction == null)
            throw new System.Exception("Can't generate overlay without an active guided faction");

        if (!(CurrentInputRequest is GroupSelectionRequest))
            throw new System.Exception("Can't generate overlay without an region selection request");

        color = SetPolityProminenceOverlayColor(cell, color);

        if (cell.SelectionFilterType == TerrainCell.FilterType.None)
        {
            color *= 0.37f;
        }
        else if (cell.SelectionFilterType == TerrainCell.FilterType.Core)
        {
            color = (color * 0.5f) + (Color.white * 0.5f);
        }

        return color;
    }

    private static bool IsLanguageBorder(Language language, TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (nCell.Group == null)
                return true;

            Language nLanguage = nCell.Group.Culture.Language;

            if (nLanguage == null)
                return true;

            if (nLanguage != language)
                return true;
        }

        return false;
    }

    private static Color SetLanguageOverlayColor(TerrainCell cell, Color color)
    {
        if (cell.Group != null)
        {
            Language groupLanguage = cell.Group.Culture.Language;

            if (groupLanguage != null)
            {
                Color languageColor = GenerateColorFromId(groupLanguage.Id);

                bool isLanguageBorder = IsLanguageBorder(groupLanguage, cell);

                if (!isLanguageBorder)
                {
                    languageColor /= 2f;
                }

                languageColor.a = 0.85f;

                color = languageColor;
            }
            else
            {
                color = GetUnincorporatedGroupColor();
            }
        }

        return color;
    }

    private static bool IsTerritoryBorder(Territory territory, TerrainCell cell)
    {
        return territory.IsPartOfInnerBorder(cell);
    }

    private static Color GetUnincorporatedGroupColor()
    {
        Color color = GetOverlayColor(OverlayColorId.GeneralDensityOptimal);
        color.a = 0.5f;

        return color;
    }

    private static Color SetPolityTerritoryOverlayColor(TerrainCell cell, Color color)
    {
        if (cell.EncompassingTerritory != null)
        {
            Polity territoryPolity = cell.EncompassingTerritory.Polity;

            color = GenerateColorFromId(territoryPolity.Id);

            bool isPolityCoreGroup = false;
            bool isFactionCoreGroup = false;

            bool isTerritoryBorder = IsTerritoryBorder(cell.EncompassingTerritory, cell);

            if (cell.Group != null)
            {
                isPolityCoreGroup = territoryPolity.CoreGroup == cell.Group;
                isFactionCoreGroup = cell.Group.GetFactionCores().Count > 0;
            }

            if (!isPolityCoreGroup)
            {
                if (isFactionCoreGroup)
                {
                    color /= 1.35f;
                }
                else if (!isTerritoryBorder)
                {
                    color /= 2.5f;
                }
                else
                {
                    color /= 1.75f;
                }
            }

            color.a = 1;
        }
        else if (cell.Group != null)
        {
            color = GetUnincorporatedGroupColor();
        }

        return color;
    }

    private static Color SetPolityClusterOverlayColor(TerrainCell cell, Color color)
    {
        CellGroup group = cell.Group;

        if (group != null)
        {
            bool useSelected = CurrentWorld.SelectedTerritory != null;

            if (cell.EncompassingTerritory != null)
            {
                Polity territoryPolity = cell.EncompassingTerritory.Polity;

                PolityProminence prominence = group.GetPolityProminence(territoryPolity);
                bool isSelected = cell.EncompassingTerritory.IsSelected;

                if (useSelected && !isSelected)
                {
                    foreach (var localProminence in group.GetPolityProminences())
                    {
                        if (localProminence.Polity.Territory.IsSelected)
                        {
                            isSelected = true;
                            prominence = localProminence;
                            break;
                        }
                    }
                }

                if (prominence?.Cluster != null)
                {
                    color = GenerateColorFromId(prominence.Cluster.Id);

                    if (useSelected && !isSelected)
                    {
                        color *= 0.5f;
                        color.a = 1;
                    }
                }
            }
            else
            {
                color = GetUnincorporatedGroupColor();
            }
        }

        return color;
    }

    private static Color SetClusterAdminCostOverlayColor(TerrainCell cell, Color color)
    {
        CellGroup group = cell?.Group;

        if (group != null)
        {
            color = GetUnincorporatedGroupColor();

            bool foundCluster = false;
            bool isSelected = false;
            float maxAdminCost = 0;
            bool useSelected = CurrentWorld.SelectedTerritory != null;

            foreach (var prominence in group.GetPolityProminences())
            {
                foundCluster = true;

                if (prominence.Polity.Territory.IsSelected)
                    isSelected = true;

                float adminCost =
                    Mathf.Min(prominence.Cluster.TotalAdministrativeCost, PolityProminenceCluster.MaxAdminCost);

                if (isSelected)
                {
                    maxAdminCost = adminCost;
                    break;
                }
                else
                {
                    maxAdminCost = Mathf.Max(adminCost, maxAdminCost);
                }
            }

            if (foundCluster)
            {
                if (CurrentMaxAdminCost < maxAdminCost)
                    CurrentMaxAdminCost = maxAdminCost;

                float value = 0;
                if (maxAdminCost > 0)
                {
                    value = maxAdminCost / CurrentMaxAdminCost;
                    value = MathUtility.ToPseudoLogaritmicScale01(value, 1f);
                }

                value = 0.25f + 0.75f * Mathf.Clamp01(value);

                if (useSelected && !isSelected)
                {
                    color = (Color.cyan + Color.yellow) * value / 2;
                }
                else
                {
                    color = Color.yellow * value;
                }
                color.a = 1;
            }

            return color;
        }

        return color;
    }

    private static Color SetFactionCoreDistanceOverlayColor(TerrainCell cell, Color color)
    {
        if (cell.Group != null)
        {
            if (cell.EncompassingTerritory != null)
            {
                Polity territoryPolity = cell.EncompassingTerritory.Polity;

                Color territoryColor = GenerateColorFromId(territoryPolity.Id);

                float factionCoreDistance = cell.Group.GetFactionCoreDistance(territoryPolity);

                float distanceFactor = Mathf.Sqrt(factionCoreDistance);
                distanceFactor = 1 - 0.9f * Mathf.Min(1, distanceFactor / 50f);

                color = territoryColor * distanceFactor;
                color.a = 1;
            }
            else
            {
                color = GetUnincorporatedGroupColor();
            }
        }

        return color;
    }

    private static Color SetPolityProminenceOverlayColor(TerrainCell cell, Color color)
    {
        if (cell.Group != null)
        {
            int polityCount = 0;
            float totalProminenceValueFactor = 0;

            Color mixedPolityColor = Color.black;
            foreach (PolityProminence p in cell.PolityProminences)
            {
                polityCount++;

                float prominenceValueFactor = 0.2f + p.Value;

                Color polityColor = GenerateColorFromId(p.PolityId);
                polityColor *= prominenceValueFactor;
                totalProminenceValueFactor += prominenceValueFactor;

                mixedPolityColor += polityColor;
            }

            color = GetUnincorporatedGroupColor();

            if ((polityCount > 0) && (totalProminenceValueFactor > 0))
            {
                totalProminenceValueFactor = Mathf.Clamp01(totalProminenceValueFactor);
                mixedPolityColor /= polityCount;
                mixedPolityColor.a = 1;

                color = (color * (1 - totalProminenceValueFactor)) + (mixedPolityColor * totalProminenceValueFactor);
            }
        }

        return color;
    }

    private static Color GenerateColorFromId(Identifier id)
    {
        int mId = id.GetHashCode();
        if (mId < 0)
        {
            mId = int.MaxValue + mId + 1;
        }

        int primaryColor = mId % 3;
        float secondaryColorIntensity = ((mId / 3) % 4) / 3f;
        float tertiaryColorIntensity = ((mId / 12) % 2) / 2f;
        int secondaryColor = (mId / 24) % 2;

        float red = 0;
        float green = 0;
        float blue = 0;

        switch (primaryColor)
        {
            case 0:
                red = 1;

                if (secondaryColor == 0)
                {
                    green = secondaryColorIntensity;
                    blue = tertiaryColorIntensity;
                }
                else
                {
                    blue = secondaryColorIntensity;
                    green = tertiaryColorIntensity;
                }

                break;
            case 1:
                green = 1;

                if (secondaryColor == 0)
                {
                    blue = secondaryColorIntensity;
                    red = tertiaryColorIntensity;
                }
                else
                {
                    red = secondaryColorIntensity;
                    blue = tertiaryColorIntensity;
                }

                break;
            case 2:
                blue = 1;

                if (secondaryColor == 0)
                {
                    red = secondaryColorIntensity;
                    green = tertiaryColorIntensity;
                }
                else
                {
                    green = secondaryColorIntensity;
                    red = tertiaryColorIntensity;
                }

                break;
        }

        Color color = new Color(red, green, blue, 1.0f);

        return color;
    }

    private static Color SetPolityContactsOverlayColor(TerrainCell cell, Color color)
    {
        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        float contactValue = 0;

        Territory selectedTerritory = CurrentWorld.SelectedTerritory;

        bool isSelectedTerritory = false;
        bool isInContact = false;

        Polity selectedPolity = null;
        Polity polity = territory.Polity;

        if (selectedTerritory != null)
        {
            selectedPolity = selectedTerritory.Polity;

            if (selectedTerritory != territory)
            {
                int count = selectedPolity.GetContactCount(polity);

                contactValue = MathUtility.ToPseudoLogaritmicScale01(count);

                isInContact = (count > 0);
            }
            else
            {
                contactValue = 1;

                isSelectedTerritory = true;
            }
        }

        Color replacementColor = GetOverlayColor(OverlayColorId.Territory);

        if (IsTerritoryBorder(territory, cell))
        {
            replacementColor = GetOverlayColor(OverlayColorId.TerritoryBorder);
        }

        if (isSelectedTerritory)
        {
            replacementColor = (0.25f * replacementColor) + (0.75f * GetOverlayColor(OverlayColorId.SelectedTerritory));
        }
        else if (isInContact)
        {
            float relationshipVal = selectedPolity.GetRelationshipValue(polity);

            Color contactColor =
                GetOverlayColor(OverlayColorId.ContactedTerritoryGood) * (relationshipVal) +
                GetOverlayColor(OverlayColorId.ContactedTerritoryBad) * (1f - relationshipVal);

            float modContactValue = 0.15f + 0.85f * contactValue;

            replacementColor = ((0.25f + 0.75f * (1f - modContactValue)) * replacementColor)
                + ((0.75f * modContactValue) * contactColor);
        }

        color = replacementColor;

        return color;
    }

    private static Color SetPolityCoreRegionsOverlayColor(TerrainCell cell, Color color)
    {
        float unselectedAlpha = 0.4f;

        Territory territory = cell.EncompassingTerritory;
        Region region = cell.Region;

        Color backColor = color;

        if (territory != null)
        {
            backColor = GenerateColorFromId(territory.Polity.Id);

            if (IsTerritoryBorder(territory, cell))
            {
                backColor /= 1.25f;
            }

            if (!territory.IsSelected)
            {
                backColor.a *= unselectedAlpha;
            }
        }
        else if (cell.Group != null)
        {
            backColor = GetUnincorporatedGroupColor();
        }
        else if ((region == null) ||
            (region.SelectionFilterType != Region.FilterType.Core))
        {
            return color;
        }

        color = backColor;

        Polity selectedPolity = CurrentWorld.SelectedTerritory?.Polity;

        if ((selectedPolity != null) &&
            (territory != CurrentWorld.SelectedTerritory) &&
            (region != null) &&
            (region.SelectionFilterType == Region.FilterType.Core))
        {
            Color regionColor = GenerateColorFromId(region.Id);
            Color selPolityColor = GenerateColorFromId(selectedPolity.Id);
            selPolityColor.a *= unselectedAlpha;

            Biome mostPresentBiome = Biome.Biomes[region.BiomeWithMostPresence];
            regionColor = Color.white * 0.2f + mostPresentBiome.Color * 0.65f + regionColor * 0.15f;

            regionColor.a *= unselectedAlpha;

            if (territory != null)
            {
                regionColor = (0.1f * regionColor) + (0.1f * backColor) + (0.8f * selPolityColor);
            }
            else
            {
                regionColor = (0.2f * regionColor) + (0.8f * selPolityColor);
            }

            if (IsRegionBorder(region, cell))
            {
                regionColor.a *= 0.8f;
            }

            color = regionColor;
        }

        return color;
    }

    private static Color SetPopulationChangeOverlayColor(TerrainCell cell, Color color)
    {
        float deltaLimitFactor = 1f;

        float prevPopulation = 0;
        float population = 0;

        float delta = 0;

        if (cell.Group != null)
        {
            prevPopulation = cell.Group.PreviousPopulation;
            population = cell.Group.Population;

            delta = population - prevPopulation;
        }

        if (delta > 0)
        {
            float value = delta / (population * deltaLimitFactor);
            value = Mathf.Clamp01(value);
            value = MathUtility.ToPseudoLogaritmicScale01(value, 1);

            color = Color.green * value;
        }
        else if (delta < 0)
        {
            float value = -delta / (prevPopulation * deltaLimitFactor);
            value = Mathf.Clamp01(value);
            value = MathUtility.ToPseudoLogaritmicScale01(value, 1);

            color = Color.red * value;
        }

        return color;
    }

    private static Color SetMigrationPressureOverlayColor(TerrainCell cell, Color color)
    {
        CellGroup group = cell.Group;

        if (group == null)
        {
            return color;
        }

        color = Color.red * group.MigrationPressure;

        return color;
    }

    private static Color SetPolityMigrationPressureOverlayColor(TerrainCell cell, Color color)
    {
        CellGroup group = cell.Group;

        if (group == null)
        {
            return color;
        }

        Territory territory = cell.EncompassingTerritory;

        PolityProminence prominence = null;
        Color polityColor = Color.black;

        if (territory != null)
        {
            Polity polity = territory.Polity;

            polityColor = GenerateColorFromId(polity.Id);
            prominence = group.GetPolityProminence(polity);
        }

        if (prominence != null)
        {
            float nonPolPressure = group.MigrationPressure - prominence.MigrationPressure;
            Color pColor = (Color.white * nonPolPressure) + (polityColor * prominence.MigrationPressure);

            color = (0.5f * polityColor) + (0.5f * pColor);
        }
        else
        {
            color = Color.red * group.MigrationPressure * 0.5f;
        }

        return color;
    }

    private static Color SetPopulationDensityOverlayColor(TerrainCell cell, Color color, int? maxPopulation)
    {
        if ((maxPopulation == null) || (maxPopulation <= 0))
            return color;

        float maxPopFactor = cell.MaxAreaPercent * maxPopulation.Value / 5f;

        float population = 0;

        if (cell.Group != null)
        {
            population = cell.Group.Population;

            if (cell.Group.MigrationTagged && DisplayMigrationTaggedGroup)
                return Color.green;

#if DEBUG
            if (cell.Group.DebugTagged && DisplayDebugTaggedGroups)
                return Color.green;
#endif

            if (population > 0)
            {
                //float value = (population + maxPopFactor) / (maxPopulation.Value + maxPopFactor);
                float value = population / maxPopulation.Value;

                // Use logaritmic scale (TODO: Make this optional)
                value = MathUtility.ToPseudoLogaritmicScale01(value, 1f);

                color = Color.red * value;
            }
        }

        return color;
    }

    private static Color GetPopCulturalAttributeOverlayColor(float value)
    {
        value = 0.15f + 0.85f * Mathf.Clamp01(value);

        return Color.cyan * value;
    }

    private static Color GetPolityCulturalAttributeOverlayColor(float value, bool isTerritoryBorder = false)
    {
        Color baseColor = GetUnincorporatedGroupColor();

        Color color = isTerritoryBorder ? new Color(0, 0.75f, 1.0f) : Color.cyan;

        value = 0.15f + 0.85f * Mathf.Clamp01(value);

        return (baseColor * (1 - value)) + (color * value);
    }

    private static Color SetPopCulturalPreferenceOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
            return color;

        if (cell.Group != null)
        {
            if ((cell.Group.Population > 0) &&
                cell.Group.Culture.GetPreference(_planetOverlaySubtype) is CellCulturalPreference preference)
            {
                color = GetPopCulturalAttributeOverlayColor(preference.Value);
            }
        }

        return color;
    }

    private static Color SetPolityCulturalPreferenceOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        CulturalPreference preference = territory.Polity.Culture.GetPreference(_planetOverlaySubtype);

        if (preference == null)
            return GetUnincorporatedGroupColor();

        color = GetPolityCulturalAttributeOverlayColor(preference.Value, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color SetPolityAdminCostOverlayColor(TerrainCell cell, Color color)
    {
        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        float adminCost =
            Mathf.Min(territory.Polity.TotalAdministrativeCost, Polity.MaxAdminCost);

        if (CurrentMaxAdminCost < adminCost)
            CurrentMaxAdminCost = adminCost;

        float value = 0;
        if (adminCost > 0)
        {
            value = adminCost / CurrentMaxAdminCost;
            value = MathUtility.ToPseudoLogaritmicScale01(value, 1f);
        }

        Color baseColor = GetUnincorporatedGroupColor();

        color = IsTerritoryBorder(territory, cell) ? new Color(1, 0.75f, 0f) : Color.yellow;

        value = 0.25f + 0.75f * Mathf.Clamp01(value);

        color = (baseColor * (1 - value)) + (color * value);

        return color;
    }

    private static Color SetPopCulturalActivityOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
            return color;

        if (cell.Group != null)
        {
            if ((cell.Group.Population > 0) &&
                cell.Group.Culture.GetActivity(_planetOverlaySubtype) is CellCulturalActivity activity)
            {
                if (activity.Contribution > 0)
                {
                    color = GetPopCulturalAttributeOverlayColor(activity.Contribution);
                }
            }
        }

        return color;
    }

    private static Color SetPolityCulturalActivityOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        CulturalActivity activity = territory.Polity.Culture.GetActivity(_planetOverlaySubtype);

        if (activity == null)
            return GetUnincorporatedGroupColor();

        color = GetPolityCulturalAttributeOverlayColor(activity.Contribution, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color SetPopCulturalSkillOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
            return color;

        if (cell.Group != null)
        {
            if ((cell.Group.Population > 0) &&
                cell.Group.Culture.GetSkill(_planetOverlaySubtype) is CellCulturalSkill skill)
            {
                if (skill.Value >= 0.001)
                {
                    color = GetPopCulturalAttributeOverlayColor(skill.Value);
                }
            }
        }

        return color;
    }

    private static Color SetPolityCulturalSkillOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        CulturalSkill skill = territory.Polity.Culture.GetSkill(_planetOverlaySubtype);

        if (skill == null)
            return GetUnincorporatedGroupColor();

        color = GetPolityCulturalAttributeOverlayColor(skill.Value, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color SetPopCulturalKnowledgeOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
            return color;

        if (cell.Group != null)
        {
            if ((cell.Group.Population > 0) &&
                cell.Group.Culture.GetKnowledge(_planetOverlaySubtype) is CellCulturalKnowledge knowledge)
            {
                float highestLimit = knowledge.GetHighestLimit();

                if (highestLimit <= 0)
                    throw new System.Exception("Highest Limit is less or equal to 0");

                float normalizedValue = knowledge.Value / highestLimit;

                if (normalizedValue >= 0.001f)
                {
                    color = GetPopCulturalAttributeOverlayColor(normalizedValue);
                }
            }
        }

        return color;
    }

    private static Color SetPolityCulturalKnowledgeOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        CulturalKnowledge knowledge = territory.Polity.Culture.GetKnowledge(_planetOverlaySubtype);

        if (knowledge == null)
            return GetUnincorporatedGroupColor();

        if (!(territory.Polity.CoreGroup.Culture.GetKnowledge(_planetOverlaySubtype) is CellCulturalKnowledge cellKnowledge))
            return GetUnincorporatedGroupColor();

        float highestLimit = cellKnowledge.GetHighestLimit();

        if (highestLimit <= 0)
            throw new System.Exception("Highest Limit is less or equal to 0");

        float normalizedValue = knowledge.Value / highestLimit;
        color = GetPolityCulturalAttributeOverlayColor(normalizedValue, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color SetPopCulturalDiscoveryOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
            return color;

        if (cell.Group != null)
        {
            if ((cell.Group.Population > 0) && cell.Group.Culture.HasDiscovery(_planetOverlaySubtype))
            {
                color = GetPopCulturalAttributeOverlayColor(1);
            }
        }

        return color;
    }

    private static Color SetPolityCulturalDiscoveryOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            if (cell.Group == null)
                return color;

            return GetUnincorporatedGroupColor();
        }

        if (!territory.Polity.Culture.HasDiscovery(_planetOverlaySubtype))
            return GetUnincorporatedGroupColor();

        color = GetPolityCulturalAttributeOverlayColor(1, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color GetOverlayColor(OverlayColorId id)
    {
        return _overlayPalette[(int)id];
    }

    private static Color SetDrainageBasinOverlayColor(TerrainCell cell, Color color)
    {
        float baseAlpha = 0.35f;
        color = Color.black;
        color.a = baseAlpha;

        if (cell.FlowingWater > 0)
        {
            float accPercent = 1 - cell.FlowingWater / (CurrentWorld.MaxWaterAccumulation - cell.Rainfall);
            accPercent = Mathf.Pow(accPercent, 10);
            accPercent = 1 - accPercent;
            float value = 0.1f + (0.90f * accPercent);

            //Color riverColor = GenerateColorFromId(cell.RiverId);
            //color += riverColor * value;

            color += GetOverlayColor(OverlayColorId.RiverBasins) * value;
            color.a = baseAlpha + (1 - baseAlpha) * value;
        }

        return color;
    }

    private static Color SetDrainageBasinOverlayShaderInfoColor(TerrainCell cell)
    {
        Color color = Color.black;

        if (cell.FlowingWater > 0)
        {
            float flowOffset = ((cell.RiverId * 11) + cell.RiverLength) / 10f;

            float riverLengthValue = Mathf.Repeat(flowOffset, 1);

            color = Color.white * riverLengthValue;
        }

        return color;
    }

    private static Color SetArabilityOverlayColor(TerrainCell cell, Color color)
    {
        float baseAlpha = 0.5f;

        color = GetOverlayColor(OverlayColorId.Arability) * cell.Arability;
        color.a = baseAlpha + ((1 - baseAlpha) * cell.Arability);

        return color;
    }

    private static Color SetHillinessOverlayColor(TerrainCell cell, Color color)
    {
        color = GetLowMedHighColor(1 - cell.Hilliness);

        return color;
    }

    private static Color SetBiomeTraitPresenceOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == NoOverlaySubtype)
            return color;

        color = GetLowMedHighColor(cell.GetBiomeTraitPresence(_planetOverlaySubtype) * (1 - cell.FarmlandPercentage));

        return color;
    }

    private static Color SetAccessibilityOverlayColor(TerrainCell cell, Color color)
    {
        color = GetLowMedHighColor(cell.Accessibility);

        return color;
    }

    private static Color GetLowMedHighColor(float value)
    {
        Color color = GetOverlayColor(OverlayColorId.LowValue) * Mathf.Max(0, 1 - (2 * value));
        color += GetOverlayColor(OverlayColorId.MedValue) * Mathf.Max(0, 1 - 2 * Mathf.Abs(value - 0.5f));
        color += GetOverlayColor(OverlayColorId.HighValue) * Mathf.Max(0, (2 * value) - 1);
        color *= 0.5f;

        return color;
    }

    private static Color SetLayerOverlayColor(TerrainCell cell, Color color)
    {
        float baseAlpha = 0.5f;

        color = Color.black;
        color.a = baseAlpha;

        if (_planetOverlaySubtype == NoOverlaySubtype)
            return color;

        Layer layer = Layer.Layers[_planetOverlaySubtype];

        float normalizedValue = cell.GetLayerValue(_planetOverlaySubtype);
        normalizedValue = normalizedValue / layer.MaxPresentValue;

        if (normalizedValue >= 0.001f)
        {
            float intensity = 0.15f + 0.85f * normalizedValue;

            color += layer.Color * intensity;
            color.a = baseAlpha + ((1 - baseAlpha) * intensity);
        }

        return color;
    }

    private static Color SetFarmlandOverlayColor(TerrainCell cell, Color color)
    {
        float value = cell.FarmlandPercentage;

        if (value >= 0.05f)
        {
            value = 0.15f + 0.85f * value;

            color = GetOverlayColor(OverlayColorId.Farmland) * value;
        }

        return color;
    }

    private static Color SetGeneralOverlayColor(TerrainCell cell, Color baseColor, int? maxPopulation)
    {
        bool hasGroup = false;
        bool inTerritory = false;
        Color densityColorSubOptimal = GetOverlayColor(OverlayColorId.GeneralDensitySubOptimal);
        Color cellColor = GetOverlayColor(OverlayColorId.GeneralDensityOptimal);

        if (cell.EncompassingTerritory != null)
        {
            inTerritory = true;

            Polity territoryPolity = cell.EncompassingTerritory.Polity;

            cellColor = GenerateColorFromId(territoryPolity.Id);

            bool isTerritoryBorder = IsTerritoryBorder(cell.EncompassingTerritory, cell);
            bool isCoreGroup =
                (cell.Group != null) && (territoryPolity.CoreGroup == cell.Group);

            if (!isCoreGroup)
            {
                if (!isTerritoryBorder)
                {
                    cellColor /= 2.5f;
                }
                else
                {
                    cellColor /= 1.75f;
                }
            }
        }

        if (cell.Group != null)
        {
            hasGroup = true;

            if (!inTerritory)
            {
                if (cell.Group.Culture == null)
                {
                    throw new System.NullReferenceException("group " + cell.Position + " culture not initialized...");
                }

                if (cell.Group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out int knowledgeValue))
                {
                    float minValue = SocialOrganizationKnowledge.MinValueForTribeFormation;
                    float startValue = SocialOrganizationKnowledge.InitialValue;

                    float knowledgeFactor = Mathf.Clamp01((knowledgeValue - startValue) / (minValue - startValue));

                    cellColor = (cellColor * knowledgeFactor) + densityColorSubOptimal * (1f - knowledgeFactor);
                }
                else
                {
                    cellColor = densityColorSubOptimal;
                }
            }
        }

        if (hasGroup && !inTerritory)
        {
            baseColor = cellColor;
            baseColor.a = 0.5f;
        }
        else if (inTerritory)
        {
            baseColor = cellColor;
            baseColor.a = 0.85f;
        }

        return baseColor;
    }

    private static Color SetUpdateSpanOverlayColor(TerrainCell cell, Color color)
    {
        float normalizedValue = 0;
        float population = 0;

        if (cell.Group != null)
        {
            population = cell.Group.Population;

            long lastUpdateDate = cell.Group.LastUpdateDate;
            long nextUpdateDate = cell.Group.NextUpdateDate;
            long updateSpan = nextUpdateDate - lastUpdateDate;

            if (CurrentMaxUpdateSpan < updateSpan)
                CurrentMaxUpdateSpan = updateSpan;

            float maxUpdateSpan =
                Mathf.Min(CurrentMaxUpdateSpan, CellGroup.MaxUpdateSpan);

            normalizedValue = Mathf.Clamp01(updateSpan / maxUpdateSpan);
            normalizedValue = 1 - MathUtility.ToPseudoLogaritmicScale01(normalizedValue, 1f);
        }

        if ((population > 0) && (normalizedValue > 0))
        {
            color = Color.red * normalizedValue;
        }

        return color;
    }

    private static Color SetMigrationOverlayColor(TerrainCell cell, Color color)
    {
        if ((cell.Group != null)
            && (cell.Group.LastPopulationMigration != null)
            && (cell.Group.LastPopulationMigration.EndDate > LastTextureUpdateDate))
        {
            MigratingPopulationSnapshot migrationPop = cell.Group.LastPopulationMigration;

            if (migrationPop.PolityId != null)
            {
                color = GenerateColorFromId(migrationPop.PolityId);
            }
            else
            {
                color = Color.gray;
            }
        }
        else if (cell.EncompassingTerritory != null)
        {
            Polity territoryPolity = cell.EncompassingTerritory.Polity;

            color = GenerateColorFromId(territoryPolity.Id);
            color *= 0.5f;
        }
        else if (cell.Group != null)
        {
            color = GetUnincorporatedGroupColor();
        }

        return color;
    }

    private static Color SetTemperatureOverlayColor(TerrainCell cell, Color color)
    {
        float span = World.MaxPossibleTemperature - World.MinPossibleTemperature;

        float value = (cell.Temperature - (World.MinPossibleTemperature + Manager.TemperatureOffset)) / span;

        color = new Color(value, 0, 1f - value)
        {
            a = 0.65f
        };

        return color;
    }

    private static Color SetRainfallOverlayColor(TerrainCell cell, Color color)
    {
        Color addColor = Color.black;

        float maxPossibleRainfall = Mathf.Max(World.MaxPossibleRainfall, CurrentWorld.MaxRainfall);

        if (cell.Rainfall > 0)
        {
            float value = 0.05f + (0.95f * cell.Rainfall / maxPossibleRainfall);

            addColor = Color.green * value;
        }

        color = addColor;
        color.a = 0.65f;

        return color;
    }

    private static Color GenerateAltitudeContourColor(TerrainCell cell)
    {
        Color color = new Color(1, 0.6f, 0);

        float shadeValue = 1.0f;

        float altitude = cell.Altitude;

        float value = Mathf.Max(0, altitude / CurrentWorld.MaxAltitude);

        if (cell.WaterBiomePresence >= 0.5f)
        {
            value = Mathf.Max(0, 1f - altitude / CurrentWorld.MinAltitude);
            color = Color.blue;
        }

        if (cell.BiomeWithMostPresence != null)
        {
            if (Biome.Biomes[cell.BiomeWithMostPresence].TerrainType == BiomeTerrainType.Ice)
            {
                color = Color.white;
            }
        }
        else
        {
            Debug.LogWarning("cell has no biome presence: " + cell.Position);
        }

        while (shadeValue > value)
        {
            shadeValue -= 0.05f;
        }

        shadeValue = 0.5f * shadeValue + 0.5f;

        color = new Color(color.r * shadeValue, color.g * shadeValue, color.b * shadeValue);

        return color;
    }

    private static XmlAttributeOverrides GenerateAttributeOverrides()
    {
        XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();

        return attrOverrides;
    }

    public static Texture2D LoadTexture(string path)
    {
        if (MainThread == Thread.CurrentThread)
        {
            return LoadTextureInternal(path);
        }
        else
        {
            // we need to execute this operation in the main thread...
            return EnqueueTask(() => LoadTextureInternal(path));
        }
    }

    private static Texture2D LoadTextureInternal(string path)
    {
        Texture2D texture;

        if (File.Exists(path))
        {
            byte[] data = File.ReadAllBytes(path);
            texture = new Texture2D(1, 1);
            if (texture.LoadImage(data))
                return texture;
        }

        return null;
    }

    public static Sprite CreateSprite(Texture2D texture)
    {
        if (MainThread == Thread.CurrentThread)
        {
            return CreateSpriteInternal(texture);
        }
        else
        {
            // we need to execute this operation in the main thread...
            return EnqueueTask(() => CreateSpriteInternal(texture));
        }
    }

    private static Sprite CreateSpriteInternal(Texture2D texture)
    {
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));

        return sprite;
    }

    public static TextureValidationResult ValidateTexture(Texture2D texture)
    {
        if ((texture.width < WorldWidth) && (texture.height < WorldHeight))
        {
            return TextureValidationResult.NotMinimumRequiredDimensions;
        }

        return TextureValidationResult.Ok;
    }

    public static void LoadMods(ICollection<string> paths)
    {
        if (paths.Count == 0)
            throw new System.ArgumentException("Number of mods to load can't be zero");

        World.ResetStaticModData();

        CellGroup.ResetEventGenerators();
        Faction.ResetEventGenerators();
        Polity.ResetEventGenerators();

        Layer.ResetLayers();
        Biome.ResetBiomes();

        Adjective.ResetAdjectives();
        RegionAttribute.ResetAttributes();
        Element.ResetElements();

        CurrentDiscoveryUid = 0;
        Discovery.ResetDiscoveries();
        Discovery033.ResetDiscoveries();
        Knowledge.ResetKnowledges();

        PreferenceGenerator.ResetPreferenceGenerators();
        EventGenerator.ResetGenerators();

        ActionCategory.ResetActionCategories();
        ModAction.ResetActions();

        ModDecision.ResetDecisions();

        Knowledge.InitializeKnowledges();

        float progressPerMod = 0.1f / paths.Count;

        foreach (string path in paths)
        {
            if (_progressCastMethod != null)
            {
                string directoryName = Path.GetFileName(path);

                _progressCastMethod(LastStageProgress, "Loading Mod '" + directoryName + "'...");
            }

            LoadMod(path, progressPerMod);

            LastStageProgress += progressPerMod;
        }

        PreferenceGenerator.InitializePreferenceGenerators();
        Discovery033.InitializeDiscoveries();

        EventGenerator.InitializeGenerators();
    }

    delegate void LoadModFileDelegate(string filename);

    private static void TryLoadModFiles(
        LoadModFileDelegate loadModFile,
        string path,
        float progressPerModSegment)
    {
        if (!Directory.Exists(path))
            return;

        string[] files = Directory.GetFiles(path, "*.json");

        if (files.Length > 0)
        {
            float progressPerFile = progressPerModSegment / files.Length;
            float accProgress = LastStageProgress;

            foreach (string file in files)
            {
                loadModFile(file);

                accProgress += progressPerFile;

                _progressCastMethod?.Invoke(accProgress);
            }
        }
    }

    private static void LoadMod(string path, float progressPerMod)
    {
        string version = ModVersionReader.GetLoaderVersion(path);

        if (version.StartsWith(ModVersionReader.LoaderVersion033))
        {
            LoadMod033(path, progressPerMod);
        }
        else if (version.StartsWith(ModVersionReader.LoaderVersion034))
        {
            LoadMod034(path, progressPerMod);
        }
        else
        {
            throw new System.Exception("Unsupported mod version: " + version);
        }
    }

    private static void LoadMod034(string path, float progressPerMod)
    {
        if (!Directory.Exists(path))
        {
            throw new System.ArgumentException("Mod path '" + path + "' not found");
        }

        float progressPerSegment = progressPerMod / 11f;

        TryLoadModFiles(Layer.LoadLayersFile033, Path.Combine(path, @"Layers"), progressPerSegment);
        TryLoadModFiles(Biome.LoadBiomesFile033, Path.Combine(path, @"Biomes"), progressPerSegment);
        TryLoadModFiles(Adjective.LoadAdjectivesFile033, Path.Combine(path, @"Adjectives"), progressPerSegment);
        TryLoadModFiles(RegionAttribute.LoadRegionAttributesFile033, Path.Combine(path, @"RegionAttributes"), progressPerSegment);
        TryLoadModFiles(Element.LoadElementsFile033, Path.Combine(path, @"Elements"), progressPerSegment);
        TryLoadModFiles(PreferenceGenerator.LoadPreferencesFile, Path.Combine(path, @"Preferences"), progressPerSegment);
        TryLoadModFiles(Discovery.LoadDiscoveriesFile, Path.Combine(path, @"Discoveries"), progressPerSegment);
        TryLoadModFiles(EventGenerator.LoadEventFile, Path.Combine(path, @"Events"), progressPerSegment);
        TryLoadModFiles(ActionCategory.LoadActionCategoryFile, Path.Combine(path, @"Actions", @"Categories"), progressPerSegment);
        TryLoadModFiles(ModAction.LoadActionFile, Path.Combine(path, @"Actions"), progressPerSegment);
        TryLoadModFiles(ModDecision.LoadDecisionFile, Path.Combine(path, @"Decisions"), progressPerSegment);
    }

    private static void LoadMod033(string path, float progressPerMod)
    {
        if (!Directory.Exists(path))
        {
            throw new System.ArgumentException("Mod path '" + path + "' not found");
        }

        float progressPerSegment = progressPerMod / 6f;

        TryLoadModFiles(Layer.LoadLayersFile033, Path.Combine(path, @"Layers"), progressPerSegment);
        TryLoadModFiles(Biome.LoadBiomesFile033, Path.Combine(path, @"Biomes"), progressPerSegment);
        TryLoadModFiles(Adjective.LoadAdjectivesFile033, Path.Combine(path, @"Adjectives"), progressPerSegment);
        TryLoadModFiles(RegionAttribute.LoadRegionAttributesFile033, Path.Combine(path, @"RegionAttributes"), progressPerSegment);
        TryLoadModFiles(Element.LoadElementsFile033, Path.Combine(path, @"Elements"), progressPerSegment);
        TryLoadModFiles(Discovery033.LoadDiscoveriesFile033, Path.Combine(path, @"Discoveries"), progressPerSegment);
    }

    public static void InvokeGuidedFactionStatusChangeEvent()
    {
        EventManager.GuidedFactionStatusChange.Invoke();
    }
}
