using UnityEngine;
using System.Collections;

public class TerrainCell {

	public float Altitude;
	public float Rainfall;
}

public class World {

	public const int NumContinents = 7;
	public const float ContinentFactor = 0.7f;
	public const float ContinentWidthFactor = 5f;

	public const float MinAltitude = -10000;
	public const float MaxAltitude = 10000;
	public const float MountainRangeWidthFactor = 15f;
	public const float TerrainNoiseFactor1 = 0.2f;
	public const float TerrainNoiseFactor2 = 0.15f;
	public const float TerrainNoiseFactor3 = 0.1f;
	
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
				Mathf.Repeat(Random.Range(Width*i*2/5, Width*(i + 2)*2/5), Width),
				Random.Range(Height * 2f/7f, Height * 5f/7f));
		}
	}
	
	private float GetContinentModifier (int x, int y) {

		float maxValue = 0;

		float betaFactor = ContinentWidthFactor * Width/1.5f * (1 - Mathf.Sin(Mathf.PI * y / Height));

		for (int i = 0; i < NumContinents; i++)
		{
			Vector2 continentOffset = _continentOffsets[i];
			float contX = continentOffset.x;
			float contY = continentOffset.y;

			float distX = Mathf.Min(Mathf.Abs(contX - x), Mathf.Abs(Width + contX - x));
			distX = Mathf.Min(distX, Mathf.Abs(contX - x - Width));
			float distY = 2 * Mathf.Abs(contY - y);

			float dist = new Vector2(distX, distY).magnitude;

			float value = Mathf.Max(0, 1f - ContinentWidthFactor*dist/((float)Width + betaFactor)); 

			maxValue = Mathf.Max(maxValue, value);
		}

		return maxValue;
	}

	private void GenerateTerrainAltitude () {

		GenerateContinents();
		
		int sizeX = Width;
		int sizeY = Height;

		float radius1 = 0.5f;
		float radius2 = 4f;
		float radius3 = 4f;
		float radius4 = 8f;
		float radius5 = 16f;

		Vector3 offset1 = GenerateRandomOffsetVector();
		Vector3 offset2 = GenerateRandomOffsetVector();
		Vector3 offset3 = GenerateRandomOffsetVector();
		Vector3 offset4 = GenerateRandomOffsetVector();
		Vector3 offset5 = GenerateRandomOffsetVector();
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				float alpha = (j / (float)sizeY) * Mathf.PI;

				float continentValue = GetContinentModifier(i, j);
				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);
				float value3 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius3, offset3);
				float value4 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius4, offset4);
				float value5 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius5, offset5);

				float value = MathUtility.MixValues(value1, value2, 0.1f);
				value = GetMountainRangeNoiseFromRandomNoise(value);
				value = value * MathUtility.MixValues(1, continentValue, 0.3f);

				value = MathUtility.MixValues(value, continentValue, ContinentFactor);
				value = MathUtility.MixValues(value, value3, TerrainNoiseFactor1);
				value = value * MathUtility.MixValues(1, value4, TerrainNoiseFactor2);
				value = value * MathUtility.MixValues(1, value5, TerrainNoiseFactor3);
				
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
	
	private float GetMountainRangeNoiseFromRandomNoise(float noise) {

		noise = (noise * 2) - 1;
		
		float value1 = -Mathf.Exp(-Mathf.Pow(noise*MountainRangeWidthFactor + 1, 2));
		float value2 = Mathf.Exp(-Mathf.Pow(noise*MountainRangeWidthFactor - 1, 2));
		
		float value = (value1 + value2 + 1) / 2f;
		
		return value;
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
