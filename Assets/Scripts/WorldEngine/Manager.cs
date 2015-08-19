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
	Temperature,
	Rainfall,
	Population
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

public class Manager {

	public const int WorldWidth = 400;
	public const int WorldHeight = 200;
	
	public static Thread MainThread { get; private set; }
	
	public static string SavePath { get; private set; }
	public static string ExportPath { get; private set; }
	
	public static string WorldName { get; set; }

	private static Manager _manager = new Manager();

	private static PlanetView _planetView = PlanetView.Biomes;
	private static PlanetOverlay _planetOverlay = PlanetOverlay.None;
	
	private static List<Color> _biomePalette = new List<Color>();
	private static List<Color> _mapPalette = new List<Color>();
	
	private ProgressCastDelegate _progressCastMethod = null;
	
	private World _currentWorld = null;
	
	private Texture2D _currentSphereTexture = null;
	private Texture2D _currentMapTexture = null;

	private Queue<IManagerTask> _taskQueue = new Queue<IManagerTask>();

	private bool _worldReady = false;

	private int _cellLoadCount = 0;
	private int _cellsToLoad = 0;

	public XmlAttributeOverrides AttributeOverrides { get; private set; }

	public static bool WorldReady {

		get {
			return _manager._worldReady;
		}
	}
	
	public static void UpdateMainThreadReference () {
		
		MainThread = Thread.CurrentThread;
	}

	private Manager () {

		InitializeSavePath ();
		InitializeExportPath ();

		AttributeOverrides = GenerateAttributeOverrides ();
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
		
		_manager._worldReady = false;
		
		_manager._progressCastMethod = progressCastMethod;
		
		ThreadPool.QueueUserWorkItem (state => {
			
			ExportMapTextureToFile (path, uvRect);
			
			_manager._worldReady = true;
		});
	}
	
	public static void RefreshTextures () { 

		GenerateSphereTextureFromWorld(CurrentWorld);
		GenerateMapTextureFromWorld(CurrentWorld);
	}

	public static void GenerateNewWorld (int seed) {

		//ManagerTask<int> seed = Manager.EnqueueTask (() => Random.Range (0, int.MaxValue));

		World world = new World(WorldWidth, WorldHeight, seed);
		
		if (_manager._progressCastMethod != null)
			world.ProgressCastMethod = _manager._progressCastMethod;

		world.Initialize ();
		world.Generate ();
		world.FinalizeGeneration ();

		_manager._currentWorld = world;
	}
	
	public static void GenerateNewWorldAsync (int seed, ProgressCastDelegate progressCastMethod = null) {

		_manager._worldReady = false;
		
		_manager._progressCastMethod = progressCastMethod;

		ThreadPool.QueueUserWorkItem (state => {
			
			GenerateNewWorld (seed);
			
			_manager._worldReady = true;
		});
	}
	
	public static void SaveWorld (string path) {

		XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);
		FileStream stream = new FileStream(path, FileMode.Create);

		serializer.Serialize(stream, _manager._currentWorld);

		stream.Close();
	}
	
	public static void SaveWorldAsync (string path, ProgressCastDelegate progressCastMethod = null) {
		
		_manager._worldReady = false;
		
		_manager._progressCastMethod = progressCastMethod;
		
		ThreadPool.QueueUserWorkItem (state => {
			
			SaveWorld (path);
			
			_manager._worldReady = true;
		});
	}
	
	public static void LoadWorld (string path) {
		
		ResetWorldLoadTrack ();

		XmlSerializer serializer = new XmlSerializer(typeof(World), _manager.AttributeOverrides);
		FileStream stream = new FileStream(path, FileMode.Open);

		_manager._currentWorld = serializer.Deserialize(stream) as World;

		_manager._currentWorld.FinalizeLoad ();

		stream.Close();
	}
	
	public static void LoadWorldAsync (string path, ProgressCastDelegate progressCastMethod = null) {
		
		_manager._worldReady = false;
		
		_manager._progressCastMethod = progressCastMethod;
		
		ThreadPool.QueueUserWorkItem (state => {
			
			LoadWorld (path);
			
			_manager._worldReady = true;
		});
	}

	public static void ResetWorldLoadTrack () {
		
		_manager._cellLoadCount = 0;
		_manager._cellsToLoad = WorldWidth*WorldHeight;
	}
	
	public static void UpdateWorldLoadTrack () {
		
		_manager._cellLoadCount += 1;

		float value = _manager._cellLoadCount / (float)_manager._cellsToLoad;
		
		if (_manager._progressCastMethod == null)
			return;

		_manager._progressCastMethod (Mathf.Min(1, value));
	}

	public static void SetPlanetOverlay (PlanetOverlay value) {
	
		_planetOverlay = value;
	}
	
	public static void SetPlanetView (PlanetView value) {
		
		_planetView = value;
	}
	
	public static Texture2D GenerateMapTextureFromWorld (World world) {
		
		int sizeX = world.Width;
		int sizeY = world.Height;
		
		int r = 4;
		
		Texture2D texture = new Texture2D(sizeX*r, sizeY*r, TextureFormat.ARGB32, false);
		
		for (int i = 0; i < sizeX; i++) {
			for (int j = 0; j < sizeY; j++) {
//				if (((i % 20) == 0) || ((j % 20) == 0)) {
//
//					texture.SetPixel(i, j, Color.black);
//
//					continue;
//				}

				Color cellColor = GenerateColorFromTerrainCell(world.Terrain[i][j]);

				for (int m = 0; m < r; m++) {
					for (int n = 0; n < r; n++) {

						texture.SetPixel(i*r + m, j*r + n, cellColor);
					}
				}
			}
		}
		
		texture.Apply();

		_manager._currentMapTexture = texture;
		
		return texture;
	}
	
	public static Texture2D GenerateSphereTextureFromWorld (World world) {
		
		int sizeX = world.Width;
		int sizeY = world.Height*2;
		
		int r = 4;
		
		Texture2D texture = new Texture2D(sizeX*r, sizeY*r, TextureFormat.ARGB32, false);
		
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				float factorJ = (1f - Mathf.Cos(Mathf.PI*(float)j/(float)sizeY))/2f;

				int trueJ = (int)(world.Height * factorJ);

//				if (((i % 20) == 0) || (((int)trueJ % 20) == 0)) {
//					
//					texture.SetPixel(i, j, Color.black);
//					
//					continue;
//				}

				Color cellColor = GenerateColorFromTerrainCell(world.Terrain[i][trueJ]);
				
				for (int m = 0; m < r; m++) {
					for (int n = 0; n < r; n++) {
						
						texture.SetPixel(i*r + m, j*r + n, cellColor);
					}
				}
			}
		}
		
		texture.Apply();
		
		_manager._currentSphereTexture = texture;
		
		return texture;
	}
	
	private static Dictionary<string, TerrainCell> GetNeighborCells (TerrainCell cell) {

		return GetNeighborCells (cell.World, cell.Longitude, cell.Latitude);
	}

	private static Dictionary<string, TerrainCell> GetNeighborCells (World world, int longitude, int latitude) {
	
		Dictionary<string, TerrainCell> neighbors = new Dictionary<string, TerrainCell> ();

		int wLongitude = (world.Width + longitude - 1) % world.Width;
		int eLongitude = (longitude + 1) % world.Width;

		if (latitude < (world.Height - 1)) {
			
			neighbors.Add("northwest", world.Terrain[wLongitude][latitude + 1]);
			neighbors.Add("north", world.Terrain[longitude][latitude + 1]);
			neighbors.Add("northeast", world.Terrain[eLongitude][latitude + 1]);
		}
		
		neighbors.Add("west", world.Terrain[wLongitude][latitude]);
		neighbors.Add("east", world.Terrain[eLongitude][latitude]);
		
		if (latitude > 0) {
			
			neighbors.Add("southwest", world.Terrain[wLongitude][latitude - 1]);
			neighbors.Add("south", world.Terrain[longitude][latitude - 1]);
			neighbors.Add("southeast", world.Terrain[eLongitude][latitude - 1]);
		}
		
		return neighbors;
	}

	private static float GetSlant (TerrainCell cell) {

		Dictionary<string, TerrainCell> neighbors = GetNeighborCells (cell);

		float wAltitude = 0;
		float eAltitude = 0;

		int c = 0;
		TerrainCell nCell = null;

		if (neighbors.TryGetValue ("west", out nCell)) {
			
			wAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue ("southwest", out nCell)) {
			
			wAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue ("south", out nCell)) {
			
			wAltitude += nCell.Altitude;
			c++;
		}

		wAltitude /= (float)c;

		c = 0;
		
		if (neighbors.TryGetValue ("east", out nCell)) {
			
			eAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue ("northeast", out nCell)) {
			
			eAltitude += nCell.Altitude;
			c++;
		}
		
		if (neighbors.TryGetValue ("north", out nCell)) {
			
			eAltitude += nCell.Altitude;
			c++;
		}
		
		eAltitude /= (float)c;
	
		return wAltitude - eAltitude;
	}
	
	private static bool IsCoastline (TerrainCell cell) {

		if (cell.Altitude <= 0)
			return false;
		
		Dictionary<string, TerrainCell> neighbors = GetNeighborCells (cell);

		TerrainCell nCell = null;
		
		if (neighbors.TryGetValue ("west", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}
		
		if (neighbors.TryGetValue ("northwest", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}
		
		if (neighbors.TryGetValue ("north", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}
		
		if (neighbors.TryGetValue ("northeast", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}
		
		if (neighbors.TryGetValue ("east", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}
		
		if (neighbors.TryGetValue ("southeast", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}
		
		if (neighbors.TryGetValue ("south", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}
		
		if (neighbors.TryGetValue ("southwest", out nCell)) {
			
			if (nCell.Altitude <= 0) return true;
		}

		return false;
	}
	
	private static bool IsNearCoastline (TerrainCell cell) {
		
		if (cell.Altitude > 0)
			return false;
		
		Dictionary<string, TerrainCell> neighbors = GetNeighborCells (cell);
		
		TerrainCell nCell = null;
		
		if (neighbors.TryGetValue ("west", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		if (neighbors.TryGetValue ("northwest", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		if (neighbors.TryGetValue ("north", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		if (neighbors.TryGetValue ("northeast", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		if (neighbors.TryGetValue ("east", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		if (neighbors.TryGetValue ("southeast", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		if (neighbors.TryGetValue ("south", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		if (neighbors.TryGetValue ("southwest", out nCell)) {
			
			if (nCell.Altitude > 0) return true;
		}
		
		return false;
	}
	
	private static Color GenerateColorFromTerrainCell (TerrainCell cell) {

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
			throw new System.Exception("Unsupported Planet View Type");
		}

		switch (_planetOverlay) {
		
		case PlanetOverlay.None:
			break;

		case PlanetOverlay.Population:
			color = SetPopulationOverlayColor(cell, color);
			break;
			
		case PlanetOverlay.Temperature:
			color = SetTemperatureOverlayColor(cell, color);
			break;
			
		case PlanetOverlay.Rainfall:
			color = SetRainfallOverlayColor(cell, color);
			break;
			
		default:
			throw new System.Exception("Unsupported Planet Overlay Type");
		}

		color.a = 1;
		
		return color;
	}
	
	private static Color GenerateCoastlineColor (TerrainCell cell) {
		
		if (_mapPalette.Count == 0) {
			
			return Color.black;
		}
		
		if (IsCoastline (cell)) {
			
			return _mapPalette[2];
		}
		
		if (IsNearCoastline (cell)) {
			
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
			
			value = (2 - altitude / World.MinPossibleAltitude) / 2f;

			Color color1 = Color.blue;
			
			return new Color(color1.r * value, color1.g * value, color1.b * value);
		}
		
		value = (1 + altitude / World.MaxPossibleAltitude) / 2f;
		
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
	
	private static Color SetPopulationOverlayColor (TerrainCell cell, Color color) {

		float greyscale = (color.r + color.g + color.b);// * 4 / 3;

		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		float totalPopulation = 0;
		
		for (int i = 0; i < cell.HumanGroups.Count; i++) {

			totalPopulation += cell.HumanGroups[i].Population;
		}

		if (totalPopulation > 0) {
			
			color = Color.red;
		}

		return color;
	}
	
	private static Color SetTemperatureOverlayColor (TerrainCell cell, Color color) {
		
		float greyscale = (color.r + color.g + color.b);// * 4 / 3;
		
		color.r = (greyscale + color.r) / 6f;
		color.g = (greyscale + color.g) / 6f;
		color.b = (greyscale + color.b) / 6f;

		Color addColor;
		
		float span = World.MaxPossibleTemperature - World.MinPossibleTemperature;
		
		float value = (cell.Temperature - World.MinPossibleTemperature) / span;
		
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
			
			float value = cell.Rainfall / World.MaxPossibleRainfall;
			
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
	
	private static float NormalizeRainfall (float rainfall) {
		
		if (rainfall < 0) return 0;
		
		return rainfall / World.MaxPossibleRainfall;
	}
	
	private static float NormalizeTemperature (float temperature) {
		
		float span = World.MaxPossibleTemperature - World.MinPossibleTemperature;
		
		return (temperature - World.MinPossibleTemperature) / span;
	}

	private static XmlAttributeOverrides GenerateAttributeOverrides () {

		XmlAttributes attrs = new XmlAttributes();

		XmlElementAttribute attr = new XmlElementAttribute();
		attr.ElementName = "UpdateGroupEvent";
		attr.Type = typeof(UpdateGroupEvent);

		attrs.XmlElements.Add(attr);

		XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();

		attrOverrides.Add(typeof(World), "EventsToHappen", attrs);

		return attrOverrides;
	}
}
