using UnityEngine;
using System.Collections;

public class Manager {

	private static Manager _manager = new Manager();

	private World _currentWorld = null;

	private Texture2D _currentTexture = null;

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

	public static World GenerateNewWorld () {

		int width = 200;
		int height = 100;
		int seed = Random.Range(0, int.MaxValue);
		//int seed = 0;

		World world = new World(width, height, seed);
		world.Generate();

		_manager._currentWorld = world;

		return world;
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

		Color altitudeColor = GenerateAltitudeColor(cell.Altitude);
		Color rainfallColor = GenerateRainfallColor(cell.Rainfall);

		float normalizedRainfall = NormalizeRainfall(cell.Rainfall);

		float r = (altitudeColor.r * (1f - normalizedRainfall)) + (rainfallColor.r * normalizedRainfall);
		float g = (altitudeColor.g * (1f - normalizedRainfall)) + (rainfallColor.g * normalizedRainfall);
		float b = (altitudeColor.b * (1f - normalizedRainfall)) + (rainfallColor.b * normalizedRainfall);
		
		return new Color(r, g, b);
	}

	private static Color GenerateAltitudeColor (float altitude) {
		
		if (altitude < 0) return Color.blue;
		
		float span = World.MaxAltitude - World.MinAltitude;
		
		float value = (altitude - World.MinAltitude) / span;
		
		Color brown = new Color(0.58f, 0.29f, 0);
		
		return new Color(brown.r * value, brown.g * value, brown.b * value);
	}
	
	private static Color GenerateRainfallColor (float rainfall) {
		
		if (rainfall < 0) return Color.black;
		
		float value = rainfall / World.MaxRainfall;
		
		Color green = Color.green;
		
		return new Color(green.r * value, green.g * value, green.b * value);
	}
	
	private static float NormalizeRainfall (float rainfall) {
		
		if (rainfall < 0) return 0;
		
		return rainfall / World.MaxRainfall;
	}
}
