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
	
	public static string SavePath { get; private set; }

	private static Manager _manager = new Manager();

	private static bool _rainfallVisible = false;
	private static bool _temperatureVisible = false;
	private static PlanetView _planetView = PlanetView.Biomes;
	
	private static List<Color> _biomePalette = new List<Color>();
	private static List<Color> _mapPalette = new List<Color>();
	
	private World _currentWorld = null;
	
	private Texture2D _currentSphereTexture = null;
	private Texture2D _currentMapTexture = null;

	private Queue<IManagerTask> _taskQueue = new Queue<IManagerTask>();

	private Manager () {

		string path = Path.GetFullPath (@"Saves\");

		if (!Directory.Exists (path)) {

			Directory.CreateDirectory(path);
		}

		SavePath = path;
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

		lock (_manager._taskQueue) {
		
			_manager._taskQueue.Enqueue(task);
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
	
	public static void RefreshTextures () { 

		GenerateSphereTextureFromWorld(CurrentWorld);
		GenerateMapTextureFromWorld(CurrentWorld);
	}

	public static World GenerateNewWorld () {

		int width = 400;
		int height = 200;
		int seed = Random.Range(0, int.MaxValue);
		//int seed = 4;

		World world = new World(width, height, seed);

		world.Initialize();
		world.Generate();

		_manager._currentWorld = world;

		return world;
	}
	
	public static World GenerateNewWorldAsync (ProgressCastDelegate progressCastMethod = null) {
		
		int width = 400;
		int height = 200;
		int seed = Random.Range(0, int.MaxValue);
		//int seed = 4;
		
		World world = new World(width, height, seed);

		if (progressCastMethod != null)
			world.ProgressCastMethod = progressCastMethod;
		
		world.Initialize();

		ThreadPool.QueueUserWorkItem (GenerateWorldCallback, world);
		
		_manager._currentWorld = world;
		
		return world;
	}

	public static void GenerateWorldCallback (object state) {

		World world = state as World;

		world.Generate ();
	}
	
	public static void SaveWorld (string path) {

		XmlSerializer serializer = new XmlSerializer(typeof(World));
		FileStream stream = new FileStream(path, FileMode.Create);

		serializer.Serialize(stream, _manager._currentWorld);

		stream.Close();
	}
	
	public static void LoadWorld (string path) {

		XmlSerializer serializer = new XmlSerializer(typeof(World));
		FileStream stream = new FileStream(path, FileMode.Open);

		_manager._currentWorld = serializer.Deserialize(stream) as World;

		_manager._currentWorld.FinalizeLoading ();

		stream.Close();
	}

	public static void SetRainfallVisible (bool value) {
	
		_rainfallVisible = value;
	}
	
	public static void SetTemperatureVisible (bool value) {
		
		_temperatureVisible = value;
	}
	
	public static void SetPlanetView (PlanetView value) {
		
		_planetView = value;
	}
	
	public static Texture2D GenerateMapTextureFromWorld (World world) {
		
		int sizeX = world.Width;
		int sizeY = world.Height;
		
		Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
		
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
//				if (((i % 20) == 0) || ((j % 20) == 0)) {
//
//					texture.SetPixel(i, j, Color.black);
//
//					continue;
//				}

				texture.SetPixel(i, j, GenerateColorFromTerrainCell(world.Terrain[i][j]));
			}
		}
		
		texture.Apply();

		_manager._currentMapTexture = texture;
		
		return texture;
	}
	
	public static Texture2D GenerateSphereTextureFromWorld (World world) {
		
		int sizeX = world.Width;
		int sizeY = world.Height*2;
		
		Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
		
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
				
				texture.SetPixel(i, j, GenerateColorFromTerrainCell(world.Terrain[i][trueJ]));
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

		Color baseColor = Color.black;

		if (_planetView == PlanetView.Biomes) {
			baseColor = GenerateBiomeColor (cell);
		} else if (_planetView == PlanetView.Elevation) {
			baseColor = GenerateAltitudeContourColor(cell.Altitude);
		} else {
			baseColor = GenerateCoastlineColor(cell);
		}
		
		float r = baseColor.r;
		float g = baseColor.g;
		float b = baseColor.b;

		int normalizer = 1;

		if (_rainfallVisible || _temperatureVisible) {

			float grey = (r + g + b) / 3f;

			r = grey;
			g = grey;
			b = grey;
		}

		if (_rainfallVisible)
		{
			Color rainfallColor = GenerateRainfallColor(cell.Rainfall);
			
			normalizer += 1;
			
			r += rainfallColor.r;
			g += rainfallColor.g;
			b += rainfallColor.b;
		}
		
		if (_temperatureVisible)
		{
			Color temperatureColor = GenerateTemperatureColor(cell.Temperature);
			
			normalizer += 1;
			
			r += temperatureColor.r;
			g += temperatureColor.g;
			b += temperatureColor.b;
		}

		r /= (float)normalizer;
		g /= (float)normalizer;
		b /= (float)normalizer;
		
		Color resultColor = new Color (r, g, b);
		
		return resultColor;
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
		
		for (int i = 0; i < cell.Biomes.Count; i++) {
			
			Color biomeColor = _biomePalette[cell.Biomes[i].ColorId];
			float biomePresence = cell.BiomePresences[i];
			
			color.r += biomeColor.r * biomePresence;
			color.g += biomeColor.g * biomePresence;
			color.b += biomeColor.b * biomePresence;
		}
		
		return color * slantFactor * altitudeFactor;
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
	
	private static Color GenerateRainfallColor (float rainfall) {
		
		if (rainfall < 0) return Color.black;
		
		float value = rainfall / World.MaxPossibleRainfall;
		
		Color green = Color.green;
		
		return new Color(green.r * value, green.g * value, green.b * value);
	}
	
	private static Color GenerateTemperatureContourColor (float rainfall) {
		
		float span = CurrentWorld.MaxTemperature - CurrentWorld.MinTemperature;
		
		float value;
		
		float shadeValue = 1f;
		
		value = (rainfall - CurrentWorld.MinTemperature) / span;
		
		while (shadeValue > value)
		{
			shadeValue -= 0.1f;
		}
		
		Color color = new Color(shadeValue, 0, 1f - shadeValue);
		
		return color;
	}
	
	private static Color GenerateTemperatureColor (float temperature) {

		float span = World.MaxPossibleTemperature - World.MinPossibleTemperature;

		float value = (temperature - World.MinPossibleTemperature) / span;
		
		return new Color(value, 0, 1f - value);
	}
	
	private static float NormalizeRainfall (float rainfall) {
		
		if (rainfall < 0) return 0;
		
		return rainfall / World.MaxPossibleRainfall;
	}
	
	private static float NormalizeTemperature (float temperature) {
		
		float span = World.MaxPossibleTemperature - World.MinPossibleTemperature;
		
		return (temperature - World.MinPossibleTemperature) / span;
	}
}
