using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
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
    PolityCulturalPreference,
    PolityCulturalActivity,
    PolityCulturalSkill,
    PolityCulturalKnowledge,
    PolityCulturalDiscovery,
    Temperature,
    Rainfall,
    DrainageBasins,
    Arability,
    Accessibility,
    Hilliness,
    BiomeTrait,
    Layer,
    Region,
    Language,
    PopChange,
    UpdateSpan,
    PolityCluster
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

    public const string NoOverlaySubtype = "None";

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

    public static bool LayersPresent = false;

    public static float LastStageProgress = 0;

    public static Thread MainThread { get; private set; }

    public static string SavePath { get; private set; }
    public static string HeightmapsPath { get; private set; }
    public static string ExportPath { get; private set; }

    public static string[] SupportedHeightmapFormats = { ".PSD", ".TIFF", ".JPG", ".TGA", ".PNG", ".BMP", ".PICT" };

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
    public static bool DebugModeEnabled = false;
    public static bool AnimationShadersEnabled = true;

    public static List<string> ActiveModPaths = new List<string>() { Path.Combine(@"Mods", "Base") };
    public static bool ModsAlreadyLoaded = false;

    public static Dictionary<string, LayerSettings> LayerSettings = new Dictionary<string, LayerSettings>();

    public static GameMode GameMode = GameMode.None;

    public static bool ViewingGlobe = false;

    public static int LastEventsTriggeredCount = 0;
    public static int LastMapUpdateCount = 0;
    public static int LastPixelUpdateCount = 0;
    public static long LastDateSpan = 0;

    public static bool DisableShortcuts = false;

    private static bool _isLoadReady = false;

    private static string _debugLogFilename = "debug";
    private static string _debugLogExt = ".log";
    private static StreamWriter _debugLogStream = null;
    private static bool _backupDebugLog = false;

    private static HashSet<TerrainCell> _lastUpdatedCells;

    private static int _resolutionWidthWindowed = 1600;
    private static int _resolutionHeightWindowed = 900;

    private static bool _resolutionInitialized = false;

    private static CellUpdateType _observableUpdateTypes = CellUpdateType.None;
    private static CellUpdateSubType _observableUpdateSubTypes = CellUpdateSubType.None;

    private static Manager _manager = new Manager();

    private static PlanetView _planetView = PlanetView.Biomes;
    private static PlanetOverlay _planetOverlay = PlanetOverlay.None;
    private static string _planetOverlaySubtype = "None";

    private static List<Color> _biomePalette = new List<Color>();
    private static List<Color> _mapPalette = new List<Color>();
    private static List<Color> _overlayPalette = new List<Color>();

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

    private ProgressCastDelegate _progressCastMethod = null;

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

    private long _currentMaxUpdateSpan = 0;

    private Queue<IManagerTask> _taskQueue = new Queue<IManagerTask>();

    private bool _performingAsyncTask = false;
    private bool _simulationRunning = false;
    private bool _worldReady = false;

    public XmlAttributeOverrides AttributeOverrides { get; private set; }

    public static bool PerformingAsyncTask
    {
        get
        {
            return _manager._performingAsyncTask;
        }
    }

    public static bool SimulationRunning
    {
        get
        {
            return _manager._simulationRunning;
        }
    }

    public static bool WorldIsReady
    {
        get
        {
            return _manager._worldReady;
        }
    }

    public static bool SimulationCanRun
    {
        get
        {
            bool canRun = (_manager._currentWorld.CellGroupCount > 0);

            return canRun;
        }
    }

    public static PlanetOverlay PlanetOverlay
    {
        get
        {
            return _planetOverlay;
        }
    }

    public static string PlanetOverlaySubtype
    {
        get
        {
            return _planetOverlaySubtype;
        }
    }

    public static bool DisplayRoutes
    {
        get
        {
            return _displayRoutes;
        }
    }

    public static bool DisplayGroupActivity
    {
        get
        {
            return _displayGroupActivity;
        }
    }

    public static int UndoableEditorActionsCount
    {
        get
        {
            return _undoableEditorActions.Count;
        }
    }

    public static int RedoableEditorActionsCount
    {
        get
        {
            return _redoableEditorActions.Count;
        }
    }

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

        _lastUpdatedCells = new HashSet<TerrainCell>();

        /// static initalizations

        Tribe.GenerateTribeNounVariations();
    }

    private static bool CanHandleKeyInput(bool requireCtrl, bool requireShift)
    {
        if (requireCtrl != (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            return false;

        if (requireShift != (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            return false;

        if (DisableShortcuts)
            return false;

        return true;
    }

    public static void HandleKeyUp(KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action)
    {
        if (!Input.GetKeyUp(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift))
            return;

        action.Invoke();
    }

    public static void HandleKey(KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action)
    {
        if (!Input.GetKey(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift))
            return;

        action.Invoke();
    }

    public static void HandleKeyDown(KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action)
    {
        if (!Input.GetKeyDown(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift))
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
            worldInfoStr += "[Date: " + GetDateString(CurrentWorld.CurrentDate) + "] - ";
        }

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
        _manager._simulationRunning = !state;
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

    public static Vector2 GetUVFromMapCoordinates(WorldPosition mapPosition)
    {
        return new Vector2(mapPosition.Longitude / (float)CurrentWorld.Width, mapPosition.Latitude / (float)CurrentWorld.Height);
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
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod;

        if (_manager._progressCastMethod == null)
        {
            _manager._progressCastMethod = (value, message, reset) => { };
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

            _manager._performingAsyncTask = false;
            _manager._simulationRunning = true;
        });
    }

    public static void GeneratePointerOverlayTextures()
    {
        GeneratePointerOverlayTextureFromWorld(CurrentWorld);
    }

    public static void GenerateTextures(bool doMapTexture, bool doOverlayMapTexture)
    {
        if (DebugModeEnabled)
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
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        UpdatedCells.UnionWith(polity.Territory.GetCells());

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.UnionWith(polity.Territory.GetCells());
        }

        if (polity.Territory.IsSelected)
        {
            HighlightedCells.UnionWith(polity.Territory.GetCells());
        }
    }

    public static void AddUpdatedCells(ICollection<TerrainCell> cells, CellUpdateType updateType, CellUpdateSubType updateSubType, bool highlight)
    {
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        UpdatedCells.UnionWith(cells);

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.UnionWith(cells);
        }

        if (highlight)
        {
            HighlightedCells.UnionWith(cells);
        }
    }

    public static void AddHighlightedCell(TerrainCell cell, CellUpdateType updateType)
    {
        HighlightedCells.Add(cell);
    }

    public static void AddHighlightedCells(ICollection<TerrainCell> cells, CellUpdateType updateType)
    {
        foreach (TerrainCell cell in cells)
            HighlightedCells.Add(cell);
    }

    public static void GenerateRandomHumanGroup(int initialPopulation)
    {
        World world = _manager._currentWorld;

        if (_manager._progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _manager._progressCastMethod;
        }

        world.GenerateRandomHumanGroups(1, initialPopulation);
    }

    public static void GenerateHumanGroup(int longitude, int latitude, int initialPopulation)
    {
        World world = _manager._currentWorld;

        if (_manager._progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _manager._progressCastMethod;
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
        _manager._worldReady = false;

        LastStageProgress = 0;

        ProgressCastDelegate progressCastMethod;

        if (_manager._progressCastMethod == null)
            progressCastMethod = (value, message, reset) => { };
        else
            progressCastMethod = _manager._progressCastMethod;

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
        _manager._currentMaxUpdateSpan = 0;

        _manager._worldReady = true;

        ForceWorldCleanup();
    }

    public static void GenerateNewWorldAsync(int seed, Texture2D heightmap = null, ProgressCastDelegate progressCastMethod = null)
    {
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod;

        if (_manager._progressCastMethod == null)
        {
            _manager._progressCastMethod = (value, message, reset) => { };
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

            _manager._performingAsyncTask = false;
            _manager._simulationRunning = true;
        });
    }

    public static void RegenerateWorld(GenerationType type)
    {
        _manager._worldReady = false;

        World world = _manager._currentWorld;

        if (_manager._progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _manager._progressCastMethod;
        }

        world.StartReinitialization(0f, 1.0f);
        world.Regenerate(type);
        world.FinishInitialization();

        _manager._currentCellSlants = new float?[world.Width, world.Height];
        _manager._currentMaxUpdateSpan = 0;

        _manager._worldReady = true;

        ForceWorldCleanup();
    }

    public static void RegenerateWorldAsync(GenerationType type, ProgressCastDelegate progressCastMethod = null)
    {
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod;

        if (_manager._progressCastMethod == null)
        {
            _manager._progressCastMethod = (value, message, reset) => { };
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

            _manager._performingAsyncTask = false;
            _manager._simulationRunning = true;
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
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod;

        if (_manager._progressCastMethod == null)
        {
            _manager._progressCastMethod = (value, message, reset) => { };
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

            _manager._performingAsyncTask = false;
            _manager._simulationRunning = true;
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
        _manager._worldReady = false;

        LastStageProgress = 0;

        ProgressCastDelegate progressCastMethod;

        if (_manager._progressCastMethod == null)
            progressCastMethod = (value, message, reset) => { };
        else
            progressCastMethod = _manager._progressCastMethod;

        ResetWorldLoadTrack();

        World world;

        XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);

        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            world = serializer.Deserialize(stream) as World;
        }

        if (_manager._progressCastMethod == null)
        {
            world.ProgressCastMethod = (value, message, reset) => { };
        }
        else
        {
            world.ProgressCastMethod = _manager._progressCastMethod;
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

        world.FinalizeLoad(progressBeforeFinalizing, 1.0f, _manager._progressCastMethod);

        _manager._currentWorld = world;
        _manager._currentCellSlants = new float?[world.Width, world.Height];
        _manager._currentMaxUpdateSpan = 0;

        WorldBeingLoaded = null;

        _manager._worldReady = true;

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

        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod;

        if (_manager._progressCastMethod == null)
        {
            _manager._progressCastMethod = (value, message, reset) => { };
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

            _manager._performingAsyncTask = false;
            _manager._simulationRunning = true;
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

        _manager._progressCastMethod?.Invoke(Mathf.Min(1, value));
    }

    private static void SetObservableUpdateTypes(PlanetOverlay overlay, string planetOverlaySubtype = "None")
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
            _observableUpdateTypes = CellUpdateType.Cell;
        }
        else if (overlay == PlanetOverlay.Region)
        {
            _observableUpdateTypes = CellUpdateType.Region;
        }
        else if (overlay == PlanetOverlay.PolityCluster)
        {
            _observableUpdateTypes = CellUpdateType.Cluster;
        }
        else if (overlay == PlanetOverlay.Language)
        {
            _observableUpdateTypes = CellUpdateType.Language;
        }
        else if ((overlay == PlanetOverlay.PolityTerritory) ||
            (overlay == PlanetOverlay.PolityContacts) ||
            (overlay == PlanetOverlay.PolityCulturalPreference) ||
            (overlay == PlanetOverlay.PolityCulturalActivity) ||
            (overlay == PlanetOverlay.PolityCulturalDiscovery) ||
            (overlay == PlanetOverlay.PolityCulturalKnowledge) ||
            (overlay == PlanetOverlay.PolityCulturalSkill))
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

    private static void SetObservableUpdateSubtypes(PlanetOverlay overlay, string planetOverlaySubtype = "None")
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
        else if ((overlay == PlanetOverlay.Region) ||
            (overlay == PlanetOverlay.PolityCluster) ||
            (overlay == PlanetOverlay.Language))
        {
            _observableUpdateSubTypes = CellUpdateSubType.Membership;
        }
        else if (overlay == PlanetOverlay.PolityTerritory)
        {
            _observableUpdateSubTypes = CellUpdateSubType.MembershipAndCore;
        }
        else if (overlay == PlanetOverlay.PolityContacts)
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

    public static void SetPlanetOverlay(PlanetOverlay overlay, string planetOverlaySubtype = "None")
    {
        SetObservableUpdateTypes(overlay, planetOverlaySubtype);
        SetObservableUpdateSubtypes(overlay, planetOverlaySubtype);

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
            AddHighlightedCells(CurrentWorld.SelectedRegion.GetCells(), CellUpdateType.Region);

            CurrentWorld.SelectedRegion.IsSelected = false;
            CurrentWorld.SelectedRegion = null;
        }

        if (region != null)
        {
            CurrentWorld.SelectedRegion = region;
            CurrentWorld.SelectedRegion.IsSelected = true;

            AddHighlightedCells(CurrentWorld.SelectedRegion.GetCells(), CellUpdateType.Region);
        }
    }

    public static void SetSelectedTerritory(Territory territory)
    {
        if (CurrentWorld.SelectedTerritory != null)
        {
            AddHighlightedCells(CurrentWorld.SelectedTerritory.GetCells(), CellUpdateType.Territory);

            Polity selectedPolity = CurrentWorld.SelectedTerritory.Polity;

            CurrentWorld.SelectedTerritory.IsSelected = false;
            CurrentWorld.SelectedTerritory = null;

            if (_planetOverlay == PlanetOverlay.PolityContacts)
            {
                foreach (PolityContact contact in selectedPolity.GetContacts())
                {
                    AddHighlightedCells(contact.Polity.Territory.GetCells(), CellUpdateType.Territory);
                }
            }
        }

        if (territory != null)
        {
            CurrentWorld.SelectedTerritory = territory;
            CurrentWorld.SelectedTerritory.IsSelected = true;

            AddHighlightedCells(CurrentWorld.SelectedTerritory.GetCells(), CellUpdateType.Territory);

            if (_planetOverlay == PlanetOverlay.PolityContacts)
            {
                Polity selectedPolity = territory.Polity;

                foreach (PolityContact contact in selectedPolity.GetContacts())
                {
                    AddHighlightedCells(contact.Polity.Territory.GetCells(), CellUpdateType.Territory);
                }
            }
        }
    }

    public static void SetSelectedCell(TerrainCell cell)
    {
        if (CurrentWorld.SelectedCell != null)
        {
            AddHighlightedCell(CurrentWorld.SelectedCell, CellUpdateType.All);

            CurrentWorld.SelectedCell.IsSelected = false;
            CurrentWorld.SelectedCell = null;
        }

        if (cell == null)
            return;

        CurrentWorld.SelectedCell = cell;
        CurrentWorld.SelectedCell.IsSelected = true;

        AddHighlightedCell(CurrentWorld.SelectedCell, CellUpdateType.All);

        SetSelectedRegion(cell.Region);
        SetSelectedTerritory(cell.EncompassingTerritory);
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

        if (faction != null)
        {
            faction.SetUnderPlayerGuidance(true);
        }

        CurrentWorld.GuidedFaction = faction;
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
        foreach (TerrainCell nCell in cell.Neighbors.Values)
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
        if (DebugModeEnabled)
        {
            UpdatedPixelCount = 0;
        }

        //Profiler.BeginSample("UpdateMapTextureColors");

        UpdateMapTextureColors();
        UpdateMapOverlayTextureColors();
        UpdateMapActivityTextureColors();

        if (Manager.AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            UpdateMapOverlayShaderTextureColors();
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("CurrentMapTexture.SetPixels32");

        CurrentMapTexture.SetPixels32(_manager._currentMapTextureColors);
        CurrentMapOverlayTexture.SetPixels32(_manager._currentMapOverlayTextureColors);
        CurrentMapActivityTexture.SetPixels32(_manager._currentMapActivityTextureColors);

        if (Manager.AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            CurrentMapOverlayShaderInfoTexture.SetPixels32(_manager._currentMapOverlayShaderInfoColor);
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("CurrentMapTexture.Apply");

        CurrentMapTexture.Apply();
        CurrentMapOverlayTexture.Apply();
        CurrentMapActivityTexture.Apply();

        if (Manager.AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            CurrentMapOverlayShaderInfoTexture.Apply();
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("ResetUpdatedAndHighlightedCells");

        ResetUpdatedAndHighlightedCells();

        //Profiler.EndSample();
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

        foreach (TerrainCell cell in UpdatedCells)
        {
            if (cell == null)
            {
                throw new System.NullReferenceException("Updated cell is null");
            }

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
        if (cell.IsSelected)
            return true;

        if ((_observableUpdateTypes & CellUpdateType.Region) == CellUpdateType.Region)
        {
            if ((cell.Region != null) && cell.Region.IsSelected)
                return true;
        }
        else if (((_observableUpdateTypes & CellUpdateType.Territory) == CellUpdateType.Territory) &&
            (_planetOverlay != PlanetOverlay.PolityContacts))
        {
            if ((cell.EncompassingTerritory != null) && cell.EncompassingTerritory.IsSelected)
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

                if (DebugModeEnabled)
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

                if (DebugModeEnabled)
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

                if (DebugModeEnabled)
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

                if (DebugModeEnabled)
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

                        if (DebugModeEnabled)
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

            case PlanetOverlay.PolityCluster:
                color = SetPolityClusterOverlayColor(cell, color);
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

            case PlanetOverlay.Language:
                color = SetLanguageOverlayColor(cell, color);
                break;

            case PlanetOverlay.PopChange:
                color = SetPopulationChangeOverlayColor(cell, color);
                break;

            case PlanetOverlay.UpdateSpan:
                color = SetUpdateSpanOverlayColor(cell, color);
                break;

            default:
                throw new System.Exception("Unsupported Planet Overlay Type");
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
            float maxRouteChance = float.MinValue;

            foreach (Route route in cell.CrossingRoutes)
            {
                if (route.Used)
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
                }
            }

            if (maxRouteChance > 0)
            {
                float alpha = MathUtility.ToPseudoLogaritmicScale01(maxRouteChance, 1f);

                Color color = GetOverlayColor(OverlayColorId.ActiveRoute);
                //color.a = (maxRouteChance * 0.7f) + 0.3f;
                color.a = alpha;

                return color;
            }
        }

        if (CellShouldBeHighlighted(cell))
        {
            return Color.white * 0.75f;
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

    private static bool IsLanguageBorder(Language language, TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.Neighbors.Values)
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
        return territory.IsPartOfBorder(cell);
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

            bool isTerritoryBorder = IsTerritoryBorder(cell.EncompassingTerritory, cell);
            bool isPolityCoreGroup = territoryPolity.CoreGroup == cell.Group;
            bool isFactionCoreGroup = cell.Group.GetFactionCores().Count > 0;

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
        if (cell.Group != null)
        {
            if (cell.EncompassingTerritory != null)
            {
                Polity territoryPolity = cell.EncompassingTerritory.Polity;

                PolityProminence prominence = cell.Group.GetPolityProminence(territoryPolity);

                if (prominence.Cluster != null)
                {
                    color = GenerateColorFromId(prominence.Cluster.Id);
                }
            }
            else
            {
                color = GetUnincorporatedGroupColor();
            }
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

                PolityProminence pi = cell.Group.GetPolityProminence(territoryPolity);

                float distanceFactor = Mathf.Sqrt(pi.FactionCoreDistance);
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
            foreach (PolityProminence p in cell.Group.GetPolityProminences())
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

        return new Color(red, green, blue, 1.0f);
    }

    private static Color SetPolityContactsOverlayColor(TerrainCell cell, Color color)
    {
        if (cell.Group == null)
            return color;

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
            return GetUnincorporatedGroupColor();

        float contactValue = 0;

        Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

        bool isSelectedTerritory = false;
        bool isInContact = false;

        Polity selectedPolity = null;
        Polity polity = territory.Polity;

        if (selectedTerritory != null)
        {
            selectedPolity = selectedTerritory.Polity;

            if (selectedTerritory != territory)
            {
                int contactGroupCount = selectedPolity.GetContactGroupCount(polity);

                contactValue = MathUtility.ToPseudoLogaritmicScale01(contactGroupCount);

                isInContact = (contactValue > 0);
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

    private static Color SetPopulationChangeOverlayColor(TerrainCell cell, Color color)
    {
        float deltaLimitFactor = 0.1f;

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

            color = Color.green * value;
        }
        else if (delta < 0)
        {
            float value = -delta / (prevPopulation * deltaLimitFactor);
            value = Mathf.Clamp01(value);

            color = Color.red * value;
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
        if (_planetOverlaySubtype == "None")
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
        if (cell.Group == null)
            return color;

        if (_planetOverlaySubtype == "None")
            return GetUnincorporatedGroupColor();

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
            return GetUnincorporatedGroupColor();

        CulturalPreference preference = territory.Polity.Culture.GetPreference(_planetOverlaySubtype);

        if (preference == null)
            return GetUnincorporatedGroupColor();

        color = GetPolityCulturalAttributeOverlayColor(preference.Value, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color SetPopCulturalActivityOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == "None")
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
        if (cell.Group == null)
            return color;

        if (_planetOverlaySubtype == "None")
            return GetUnincorporatedGroupColor();

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
            return GetUnincorporatedGroupColor();

        CulturalActivity activity = territory.Polity.Culture.GetActivity(_planetOverlaySubtype);

        if (activity == null)
            return GetUnincorporatedGroupColor();

        color = GetPolityCulturalAttributeOverlayColor(activity.Contribution, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color SetPopCulturalSkillOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == "None")
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
        if (cell.Group == null)
            return color;

        if (_planetOverlaySubtype == "None")
            return GetUnincorporatedGroupColor();

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
            return GetUnincorporatedGroupColor();

        CulturalSkill skill = territory.Polity.Culture.GetSkill(_planetOverlaySubtype);

        if (skill == null)
            return GetUnincorporatedGroupColor();

        color = GetPolityCulturalAttributeOverlayColor(skill.Value, IsTerritoryBorder(territory, cell));

        return color;
    }

    private static Color SetPopCulturalKnowledgeOverlayColor(TerrainCell cell, Color color)
    {
        if (_planetOverlaySubtype == "None")
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
        if (cell.Group == null)
            return color;

        if (_planetOverlaySubtype == "None")
            return GetUnincorporatedGroupColor();

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
            return GetUnincorporatedGroupColor();

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
        if (_planetOverlaySubtype == "None")
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
        if (cell.Group == null)
            return color;

        if (_planetOverlaySubtype == "None")
            return GetUnincorporatedGroupColor();

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
            return GetUnincorporatedGroupColor();

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
        if (_planetOverlaySubtype == "None")
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

        if (_planetOverlaySubtype == "None")
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
        Color groupColor = GetOverlayColor(OverlayColorId.GeneralDensityOptimal);

        if (cell.EncompassingTerritory != null)
        {
            inTerritory = true;

            Polity territoryPolity = cell.EncompassingTerritory.Polity;

            groupColor = GenerateColorFromId(territoryPolity.Id);

            bool isTerritoryBorder = IsTerritoryBorder(cell.EncompassingTerritory, cell);
            bool isCoreGroup = territoryPolity.CoreGroup == cell.Group;

            if (!isCoreGroup)
            {
                if (!isTerritoryBorder)
                {
                    groupColor /= 2.5f;
                }
                else
                {
                    groupColor /= 1.75f;
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

                    groupColor = (groupColor * knowledgeFactor) + densityColorSubOptimal * (1f - knowledgeFactor);
                }
                else
                {
                    groupColor = densityColorSubOptimal;
                }
            }
        }

        if (hasGroup && !inTerritory)
        {
            baseColor = groupColor;
            baseColor.a = 0.5f;
        }
        else if (inTerritory)
        {
            baseColor = groupColor;
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

            if (_manager._currentMaxUpdateSpan < updateSpan)
                _manager._currentMaxUpdateSpan = updateSpan;

            float maxUpdateSpan = CellGroup.MaxUpdateSpan;

            maxUpdateSpan = Mathf.Min(_manager._currentMaxUpdateSpan, maxUpdateSpan);

            normalizedValue = 1f - updateSpan / maxUpdateSpan;

            normalizedValue = Mathf.Clamp01(normalizedValue);
        }

        if ((population > 0) && (normalizedValue > 0))
        {
            color = Color.red * normalizedValue;
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

        Layer.ResetLayers();
        Biome.ResetBiomes();

        Adjective.ResetAdjectives();
        RegionAttribute.ResetAttributes();
        Element.ResetElements();

        Discovery.ResetDiscoveries();
        Knowledge.ResetKnowledges();

        EventGenerator.ResetGenerators();
        ModDecision.ResetDecisions();

        // TODO: This should happend after mods are loaded. And preferences
        // should be loaded from mods...
        CulturalPreference.InitializePreferences();

        float progressPerMod = 0.1f / paths.Count;

        foreach (string path in paths)
        {
            if (_manager._progressCastMethod != null)
            {
                string directoryName = Path.GetFileName(path);

                _manager._progressCastMethod(LastStageProgress, "Loading Mod '" + directoryName + "'...");
            }

            LoadMod(path, progressPerMod);

            LastStageProgress += progressPerMod;
        }

        Knowledge.InitializeKnowledges();
        Discovery.InitializeDiscoveries();

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

                _manager._progressCastMethod?.Invoke(accProgress);
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

        float progressPerSegment = progressPerMod / 8f;

        TryLoadModFiles(Layer.LoadLayersFile, Path.Combine(path, @"Layers"), progressPerSegment);
        TryLoadModFiles(Biome.LoadBiomesFile, Path.Combine(path, @"Biomes"), progressPerSegment);
        TryLoadModFiles(Adjective.LoadAdjectivesFile, Path.Combine(path, @"Adjectives"), progressPerSegment);
        TryLoadModFiles(RegionAttribute.LoadRegionAttributesFile, Path.Combine(path, @"RegionAttributes"), progressPerSegment);
        TryLoadModFiles(Element.LoadElementsFile, Path.Combine(path, @"Elements"), progressPerSegment);
        TryLoadModFiles(Discovery.LoadDiscoveriesFile033, Path.Combine(path, @"Discoveries"), progressPerSegment);
        TryLoadModFiles(EventGenerator.LoadEventFile, Path.Combine(path, @"Events"), progressPerSegment);
        TryLoadModFiles(ModDecision.LoadDecisionFile, Path.Combine(path, @"Decisions"), progressPerSegment);
    }

    private static void LoadMod033(string path, float progressPerMod)
    {
        if (!Directory.Exists(path))
        {
            throw new System.ArgumentException("Mod path '" + path + "' not found");
        }

        float progressPerSegment = progressPerMod / 6f;

        TryLoadModFiles(Layer.LoadLayersFile, Path.Combine(path, @"Layers"), progressPerSegment);
        TryLoadModFiles(Biome.LoadBiomesFile, Path.Combine(path, @"Biomes"), progressPerSegment);
        TryLoadModFiles(Adjective.LoadAdjectivesFile, Path.Combine(path, @"Adjectives"), progressPerSegment);
        TryLoadModFiles(RegionAttribute.LoadRegionAttributesFile, Path.Combine(path, @"RegionAttributes"), progressPerSegment);
        TryLoadModFiles(Element.LoadElementsFile, Path.Combine(path, @"Elements"), progressPerSegment);
        TryLoadModFiles(Discovery.LoadDiscoveriesFile033, Path.Combine(path, @"Discoveries"), progressPerSegment);
    }
}
