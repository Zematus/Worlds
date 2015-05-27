using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class Manager {

	private static Manager _manager = new Manager();

	private World _currentWorld = null;

	private Texture2D _currentTexture = null;

	private static bool _rainfallVisible = false;
	private static bool _temperatureVisible = false;

	public static World CurrentWorld { 
		get {

			if (_manager._currentWorld == null)
				GenerateNewWorld();

			return _manager._currentWorld; 
		}
	}
	
	public static Texture2D CurrentTexture { 
		get {
			
			if (_manager._currentTexture == null)
				GenerateTextureFromWorld(CurrentWorld);
			
			return _manager._currentTexture; 
		}
	}
	
	public static void RefreshTexture () { 

		GenerateTextureFromWorld(CurrentWorld);
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
	
	public static void SaveWorld () {

		XmlSerializer serializer = new XmlSerializer(typeof(World));
		FileStream stream = new FileStream("test_world.xml", FileMode.Create);

		serializer.Serialize(stream, _manager._currentWorld);

		stream.Close();
	}
	
	public static void LoadWorld () {

		XmlSerializer serializer = new XmlSerializer(typeof(World));
		FileStream stream = new FileStream("test_world.xml", FileMode.Open);

		_manager._currentWorld = serializer.Deserialize(stream) as World;

		stream.Close();
	}

	public static void SetRainfallVisible (bool value) {
	
		_rainfallVisible = value;
	}
	
	public static void SetTemperatureVisible (bool value) {
		
		_temperatureVisible = value;
	}
	
	public static Texture2D GenerateTextureFromWorld (World world) {
		
		int sizeX = world.Width;
		int sizeY = world.Height;
		
		Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
		
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				texture.SetPixel(i, j, GenerateColorFromTerrainCell(world.Terrain[i][j]));
			}
		}
		
		texture.Apply();

		_manager._currentTexture = texture;
		
		return texture;
	}
	
	private static Color GenerateGreyScaleColor (float value) {
		
		return new Color(value,value,value);
	}
	
	private static Color GenerateColorFromTerrainCell (TerrainCell cell) {

		Color altitudeColor = GenerateAltitudeContourColor(cell.Altitude);
		Color rainfallColor = Color.black;
		Color temperatureColor = Color.black;
		
		float r = altitudeColor.r;
		float g = altitudeColor.g;
		float b = altitudeColor.b;

		if (_rainfallVisible)
		{
			rainfallColor = GenerateRainfallColor(cell.Rainfall);
			float normalizedRainfall = NormalizeRainfall(cell.Rainfall);
			
			r = (r * (1f - normalizedRainfall)) + (rainfallColor.r * normalizedRainfall);
			g = (g * (1f - normalizedRainfall)) + (rainfallColor.g * normalizedRainfall);
			b = (b * (1f - normalizedRainfall)) + (rainfallColor.b * normalizedRainfall);
		}
		
		if (_temperatureVisible)
		{
			temperatureColor = GenerateTemperatureColor(cell.Temperature);
			float normalizedTemperature = NormalizeTemperature(cell.Temperature);
			
			r = (r * (1f - normalizedTemperature)) + (temperatureColor.r * normalizedTemperature);
			g = (g * (1f - normalizedTemperature)) + (temperatureColor.g * normalizedTemperature);
			b = (b * (1f - normalizedTemperature)) + (temperatureColor.b * normalizedTemperature);
		}
		
		return new Color(r, g, b);
	}

	private static Color GenerateAltitudeColor (float altitude) {
		
		float value;
		
		if (altitude < 0) {
			
			value = (2 - altitude / World.MinAltitude) / 2f;

			Color color1 = Color.blue;
			
			return new Color(color1.r * value, color1.g * value, color1.b * value);
		}
		
		value = (1 + altitude / World.MaxAltitude) / 2f;
		//value = altitude / World.MaxAltitude;
		
		Color color2 = new Color(0.58f, 0.29f, 0);
		//Color color2 = Color.white;
		
		return new Color(color2.r * value, color2.g * value, color2.b * value);
	}
	
	private static Color GenerateAltitudeContourColor (float altitude) {
		
		float value;

		Color color = Color.white;
		
		if (altitude < 0) {
			
			value = (2 - altitude / World.MinAltitude) / 2f;
			
			color = Color.blue;
			
			return new Color(color.r * value, color.g * value, color.b * value);
		}

		float shadeValue = 1.0f;

		value = altitude / World.MaxAltitude;

		while (shadeValue > value)
		{
			shadeValue -= 0.1f;
		}

		color = new Color(color.r * shadeValue, color.g * shadeValue, color.b * shadeValue);
		
		return color;
	}
	
	private static Color GenerateRainfallColor (float rainfall) {
		
		if (rainfall < 0) return Color.black;
		
		float value = rainfall / World.MaxRainfall;
		
		Color green = Color.green;
		
		return new Color(green.r * value, green.g * value, green.b * value);
	}
	
	private static Color GenerateTemperatureColor (float temperature) {

		float span = World.MaxTemperature - World.MinTemperature;

		float value = (temperature - World.MinTemperature) / span;
		
		return new Color(value, 1f - value, 0);
	}
	
	private static float NormalizeRainfall (float rainfall) {
		
		if (rainfall < 0) return 0;
		
		return rainfall / World.MaxRainfall;
	}
	
	private static float NormalizeTemperature (float temperature) {
		
		float span = World.MaxTemperature - World.MinTemperature;
		
		return (temperature - World.MinTemperature) / span;
	}
}
