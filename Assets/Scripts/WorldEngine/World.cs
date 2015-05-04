using UnityEngine;
using System.Collections;

public class TerrainCell {

	public float Altitude;
	public float Rainfall;
}

public class World {

	public const int NumContinents = 3;
	public const float ContinentFactor = 0.5f;

	public const float MinAltitude = -10000;
	public const float MaxAltitude = 10000;
	public const float MountainRangeFactor = 0.1f;
	//public const int MountainRangeMultiplier = 1;
	public const float MountainRangeWidthFactor = 10f;
	
	public const float MinRainfall = -10;
	public const float MaxRainfall = 100;
	public const float RainfallAltitudeFactor = 1.0f;

	public int Width { get; private set; }
	public int Height { get; private set; }

	public int Seed { get; private set; }

	public TerrainCell[][] Terrain;

	private Vector2[] _continentOffsets;

	public World(int width, int height, int seed) {
	
		Width = width;
		Height = height;
		Seed = seed;
		
		Random.seed = Seed;

		Terrain = new TerrainCell[width][];

		for (int i = 0; i < width; i++)
		{
			TerrainCell[] column = new TerrainCell[height];

			for (int j = 0; j < height; j++)
			{
				column[j] = new TerrainCell();
			}

			Terrain[i] = column;
		}

		_continentOffsets = new Vector2[NumContinents];
	}

	public void Generate () {

		GenerateTerrainAltitude();
		//GenerateTerrainRainfall();
	}

	private void GenerateContinents () {

		for (int i = 0; i < NumContinents; i++)
		{
			_continentOffsets[i] = new Vector2(
				Random.Range(1, Width - 1),
				Random.Range(1, Height - 1));
		}
	}
	
	private float GetContinentModifier (int x, int y) {

		float maxValue = 0;

		for (int i = 0; i < NumContinents; i++)
		{
			Vector2 continentOffset = _continentOffsets[i];
			float contX = continentOffset.x;
			float contY = continentOffset.y;

			float distX = Mathf.Min(Mathf.Abs(contX - x), Mathf.Abs(Width + contX - x));
			float distY = Mathf.Abs(contY - y);

			float factorX = Mathf.Max(0, 1f - 1*distX/(float)Width);
			float factorY = Mathf.Max(0, 1f - 1*distY/(float)Height);

			float value = (factorX*factorX * factorY*factorY);

			maxValue = Mathf.Max(maxValue, value);
		}

		return maxValue;
	}

	private void GenerateTerrainAltitude () {

		GenerateContinents();
		
		int sizeX = Width;
		int sizeY = Height;
		
		float radius1 = 2f;
		float radius2 = 1f;

		Vector3 offset1 = GenerateRandomOffsetVector();
		Vector3 offset2 = GenerateRandomOffsetVector();
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetMountainRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);

				float value = MathUtility.MixValues(value1, value2, MountainRangeFactor);
				value = MathUtility.MixValues(value, GetContinentModifier(i, j), ContinentFactor);
				//value = value * MathUtility.MixValues(1, GetContinentModifier(i, j), ContinentFactor);
				
				Terrain[i][j].Altitude = CalculateAltitude(value);
			}
		}
	}

	private Vector3 GenerateRandomOffsetVector () {
	
		return Random.insideUnitSphere * 1000;
	}

	private float GetRandomNoiseFromPolarCoordinates (float alpha, float beta, float radius, Vector3 offset) {

		Vector3 pos = MathUtility.GetCartesianCoordinates(alpha,beta,radius) + offset;
		
		return PerlinNoise.GetValue(pos.x, pos.y, pos.z);
	}

	private float GetMountainRandomNoiseFromPolarCoordinates(float alpha, float beta, float radius, Vector3 offset) {
	
		float noise = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius, offset);
		noise = (noise * 2) - 1;
		
//		if (noise < 0.5) 
//		{	
//			int debug = 0;
//		}

		//float value = Mathf.Sin(noise * Mathf.PI * MountainRangeMultiplier);

		float value1 = Mathf.Exp(-Mathf.Pow(noise*MountainRangeWidthFactor + 1, 2));
		float value2 = -Mathf.Exp(-Mathf.Pow(noise*MountainRangeWidthFactor - 1, 2));

		float value = (value1 + value2 + 1) / 2f;

		return value;// * value * value * value;// * value * value;
	}

	private float CalculateAltitude (float value) {
	
		float span = MaxAltitude - MinAltitude;

		return (value * span) + MinAltitude;
	}
	
	private void GenerateTerrainRainfall () {
		
		int sizeX = Width;
		int sizeY = Height;
		
		float radius = 2f;
		
		Vector3 offset = GenerateRandomOffsetVector();
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				TerrainCell cell = Terrain[i][j];

				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				float value = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius, offset);

				float baseRainfall = CalculateRainfall(value);

				float altitudeFactor = Mathf.Clamp((cell.Altitude / MaxAltitude) * RainfallAltitudeFactor, 0f, 1f);
				
				cell.Rainfall = baseRainfall * (1f - altitudeFactor);
			}
		}
	}
	
	private float CalculateRainfall (float value) {
		
		float span = MaxRainfall - MinRainfall;

		float rainfall = (value * span) + MinRainfall;

		rainfall = Mathf.Clamp(rainfall, 0, MaxRainfall);
		
		return rainfall;
	}
}
