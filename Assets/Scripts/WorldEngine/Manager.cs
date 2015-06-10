using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class Manager {

	private static Manager _manager = new Manager();

	private World _currentWorld = null;

	private Texture2D _currentTexture = null;

	private static bool _rainfallVisible = false;
	private static bool _temperatureVisible = false;
	private static bool _biomesVisible = false;
	
	private static List<Color> _biomePalette = new List<Color>();

	public static void SetBiomePalette (IEnumerable<Color> colors) {

		_biomePalette.Clear ();
		_biomePalette.AddRange (colors);
	}

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

		stream.Close();
	}

	public static void SetRainfallVisible (bool value) {
	
		_rainfallVisible = value;
	}
	
	public static void SetTemperatureVisible (bool value) {
		
		_temperatureVisible = value;
	}
	
	public static void SetBiomesVisible (bool value) {
		
		_biomesVisible = value;
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
	
	private static Color GenerateColorFromTerrainCell (TerrainCell cell) {

		if (_biomesVisible) {

			return GenerateBiomeColor (cell);
		}

		Color altitudeColor = GenerateAltitudeContourColor(cell.Altitude);
		Color rainfallColor = Color.black;
		Color temperatureColor = Color.black;
		
		float r = altitudeColor.r;
		float g = altitudeColor.g;
		float b = altitudeColor.b;

		int normalizer = 1;

		if (_rainfallVisible)
		{
			rainfallColor = GenerateRainfallContourColor(cell.Rainfall);
			
			normalizer++;
			
			r = (r + rainfallColor.r);
			g = (g + rainfallColor.g);
			b = (b + rainfallColor.b);
		}
		
		if (_temperatureVisible)
		{
			temperatureColor = GenerateTemperatureContourColor(cell.Temperature);
			
			normalizer++;
			
			r = (r + temperatureColor.r);
			g = (g + temperatureColor.g);
			b = (b + temperatureColor.b);
		}

		r /= (float)normalizer;
		g /= (float)normalizer;
		b /= (float)normalizer;
		
		return new Color(r, g, b);
	}

	private static Color GenerateAltitudeColor (float altitude) {
		
		float value;
		
		if (altitude < 0) {
			
			value = (2 - altitude / World.MinPossibleAltitude) / 2f;

			Color color1 = Color.blue;
			
			return new Color(color1.r * value, color1.g * value, color1.b * value);
		}
		
		value = (1 + altitude / World.MaxPossibleAltitude) / 2f;
		
		Color color2 = new Color(0.58f, 0.29f, 0);
		//Color color2 = Color.white;
		
		return new Color(color2.r * value, color2.g * value, color2.b * value);
	}
	
	private static Color GenerateBiomeColor (TerrainCell cell) {
		
		Color color = Color.black;

		foreach (KeyValuePair<Biome, float> pair in cell.BiomePresences)
		{
			Color biomeColor = _biomePalette[pair.Key.ColorId];
			float biomePresence = pair.Value;

			color.r += biomeColor.r * biomePresence;
			color.g += biomeColor.g * biomePresence;
			color.b += biomeColor.b * biomePresence;
		}
		
		return color;
	}
	
	private static Color GenerateAltitudeContourColor (float altitude) {
		
		float value;

		Color color = Color.white;
		
		if (altitude < 0) {
			
			value = (2 - altitude / World.MinPossibleAltitude) / 2f;
			
			color = Color.blue;
			
			return new Color(color.r * value, color.g * value, color.b * value);
		}

		float shadeValue = 1.0f;

		value = altitude / CurrentWorld.MaxAltitude;

		while (shadeValue > value)
		{
			shadeValue -= 0.1f;
		}

		color = new Color(color.r * shadeValue, color.g * shadeValue, color.b * shadeValue);
		
		return color;
	}
	
	private static Color GenerateRainfallContourColor (float rainfall) {
		
		float value;
		
		Color color = Color.green;
		
		float shadeValue = 1.0f;
		
		value = rainfall / CurrentWorld.MaxRainfall;
		
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
		
		return new Color(value, 1f - value, 0);
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
