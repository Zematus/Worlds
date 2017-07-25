using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

public enum PlanetView {

	Elevation,
	Biomes,
	Coastlines
}

public enum PlanetOverlay {

	None,
	General,
	PopDensity,
	FarmlandDistribution,
	PopCulturalActivity,
	PopCulturalSkill,
	PopCulturalKnowledge,
	PopCulturalDiscovery,
	PolityTerritory,
	PolityCoreDistance,
	PolityInfluence,
	PolityCulturalActivity,
	PolityCulturalSkill,
	PolityCulturalKnowledge,
	PolityCulturalDiscovery,
	Temperature,
	Rainfall,
	Arability,
	Region,
	Language,
	PopChange,
	UpdateSpan
}

public enum OverlayColorId {

	None = -1,
	Arability = 0,
	Farmland = 1,
	GeneralDensitySubOptimal = 2,
	GeneralDensityOptimal = 3
}

public delegate T ManagerTaskDelegate<T> ();

public interface IManagerTask {

	void Execute ();
}

public class ManagerTask<T> : IManagerTask {

	public const int SleepTime = 100;

	public bool IsRunning { get; private set; }

	private ManagerTaskDelegate<T> _taskDelegate;

	private T _result;

	public ManagerTask (ManagerTaskDelegate<T> taskDelegate) {

		IsRunning = true;
	
		_taskDelegate = taskDelegate;
	}

	public void Execute () {
	
		_result = _taskDelegate ();

		IsRunning = false;
	}

	public void Wait () {

		while (IsRunning) {
		
			Thread.Sleep (SleepTime);
		}
	}

	public T Result {

		get {
			if (IsRunning) Wait ();

			return _result;
		}
	}

	public static implicit operator T(ManagerTask<T> task) {

		return task.Result;
	}
}

public class AppSettings {

	public float TemperatureOffset = 0;
	public float RainfallOffset = 0;
	public float SeaLevelOffset = 0;
	public bool Fullscreen = true;

	public AppSettings () {
	}

	public void Put () {

		TemperatureOffset = Manager.TemperatureOffset;
		RainfallOffset = Manager.RainfallOffset;
		SeaLevelOffset = Manager.SeaLevelOffset;
		Fullscreen = Manager.IsFullscreen;
	}

	public void Take () {

		Manager.TemperatureOffset = TemperatureOffset;
		Manager.RainfallOffset = RainfallOffset;
		Manager.SeaLevelOffset = SeaLevelOffset;
		Manager.IsFullscreen = Fullscreen;
	}
}

public class Manager {

	#if DEBUG

	public delegate void RegisterDebugEventDelegate (string eventType, object data);

	public static RegisterDebugEventDelegate RegisterDebugEvent = null; 

	public class Debug_TracingData {

		public long GroupId;
		public long PolityId;
		public int Longitude;
		public int Latitude;
	}

	public static Debug_TracingData TracingData = new Manager.Debug_TracingData ();

	public static bool TrackGenRandomCallers = false;

	#endif

	public static string CurrentVersion = "0.026d";

//	public static bool RecordingEnabled = false;

//	public static IRecorder Recorder = DefaultRecorder.Default;

	public const int WorldWidth = 400;
	public const int WorldHeight = 200;

	public static float ProgressIncrement = 0.20f;
	
	public static Thread MainThread { get; private set; }
	
	public static string SavePath { get; private set; }
	public static string ExportPath { get; private set; }
	
	public static string WorldName { get; set; }
	
	public static HashSet<TerrainCell> UpdatedCells { get; private set; }
	
	public static int PixelToCellRatio = 4;

	public static float TemperatureOffset = 0;
	public static float RainfallOffset = 0;
	public static float SeaLevelOffset = 0;

	public static bool DisplayMigrationTaggedGroup = false;
	
	public static bool DisplayDebugTaggedGroups = false;
	
	public static World WorldBeingLoaded = null;

	public static bool IsFullscreen = false;

	private static bool _isLoadReady = false;

	private static List<TerrainCell> _lastUpdatedCells;

	private static int _resolutionWidthWindowed = 1366;
	private static int _resolutionHeightWindowed = 768;

	private static bool _resolutionInitialized = false;

	private static CellUpdateType _observableUpdateTypes = CellUpdateType.Cell | CellUpdateType.Group;

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
	
	private ProgressCastDelegate _progressCastMethod = null;
	
	private World _currentWorld = null;
	
	private Texture2D _currentSphereTexture = null;
	private Texture2D _currentMapTexture = null;
	
	private Color32[] _currentSphereTextureColors = null;
	private Color32[] _currentMapTextureColors = null;

	private float?[,] _currentCellSlants;

	private int _currentMaxUpdateSpan = 0;

	private Queue<IManagerTask> _taskQueue = new Queue<IManagerTask>();
	
	private bool _performingAsyncTask = false;
	private bool _simulationRunning = false;
	private bool _worldReady = false;

	public XmlAttributeOverrides AttributeOverrides { get; private set; }
	
	public static bool PerformingAsyncTask {
		
		get {
			return _manager._performingAsyncTask;
		}
	}

	public static bool SimulationRunning {

		get {
			return _manager._simulationRunning;
		}
	}

	public static bool WorldReady {
		
		get {
			return _manager._worldReady;
		}
	}
	
	public static bool SimulationCanRun {
		
		get {

			bool canRun = (_manager._currentWorld.CellGroupCount > 0);

			return canRun;
		}
	}

	public static PlanetOverlay PlanetOverlay {

		get { 
			return _planetOverlay;
		}
	}

	public static string PlanetOverlaySubtype {

		get { 
			return _planetOverlaySubtype;
		}
	}

	public static bool DisplayRoutes {

		get { 
			return _displayRoutes;
		}
	}

	public static bool DisplayGroupActivity {

		get { 
			return _displayGroupActivity;
		}
	}
	
	public static void UpdateMainThreadReference () {
		
		MainThread = Thread.CurrentThread;
	}

	private Manager () {

		InitializeSavePath ();
		InitializeExportPath ();

		AttributeOverrides = GenerateAttributeOverrides ();

		UpdatedCells = new HashSet<TerrainCell> ();
		_lastUpdatedCells = new List<TerrainCell> ();
	}

	private void InitializeSavePath () {
		
		string path = Path.GetFullPath (@"Saves\");
		
		if (!Directory.Exists (path)) {
			
			Directory.CreateDirectory(path);
		}
		
		SavePath = path;
	}
	
	private void InitializeExportPath () {
		
		string path = Path.GetFullPath (@"Images\");
		
		if (!Directory.Exists (path)) {
			
			Directory.CreateDirectory(path);
		}
		
		ExportPath = path;
	}

	public static void SetFullscreen (bool state) {

		IsFullscreen = state;

		if (state) {
			Resolution currentResolution = Screen.currentResolution;

			Screen.SetResolution(currentResolution.width, currentResolution.height, state);
		} else {
			Screen.SetResolution(_resolutionWidthWindowed, _resolutionHeightWindowed, state);
		}
	}

	public static void InitializeScreen () {
	
		if (_resolutionInitialized)
			return;

		SetFullscreen (IsFullscreen);

		_resolutionInitialized = true;
	}
	
	public static void InterruptSimulation (bool state) {
		
		_manager._simulationRunning = !state;
	}
	
	public static void ExecuteTasks (int count) {

		for (int i = 0; i < count; i++) {
		
			if (!ExecuteNextTask ()) break;
		}
	}
	
	public static bool ExecuteNextTask () {

		IManagerTask task;
		
		lock (_manager._taskQueue) {

			if (_manager._taskQueue.Count <= 0)
				return false;
			
			task = _manager._taskQueue.Dequeue();
		}

		task.Execute ();

		return true;
	}

	public static ManagerTask<T> EnqueueTask<T> (ManagerTaskDelegate<T> taskDelegate) {

		ManagerTask<T> task = new ManagerTask<T> (taskDelegate);

		if (MainThread == Thread.CurrentThread) {
			task.Execute ();
		} else {
			lock (_manager._taskQueue) {
				
				_manager._taskQueue.Enqueue (task);
			}
		}

		return task;
	}
	
	public static void EnqueueTaskAndWait<T> (ManagerTaskDelegate<T> taskDelegate) {
		
		EnqueueTask (taskDelegate).Wait ();
	}

	public static void SetBiomePalette (IEnumerable<Color> colors) {

		_biomePalette.Clear ();
		_biomePalette.AddRange (colors);
	}
	
	public static void SetMapPalette (IEnumerable<Color> colors) {
		
		_mapPalette.Clear ();
		_mapPalette.AddRange (colors);
	}

	public static void SetOverlayPalette (IEnumerable<Color> colors) {

		_overlayPalette.Clear ();
		_overlayPalette.AddRange (colors);
	}

	public static World CurrentWorld { 
		get {

			return _manager._currentWorld; 
		}
	}
	
	public static Texture2D CurrentSphereTexture { 
		get {

			return _manager._currentSphereTexture; 
		}
	}
	
	public static Texture2D CurrentMapTexture { 
		get {

			return _manager._currentMapTexture; 
		}
	}
	
	public static void ExportMapTextureToFile (string path, Rect uvRect) {

		Texture2D mapTexture = _manager._currentMapTexture;
		Texture2D exportTexture = null;

		Manager.EnqueueTaskAndWait (() => {
			int width = mapTexture.width;
			int height = mapTexture.height;

			int xOffset = (int)Mathf.Floor (uvRect.x * width);

			exportTexture = new Texture2D (
				width,
				height,
				mapTexture.format,
				false);

			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {

					int finalX = (i + xOffset) % width;

					exportTexture.SetPixel (i, j, mapTexture.GetPixel (finalX, j));
				}
			}

			return true;
		});
		
		ManagerTask<byte[]> bytes = Manager.EnqueueTask (() => exportTexture.EncodeToPNG ());

		File.WriteAllBytes(path, bytes);

		Manager.EnqueueTaskAndWait (() => {
			Object.Destroy (exportTexture);
			return true;
		});
	}
	
	public static void ExportMapTextureToFileAsync (string path, Rect uvRect, ProgressCastDelegate progressCastMethod = null) {
		
		_manager._simulationRunning = false;
		_manager._performingAsyncTask = true;
		
		_manager._progressCastMethod = progressCastMethod;
		
		if (_manager._progressCastMethod == null) {
		
			_manager._progressCastMethod = (value, message, reset) => {};
		}
		
		ThreadPool.QueueUserWorkItem (state => {
			
			ExportMapTextureToFile (path, uvRect);
			
			_manager._performingAsyncTask = false;
			_manager._simulationRunning = true;
		});
	}
	
	public static void GenerateTextures () { 

		//GenerateSphereTextureFromWorld(CurrentWorld);
		GenerateMapTextureFromWorld(CurrentWorld);

		ResetUpdatedCells ();
	}

	public static void AddUpdatedCell (TerrainCell cell, CellUpdateType updateType) {

		if ((_observableUpdateTypes & updateType) == updateType) {
			
			UpdatedCells.Add (cell);
		}
	}

	public static void AddUpdatedCells (ICollection<TerrainCell> cells, CellUpdateType updateType) {

		if ((_observableUpdateTypes & updateType) == updateType) {

			foreach (TerrainCell cell in cells)
				UpdatedCells.Add (cell);
		}
	}

	public static void GenerateRandomHumanGroup (int initialPopulation) {

		World world = _manager._currentWorld;
		
		if (_manager._progressCastMethod == null) {
			world.ProgressCastMethod = (value, message, reset) => {};
		} else {
			world.ProgressCastMethod = _manager._progressCastMethod;
		}

		world.GenerateRandomHumanGroups (1, initialPopulation);
	}

	public static void GenerateHumanGroup (int longitude, int latitude, int initialPopulation) {
		
		World world = _manager._currentWorld;
		
		if (_manager._progressCastMethod == null) {
			world.ProgressCastMethod = (value, message, reset) => {};
		} else {
			world.ProgressCastMethod = _manager._progressCastMethod;
		}
		
		world.GenerateHumanGroup (longitude, latitude, initialPopulation);
	}

	public static void GenerateNewWorld (int seed) {

		_manager._worldReady = false;

		World world = new World(WorldWidth, WorldHeight, seed);
		
		if (_manager._progressCastMethod == null) {
			world.ProgressCastMethod = (value, message, reset) => {};
		} else {
			world.ProgressCastMethod = _manager._progressCastMethod;
		}

		world.StartInitialization (0f, ProgressIncrement);
		world.Generate ();
		world.FinishInitialization ();

		_manager._currentWorld = world;

		_manager._currentCellSlants = new float?[world.Width, world.Height];

		_manager._currentMaxUpdateSpan = 0;

		_manager._worldReady = true;
	}
	
	public static void GenerateNewWorldAsync (int seed, ProgressCastDelegate progressCastMethod = null) {

		_manager._simulationRunning = false;
		_manager._performingAsyncTask = true;
		
		_manager._progressCastMethod = progressCastMethod;

		if (_manager._progressCastMethod == null) {
			
			_manager._progressCastMethod = (value, message, reset) => {};
		}

		ThreadPool.QueueUserWorkItem (state => {
			
			GenerateNewWorld (seed);
			
			_manager._performingAsyncTask = false;
			_manager._simulationRunning = true;
		});
	}

	public static void SaveAppSettings (string path) {

		AppSettings settings = new AppSettings ();

		settings.Put ();

		XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
		FileStream stream = new FileStream(path, FileMode.Create);

		serializer.Serialize(stream, settings);

		stream.Close ();
	}

	public static void LoadAppSettings (string path) {

		if (!File.Exists (path))
			return;

		XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
		FileStream stream = new FileStream(path, FileMode.Open);

		AppSettings settings = serializer.Deserialize(stream) as AppSettings;

		stream.Close ();

		settings.Take ();
	}
	
	public static void SaveWorld (string path) {

		_manager._currentWorld.Synchronize ();

		XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);
		FileStream stream = new FileStream(path, FileMode.Create);

		serializer.Serialize(stream, _manager._currentWorld);

		stream.Close ();
	}
	
	public static void SaveWorldAsync (string path, ProgressCastDelegate progressCastMethod = null) {
		
		_manager._simulationRunning = false;
		_manager._performingAsyncTask = true;
		
		_manager._progressCastMethod = progressCastMethod;
		
		if (_manager._progressCastMethod == null) {
			
			_manager._progressCastMethod = (value, message, reset) => {};
		}
		
		ThreadPool.QueueUserWorkItem (state => {
			
			SaveWorld (path);
			
			_manager._performingAsyncTask = false;
			_manager._simulationRunning = true;
		});
	}
	
	public static void LoadWorld (string path) {

		_manager._worldReady = false;

		float baseProgressIncrement = ProgressIncrement;
		ProgressIncrement = 0.08f;
		
		ResetWorldLoadTrack ();

		XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);
		FileStream stream = new FileStream(path, FileMode.Open);

		World world = serializer.Deserialize(stream) as World;

		stream.Close ();
		
		if (_manager._progressCastMethod == null) {
			world.ProgressCastMethod = (value, message, reset) => {};
		} else {
			world.ProgressCastMethod = _manager._progressCastMethod;
		}

		float initialProgressIncrement = ProgressIncrement;

		world.StartInitialization (initialProgressIncrement, ProgressIncrement);
		world.GenerateTerrain ();
		world.FinishInitialization ();
		
		if (_manager._progressCastMethod != null) {
			_manager._progressCastMethod (0.5f, "Finalizing...");
		}

		world.FinalizeLoad (0.5f, 1.0f, _manager._progressCastMethod);

		ProgressIncrement = baseProgressIncrement;

		_manager._currentWorld = world;

		_manager._currentCellSlants = new float?[world.Width, world.Height];

		_manager._currentMaxUpdateSpan = 0;

		WorldBeingLoaded = null;

		_manager._worldReady = true;
	}
	
	public static void LoadWorldAsync (string path, ProgressCastDelegate progressCastMethod = null) {
		
		_manager._simulationRunning = false;
		_manager._performingAsyncTask = true;
		
		_manager._progressCastMethod = progressCastMethod;
		
		if (_manager._progressCastMethod == null) {
			
			_manager._progressCastMethod = (value, message, reset) => {};
		}
		
		ThreadPool.QueueUserWorkItem (state => {
			
			LoadWorld (path);
			
			_manager._performingAsyncTask = false;
			_manager._simulationRunning = true;
		});
	}

	public static void ResetWorldLoadTrack () {

		_isLoadReady = false;
	}

	public static void InitializeWorldLoadTrack () {

		_isLoadReady = true;

		_totalLoadTicks = WorldBeingLoaded.EventsToHappenCount;
		_totalLoadTicks += WorldBeingLoaded.CellGroupCount;
		_totalLoadTicks += WorldBeingLoaded.TerrainCellChangesListCount;

		_loadTicks = 0;
	}
	
	public static void UpdateWorldLoadTrackEventCount () {
		
		if (!_isLoadReady)
			InitializeWorldLoadTrack ();
		
		_loadTicks += 1;
		
		float value = ProgressIncrement * _loadTicks / (float)_totalLoadTicks;
		
		if (_manager._progressCastMethod != null) {
			_manager._progressCastMethod (Mathf.Min (1, value));
		}
	}

	public static void SetPlanetOverlay (PlanetOverlay value, string planetOverlaySubtype = "None") {

		if ((value == PlanetOverlay.None) ||
			(value == PlanetOverlay.Arability) ||
			(value == PlanetOverlay.Rainfall) ||
			(value == PlanetOverlay.Temperature)) {

			_observableUpdateTypes = CellUpdateType.Cell;

		} else if (value == PlanetOverlay.Region) {
			
			_observableUpdateTypes &= ~(CellUpdateType.Group | CellUpdateType.Territory);
			_observableUpdateTypes |= CellUpdateType.Region;

		} else if ((value == PlanetOverlay.PolityTerritory) ||
			(value == PlanetOverlay.PolityCulturalActivity) ||
			(value == PlanetOverlay.PolityCulturalDiscovery) ||
			(value == PlanetOverlay.PolityCulturalKnowledge) ||
			(value == PlanetOverlay.PolityCulturalSkill) ||
			(value == PlanetOverlay.General)) {

			_observableUpdateTypes &= ~CellUpdateType.Region;
			_observableUpdateTypes |= (CellUpdateType.Group | CellUpdateType.Territory);

		} else {

			_observableUpdateTypes &= ~(CellUpdateType.Territory | CellUpdateType.Region);
			_observableUpdateTypes |= CellUpdateType.Group;
		}
	
		_planetOverlay = value;
		_planetOverlaySubtype = planetOverlaySubtype;
	}

	public static void SetDisplayRoutes (bool value) {

		if (value)
			_observableUpdateTypes |= CellUpdateType.Route;
		else
			_observableUpdateTypes &= ~CellUpdateType.Route;
	
		_displayRoutes = value;
	}

	public static void SetDisplayGroupActivity (bool value) {

		_displayGroupActivity = value;
	}
	
	public static void SetPlanetView (PlanetView value) {
		
		_planetView = value;
	}

	public static void SetSelectedCell (int longitude, int latitude) {

		SetSelectedCell (CurrentWorld.GetCell (longitude, latitude));
	}

	public static void SetSelectedCell (WorldPosition position) {

		SetSelectedCell (CurrentWorld.GetCell (position));
	}

	public static void SetSelectedCell (TerrainCell cell) {

		if (CurrentWorld.SelectedCell != null) {

			AddUpdatedCell (CurrentWorld.SelectedCell, CellUpdateType.Cell);
		
			CurrentWorld.SelectedCell.IsSelected = false;
			CurrentWorld.SelectedCell = null;
		}

		if (CurrentWorld.SelectedRegion != null) {

			AddUpdatedCells (CurrentWorld.SelectedRegion.GetCells (), CellUpdateType.Region);

			CurrentWorld.SelectedRegion.IsSelected = false;
			CurrentWorld.SelectedRegion = null;
		}

		if (CurrentWorld.SelectedTerritory != null) {

			AddUpdatedCells (CurrentWorld.SelectedTerritory.GetCells (), CellUpdateType.Territory);

			CurrentWorld.SelectedTerritory.IsSelected = false;
			CurrentWorld.SelectedTerritory = null;
		}

		if (cell == null)
			return;

		CurrentWorld.SelectedCell = cell;
		CurrentWorld.SelectedCell.IsSelected = true;

		AddUpdatedCell (CurrentWorld.SelectedCell, CellUpdateType.Cell);

		if (cell.Region != null) {

			CurrentWorld.SelectedRegion = cell.Region;
			CurrentWorld.SelectedRegion.IsSelected = true;

			AddUpdatedCells (CurrentWorld.SelectedRegion.GetCells (), CellUpdateType.Territory);
		}

		if (cell.EncompassingTerritory != null) {

			CurrentWorld.SelectedTerritory = cell.EncompassingTerritory;
			CurrentWorld.SelectedTerritory.IsSelected = true;

			AddUpdatedCells (CurrentWorld.SelectedTerritory.GetCells (), CellUpdateType.Territory);
		}
	}

	public static void SetPolityFocus (Polity polity) {

		if (CurrentWorld.FocusedPolity != null) {

			CurrentWorld.FocusedPolity.IsFocused = false;
			CurrentWorld.FocusedPolity = null;
		}

		if (polity == null)
			return;

		polity.IsFocused = true;
		CurrentWorld.FocusedPolity = polity;
	}

	public static void ResetUpdatedCells () {

		_lastUpdatedCells.Clear ();
		_lastUpdatedCells.AddRange (UpdatedCells);
		UpdatedCells.Clear ();
	}

	public static void UpdateTextures () {

		UpdateMapTextureColors (_manager._currentMapTextureColors);
		CurrentMapTexture.SetPixels32 (_manager._currentMapTextureColors);

		CurrentMapTexture.Apply ();

		// TODO: Reenable this part for globe view
//		UpdateSphereTextureColors (_manager._currentSphereTextureColors);
//		CurrentSphereTexture.SetPixels32 (_manager._currentSphereTextureColors);
//
//		CurrentSphereTexture.Apply ();

		ResetUpdatedCells ();
	}

	public static void UpdateMapTextureColors (Color32[] textureColors) {

		foreach (TerrainCell cell in _lastUpdatedCells) {

			if (UpdatedCells.Contains (cell))
				continue;

			UpdateMapTextureColorsFromCell (textureColors, cell);
		}
		
		foreach (TerrainCell cell in UpdatedCells) {
			
			UpdateMapTextureColorsFromCell (textureColors, cell, _displayGroupActivity);
		}
	}

	public static void DisplayCellDataOnMapTexture (Color32[] textureColors, TerrainCell cell, bool showData) {

		CellGroup cellGroup = cell.Group; 

		if ((cellGroup != null) && (cellGroup.SeaMigrationRoute != null)) {

			DisplayRouteOnMapTexture (textureColors, cellGroup.SeaMigrationRoute, showData);
		}

		World world = cell.World;

		int sizeX = world.Width;

		int r = PixelToCellRatio;

		int i = cell.Longitude;
		int j = cell.Latitude;

		Color cellColor = GenerateColorFromTerrainCell(cell, _displayGroupActivity);

		if (showData) {
			cellColor = new Color (0.5f + (cellColor.r * 0.5f), 0.5f + (cellColor.g * 0.5f), 0.5f + (cellColor.b * 0.5f));
		}

		for (int m = 0; m < r; m++) {
			for (int n = 0; n < r; n++) {

				int offsetY = sizeX * r * (j * r + n);
				int offsetX = i * r + m;

				textureColors [offsetY + offsetX] = cellColor;
			}
		}

		UpdatedCells.Add (cell);
	}

	public static void DisplayRouteOnMapTexture (Color32[] textureColors, Route route, bool showRoute) {

		World world = route.World;

		int sizeX = world.Width;

		int r = PixelToCellRatio;

		foreach (TerrainCell cell in route.Cells) {

			int i = cell.Longitude;
			int j = cell.Latitude;

			Color cellColor = Color.cyan;

			if (!showRoute) {

				cellColor = GenerateColorFromTerrainCell(cell, _displayGroupActivity);
			}

			for (int m = 0; m < r; m++) {
				for (int n = 0; n < r; n++) {

					int offsetY = sizeX * r * (j * r + n);
					int offsetX = i * r + m;

					textureColors [offsetY + offsetX] = cellColor;
				}
			}

			UpdatedCells.Add (cell);
		}
	}

	public static bool CellShouldBeHighlighted (TerrainCell cell) {

		if (cell.IsSelected)
			return true;
	
		if ((_observableUpdateTypes & CellUpdateType.Region) == CellUpdateType.Region) {
		
			if ((cell.Region != null) && cell.Region.IsSelected)
				return true;
			
		} else if ((_observableUpdateTypes & CellUpdateType.Territory) == CellUpdateType.Territory) {

			if ((cell.EncompassingTerritory != null) && cell.EncompassingTerritory.IsSelected)
				return true;
		}

		return false;
	}
	
	public static void UpdateMapTextureColorsFromCell (Color32[] textureColors, TerrainCell cell, bool highlightCells = false) {

		World world = cell.World;

		int sizeX = world.Width;
		
		int r = PixelToCellRatio;
		
		int i = cell.Longitude;
		int j = cell.Latitude;
		
		Color cellColor = GenerateColorFromTerrainCell(cell, highlightCells);
		
		for (int m = 0; m < r; m++) {
			for (int n = 0; n < r; n++) {
				
				int offsetY = sizeX * r * (j*r + n);
				int offsetX = i*r + m;
				
				textureColors[offsetY + offsetX] = cellColor;
			}
		}
	}
	
	public static Texture2D GenerateMapTextureFromWorld (World world) {
		
//		UpdatedCells.Clear ();
		
		int sizeX = world.Width;
		int sizeY = world.Height;
		
		int r = PixelToCellRatio;
		
		Color32[] textureColors = new Color32[sizeX * sizeY * r * r];
		
		Texture2D texture = new Texture2D(sizeX*r, sizeY*r, TextureFormat.ARGB32, false);
		
		for (int i = 0; i < sizeX; i++) {
			for (int j = 0; j < sizeY; j++) {

				Color cellColor = GenerateColorFromTerrainCell(world.TerrainCells[i][j]);

				for (int m = 0; m < r; m++) {
					for (int n = 0; n < r; n++) {
						
						int offsetY = sizeX * r * (j*r + n);
						int offsetX = i*r + m;
						
						textureColors[offsetY + offsetX] = cellColor;
					}
				}
			}
		}
		
		texture.SetPixels32 (textureColors);

		texture.Apply();
		
		_manager._currentMapTextureColors = textureColors;
		_manager._currentMapTexture = texture;
		
		return texture;
	}

	public static void UpdateSphereTextureColors (Color32[] textureColors) {

		foreach (TerrainCell cell in UpdatedCells) {

			UpdateSphereTextureColorsFromCell (textureColors, cell);
		}
	}

	public static void UpdateSphereTextureColorsFromCell (Color32[] textureColors, TerrainCell cell) {

		World world = cell.World;
		
		int sizeX = world.Width;
		int sizeY = world.Height*2;
		
		int r = PixelToCellRatio;

		int i = cell.Longitude;
		int j = cell.Latitude;

		float factorJ = (1f - Mathf.Cos(Mathf.PI*(float)j/(float)sizeY))/2f;
		
		int trueJ = (int)(world.Height * factorJ);
		
		Color cellColor = GenerateColorFromTerrainCell(world.TerrainCells[i][trueJ]);
		
		for (int m = 0; m < r; m++) {
			for (int n = 0; n < r; n++) {
				
				int offsetY = sizeX * r * (j*r + n);
				int offsetX = i*r + m;
				
				textureColors[offsetY + offsetX] = cellColor;
			}
		}
	}
	
	public static Texture2D GenerateSphereTextureFromWorld (World world) {

//		UpdatedCells.Clear ();
		
		int sizeX = world.Width;
		int sizeY = world.Height*2;
		
		int r = PixelToCellRatio;
		
		Color32[] textureColors = new Color32[sizeX * sizeY * r * r];
		
		Texture2D texture = new Texture2D(sizeX*r, sizeY*r, TextureFormat.ARGB32, false);
		
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				float factorJ = (1f - Mathf.Cos(Mathf.PI*(float)j/(float)sizeY))/2f;

				int trueJ = (int)(world.Height * factorJ);

				Color cellColor = GenerateColorFromTerrainCell(world.TerrainCells[i][trueJ]);
				
				for (int m = 0; m < r; m++) {
					for (int n = 0; n < r; n++) {

						int offsetY = sizeX * r * (j*r + n);
						int offsetX = i*r + m;

						textureColors[offsetY + offsetX] = cellColor;
					}
				}
			}
		}

		texture.SetPixels32 (textureColors);

		texture.Apply();

		_manager._currentSphereTextureColors = textureColors;
		_manager._currentSphereTexture = texture;
		
		return texture;
	}

	private static float GetSlant (TerrainCell cell) {

		if (_manager._currentCellSlants [cell.Longitude, cell.Latitude] != null) {
		
			return _manager._currentCellSlants [cell.Longitude, cell.Latitude].Value;
		}

		Dictionary<Direction, TerrainCell> neighbors = cell.Neighbors;

		float wAltitude = 0;
		float eAltitude = 0;

		int c = 0;
		TerrainCell nCell = null;

		if (neighbors.TryGetValue (Direction.West, out nCell)) {
			
			wAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue (Direction.Southwest, out nCell)) {
			
			wAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue (Direction.South, out nCell)) {
			
			wAltitude += nCell.Altitude;
			c++;
		}

		wAltitude /= (float)c;

		c = 0;
		
		if (neighbors.TryGetValue (Direction.East, out nCell)) {
			
			eAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue (Direction.Northeast, out nCell)) {
			
			eAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue (Direction.North, out nCell)) {
			
			eAltitude += nCell.Altitude;
			c++;
		}
		
		eAltitude /= (float)c;

		float value = wAltitude - eAltitude;

		_manager._currentCellSlants [cell.Longitude, cell.Latitude] = value;
	
		return value;
	}
	
	private static bool IsCoastSea (TerrainCell cell) {

		if (cell.Altitude <= 0)
			return false;

		return cell.IsPartOfCoastline;
	}
	
	private static bool IsCoastLand (TerrainCell cell) {
		
		if (cell.Altitude > 0)
			return false;
		
		return cell.IsPartOfCoastline;
	}
	
	private static Color GenerateColorFromTerrainCell (TerrainCell cell, bool highlightCells = false) {

		if (_displayRoutes && cell.HasCrossingRoutes) {
		
			return Color.magenta;
		}

		Color color = Color.black;

		switch (_planetView) {
			
		case PlanetView.Biomes:
			color = GenerateBiomeColor (cell);
			break;
			
		case PlanetView.Elevation:
			color = GenerateAltitudeContourColor (cell.Altitude);
			break;
			
		case PlanetView.Coastlines:
			color = GenerateCoastlineColor (cell);
			break;

		default:
			throw new System.Exception ("Unsupported Planet View Type");
		}

		switch (_planetOverlay) {
		
		case PlanetOverlay.None:
			break;

		case PlanetOverlay.General:
			color = SetGeneralOverlayColor (cell, color);
			break;

		case PlanetOverlay.PopDensity:
			color = SetPopulationDensityOverlayColor (cell, color);
			break;

		case PlanetOverlay.FarmlandDistribution:
			color = SetFarmlandOverlayColor (cell, color);
			break;

		case PlanetOverlay.PopCulturalActivity:
			color = SetPopCulturalActivityOverlayColor (cell, color);
			break;
			
		case PlanetOverlay.PopCulturalSkill:
			color = SetPopCulturalSkillOverlayColor (cell, color);
			break;
			
		case PlanetOverlay.PopCulturalKnowledge:
			color = SetPopCulturalKnowledgeOverlayColor (cell, color);
			break;

		case PlanetOverlay.PopCulturalDiscovery:
			color = SetPopCulturalDiscoveryOverlayColor (cell, color);
			break;

		case PlanetOverlay.PolityTerritory:
			color = SetPolityTerritoryOverlayColor (cell, color);
			break;

		case PlanetOverlay.PolityCoreDistance:
			color = SetPolityCoreDistanceOverlayColor (cell, color);
			break;

		case PlanetOverlay.PolityInfluence:
			color = SetPolityInfluenceOverlayColor (cell, color);
			break;

		case PlanetOverlay.PolityCulturalActivity:
			color = SetPolityCulturalActivityOverlayColor (cell, color);
			break;

		case PlanetOverlay.PolityCulturalSkill:
			color = SetPolityCulturalSkillOverlayColor (cell, color);
			break;

		case PlanetOverlay.PolityCulturalKnowledge:
			color = SetPolityCulturalKnowledgeOverlayColor (cell, color);
			break;

		case PlanetOverlay.PolityCulturalDiscovery:
			color = SetPolityCulturalDiscoveryOverlayColor (cell, color);
			break;

		case PlanetOverlay.Temperature:
			color = SetTemperatureOverlayColor (cell, color);
			break;

		case PlanetOverlay.Rainfall:
			color = SetRainfallOverlayColor (cell, color);
			break;

		case PlanetOverlay.Arability:
			color = SetArabilityOverlayColor (cell, color);
			break;

		case PlanetOverlay.Region:
			color = SetRegionOverlayColor (cell, color);
			break;

		case PlanetOverlay.Language:
			color = SetLanguageOverlayColor (cell, color);
			break;

		case PlanetOverlay.PopChange:
			color = SetPopulationChangeOverlayColor (cell, color);
			break;

		case PlanetOverlay.UpdateSpan:
			color = SetUpdateSpanOverlayColor (cell, color);
			break;
			
		default:
			throw new System.Exception("Unsupported Planet Overlay Type");
		}

		if (CellShouldBeHighlighted (cell)) {
			color = color * 0.5f + Color.white * 0.5f;
		} else if (highlightCells) {
			color = color * 0.75f + Color.white * 0.25f;
		}

		color.a = 1;
		
		return color;
	}
	
	private static Color GenerateCoastlineColor (TerrainCell cell) {
		
		if (_mapPalette.Count == 0) {
			
			return Color.black;
		}
		
		if (IsCoastSea (cell)) {
			
			return _mapPalette[2];
		}
		
		if (IsCoastLand (cell)) {
			
			return _mapPalette[3];
		}

		if (cell.Altitude > 0) {

			float slant = GetSlant (cell);
			float altDiff = CurrentWorld.MaxAltitude - CurrentWorld.MinAltitude;

			float slantFactor = Mathf.Min(1, -(20 * slant / altDiff));
			
			if (slantFactor > 0.1f) {

				return (_mapPalette[4] * slantFactor) + (_mapPalette[1] * (1 - slantFactor));
			}

			return _mapPalette[1];
		}

		return _mapPalette[0];
	}

	private static Color GenerateAltitudeColor (float altitude) {
		
		float value;
		
		if (altitude < 0) {
			
			value = (2 - altitude / World.MinPossibleAltitude - Manager.SeaLevelOffset) / 2f;

			Color color1 = Color.blue;
			
			return new Color(color1.r * value, color1.g * value, color1.b * value);
		}
		
		value = (1 + altitude / (World.MaxPossibleAltitude - Manager.SeaLevelOffset)) / 2f;
		
		Color color2 = new Color(1f, 0.6f, 0);
		
		return new Color(color2.r * value, color2.g * value, color2.b * value);
	}
	
	private static Color GenerateBiomeColor (TerrainCell cell) {

		float slant = GetSlant (cell);
		float altDiff = CurrentWorld.MaxAltitude - CurrentWorld.MinAltitude;

		float slantFactor = Mathf.Min (1f, (4f + (10f * slant / altDiff)) / 5f);

		float altitudeFactor = Mathf.Min (1f, (0.5f + ((cell.Altitude - CurrentWorld.MinAltitude) / altDiff)) / 1.5f);
		
		Color color = Color.black;
		
		if (_biomePalette.Count == 0) {
			
			return color;
		}
		
		for (int i = 0; i < cell.PresentBiomeNames.Count; i++) {

			string biomeName = cell.PresentBiomeNames[i];

			Biome biome = Biome.Biomes[biomeName];
			
			Color biomeColor = _biomePalette[biome.ColorId];
			float biomePresence = cell.BiomePresences[i];
			
			color.r += biomeColor.r * biomePresence;
			color.g += biomeColor.g * biomePresence;
			color.b += biomeColor.b * biomePresence;
		}
		
		return color * slantFactor * altitudeFactor;
	}

	private static bool IsRegionBorder (Region region, TerrainCell cell) {

		return region.IsInnerBorderCell (cell);
	}

	private static Color SetRegionOverlayColor (TerrainCell cell, Color color) {

		Color biomeColor = color;

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		Region region = cell.Region;

		if (region != null) {

			Color regionIdColor = GenerateColorFromId (region.Id);

			Color regionColor = (biomeColor * 0.85f) + (regionIdColor * 0.15f);

			bool isRegionBorder = IsRegionBorder (region, cell);

			if (!isRegionBorder) {
				regionColor /= 1.5f;
			}

			color.r = regionColor.r;
			color.g = regionColor.g;
			color.b = regionColor.b;
		}

		return color;
	}

	private static bool IsLanguageBorder (Language language, TerrainCell cell) {

		foreach (TerrainCell nCell in cell.Neighbors.Values) {

			if (nCell.Group == null)
				return true;

			Language nLanguage = nCell.Group.Culture.Language;

			if (nLanguage == null)
				return true;

			if (nLanguage.Id != language.Id)
				return true;
		}

		return false;
	}

	private static Color SetLanguageOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.GetBiomePresence (Biome.Ocean) >= 1f) {

			return color;
		}

		if (cell.Group != null) {

			color.r += 1.5f / 9f;
			color.g += 1.5f / 9f;
			color.b += 1.5f / 9f;

			Language groupLanguage = cell.Group.Culture.Language;

			if (groupLanguage != null) {

				Color languageColor = GenerateColorFromId (groupLanguage.Id);

				bool isLanguageBorder = IsLanguageBorder (groupLanguage, cell);

				if (!isLanguageBorder) {
					languageColor /= 2f;
				}

				color.r = languageColor.r;
				color.g = languageColor.g;
				color.b = languageColor.b;
			}
		}

		return color;
	}

	private static bool IsTerritoryBorder (Territory territory, TerrainCell cell) {

		return territory.IsPartOfBorder (cell);
	}

	private static Color SetPolityTerritoryOverlayColor (TerrainCell cell, Color color) {
		
		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.GetBiomePresence (Biome.Ocean) >= 1f) {

			return color;
		}

		if (cell.EncompassingTerritory != null) {

			Polity territoryPolity = cell.EncompassingTerritory.Polity;

			Color territoryColor = GenerateColorFromId (territoryPolity.Id);

			bool isTerritoryBorder = IsTerritoryBorder (cell.EncompassingTerritory, cell);
			bool isCoreGroup = territoryPolity.CoreGroup == cell.Group;

			if (!isCoreGroup) {
				if (!isTerritoryBorder) {
					territoryColor /= 2.5f;
				} else {
					territoryColor /= 1.75f;
				}
			}

			color.r = territoryColor.r;
			color.g = territoryColor.g;
			color.b = territoryColor.b;

		} else if (cell.Group != null) {

			color.r += 1.5f / 9f;
			color.g += 1.5f / 9f;
			color.b += 1.5f / 9f;
		}

		return color;
	}

	private static Color SetPolityCoreDistanceOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.GetBiomePresence (Biome.Ocean) >= 1f) {

			return color;
		}

		if (cell.Group != null) {
			if (cell.EncompassingTerritory != null) {

				Polity territoryPolity = cell.EncompassingTerritory.Polity;

				Color territoryColor = GenerateColorFromId (territoryPolity.Id);

				PolityInfluence pi = cell.Group.GetPolityInfluence (territoryPolity);

				float distanceFactor = Mathf.Sqrt (pi.CoreDistance);
				distanceFactor = 1 - 0.9f * Mathf.Min (1, distanceFactor / 50f);

				color.r = territoryColor.r * distanceFactor;
				color.g = territoryColor.g * distanceFactor;
				color.b = territoryColor.b * distanceFactor;

			} else {

				color.r += 1.5f / 9f;
				color.g += 1.5f / 9f;
				color.b += 1.5f / 9f;
			}
		}

		return color;
	}

	private static Color SetPolityInfluenceOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.GetBiomePresence (Biome.Ocean) >= 1f) {

			return color;
		}

		if (cell.Group != null) {

			int polityCount = 0;
			float totalInfluenceValueFactor = 0;

			Color mixedPolityColor = Color.black;
			foreach (PolityInfluence p in cell.Group.GetPolityInfluences ()) {

				polityCount++;

				float influenceValueFactor = 0.2f + p.Value;

				Color polityColor = GenerateColorFromId (p.PolityId);
				polityColor *= influenceValueFactor;
				totalInfluenceValueFactor += 1.2f;

				mixedPolityColor += polityColor;
			}

			color.r += 1.5f / 9f;
			color.g += 1.5f / 9f;
			color.b += 1.5f / 9f;

			if (polityCount > 0) {

				mixedPolityColor /= totalInfluenceValueFactor;

				color.r += mixedPolityColor.r * (1 - color.r);
				color.g += mixedPolityColor.g * (1 - color.g);
				color.b += mixedPolityColor.b * (1 - color.b);

			}
		}

		return color;
	}

	private static Color GenerateColorFromId (long id) {
	
		long primaryColor = id % 3;
		float secondaryColorIntensity = ((id / 3) % 4) / 3f;
		float tertiaryColorIntensity = (id / 12) % 2 / 2f;
		long secondaryColor = (id / 24) % 2;

		float red = 0;
		float green = 0;
		float blue = 0;

		switch (primaryColor) {
		case 0:
			red = 1;

			if (secondaryColor == 0) {
				green = secondaryColorIntensity;
				blue = tertiaryColorIntensity;
			} else {
				blue = secondaryColorIntensity;
				green = tertiaryColorIntensity;
			}

			break;
		case 1:
			green = 1;

			if (secondaryColor == 0) {
				blue = secondaryColorIntensity;
				red = tertiaryColorIntensity;
			} else {
				red = secondaryColorIntensity;
				blue = tertiaryColorIntensity;
			}

			break;
		case 2:
			blue = 1;

			if (secondaryColor == 0) {
				red = secondaryColorIntensity;
				green = tertiaryColorIntensity;
			} else {
				green = secondaryColorIntensity;
				red = tertiaryColorIntensity;
			}

			break;
		}

		return new Color (red, green, blue);
	}

	private static Color SetPopulationChangeOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		float deltaLimitFactor = 0.02f;

		float prevPopulation = 0;
		float population = 0;

		float delta = 0;

		if (cell.Group != null) {

			prevPopulation = cell.Group.PreviousPopulation;
			population = cell.Group.Population;

			delta = population - prevPopulation;
		}

		if (delta > 0) {

			float value = delta / (population * deltaLimitFactor);
			value = Mathf.Clamp01 (value);

			color = (color * (1 - value)) + (Color.green * value);
		} else if (delta < 0) {

			float value = -delta / (prevPopulation * deltaLimitFactor);
			value = Mathf.Clamp01 (value);

			color = (color * (1 - value)) + (Color.red * value);
		}

		return color;
	}
	
	private static Color SetPopulationDensityOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		if (CurrentWorld.MostPopulousGroup == null)
			return color;

		int maxPopulation = CurrentWorld.MostPopulousGroup.Population;

		if (maxPopulation <= 0)
			return color;

		float areaFactor = cell.Area/TerrainCell.MaxArea;
		
		float maxPopFactor = areaFactor * maxPopulation / 5f;

		float population = 0;

		if (cell.Group != null) {

			population = cell.Group.Population;
		
			if (cell.Group.MigrationTagged && DisplayMigrationTaggedGroup)
				return Color.green;

			if (cell.Group.DebugTagged && DisplayDebugTaggedGroups)
				return Color.green;
		}

		if (population > 0) {

			float value = (population + maxPopFactor) / (maxPopulation + maxPopFactor);
			
			color = (color * (1 - value)) + (Color.red * value);
		}

		return color;
	}

	private static Color SetPopCulturalActivityOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		if (_planetOverlaySubtype == "None")
			return color;

		float activityContribution = 0;
		float population = 0;

		if (cell.Group != null) {

			CellCulturalActivity activity = cell.Group.Culture.GetActivity(_planetOverlaySubtype) as CellCulturalActivity;

			population = cell.Group.Population;

			if (activity != null) {
				activityContribution = activity.Contribution;
			}
		}

		if ((population > 0) && (activityContribution >= 0.001)) {

			float value = 0.05f + 0.95f * activityContribution;

			color = (color * (1 - value)) + (Color.cyan * value);
		}

		return color;
	}

	private static Color SetPolityCulturalActivityOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.GetBiomePresence (Biome.Ocean) >= 1f)
			return color;

		if (cell.Group == null)
			return color;

		color.r += 1.5f / 9f;
		color.g += 1.5f / 9f;
		color.b += 1.5f / 9f;

		if (_planetOverlaySubtype == "None")
			return color;

		Territory territory = cell.EncompassingTerritory;

		if (territory == null)
			return color;

		CulturalActivity activity = territory.Polity.Culture.GetActivity(_planetOverlaySubtype);

		if (activity == null)
			return color;

		float activityContribution = 0;

		activityContribution = activity.Contribution;

		if (activityContribution < 0.001f)
			return color;

		float value = 0.05f + 0.95f * activityContribution;

		Color addedColor = Color.cyan;

		if (IsTerritoryBorder (territory, cell)) {

			// A slightly bluer shade of cyan
			addedColor = new Color (0, 0.75f, 1.0f);
		}

		color = (color * (1 - value)) + (addedColor * value);

		return color;
	}
	
	private static Color SetPopCulturalSkillOverlayColor (TerrainCell cell, Color color) {
		
		float greyscale = (color.r + color.g + color.b);
		
		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		if (_planetOverlaySubtype == "None")
			return color;

		float skillValue = 0;
		float population = 0;

		if (cell.Group != null) {

			CellCulturalSkill skill = cell.Group.Culture.GetSkill(_planetOverlaySubtype) as CellCulturalSkill;
			
			population = cell.Group.Population;

			if (skill != null) {
				skillValue = skill.Value;
			}
		}
		
		if ((population > 0) && (skillValue >= 0.001)) {
			
			float value = 0.05f + 0.95f * skillValue;
			
			color = (color * (1 - value)) + (Color.cyan * value);
		}
		
		return color;
	}

	private static Color SetPolityCulturalSkillOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.Group == null)
			return color;

		color.r += 1.5f / 9f;
		color.g += 1.5f / 9f;
		color.b += 1.5f / 9f;

		if (_planetOverlaySubtype == "None")
			return color;

		Territory territory = cell.EncompassingTerritory;

		if (territory == null)
			return color;

		CulturalSkill skill = territory.Polity.Culture.GetSkill(_planetOverlaySubtype);

		if (skill == null)
			return color;

		float skillValue = 0;

		skillValue = skill.Value;

		if (skillValue < 0.001)
			return color;

		float value = 0.05f + 0.95f * skillValue;

		Color addedColor = Color.cyan;

		if (IsTerritoryBorder (territory, cell)) {

			// A slightly bluer shade of cyan
			addedColor = new Color (0, 0.75f, 1.0f);
		}

		color = (color * (1 - value)) + (addedColor * value);

		return color;
	}
	
	private static Color SetPopCulturalKnowledgeOverlayColor (TerrainCell cell, Color color) {
		
		float greyscale = (color.r + color.g + color.b);
		
		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;
		
		if (_planetOverlaySubtype == "None")
			return color;
		
		float normalizedValue = 0;
		float population = 0;
		
		if (cell.Group != null) {
			
			CellCulturalKnowledge knowledge = cell.Group.Culture.GetKnowledge(_planetOverlaySubtype) as CellCulturalKnowledge;
			
			population = cell.Group.Population;
			
			if (knowledge != null) {
				
				float highestAsymptote = knowledge.GetHighestAsymptote ();
				
				if (highestAsymptote <= 0)
					throw new System.Exception ("Highest Asymptote is less or equal to 0");

				normalizedValue = knowledge.Value / highestAsymptote;
			}
		}
		
		if ((population > 0) && (normalizedValue >= 0.001f)) {
			
			float value = 0.05f + 0.95f * normalizedValue;
			
			color = (color * (1 - value)) + (Color.cyan * value);
		}
		
		return color;
	}

	private static Color SetPolityCulturalKnowledgeOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.GetBiomePresence (Biome.Ocean) >= 1f)
			return color;

		if (cell.Group == null)
			return color;

		color.r += 1.5f / 9f;
		color.g += 1.5f / 9f;
		color.b += 1.5f / 9f;

		if (_planetOverlaySubtype == "None")
			return color;

		Territory territory = cell.EncompassingTerritory;

		if (territory == null)
			return color;

		CulturalKnowledge knowledge = territory.Polity.Culture.GetKnowledge(_planetOverlaySubtype);

		CellCulturalKnowledge cellKnowledge = territory.Polity.CoreGroup.Culture.GetKnowledge(_planetOverlaySubtype) as CellCulturalKnowledge;

		if (knowledge == null)
			return color;
		
		if (cellKnowledge == null)
			return color;

		float normalizedValue = 0;

		float highestAsymptote = cellKnowledge.GetHighestAsymptote ();

		if (highestAsymptote <= 0)
			throw new System.Exception ("Highest Asymptote is less or equal to 0");

		normalizedValue = knowledge.Value / highestAsymptote;

		if (normalizedValue < 0.001)
			return color;

		float value = 0.05f + 0.95f * normalizedValue;

		Color addedColor = Color.cyan;

		if (IsTerritoryBorder (territory, cell)) {

			// A slightly bluer shade of cyan
			addedColor = new Color (0, 0.75f, 1.0f);
		}

		color = (color * (1 - value)) + (addedColor * value);

		return color;
	}

	private static Color SetPopCulturalDiscoveryOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		if (_planetOverlaySubtype == "None")
			return color;

		float normalizedValue = 0;
		float population = 0;

		if (cell.Group != null) {

			CulturalDiscovery discovery = cell.Group.Culture.GetDiscovery(_planetOverlaySubtype);

			population = cell.Group.Population;

			if (discovery != null) {

				normalizedValue = 1;
			}
		}

		if ((population > 0) && (normalizedValue >= 0.001f)) {

			color = (color * (1 - normalizedValue)) + (Color.cyan * normalizedValue);
		}

		return color;
	}

	private static Color SetPolityCulturalDiscoveryOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 9f;
		color.g = (greyscale + color.g) / 9f;
		color.b = (greyscale + color.b) / 9f;

		if (cell.GetBiomePresence (Biome.Ocean) >= 1f)
			return color;

		if (cell.Group == null)
			return color;

		color.r += 1.5f / 9f;
		color.g += 1.5f / 9f;
		color.b += 1.5f / 9f;

		if (_planetOverlaySubtype == "None")
			return color;

		Territory territory = cell.EncompassingTerritory;

		if (territory == null)
			return color;

		CulturalDiscovery discovery = territory.Polity.Culture.GetDiscovery(_planetOverlaySubtype);

		if (discovery == null)
			return color;

		Color addedColor = Color.cyan;

		if (IsTerritoryBorder (territory, cell)) {

			// A slightly bluer shade of cyan
			addedColor = new Color (0, 0.75f, 1.0f);
		}

		float normalizedValue = 1;

		color = (color * (1 - normalizedValue)) + (addedColor * normalizedValue);

		return color;
	}

	private static Color GetOverlayColor (OverlayColorId id) {
	
		return _overlayPalette [(int)id];
	}

	private static Color SetArabilityOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		float normalizedValue = cell.Arability;

		if (normalizedValue >= 0.001f) {

			float value = 0.05f + 0.95f * normalizedValue;

			color = (color * (1 - value)) + (GetOverlayColor(OverlayColorId.Arability) * value);
		}

		return color;
	}

	private static Color SetFarmlandOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		float normalizedValue = cell.FarmlandPercentage;

		if (normalizedValue >= 0.001f) {

			float value = 0.05f + 0.95f * normalizedValue;

			color = (color * (1 - value)) + (GetOverlayColor(OverlayColorId.Farmland) * value);
		}

		return color;
	}

	private static Color SetGeneralOverlayColor (TerrainCell cell, Color color) {

		float normalizedValue = 0;
		float population = 0;

		bool hasGroup = false;
		bool inTerritory = false;
		Color densityColorSubOptimal = GetOverlayColor(OverlayColorId.GeneralDensitySubOptimal);
		Color densityColor = GetOverlayColor(OverlayColorId.GeneralDensityOptimal);

		if (cell.EncompassingTerritory != null) {

			inTerritory = true;

			Polity territoryPolity = cell.EncompassingTerritory.Polity;

			densityColor = GenerateColorFromId (territoryPolity.Id);

			bool isTerritoryBorder = IsTerritoryBorder (cell.EncompassingTerritory, cell);
			bool isCoreGroup = territoryPolity.CoreGroup == cell.Group;

			if (!isCoreGroup) {
				if (!isTerritoryBorder) {
					densityColor /= 2.5f;
				} else {
					densityColor /= 1.75f;
				}
			}

			densityColor = Color.white * 0.15f + densityColor * 0.85f;

			normalizedValue = 0.3f;
		}

		if (cell.Group != null) {

			hasGroup = true;

			int maxPopulation = CurrentWorld.MostPopulousGroup.Population;

			population = cell.Group.Population;

			float maxPopFactor = 0.3f + 0.4f * (population / (float)maxPopulation);

			normalizedValue = maxPopFactor;

			///

			if (!inTerritory) {
				CellCulturalKnowledge knowledge = cell.Group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId) as CellCulturalKnowledge;

				if (knowledge != null) {

					float minValue = SocialOrganizationKnowledge.MinValueForTribalism;
					float startValue = SocialOrganizationKnowledge.StartValue;

					float knowledgeFactor = Mathf.Min (1f, (knowledge.Value - startValue) / (minValue - startValue));

					Color softDensityColor = densityColor * color;

					densityColor = (softDensityColor * knowledgeFactor) + (densityColorSubOptimal * (1f - knowledgeFactor));

				} else {
				
					densityColor = densityColorSubOptimal;
				}
			}
		}

		if (hasGroup || inTerritory) {

			float greyscale = (color.r + color.g + color.b) / 3f;

			color.r = (color.r + greyscale) / 2f;
			color.g = (color.g + greyscale) / 2f;
			color.b = (color.b + greyscale) / 2f;

			float value = normalizedValue;

			color = (color * (1 - value)) + (densityColor * value);
		}

		return color;
	}

	private static Color SetUpdateSpanOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		float normalizedValue = 0;
		float population = 0;

		if (cell.Group != null) {

			population = cell.Group.Population;

			int lastUpdateDate = cell.Group.LastUpdateDate;
			int nextUpdateDate = cell.Group.NextUpdateDate;
			int updateSpan = nextUpdateDate - lastUpdateDate;

			if (_manager._currentMaxUpdateSpan < updateSpan)
				_manager._currentMaxUpdateSpan = updateSpan;

			float maxUpdateSpan = Mathf.Min (_manager._currentMaxUpdateSpan, 10000);

			normalizedValue = 1f - (float)updateSpan / maxUpdateSpan;

			normalizedValue = Mathf.Clamp01 (normalizedValue);
		}

		if ((population > 0) && (normalizedValue > 0)) {

			float value = normalizedValue;

			color = (color * (1 - value)) + (Color.red * value);
		}

		return color;
	}

	private static Color SetMiscellanousDataOverlayColor (TerrainCell cell, Color color) {

		switch (_planetOverlaySubtype) {

		case "Population":
			return SetPopulationDensityOverlayColor (cell, color);

		case "PopulationChange":
			return SetPopulationChangeOverlayColor (cell, color);

		case "Political":
			return SetPolityTerritoryOverlayColor (cell, color);

		case "PolityInfluences":
			return SetPolityInfluenceOverlayColor (cell, color);

		case "Rainfall":
			return SetRainfallOverlayColor (cell, color);

		case "Temperature":
			return SetTemperatureOverlayColor (cell, color);

		case "Arability":
			return SetArabilityOverlayColor (cell, color);

		case "Farmland":
			return SetFarmlandOverlayColor (cell, color);

		case "UpdateSpan":
			return SetUpdateSpanOverlayColor (cell, color);

		case "None":
			return color;

		default:
			throw new System.Exception ("Unhandled miscellaneous data overlay subtype: " + _planetOverlaySubtype);
		}
	}
	
	private static Color SetTemperatureOverlayColor (TerrainCell cell, Color color) {
		
		float greyscale = (color.r + color.g + color.b);// * 4 / 3;
		
		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		Color addColor;
		
		float span = World.MaxPossibleTemperature - World.MinPossibleTemperature;
		
		float value = (cell.Temperature - (World.MinPossibleTemperature + Manager.TemperatureOffset)) / span;
		
		addColor = new Color(value, 0, 1f - value);
		
		color += addColor * 2f / 3f;
		
		return color;
	}
	
	private static Color SetRainfallOverlayColor (TerrainCell cell, Color color) {
		
		float greyscale = (color.r + color.g + color.b);// * 4 / 3;
		
		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;
		
		Color addColor = Color.black;
		
		if (cell.Rainfall > 0) {
			
			float value = cell.Rainfall / World.MaxPossibleRainfall + Manager.RainfallOffset;
			
			addColor = Color.green;
			
			addColor = new Color (addColor.r * value, addColor.g * value, addColor.b * value);
		}

		color += addColor * 2f / 3f;
		
		return color;
	}
	
	private static Color GenerateAltitudeContourColor (float altitude) {

		Color color = new Color(1, 0.6f, 0);
		
		float shadeValue = 1.0f;
		
		float value = Mathf.Max(0, altitude / CurrentWorld.MaxAltitude);
		
		if (altitude < 0) {
			
			value = Mathf.Max(0, (1f - altitude / CurrentWorld.MinAltitude));
			
			color = Color.blue;
		}

		while (shadeValue > value)
		{
			shadeValue -= 0.15f;
		}

		shadeValue = 0.5f * shadeValue + 0.5f;

		color = new Color(color.r * shadeValue, color.g * shadeValue, color.b * shadeValue);
		
		return color;
	}
	
	private static Color GenerateRainfallContourColor (float rainfall) {
		
		float value;
		
		Color color = Color.green;
		
		float shadeValue = 1.0f;
		
		value = Mathf.Max(0, rainfall / CurrentWorld.MaxRainfall);
		
		while (shadeValue > value)
		{
			shadeValue -= 0.1f;
		}
		
		color = new Color(color.r * shadeValue, color.g * shadeValue, color.b * shadeValue);
		
		return color;
	}
	
	private static Color GenerateTemperatureContourColor (float temperature) {
		
		float span = CurrentWorld.MaxTemperature - CurrentWorld.MinTemperature;
		
		float value;
		
		float shadeValue = 1f;
		
		value = (temperature - CurrentWorld.MinTemperature) / span;
		
		while (shadeValue > value)
		{
			shadeValue -= 0.1f;
		}
		
		Color color = new Color(shadeValue, 0, 1f - shadeValue);
		
		return color;
	}

	private static XmlAttributeOverrides GenerateAttributeOverrides () {
		
		XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();

		return attrOverrides;
	}
}
