using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

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
        public long GroupId;
        public long PolityId;
        public long FactionId;
        public long ClusterId;
        public long RegionId;
        public int Longitude;
        public int Latitude;
        public int Priority;
        public long LastSaveDate;
    }

    public static Debug_TracingData TracingData = new Debug_TracingData();

    public static bool TrackGenRandomCallers = false;
#endif

    //	public static bool RecordingEnabled = false;

    //	public static IRecorder Recorder = DefaultRecorder.Default;

    public const string NoOverlaySubtype = "None";

    public const int WorldWidth = 400;
    public const int WorldHeight = 200;

    public const float BrushStrengthFactor_Base = 0.04f;
    public const float BrushStrengthFactor_Altitude = 0.5f;
    public const float BrushStrengthFactor_Rainfall = 0.25f;
    public const float BrushStrengthFactor_Temperature = 0.25f;
    public const float BrushStrengthFactor_Layer = 0.25f;

    public const float BrushNoiseRadiusFactor = 200;

    public const string DefaultModPath = @".\Mods\";

    public const float StageProgressIncFromLoading = 0.1f;

    public const int MaxEditorBrushRadius = 25;
    public const int MinEditorBrushRadius = 1;

    public static bool LayersPresent = false;

    public static float LastStageProgress = 0;

    public static Thread MainThread { get; private set; }

    public static string SavePath { get; private set; }
    public static string HeightmapsPath { get; private set; }
    public static string ExportPath { get; private set; }

    public static string[] SupportedHeightmapFormats = new string[] { ".PSD", ".TIFF", ".JPG", ".TGA", ".PNG", ".BMP", ".PICT" };

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

    public static List<string> ActiveModPaths = new List<string>() { @"Mods\Base" };
    public static bool ModsAlreadyLoaded = false;

    public static Dictionary<string, LayerSettings> LayerSettings = new Dictionary<string, LayerSettings>();

    public static GameMode GameMode = GameMode.None;

    public static bool ViewingGlobe = false;

    public static int LastMapUpdateCount = 0;
    public static int LastPixelUpdateCount = 0;
    public static long LastDateSpan = 0;

    public static bool DisableShortcuts = false;

    private static bool _isLoadReady = false;

    private static string _debugLogFilename = @".\debug";
    private static string _debugLogExt = @".log";
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

    public static bool PerformingAsyncTask => _manager._performingAsyncTask;

    public static bool SimulationRunning => _manager._simulationRunning;

    public static bool WorldIsReady => _manager._worldReady;

    public static bool SimulationCanRun => _manager._currentWorld.CellGroupCount > 0;

    public static PlanetOverlay PlanetOverlay => _planetOverlay;

    public static string PlanetOverlaySubtype => _planetOverlaySubtype;

    public static bool DisplayRoutes => _displayRoutes;

    public static bool DisplayGroupActivity => _displayGroupActivity;

    public static int UndoableEditorActionsCount => _undoableEditorActions.Count;

    public static int RedoableEditorActionsCount => _redoableEditorActions.Count;

    public static void UpdateMainThreadReference()
    {
        MainThread = Thread.CurrentThread;
    }

    /// <summary>Initializes the game manager.</summary>
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

        // static initializations

        Tribe.GenerateTribeNounVariations();
    }

    /// <summary>
    ///   Determines if the required control and shift keys were used, and that the user is not interacting with an input field.
    /// </summary>
    /// <param name="requireCtrl">If set to <c>true</c>, requires either control key.</param>
    /// <param name="requireShift">If set to <c>true</c>, requires either shift key.</param>
    /// <returns>
    ///   <c>true</c> if the required control and shift keys were used; otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>Called on the frame when the key identified by <c>keyCode</c> has been released.</summary>
    /// <param name="keyCode">The key code.</param>
    /// <param name="requireCtrl">If set to <c>true</c>, requires either control key.</param>
    /// <param name="requireShift">If set to <c>true</c>, requires either shift key.</param>
    /// <param name="action">Function to be called if key input is valid.</param>
    public static void HandleKeyUp(KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action)
    {
        if (!Input.GetKeyUp(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift))
            return;

        action.Invoke();
    }

    /// <summary>Called when the key identified by <c>keyCode</c> is being held down.</summary>
    /// <param name="keyCode">The key code.</param>
    /// <param name="requireCtrl">If set to <c>true</c>, requires either control key.</param>
    /// <param name="requireShift">If set to <c>true</c>, requires either shift key.</param>
    /// <param name="action">Function to be called if key input is valid.</param>
    public static void HandleKey(KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action)
    {
        if (!Input.GetKey(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift))
            return;

        action.Invoke();
    }

    /// <summary>Called on the frame when a key identified by <c>keyCode</c> is initially pressed down.</summary>
    /// <param name="keyCode">The key code.</param>
    /// <param name="requireCtrl">If set to <c>true</c>, requires either control key.</param>
    /// <param name="requireShift">If set to <c>true</c>, requires either shift key.</param>
    /// <param name="action">Function to be called if key input is valid.</param>
    public static void HandleKeyDown(KeyCode keyCode, bool requireCtrl, bool requireShift, System.Action action)
    {
        if (!Input.GetKeyDown(keyCode))
            return;

        if (!CanHandleKeyInput(requireCtrl, requireShift))
            return;

        action.Invoke();
    }

    /// <summary>Clears the layer settings.</summary>
    public static void ResetLayerSettings()
    {
        LayerSettings.Clear();
    }

    /// <summary>Sets the layer settings after reseting the old layer settings.</summary>
    /// <param name="layerSettings">The list of layer settings to be set.</param>
    public static void SetLayerSettings(List<LayerSettings> layerSettings)
    {
        ResetLayerSettings();
        
        foreach (LayerSettings settings in layerSettings)
        {
            LayerSettings.Add(settings.Id, settings);
        }
    }

    /// <summary>Gets the associated layer settings for an id matching to <c>layerID</c>.</summary>
    /// <param name="layerId">The layer identifier.</param>
    /// <returns>
    ///   The layer settings associated with <c>layerId</c>.
    /// </returns>
    public static LayerSettings GetLayerSettings(string layerId)
    {
        if (!LayerSettings.TryGetValue(layerId, out LayerSettings settings))
        {
            settings = new LayerSettings(Layer.Layers[layerId]);
            LayerSettings.Add(layerId, settings);
        }

        return settings;
    }

    /// <summary>Blocks the ability to undo and redo actions on the editor.</summary>
    /// <param name="state">If set to <c>true</c>, blocking is active.</param>
    public static void BlockUndoAndRedo(bool state)
    {
        _undoAndRedoBlocked = state;
    }

    /// <summary>Registers the listener for events related to the undoable action stack.</summary>
    /// <param name="op">The listener to be registered.</param>
    public static void RegisterUndoStackUpdateOp(System.Action op)
    {
        _onUndoStackUpdate += op;
    }

    /// <summary>Deregisters the listener for events related to the undoable action stack.</summary>
    /// <param name="op">The listener to be deregistered.</param>
    public static void DeregisterUndoStackUpdateOp(System.Action op)
    {
        _onUndoStackUpdate -= op;
    }

    /// <summary>Registers the listener for events related to the redoable action stack.</summary>
    /// <param name="op">The listener to be registered.</param>
    public static void RegisterRedoStackUpdateOp(System.Action op)
    {
        _onRedoStackUpdate += op;
    }

    /// <summary>Deregisters the listener for events related to the redoable action stack.</summary>
    /// <param name="op">The listener to be deregistered.</param>
    public static void DeregisterRedoStackUpdateOp(System.Action op)
    {
        _onRedoStackUpdate -= op;
    }

    /// <summary>Undoes the last action in the editor.</summary>
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

    /// <summary>Redoes the last action in the editor.</summary>
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

    /// <summary>Performs the action identified by <c>editorAction</c> in the editor.</summary>
    /// <param name="editorAction">The action to be done in the editor.</param>
    public static void PerformEditorAction(EditorAction editorAction)
    {
        editorAction.Do();

        PushUndoableAction(editorAction);
        ResetRedoableActionsStack();
    }

    /// <summary>Resets the undoable and redoable action stacks.</summary>
    public static void ResetActionStacks()
    {
        ResetUndoableActionsStack();
        ResetRedoableActionsStack();
    }

    /// <summary>Pushes the undoable action identified by <c>action</c> onto the undoable stack.</summary>
    /// <param name="action">The action to be pushed.</param>
    public static void PushUndoableAction(EditorAction action)
    {
        _undoableEditorActions.Push(action);

        _onUndoStackUpdate?.Invoke();
    }

    /// <summary>Pushes the redoable action identified by <c>action</c> onto the redoable stack.</summary>
    /// <param name="action">The action to be pushed.</param>
    public static void PushRedoableAction(EditorAction action)
    {
        _redoableEditorActions.Push(action);

        _onRedoStackUpdate?.Invoke();
    }

    /// <summary>Pops the last action that can be undone.</summary>
    /// <returns>
    ///   Action last performed on the editor.
    /// </returns>
    public static EditorAction PopUndoableAction()
    {
        EditorAction action = _undoableEditorActions.Pop();

        _onUndoStackUpdate?.Invoke();

        return action;
    }

    /// <summary>Pops the last action that can be redone.</summary>
    /// <returns>
    ///   Action last undone on the editor.
    /// </returns>
    public static EditorAction PopRedoableAction()
    {
        EditorAction action = _redoableEditorActions.Pop();

        _onRedoStackUpdate?.Invoke();

        return action;
    }

    /// <summary>Resets the undoable action stack.</summary>
    public static void ResetUndoableActionsStack()
    {
        _undoableEditorActions.Clear();

        _onUndoStackUpdate?.Invoke();
    }

    /// <summary>Resets the redoable action stack.</summary>
    public static void ResetRedoableActionsStack()
    {
        _redoableEditorActions.Clear();

        _onRedoStackUpdate?.Invoke();
    }

    /// <summary>Initializes the debug log.</summary>
    /// <remarks>Will overwrite an old log with the same name.</remarks>
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

        string buildType = Debug.isDebugBuild ? "debug" : "release";

        _debugLogStream.WriteLine("Running Worlds " + Application.version + " (" + buildType + ")...");
        _debugLogStream.Flush();
    }

    /// <summary>Closes the debug log.</summary>
    /// <remarks>Will create a backup of the log if an exception occurs.</remarks>
    public static void CloseDebugLog()
    {
        if (_debugLogStream == null)
            return;

        _debugLogStream.Close();

        _debugLogStream = null;

        if (!_backupDebugLog)
            return;

        string logFilename = _debugLogFilename + _debugLogExt;

        string backupFilename = _debugLogFilename + System.DateTime.Now.ToString("_dd_MM_yyyy_hh_mm_ss") + _debugLogExt;
        
        File.Copy(logFilename, backupFilename);
    }

    /// <summary>Handler used for logging, tracing and debugging.</summary>
    /// <param name="logString">The string to be logged.</param>
    /// <param name="stackTrace">The stack trace.</param>
    /// <param name="type">The type of log message.</param>
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

    /// <summary>Flags the log to be backed up.</summary>
    public static void EnableLogBackup()
    {
        _backupDebugLog = true;
    }

    /// <summary>Initializes the save path.</summary>
    /// <remarks>Will create the directory if it does not already exist.</remarks>
    private static void InitializeSavePath()
    {
        string path = Path.GetFullPath(@"Saves\");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        SavePath = path;
    }

    /// <summary>Initializes the heightmaps path.</summary>
    /// <remarks>Will create the directory if it does not already exist.</remarks>
    private static void InitializeHeightmapsPath()
    {
        string path = Path.GetFullPath(@"Heightmaps\");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        HeightmapsPath = path;
    }

    /// <summary>Initializes the export image path.</summary>
    /// <remarks>Will create the directory if it does not already exist.</remarks>
    private static void InitializeExportPath()
    {
        string path = Path.GetFullPath(@"Images\");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        ExportPath = path;
    }

    /// <summary>Generates a string representation of the date based on its numeric form.</summary>
    /// <param name="date">The date in numeric form.</param>
    /// <returns>
    ///   A string representation of the date.
    /// </returns>
    public static string GetDateString(long date)
    {
        long year = date / World.YearLength;
        int day = (int)(date % World.YearLength);

        return string.Format("Year {0}, Day {1}", year, day);
    }

    /// <summary>Generates a string representation of the timespan based on its numeric form.</summary>
    /// <param name="timespan">The timespan in numeric form.</param>
    /// <returns>
    ///   A string representation of the timespan.
    /// </returns>
    public static string GetTimeSpanString(long timespan)
    {
        long years = timespan / World.YearLength;
        int days = (int)(timespan % World.YearLength);

        return string.Format("{0} years, {1} days", years, days);
    }

    /// <summary>Adds the current date to the world name.</summary>
    /// <param name="worldName">Name of the world.</param>
    /// <returns>
    ///   The world name appended by the current date.
    /// </returns>
    public static string AddDateToWorldName (string worldName)
    {
        long year = CurrentWorld.CurrentDate / World.YearLength;
        int day = (int)(CurrentWorld.CurrentDate % World.YearLength);

        return worldName + "_date_" + string.Format("{0}_{1}", year, day);
    }

    /// <summary>Removes the date, if present, from the world name.</summary>
    /// <param name="worldName">Name of the world with possible appended date.</param>
    /// <returns>
    ///   The world name.
    /// </returns>
    public static string RemoveDateFromWorldName(string worldName)
    {
        int dateIndex = worldName.LastIndexOf("_date_");

        if (dateIndex > 0)
        {
            return worldName.Substring(0, dateIndex);
        }

        return worldName;
    }

    /// <summary>Sets the game to fullscreen or windowed mode based on <c>state</c>.</summary>
    /// <param name="state">If set to <c>true</c>, makes the game go fullscreen.</param>
    public static void SetFullscreen(bool state)
    {
        FullScreenEnabled = state;

        if (state)
        {
            Resolution currentResolution = Screen.currentResolution;

            Screen.SetResolution(currentResolution.width, currentResolution.height, true);
        }
        else
        {
            Screen.SetResolution(_resolutionWidthWindowed, _resolutionHeightWindowed, false);
        }
    }

    /// <summary>Enables or disables UI scaling based on <c>state</c>.</summary>
    /// <param name="state">If set to <c>true</c>, UI scaling is activated.</param>
    public static void SetUIScaling(bool state)
    {
        UIScalingEnabled = state;
    }

    /// <summary>Initializes the screen altering options like fullscreen and UI scaling.</summary>
    public static void InitializeScreen()
    {
        if (_resolutionInitialized)
            return;

        SetFullscreen(FullScreenEnabled);
        SetUIScaling(UIScalingEnabled);

        _resolutionInitialized = true;
    }

    /// <summary>Interrupts or resumes the simulation based on <c>state</c>.</summary>
    /// <param name="state">If set to <c>true</c>, stops the simulation.</param>
    public static void InterruptSimulation(bool state)
    {
        _manager._simulationRunning = !state;
    }

    /// <summary>Executes a number of tasks determined by <c>count</c>.</summary>
    /// <param name="count">The number of tasks to execute.</param>
    public static void ExecuteTasks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!ExecuteNextTask()) break;
        }
    }

    /// <summary>Executes the next task in the manager's task queue.</summary>
    /// <returns>
    ///   <c>true</c> if there is another task to execute; otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>Enqueues the generic type task identified by <c>taskDelegate</c>.</summary>
    /// <remarks>If the function is executed on the main thread, the <c>task</c> will be immediately executed.</remarks>
    /// <param name="taskDelegate">The generic type task to be enqueued.</param>
    /// <returns>
    ///   The task that was just enqueued.
    /// </returns>
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

    /// <summary>Enqueues the generic type task identified by <c>taskDelegate</c> and waits for the result.</summary>
    /// <param name="taskDelegate">The generic type task to be enqueued.</param>
    /// <returns>
    ///   The generic type result of the enqueued task.
    /// </returns>
    public static T EnqueueTaskAndWait<T>(ManagerTaskDelegate<T> taskDelegate)
    {
        return EnqueueTask(taskDelegate).Result;
    }

    /// <summary>Enqueues the task identified by <c>taskDelegate</c>.</summary>
    /// <remarks>If the function is executed on the main thread, the <c>task</c> will be immediately executed.</remarks>
    /// <param name="taskDelegate">The task to be enqueued.</param>
    /// <returns>
    ///   The task that was just enqueued.
    /// </returns>
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

    /// <summary>Enqueues the task identified by <c>taskDelegate</c> and waits.</summary>
    /// <param name="taskDelegate">The task to be enqueued.</param>
    public static void EnqueueTaskAndWait(ManagerTaskDelegate taskDelegate)
    {
        EnqueueTask(taskDelegate).Wait();
    }

    /// <summary>Sets the range of colors for the biome palette.</summary>
    /// <param name="colors">The list of colors to be set.</param>
    public static void SetBiomePalette(IEnumerable<Color> colors)
    {
        _biomePalette.Clear();
        _biomePalette.AddRange(colors);
    }

    /// <summary>Sets the range of colors for the map palette.</summary>
    /// <param name="colors">The list of colors to be set.</param>
    public static void SetMapPalette(IEnumerable<Color> colors)
    {
        _mapPalette.Clear();
        _mapPalette.AddRange(colors);
    }

    /// <summary>Sets the range of colors for the overlay palette.</summary>
    /// <param name="colors">The list of colors to be set.</param>
    public static void SetOverlayPalette(IEnumerable<Color> colors)
    {
        _overlayPalette.Clear();
        _overlayPalette.AddRange(colors);
    }

    public static World CurrentWorld => _manager._currentWorld;

    public static Texture2D CurrentMapTexture => _manager._currentMapTexture;

    public static Texture2D CurrentMapOverlayTexture => _manager._currentMapOverlayTexture;

    public static Texture2D CurrentMapActivityTexture => _manager._currentMapActivityTexture;

    public static Texture2D CurrentMapOverlayShaderInfoTexture => _manager._currentMapOverlayShaderInfoTexture;

    public static Texture2D PointerOverlayTexture => _manager._pointerOverlayTexture;

    /// <summary>Converts a set of coordinates from the world map into a texture's UV coordinates.</summary>
    /// <param name="mapPosition">The set of coordinates from the world map.</param>
    /// <returns>
    ///   UV Coodinates for a texture.
    /// </returns>
    public static Vector2 GetUVFromMapCoordinates(WorldPosition mapPosition)
    {
        return new Vector2(mapPosition.Longitude / (float)CurrentWorld.Width, mapPosition.Latitude / (float)CurrentWorld.Height);
    }

    /// <summary>Exports the map texture to a file.</summary>
    /// <param name="path">The export image path.</param>
    /// <param name="uvRect">The map image texture coordinates.</param>
    public static void ExportMapTextureToFile(string path, Rect uvRect)
    {
        Texture2D mapTexture = _manager._currentMapTexture;
        Texture2D exportTexture = null;

        EnqueueTaskAndWait(() =>
        {
            int width = mapTexture.width;
            int height = mapTexture.height;

            int xOffset = (int) Mathf.Floor(uvRect.x * width);

            exportTexture = new Texture2D(
                width,
                height,
                mapTexture.format,
                false);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int finalX = (i + xOffset) % width; // What is this used for?

                    exportTexture.SetPixel(i, j, mapTexture.GetPixel(finalX, j));
                }
            }

            return true;
        });

        ManagerTask<byte[]> bytes = EnqueueTask(() => exportTexture.EncodeToPNG());

        File.WriteAllBytes(path, bytes);

        EnqueueTaskAndWait(() =>
        {
            Object.Destroy(exportTexture);
            return true;
        });
    }

    /// <summary>Exports the map texture to file asynchronously.</summary>
    /// <param name="path">The export image path.</param>
    /// <param name="uvRect">The map image texture coordinates.</param>
    /// <param name="progressCastMethod">The progress cast method.</param>
    public static void ExportMapTextureToFileAsync(string path, Rect uvRect, ProgressCastDelegate progressCastMethod = null)
    {
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;
        
        _manager._progressCastMethod = progressCastMethod ?? ((value, message, reset) => { });

        Debug.Log("Trying to export world map to .png file: " + Path.GetFileName(path));

        ThreadPool.QueueUserWorkItem(state =>
        {
            ExportMapTextureToFile(path, uvRect);

            _manager._performingAsyncTask = false;
            _manager._simulationRunning = true;
        });
    }

    /// <summary>Generates the pointer overlay textures.</summary>
    public static void GeneratePointerOverlayTextures()
    {
        GeneratePointerOverlayTextureFromWorld(CurrentWorld);
    }

    /// <summary>Generates the map textures.</summary>
    /// <param name="doMapTexture">If set to <c>true</c>, the map texture is generated from the current world.</param>
    /// <param name="doOverlayMapTexture">If set to <c>true</c>, all the map overlay textures are generated from the current world.</param>
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

            if (AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
            {
                GenerateMapOverlayShaderInfoTextureFromWorld(CurrentWorld);
            }
        }

        ResetUpdatedAndHighlightedCells();
    }

    /// <summary>Validates the cell update type and subtype combination.</summary>
    /// <param name="updateType">Type of cell update.</param>
    /// <param name="updateSubType">Subtype of cell update.</param>
    /// <returns>
    ///   <c>true</c> if the cell update type and subtype combination is valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool ValidUpdateTypeAndSubtype(CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (_displayRoutes && ((updateType & CellUpdateType.Route) != CellUpdateType.None))
            return true;

        if ((_observableUpdateTypes & updateType) == CellUpdateType.None)
            return false;

        if ((_observableUpdateSubTypes & updateSubType) == CellUpdateSubType.None)
            return false;

        if (_planetOverlay != PlanetOverlay.General)
            return true;

        if (((updateType & CellUpdateType.Territory) != CellUpdateType.None) &&
            ((updateSubType & CellUpdateSubType.MembershipAndCore) != CellUpdateSubType.None))
        {
            return true;
        }

        if (((updateType & CellUpdateType.Group) != CellUpdateType.None) &&
            ((updateSubType & CellUpdateSubType.Culture) != CellUpdateSubType.None))
        {
            return true;
        }

        return false;

    }

    /// <summary>Adds the cell to updated cells without checking its update type and subtype.</summary>
    /// <param name="cell">The cell to be added to updated cells.</param>
    /// <remarks>Only use this function if ValidUpdateTypeAndSubtype has already been called.</remarks>
    public static void AddUpdatedCell(TerrainCell cell)
    {
        UpdatedCells.Add(cell);
    }

    /// <summary>Adds the cell to updated cells after checking its update type and subtype.</summary>
    /// <param name="cell">The cell to be added to updated cells.</param>
    /// <param name="updateType">Type of cell update.</param>
    /// <param name="updateSubType">Subtype of cell update.</param>
    /// <remarks>
    ///   If <c>updateSubType</c> is <c>CellUpdateSubType.Terrain</c>, then the <c>cell</c> will also be added to updated terrain cells.
    /// </remarks>
    public static void AddUpdatedCell(TerrainCell cell, CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        UpdatedCells.Add(cell);

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.Add(cell);
        }
    }

    /// <summary>Adds the cells within a polity's territory to updated cells after checking its update type and subtype.</summary>
    /// <param name="polity">The polity whose territory's cells are to be added to updated cells.</param>
    /// <param name="updateType">Type of cell update.</param>
    /// <param name="updateSubType">Subtype of cell update.</param>
    /// <remarks>
    ///   If <c>updateSubType</c> is <c>CellUpdateSubType.Terrain</c>, then the <c>cell</c> will also be added to updated terrain cells.
    /// </remarks>
    public static void AddUpdatedCells(Polity polity, CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        UpdatedCells.UnionWith(polity.Territory.GetCells());

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.UnionWith(polity.Territory.GetCells());
        }
    }

    /// <summary>Adds the cells within a territory to updated cells after checking its update type and subtype.</summary>
    /// <param name="cells">The territory whose cells are to be added to updated cells.</param>
    /// <param name="updateType">Type of cell update.</param>
    /// <param name="updateSubType">Subtype of cell update.</param>
    /// <remarks>
    ///   If <c>updateSubType</c> is <c>CellUpdateSubType.Terrain</c>, then the <c>cell</c> will also be added to updated terrain cells.
    /// </remarks>
    public static void AddUpdatedCells(ICollection<TerrainCell> cells, CellUpdateType updateType, CellUpdateSubType updateSubType)
    {
        if (!ValidUpdateTypeAndSubtype(updateType, updateSubType))
            return;

        UpdatedCells.UnionWith(cells);

        if ((updateSubType & CellUpdateSubType.Terrain) == CellUpdateSubType.Terrain)
        {
            TerrainUpdatedCells.UnionWith(cells);
        }
    }

    /// <summary>Adds the cell to highlighed cells.</summary>
    /// <param name="cell">The cell to be added to highlighed cells.</param>
    /// <param name="updateType">Type of cell update.</param>
    public static void AddHighlightedCell(TerrainCell cell, CellUpdateType updateType)
    {
        HighlightedCells.Add(cell);
    }

    /// <summary>Adds the cells within a territory to highlighed cells.</summary>
    /// <param name="cells">The territory whose cells are to be added to highlighed cells.</param>
    /// <param name="updateType">Type of cell update.</param>
    public static void AddHighlightedCells(ICollection<TerrainCell> cells, CellUpdateType updateType)
    {
        foreach (TerrainCell cell in cells)
            HighlightedCells.Add(cell);
    }

    /// <summary>Generates one random human group with an initial population.</summary>
    /// <param name="initialPopulation">The initial population.</param>
    public static void GenerateRandomHumanGroup(int initialPopulation)
    {
        World world = _manager._currentWorld;

        world.ProgressCastMethod = _manager._progressCastMethod ?? ((value, message, reset) => { });

        world.GenerateRandomHumanGroups(1, initialPopulation);
    }

    /// <summary>Generates a human group with an initial population at a specific set of coordinates.</summary>
    /// <param name="longitude">The longitude.</param>
    /// <param name="latitude">The latitude.</param>
    /// <param name="initialPopulation">The initial population.</param>
    public static void GenerateHumanGroup(int longitude, int latitude, int initialPopulation)
    {
        World world = _manager._currentWorld;

        world.ProgressCastMethod = _manager._progressCastMethod ?? ((value, message, reset) => { });

        world.GenerateHumanGroup(longitude, latitude, initialPopulation);
    }

    /// <summary>Sets the current active mod paths.</summary>
    /// <param name="paths">The collection containing all the activated mod paths.</param>
    /// <remarks>Will clear the old list of active mod paths.</remarks>
    public static void SetActiveModPaths(ICollection<string> paths)
    {
        ActiveModPaths.Clear();
        ActiveModPaths.AddRange(paths);
        ActiveModPaths.Sort();

        ModsAlreadyLoaded = false;
    }

    /// <summary>Generates a new world.</summary>
    /// <param name="seed">The seed used to set the random number generator state.</param>
    /// <param name="heightmap">The heightmap to generate the world from if present.</param>
    public static void GenerateNewWorld(int seed, Texture2D heightmap)
    {
        _manager._worldReady = false;

        LastStageProgress = 0;

        ProgressCastDelegate progressCastMethod = _manager._progressCastMethod ?? ((value, message, reset) => { });

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

    /// <summary>Generates a new world asynchronously.</summary>
    /// <param name="seed">The seed used to set the random number generator state.</param>
    /// <param name="heightmap">The heightmap to generate the world from if present.</param>
    /// <param name="progressCastMethod">The progress cast method.</param>
    public static void GenerateNewWorldAsync(int seed, Texture2D heightmap = null, ProgressCastDelegate progressCastMethod = null)
    {
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod ?? ((value, message, reset) => { });

        Debug.Log(string.Format("Trying to generate world with seed: {0}, Altitude Scale: {1}, Sea Level Offset: {2}, River Strength: {3}, Avg. Temperature: {4}, Avg. Rainfall: {5}",
            seed, AltitudeScale, SeaLevelOffset, RiverStrength, TemperatureOffset, RainfallOffset));

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

    /// <summary>Regenerates one type of the generation in the world.</summary>
    /// <param name="type">The type to be regenerated; i.e., terrain, temperature, rain, etc.</param>
    public static void RegenerateWorld(GenerationType type)
    {
        _manager._worldReady = false;

        World world = _manager._currentWorld;

        world.ProgressCastMethod = _manager._progressCastMethod ?? ((value, message, reset) => { });

        world.StartReinitialization(0f, 1.0f);
        world.Regenerate(type);
        world.FinishInitialization();
        
        _manager._currentCellSlants = new float?[world.Width, world.Height];
        _manager._currentMaxUpdateSpan = 0;

        _manager._worldReady = true;

        ForceWorldCleanup();
    }

    /// <summary>Regenerates one type of the generation in the world asynchronous.</summary>
    /// <param name="type">The type to be regenerated; i.e., terrain, temperature, rain, etc.</param>
    /// <param name="progressCastMethod">The progress cast method.</param>
    public static void RegenerateWorldAsync(GenerationType type, ProgressCastDelegate progressCastMethod = null)
    {
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod ?? ((value, message, reset) => { });

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

    /// <summary>Saves the game's settings to a file specified by <c>path</c>.</summary>
    /// <param name="path">The path to be saved to.</param>
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

    /// <summary>Loads the game's settings from a file specified by <c>path</c>.</summary>
    /// <param name="path">The path to be loaded from.</param>
    public static void LoadAppSettings(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));

        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            if (serializer.Deserialize(stream) is AppSettings settings)
                settings.Take();
        }
    }


    /// <summary>Saves the world to a file specified by <c>path</c>.</summary>
    /// <param name="path">The path to be saved to.</param>
    public static void SaveWorld(string path)
    {
        _manager._currentWorld.Synchronize();

        XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, _manager._currentWorld);
        }
    }

    /// <summary>Saves the world to a file specified by <c>path</c> asynchronously.</summary>
    /// <param name="path">The path to be saved to.</param>
    /// <param name="progressCastMethod">The progress cast method.</param>
    public static void SaveWorldAsync(string path, ProgressCastDelegate progressCastMethod = null)
    {
        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod ?? ((value, message, reset) => { });

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

    /// <summary>Forces the world to be cleaned up.</summary>
    /// <para>
    ///   NOTE: Make sure there are no outside references to the world object stored in _manager._currentWorld, otherwise it is pointless to call this...
    ///   WARNING: Don't abuse this function call.
    /// </para>
    private static void ForceWorldCleanup()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    /// <summary>Tries to load active mods.</summary>
    private static void TryLoadActiveMods()
    {
        if (ModsAlreadyLoaded)
            return;

        LoadMods(ActiveModPaths);
        ModsAlreadyLoaded = true;
    }

    /// <summary>Loads the world from a file specified by <c>path</c>.</summary>
    /// <param name="path">The path to be loaded from.</param>
    public static void LoadWorld(string path)
    {
        _manager._worldReady = false;

        LastStageProgress = 0;

        ProgressCastDelegate progressCastMethod = _manager._progressCastMethod ?? ((value, message, reset) => { });

        ResetWorldLoadTrack();
        
        World world;

        XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);
        
        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            world = serializer.Deserialize(stream) as World;
        }

        world.ProgressCastMethod = _manager._progressCastMethod ?? ((value, message, reset) => { });

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

    /// <summary>Loads the world from a file specified by <c>path</c> asynchronously.</summary>
    /// <param name="path">The path to be loaded from.</param>
    /// <param name="progressCastMethod">The progress cast method.</param>
    public static void LoadWorldAsync(string path, ProgressCastDelegate progressCastMethod = null)
    {
#if DEBUG
        Debug_IsLoadedWorld = true;
#endif

        _manager._simulationRunning = false;
        _manager._performingAsyncTask = true;

        _manager._progressCastMethod = progressCastMethod ?? ((value, message, reset) => { });

        Debug.Log("Trying to load world from file: " + Path.GetFileName(path));

        ThreadPool.QueueUserWorkItem(state =>
        {
            try
            {
                LoadWorld(path);
            }
            //catch (IOException e)
            //{
            //    // DEBUG: This is a workaround. We shouldn't ignore file sharing violations
            //    if (System.Runtime.InteropServices.Marshal.GetHRForException(e) != 0x00000020)
            //    {
            //        EnqueueTaskAndWait(() =>
            //        {
            //            throw new System.Exception("Unhandled exception in LoadWorld with path: " + path, e);
            //        });
            //    }
            //}
            catch (System.Exception e)
            {
                EnqueueTaskAndWait(() =>
                {
                    throw new System.Exception("Unhandled exception in LoadWorld with path: " + path, e);
                });
            }

            _manager._performingAsyncTask = false;
            _manager._simulationRunning = true;
        });
    }

    /// <summary>Resets the world load progress value.</summary>
    public static void ResetWorldLoadTrack()
    {
        _isLoadReady = false;
    }


    /// <summary>Initializes the world load progress value.</summary>
    public static void InitializeWorldLoadTrack()
    {
        _isLoadReady = true;

        _totalLoadTicks = WorldBeingLoaded.SerializedEventCount;
        _totalLoadTicks += WorldBeingLoaded.CellGroupCount;
        _totalLoadTicks += WorldBeingLoaded.TerrainCellAlterationListCount;

        _loadTicks = 0;
    }

    /// <summary>Updates the world load progress value.</summary>
    public static void UpdateWorldLoadTrackEventCount()
    {
        if (!_isLoadReady)
            InitializeWorldLoadTrack();

        _loadTicks += 1;

        float value = LastStageProgress + (StageProgressIncFromLoading * _loadTicks / _totalLoadTicks);

        _manager._progressCastMethod?.Invoke(Mathf.Min(1, value));
    }

    /// <summary>Sets the observable update types based on the current displayed overlay.</summary>
    /// <param name="overlay">The current displayed overlay.</param>
    /// <param name="planetOverlaySubtype">The current displayed overlay subtype.</param>
    private static void SetObservableUpdateTypes(PlanetOverlay overlay, string planetOverlaySubtype = "None")
    {
        switch (overlay)
        {
            case PlanetOverlay.None:
            case PlanetOverlay.Arability:
            case PlanetOverlay.Accessibility:
            case PlanetOverlay.Hilliness:
            case PlanetOverlay.BiomeTrait:
            case PlanetOverlay.Layer:
            case PlanetOverlay.Rainfall:
            case PlanetOverlay.DrainageBasins:
            case PlanetOverlay.Temperature:
            case PlanetOverlay.FarmlandDistribution:
                _observableUpdateTypes = CellUpdateType.Cell;
                break;
            case PlanetOverlay.Region:
                _observableUpdateTypes = CellUpdateType.Region;
                break;
            case PlanetOverlay.PolityCluster:
                _observableUpdateTypes = CellUpdateType.Cluster;
                break;
            case PlanetOverlay.Language:
                _observableUpdateTypes = CellUpdateType.Language;
                break;
            case PlanetOverlay.PolityTerritory:
            case PlanetOverlay.PolityContacts:
            case PlanetOverlay.PolityCulturalPreference:
            case PlanetOverlay.PolityCulturalActivity:
            case PlanetOverlay.PolityCulturalDiscovery:
            case PlanetOverlay.PolityCulturalKnowledge:
            case PlanetOverlay.PolityCulturalSkill:
                _observableUpdateTypes = CellUpdateType.Territory;
                break;
            case PlanetOverlay.General:
                _observableUpdateTypes = CellUpdateType.Group | CellUpdateType.Territory;
                break;
            default:
                _observableUpdateTypes = CellUpdateType.Group;
                break;
        }
    }

    /// <summary>Sets the observable update subtypes based on the current displayed overlay.</summary>
    /// <param name="overlay">The current displayed overlay.</param>
    /// <param name="planetOverlaySubtype">The current displayed overlay subtype.</param>
    private static void SetObservableUpdateSubtypes(PlanetOverlay overlay, string planetOverlaySubtype = "None")
    {
        switch (overlay)
        {
            case PlanetOverlay.None:
            case PlanetOverlay.Arability:
            case PlanetOverlay.Accessibility:
            case PlanetOverlay.Hilliness:
            case PlanetOverlay.BiomeTrait:
            case PlanetOverlay.Layer:
            case PlanetOverlay.Rainfall:
            case PlanetOverlay.DrainageBasins:
            case PlanetOverlay.Temperature:
            case PlanetOverlay.FarmlandDistribution:
                _observableUpdateSubTypes = CellUpdateSubType.Terrain;
                break;
            case PlanetOverlay.Region:
            case PlanetOverlay.PolityCluster:
            case PlanetOverlay.Language:
                _observableUpdateSubTypes = CellUpdateSubType.Membership;
                break;
            case PlanetOverlay.PolityTerritory:
                _observableUpdateSubTypes = CellUpdateSubType.MembershipAndCore;
                break;
            case PlanetOverlay.PolityContacts:
                _observableUpdateSubTypes = CellUpdateSubType.Membership | CellUpdateSubType.Relationship;
                break;
            case PlanetOverlay.General:
                _observableUpdateSubTypes = CellUpdateSubType.MembershipAndCore | CellUpdateSubType.Culture;
                break;
            case PlanetOverlay.PolityCulturalPreference:
            case PlanetOverlay.PolityCulturalActivity:
            case PlanetOverlay.PolityCulturalDiscovery:
            case PlanetOverlay.PolityCulturalKnowledge:
            case PlanetOverlay.PolityCulturalSkill:
                _observableUpdateSubTypes = CellUpdateSubType.Membership | CellUpdateSubType.Culture;
                break;
            default:
                _observableUpdateSubTypes = CellUpdateSubType.All;
                break;
        }
    }

    /// <summary>Sets the current displayed planet overlay.</summary>
    /// <param name="overlay">The overlay to be displayed.</param>
    /// <param name="planetOverlaySubtype">The overlay subtype to be displayed.</param>
    public static void SetPlanetOverlay(PlanetOverlay overlay, string planetOverlaySubtype = "None")
    {
        SetObservableUpdateTypes(overlay, planetOverlaySubtype);
        SetObservableUpdateSubtypes(overlay, planetOverlaySubtype);

        _planetOverlay = overlay;
        _planetOverlaySubtype = planetOverlaySubtype;
    }

    /// <summary>Displays routes based on <c>value</c>.</summary>
    /// <param name="value">If set to <c>true</c>, displays routes.</param>
    public static void SetDisplayRoutes(bool value)
    {
        if (value)
            _observableUpdateTypes |= CellUpdateType.Route;
        else
            _observableUpdateTypes &= ~CellUpdateType.Route;

        _displayRoutes = value;
    }

    /// <summary>Displays group activity based on <c>value</c>.</summary>
    /// <param name="value">If set to <c>true</c>, displays group activity.</param>
    public static void SetDisplayGroupActivity(bool value)
    {
        _displayGroupActivity = value;
    }

    /// <summary>Sets the planet view to <c>value</c>.</summary>
    /// <param name="value">The planet view to be set.</param>
    public static void SetPlanetView(PlanetView value)
    {
        _planetView = value;
    }

    /// <summary>
    ///   Sets the currently selected cell to the cell at the given <c>latitude</c> and <c>longitude</c>.
    /// </summary>
    /// <param name="longitude">The longitude of the cell to be set to selectd.</param>
    /// <param name="latitude">The latitude of the cell to be set to selected.</param>
    public static void SetSelectedCell(int longitude, int latitude)
    {
        SetSelectedCell(CurrentWorld.GetCell(longitude, latitude));
    }

    /// <summary>Sets the selected cell to the cell at the given <c>position</c>.</summary>
    /// <param name="position">The world position of the cell to be set to selected.</param>
    public static void SetSelectedCell(WorldPosition position)
    {
        SetSelectedCell(CurrentWorld.GetCell(position));
    }

    /// <summary>Sets the selected region to <c>region</c>.</summary>
    /// <param name="region">The region to be set to selected.</param>
    public static void SetSelectedRegion(Region region)
    {
        if (CurrentWorld.SelectedRegion != null) // Why add the current selected region to highlighted cells?
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

    /// <summary>Sets the selected territory to <c>territory</c>.</summary>
    /// <param name="territory">The territory to be set to selected.</param>
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

    /// <summary>Sets the selected cell to <c>cell</c>.</summary>
    /// <param name="cell">The terrain cell to be set to selected.</param>
    public static void SetSelectedCell(TerrainCell cell)
    {
        if (CurrentWorld.SelectedCell != null)
        {
            AddHighlightedCell(CurrentWorld.SelectedCell, CellUpdateType.All);

            CurrentWorld.SelectedCell.IsSelected = false;
            CurrentWorld.SelectedCell = null;
        }

        if (cell != null)
        {
            CurrentWorld.SelectedCell = cell;
            CurrentWorld.SelectedCell.IsSelected = true;

            AddHighlightedCell(CurrentWorld.SelectedCell, CellUpdateType.All);

            SetSelectedRegion(cell.Region);
            SetSelectedTerritory(cell.EncompassingTerritory);
        }
    }

    /// <summary>Puts the given <c>polity</c> under player focus.</summary>
    /// <param name="polity">The polity to be put under player focus.</param>
    public static void SetFocusOnPolity (Polity polity) {

		if (polity == null)
			return;

		if (CurrentWorld.PolitiesUnderPlayerFocus.Contains (polity))
			return;

		polity.SetUnderPlayerFocus (true);
		CurrentWorld.PolitiesUnderPlayerFocus.Add (polity);
	}

    /// <summary>Removes the given <c>polity</c> from player focus.</summary>
    /// <param name="polity">The polity to be removed from player focus.</param>
	public static void UnsetFocusOnPolity (Polity polity) {

		if (polity == null)
			return;

		if (!CurrentWorld.PolitiesUnderPlayerFocus.Contains (polity))
			return;
			
		polity.SetUnderPlayerFocus (false);
		CurrentWorld.PolitiesUnderPlayerFocus.Remove (polity);
    }

    /// <summary>Puts the given <c>faction</c> under player guidance.</summary>
    /// <param name="faction">The faction to be put under player guidance.</param>
    /// <remarks>Only one faction can be under player guidance per world.</remarks>
    public static void SetGuidedFaction(Faction faction)
    {
        if (CurrentWorld.GuidedFaction == faction)
            return;

        CurrentWorld.GuidedFaction?.SetUnderPlayerGuidance(false);

        faction?.SetUnderPlayerGuidance(true);

        CurrentWorld.GuidedFaction = faction;
    }

    /// <summary>Resets the currently updated and highlighted cells.</summary>
    public static void ResetUpdatedAndHighlightedCells()
    {
        _lastUpdatedCells.Clear();
        _lastUpdatedCells.UnionWith(UpdatedCells);

        UpdatedCells.Clear();
        TerrainUpdatedCells.Clear();
        HighlightedCells.Clear();
    }

    /// <summary>Updates the pointer overlay textures in the world editor.</summary>
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

    /// <summary>Applies the editor brush to the world map in the editor.</summary>
    public static void ApplyEditorBrush() // This is a pretty complex function that could use some more comments
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
                int iRadius = (int) MathUtility.GetComponent(fRadius, jDiff);
                
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

    /// <summary>Determines what layer to apply the editor brush at.</summary>
    /// <param name="longitude">The longitude the editor brush was applied at.</param>
    /// <param name="latitude">The latitude the editor brush was applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
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

    /// <summary>Determines what layer to apply the flattened editor brush at.</summary>
    /// <param name="longitude">The longitude the editor brush was applied at.</param>
    /// <param name="latitude">The latitude the editor brush was applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
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

    /// <summary>Resets the slants of all the neighbor cells around <c>cell</c>.</summary>
    /// <param name="cell">The cell where the neighbors are derived from.</param>
    public static void ResetSlantsAround(TerrainCell cell) // What is a cell slant?
    {
        foreach (TerrainCell nCell in cell.Neighbors.Values)
        {
            _manager._currentCellSlants[nCell.Longitude, nCell.Latitude] = null;
        }

        _manager._currentCellSlants[cell.Longitude, cell.Latitude] = null;
    }

    /// <summary>Activates the editor brush based on the given <c>state</c>.</summary>
    /// <param name="state">If set to <c>true</c>, the editor brush is activated; otherwise, false.</param>
    public static void ActivateEditorBrush(bool state)
    {
        bool useLayerBrush = false;

        if (EditorBrushType == EditorBrushType.Layer)
        {
            if (!Layer.IsValidLayerId(_planetOverlaySubtype))
            {
                return;
            }

            useLayerBrush = true;
        }

        EditorBrushIsActive = state;

        if (state)
        {
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
            CurrentWorld.PerformTerrainAlterationDrainageRegen();
            CurrentWorld.RepeatTerrainAlterationDrainageRegen();
            CurrentWorld.FinalizeTerrainAlterationDrainageRegen();

            ActiveEditorBrushAction.FinalizeCellModifications();

            PushUndoableAction(ActiveEditorBrushAction);
            ResetRedoableActionsStack();

            ActiveEditorBrushAction = null;
        }
    }

    /// <summary>Applies the editor brush on the altitude layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
    private static void ApplyEditorBrush_Altitude(int longitude, int latitude, float distanceFactor)
    {
        float strength = EditorBrushStrength / AltitudeScale;
        float noiseRadius = BrushNoiseRadiusFactor / EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Altitude * 
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = strength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellAltitude(cell, valueOffset, EditorBrushNoise, noiseRadius);

        ResetSlantsAround(cell);
    }

    /// <summary>Applies the flattened editor brush on the altitude layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
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

    /// <summary>Applies the editor brush on the temperature layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
    private static void ApplyEditorBrush_Temperature(int longitude, int latitude, float distanceFactor)
    {
        float noiseRadius = BrushNoiseRadiusFactor / EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Temperature *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellTemperature(cell, valueOffset, EditorBrushNoise, noiseRadius);
    }

    /// <summary>Applies the flattened editor brush on the altitude layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
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

    /// <summary>Applies the editor brush on the generic? layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
    private static void ApplyEditorBrush_Layer(int longitude, int latitude, float distanceFactor)
    {
        if (!Layer.IsValidLayerId(_planetOverlaySubtype))
        {
            throw new System.Exception("Not a recognized layer Id: " + _planetOverlaySubtype);
        }

        float noiseRadius = BrushNoiseRadiusFactor / EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Layer *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellLayerData(cell, valueOffset, _planetOverlaySubtype, EditorBrushNoise, noiseRadius);
    }

    /// <summary>Applies the flattened editor brush on the generic? layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
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

    /// <summary>Applies the editor brush on the rainfall layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
    private static void ApplyEditorBrush_Rainfall(int longitude, int latitude, float distanceFactor)
    {
        float noiseRadius = BrushNoiseRadiusFactor / EditorBrushRadius;

        float strToValue = BrushStrengthFactor_Base * BrushStrengthFactor_Rainfall *
            (MathUtility.GetPseudoNormalDistribution(distanceFactor * 2) - MathUtility.NormalAt2) / (MathUtility.NormalAt0 - MathUtility.NormalAt2);
        float valueOffset = EditorBrushStrength * strToValue;

        TerrainCell cell = CurrentWorld.GetCell(longitude, latitude);

        CurrentWorld.ModifyCellRainfall(cell, valueOffset, EditorBrushNoise, noiseRadius);
    }

    /// <summary>Applies the flattened editor brush on the rainfall layer.</summary>
    /// <param name="longitude">The longitude to be applied at.</param>
    /// <param name="latitude">The latitude to be applied at.</param>
    /// <param name="distanceFactor">The distance the editor brush extends.</param>
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

    /// <summary>Updates the state of the editor brush.</summary>
    public static void UpdateEditorBrushState()
    {
        _lastEditorBrushTargetCell = EditorBrushTargetCell;
        _lastEditorBrushRadius = EditorBrushRadius;
        _editorBrushWasVisible = EditorBrushIsVisible;
    }

    /// <summary>Updates the texture colors of the map, map overlays, and map activity.</summary>
    /// <remarks>If shaders are enabled, those will be updated as well.</remarks>
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

        if (AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            UpdateMapOverlayShaderTextureColors();
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("CurrentMapTexture.SetPixels32");

        CurrentMapTexture.SetPixels32(_manager._currentMapTextureColors);
        CurrentMapOverlayTexture.SetPixels32(_manager._currentMapOverlayTextureColors);
        CurrentMapActivityTexture.SetPixels32(_manager._currentMapActivityTextureColors);

        if (AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            CurrentMapOverlayShaderInfoTexture.SetPixels32(_manager._currentMapOverlayShaderInfoColor);
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("CurrentMapTexture.Apply");

        CurrentMapTexture.Apply();
        CurrentMapOverlayTexture.Apply();
        CurrentMapActivityTexture.Apply();

        if (AnimationShadersEnabled && (PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            CurrentMapOverlayShaderInfoTexture.Apply();
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("ResetUpdatedAndHighlightedCells");

        ResetUpdatedAndHighlightedCells();

        //Profiler.EndSample();
    }

    /// <summary>Updates the pointer overlay texture colors with a new array of RBGA colors from <c>textureColors</c>.</summary>
    /// <param name="textureColors">The texture colors that will be used to update the pointer overlay texture colors.</param>
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

    /// <summary>Updates the map texture colors by applying the current map RBGA array to every terrain cell.</summary>
    public static void UpdateMapTextureColors()
    {
        Color32[] textureColors = _manager._currentMapTextureColors;

        foreach (TerrainCell cell in TerrainUpdatedCells)
        {
            UpdateMapTextureColorsFromCell(textureColors, cell);
        }
    }

    /// <summary>
    ///   Updates the map overlay texture colors by applying the current map overlay RBGA array to every terrain cell.
    /// </summary>
    public static void UpdateMapOverlayTextureColors()
    {
        Color32[] textureColors = _manager._currentMapOverlayTextureColors;

        foreach (TerrainCell cell in UpdatedCells)
        {
            UpdateMapOverlayTextureColorsFromCell(textureColors, cell);
        }
    }

    /// <summary>
    ///   Updates the map activity texture colors by applying the current map activity RBGA array to every terrain cell.
    /// </summary>
    /// <remarks>Conditionally will update group activity and routes if each respective option is true.</remarks>
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

    /// <summary>
    ///   Updates the map overlay shader texture colors by applying the current map overlay shader RBGA array to every terrain cell.
    /// </summary>
    public static void UpdateMapOverlayShaderTextureColors()
    {
        Color32[] overlayShaderInfoColors = _manager._currentMapOverlayShaderInfoColor;
        
        foreach (TerrainCell cell in UpdatedCells)
        {
            UpdateMapOverlayShaderTextureColorsFromCell(overlayShaderInfoColors, cell);
        }
    }

    /// <summary>Determines if the given <c>cell</c> should be highlighted.</summary>
    /// <param name="cell">The cell to be evaluated.</param>
    /// <returns></returns>
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

    /// <summary>Updates the pointer overlay texture colors from the editor brush.</summary>
    /// <param name="textureColors">The updated texture colors.</param>
    /// <param name="centerCell">The center cell of the editor brush.</param>
    /// <param name="radius">The radius of the editor brush.</param>
    /// <param name="erase">If set to <c>true</c>, will erase.</param>
    public static void UpdatePointerOverlayTextureColorsFromBrush(Color32[] textureColors, TerrainCell centerCell, int radius, bool erase = false)
    {
        World world = centerCell.World; // Please document this function :(

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

    /// <summary>Updates the map texture colors from the given <c>cell</c> using <c>textureColors</c>.</summary>
    /// <param name="textureColors">The texture colors to be updated.</param>
    /// <param name="cell">The cell which will be used to update the map texture colors.</param>
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

    /// <summary>Updates the map overlay texture colors from the given <c>cell</c> using <c>textureColors</c>.</summary>
    /// <param name="textureColors">The texture colors to be updated.</param>
    /// <param name="cell">The cell which will be used to update the map overlay texture colors.</param>
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

    /// <summary>Updates the map activity texture colors from the given <c>cell</c> using <c>textureColors</c>.</summary>
    /// <param name="textureColors">The texture colors to be updated.</param>
    /// <param name="cell">The cell which will be used to update the map activity texture colors.</param>
    /// <param name="displayActivityCells">If set to <c>true</c>, activity cells will be displayed.</param>
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

    /// <summary>Updates the map overlay shader texture colors from the given <c>cell</c> using <c>textureColors</c>.</summary>
    /// <param name="textureColors">The texture colors to be updated.</param>
    /// <param name="cell">The cell which will be used to update the map overlay shader texture colors.</param>
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

    /// <summary>Generates the pointer overlay texture from the given <c>world</c>.</summary>
    /// <param name="world">The world to generate the pointer overlay texture from.</param>
    /// <returns>The pointer overlay texture generated from <c>world</c>.</returns>
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
        TerrainCell nCell = null;

        if (neighbors.TryGetValue(Direction.West, out nCell))
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

        wAltitude /= c;

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

        eAltitude /= c;

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
        Color color = Color.black;

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
            foreach (Route route in cell.CrossingRoutes)
            {
                if (route.Used)
                    return GetOverlayColor(OverlayColorId.ActiveRoute);
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

        if (cell.Altitude > 0)
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
            value = (2 - altitude / World.MinPossibleAltitude - SeaLevelOffset) / 2f;

            Color color1 = Color.blue;

            return new Color(color1.r * value, color1.g * value, color1.b * value);
        }

        value = (1 + altitude / (World.MaxPossibleAltitude - SeaLevelOffset)) / 2f;

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
            Language nLanguage = nCell.Group?.Culture.Language;

            if (nLanguage == null)
                return true;

            if (nLanguage.Id != language.Id)
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
                Color languageColor = GenerateColorFromId(groupLanguage.Id, 100);

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

            color = GenerateColorFromId(territoryPolity.Id, 100);

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

                Color clusterColor = Color.grey;

                if (prominence.Cluster != null)
                {
                    color = GenerateColorFromId(prominence.Cluster.Id, 100);
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

                Color territoryColor = GenerateColorFromId(territoryPolity.Id, 100);

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

                Color polityColor = GenerateColorFromId(p.PolityId, 100);
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

    private static Color GenerateColorFromId(long id, int oom = 1)
    {
        long mId = id / oom;

        long primaryColor = mId % 3;
        float secondaryColorIntensity = ((mId / 3) % 4) / 3f;
        float tertiaryColorIntensity = ((mId / 12) % 2) / 2f;
        long secondaryColor = (mId / 24) % 2;

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

        if (cell.Group != null)
        {
            float population = cell.Group.Population;

            if (cell.Group.MigrationTagged && DisplayMigrationTaggedGroup)
                return Color.green;

#if DEBUG
            if (cell.Group.DebugTagged && DisplayDebugTaggedGroups)
                return Color.green;
#endif

            if (population > 0)
            {
                float value = (population + maxPopFactor) / (maxPopulation.Value + maxPopFactor);

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

            groupColor = GenerateColorFromId(territoryPolity.Id, 100);

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

        float value = (cell.Temperature - (World.MinPossibleTemperature + TemperatureOffset)) / span;

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

    /// <summary>Enables the XmlSerializer to override the default way of serializing a set of objects.</summary>
    /// <returns>
    ///   The newly instantiated XmlAttributeOverrides object.
    /// </returns>
    private static XmlAttributeOverrides GenerateAttributeOverrides()
    {
        XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();

        return attrOverrides;
    }

    public static Texture2D LoadTexture(string path)
    {
        if (!File.Exists(path))
            return null;

        byte[] data = File.ReadAllBytes(path);

        Texture2D texture = new Texture2D(1, 1);

        return texture.LoadImage(data) ? texture : null;
    }

    /// <summary>Determines if the given <c>texture</c> is valid.</summary>
    /// <param name="texture">The texture to be validated.</param>
    /// <returns>
    ///   The <c>TextureValidationResult</c> which will be 1 for not valid or 0 for valid.
    /// </returns>
    public static TextureValidationResult ValidateTexture(Texture2D texture)
    {
        if ((texture.width < WorldWidth) && (texture.height < WorldHeight))
        {
            return TextureValidationResult.NotMinimumRequiredDimensions;
        }

        return TextureValidationResult.Ok;
    }

    /// <summary>Loads the mods from a given <c>path</c>.</summary>
    /// <param name="paths">The list of paths to load the mods from.</param>
    public static void LoadMods(ICollection<string> paths)
    {
        if (paths.Count == 0)
            throw new System.ArgumentException("Number of mods to load can't be zero");

        World.ResetStaticModData();
        CellGroup.ResetEventGenerators();

        Layer.ResetLayers();
        Biome.ResetBiomes();

        Adjective.ResetAdjectives();
        RegionAttribute.ResetAttributes();
        Element.ResetElements();

        Discovery.ResetDiscoveries();
        Knowledge.ResetKnowledges();

        float progressPerMod = 0.1f / paths.Count;

        foreach (string path in paths)
        {
            if (_manager._progressCastMethod != null)
            {
                string directoryName = Path.GetFileName(path);

                _manager._progressCastMethod(LastStageProgress, "Loading Mod '" + directoryName + "'...");
            }

            LoadMod(path + @"\", progressPerMod);

            LastStageProgress += progressPerMod;
        }

        Knowledge.InitializeKnowledges();
        Discovery.InitializeDiscoveries();
    }

    delegate void LoadModFileDelegate(string filename);

    /// <summary>Tries to load the mod files of a particular type.</summary>
    /// <param name="loadModFile">The delegate for loading a particular mod file.</param>
    /// <param name="path">The path to the mod files trying to be loaded.</param>
    /// <param name="progressPerModSegment">The progress per mod segment value.</param>
    private static void TryLoadModFiles(LoadModFileDelegate loadModFile, string path, float progressPerModSegment)
    {
        if (!Directory.Exists(path))
            return;

        string[] files = Directory.GetFiles(path, "*.json");

        if (files.Length <= 0)
            return;

        float progressPerFile = progressPerModSegment / files.Length;
        float accProgress = LastStageProgress;

        foreach (string file in files)
        {
            loadModFile(file);

            accProgress += progressPerFile;

            _manager._progressCastMethod?.Invoke(accProgress);
        }
    }

    /// <summary>Loads the mod at the location identified by <c>path</c>.</summary>
    /// <param name="path">The path from where to load the mod.</param>
    /// <param name="progressPerMod">The progress value per mod.</param>
    private static void LoadMod(string path, float progressPerMod)
    {
        float progressPerSegment = progressPerMod / 6f;

        TryLoadModFiles(Layer.LoadLayersFile, path + @"Layers", progressPerSegment);
        TryLoadModFiles(Biome.LoadBiomesFile, path + @"Biomes", progressPerSegment);
        TryLoadModFiles(Adjective.LoadAdjectivesFile, path + @"Adjectives", progressPerSegment);
        TryLoadModFiles(RegionAttribute.LoadRegionAttributesFile, path + @"RegionAttributes", progressPerSegment);
        TryLoadModFiles(Element.LoadElementsFile, path + @"Elements", progressPerSegment);
        TryLoadModFiles(Discovery.LoadDiscoveriesFile, path + @"Discoveries", progressPerSegment);
    }
}
