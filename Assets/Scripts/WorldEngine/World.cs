using UnityEngine;
using System.Collections;

public class TerrainCell {

	public float Altitude;
	public float Rainfall;
}

public class World {

	public const float MinAltitude = -10000;
	public const float MaxAltitude = 10000;
	
	public const float MinRainfall = -10;
	public const float MaxRainfall = 100;
	public const float RainfallAltitudeFactor = 1.0f;

	public int Width { get; private set; }
	public int Height { get; private set; }

	public int Seed { get; private set; }

	public TerrainCell[][] terrain;

	public World(int width, int height, int seed) {
	
		Width = width;
		Height = height;
		Seed = seed;

		terrain = new TerrainCell[width][];

		for (int i = 0; i < width; i++)
		{
			TerrainCell[] column = new TerrainCell[height];

			for (int j = 0; j < height; j++)
			{
				column[j] = new TerrainCell();
			}

			terrain[i] = column;
		}
	}

	public void Generate () {

		Random.seed = Seed;

		GenerateTerrainAltitude();
		GenerateTerrainRainfall();
	}

	private void GenerateTerrainAltitude () {
		
		Vector3 offset = Random.insideUnitSphere * 1000;
		
		int sizeX = Width;
		int sizeY = Height;
		
		float radius = 2f;
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				Vector3 pos = MathUtility.GetCartesianCoordinates(alpha,beta,radius) + offset;
				
				float value = PerlinNoise.GetValue(pos.x, pos.y, pos.z);
				
				terrain[i][j].Altitude = CalculateAltitudeFromNoise(value);
			}
		}
	}

	private float CalculateAltitudeFromNoise (float value) {
	
		float span = MaxAltitude - MinAltitude;

		return (value * span) + MinAltitude;
	}
	
	private void GenerateTerrainRainfall () {
		
		Vector3 offset = Random.insideUnitSphere * 1000;
		
		int sizeX = Width;
		int sizeY = Height;
		
		float radius = 2f;
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				TerrainCell cell = terrain[i][j];

				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				Vector3 pos = MathUtility.GetCartesianCoordinates(alpha,beta,radius) + offset;
				
				float value = PerlinNoise.GetValue(pos.x, pos.y, pos.z);

				float baseRainfall = CalculateRainfallFromNoise(value);

				float altitudeFactor = Mathf.Clamp((cell.Altitude / MaxAltitude) * RainfallAltitudeFactor, 0f, 1f);
				
				cell.Rainfall = baseRainfall * (1f - altitudeFactor);
			}
		}
	}
	
	private float CalculateRainfallFromNoise (float value) {
		
		float span = MaxRainfall - MinRainfall;

		float rainfall = (value * span) + MinRainfall;

		rainfall = Mathf.Clamp(rainfall, 0, MaxRainfall);
		
		return rainfall;
	}
}
