﻿using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class Biome {

	[XmlAttribute]
	public float Altitude;
	[XmlAttribute]
	public float Rainfall;
	[XmlAttribute]
	public float Temperature;
}

public class TerrainCell {
	
	[XmlAttribute]
	public float Altitude;
	[XmlAttribute]
	public float Rainfall;
	[XmlAttribute]
	public float Temperature;

	[XmlIgnore]
	public float RainAcc;
	[XmlIgnore]
	public float PrevRainAcc;
}

[XmlRoot]
public class World {

	public const int NumContinents = 7;
	public const float ContinentFactor = 0.75f;
	//public const float ContinentFactor = 0.5f;
	//public const float ContinentFactor = 0f;
	public const float ContinentMinWidthFactor = 3f;
	public const float ContinentMaxWidthFactor = 7f;

	public const float MinAltitude = -10000;
	public const float MaxAltitude = 10000;
	//public const float MountainRangeMixFactor = 0.1f;
	public const float MountainRangeMixFactor = 0.075f;
	public const float MountainRangeWidthFactor = 25f;
	public const float TerrainNoiseFactor1 = 0.2f;
	public const float TerrainNoiseFactor2 = 0.15f;
	public const float TerrainNoiseFactor3 = 0.1f;
	
	public const float MinRainfall = -20;
	public const float MaxRainfall = 100;
	public const float RainfallAltitudeFactor = 1.0f;
	
	public const float MinTemperature = -50;
	public const float MaxTemperature = 30;
	public const float TemperatureAltitudeFactor = 1.0f;
	
	[XmlAttribute]
	public int Width { get; private set; }
	[XmlAttribute]
	public int Height { get; private set; }
	
	[XmlAttribute]
	public int Seed { get; private set; }

	public TerrainCell[][] Terrain;

	private Vector2[] _continentOffsets;
	private float[] _continentWidths;

	private bool _initialized = false;

	public World () {
	}

	public World (int width, int height, int seed) {
	
		Width = width;
		Height = height;
		Seed = seed;

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
		_continentWidths = new float[NumContinents];
	}

	public void Initialize () {

		if (_initialized)
			return;

		_initialized = true;

		Random.seed = Seed;
	}

	public void Generate () {

		GenerateTerrainAltitude();
		GenerateTerrainRainfall();
		GenerateTerrainTemperature();
	}

	private void GenerateContinents () {

		for (int i = 0; i < NumContinents; i++)
		{
			_continentOffsets[i] = new Vector2(
				Mathf.Repeat(Random.Range(Width*i*2/5, Width*(i + 2)*2/5), Width),
				Random.Range(Height * 1f/6f, Height * 5f/6f));
			_continentWidths[i] = Random.Range(ContinentMinWidthFactor, ContinentMaxWidthFactor);
		}
	}
	
	private float GetContinentModifier (int x, int y) {

		float maxValue = 0;

		float betaFactor = Mathf.Sin(Mathf.PI * y / Height);

		for (int i = 0; i < NumContinents; i++)
		{
			Vector2 continentOffset = _continentOffsets[i];
			float contX = continentOffset.x;
			float contY = continentOffset.y;

			float distX = Mathf.Min(Mathf.Abs(contX - x), Mathf.Abs(Width + contX - x));
			distX = Mathf.Min(distX, Mathf.Abs(contX - x - Width));
			distX *= betaFactor;

			float distY = Mathf.Abs(contY - y);

			float dist = new Vector2(distX, distY).magnitude;

			float continentWidth = _continentWidths[i];

			float value = Mathf.Max(0, 1f - continentWidth*dist/((float)Width));

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

				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);
				float value3 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius3, offset3);
				float value4 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius4, offset4);
				float value5 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius5, offset5);

				float valueA = GetContinentModifier(i, j);
				valueA = MathUtility.MixValues(valueA, value3, TerrainNoiseFactor1);
				valueA = valueA * MathUtility.MixValues(1, value4, TerrainNoiseFactor2);
				valueA = valueA * MathUtility.MixValues(1, value5, TerrainNoiseFactor3);

				float valueB = (valueA * 0.04f) + 0.48f;
				
				float valueC = MathUtility.MixValues(value1, value2, MountainRangeMixFactor);
				valueC = GetMountainRangeNoiseFromRandomNoise(valueC);
				valueC = MathUtility.MixValues(valueC, value3, TerrainNoiseFactor1 * 1.5f);
				valueC = valueC * MathUtility.MixValues(1, value4, TerrainNoiseFactor2 * 1.5f);
				valueC = valueC * MathUtility.MixValues(1, value5, TerrainNoiseFactor3 * 1.5f);
				
				float valueD = MathUtility.MixValues(valueA, valueC, 0.25f);
				valueD = MathUtility.MixValues(valueD, valueC, 0.1f);
				valueD = MathUtility.MixValues(valueD, valueB, 0.1f);
				
				Terrain[i][j].Altitude = CalculateAltitude(valueD);
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
		float value3 = -Mathf.Exp(-Mathf.Pow(noise*MountainRangeWidthFactor + MountainRangeWidthFactor/2f, 2));
		float value4 = Mathf.Exp(-Mathf.Pow(noise*MountainRangeWidthFactor - MountainRangeWidthFactor/2f, 2));
		
		float value = (value1 + value2 + value3 + value4 + 1) / 2f;
		
		return value;
	}

	private float CalculateAltitude (float value) {
	
		float span = MaxAltitude - MinAltitude;

		return (value * span) + MinAltitude;
	}
	
	private void GenerateTerrainRainfall2 () {
		
		int sizeX = Width;
		int sizeY = Height;

		int maxCycles = Width/5;

		float accRainFactor = 0.06f;
		float rainfallFactor = 0.005f;
		float rainfallAltFactor = 0.05f;

		for (int c = 0; c < maxCycles; c++)
		{
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					int prevI = (i - 1 + Width) % Width;
					int prevJ = j;

					if (j < (sizeY / 4))
					{
						prevJ = j + 1;
					}
					else if (j < (sizeY / 2))
					{
						prevJ = j - 1;
						prevI = (i + 1) % Width;
					}
					else if (j < ((sizeY * 3) / 4))
					{
						prevJ = j + 1;
						prevI = (i + 1) % Width;
					}
					else
					{
						prevJ = j - 1;
					}

					TerrainCell cell = Terrain[i][j];
					cell.PrevRainAcc = cell.RainAcc;
					cell.RainAcc = 0;

					if (cell.Altitude < 0)
					{
						cell.RainAcc += accRainFactor * MaxRainfall;
					}
					
					TerrainCell prevCell = Terrain[prevI][prevJ];

					float altDiff = Mathf.Max(0, cell.Altitude) - Mathf.Max(0, prevCell.Altitude);
					float altitudeFactor = Mathf.Max(0, (rainfallAltFactor * altDiff / MaxAltitude));
					float finalRainFactor = Mathf.Min(1, rainfallFactor + altitudeFactor);

					cell.RainAcc += prevCell.PrevRainAcc;
					cell.RainAcc = Mathf.Min(cell.RainAcc, MaxRainfall / rainfallFactor);

					float accRainfall = cell.RainAcc * finalRainFactor;
					cell.Rainfall += accRainfall;
					cell.RainAcc -= accRainfall;

					cell.RainAcc = Mathf.Max(cell.RainAcc, 0);
				}
			}
		}
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
	
	private void GenerateTerrainTemperature () {
		
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

				float latitudeModifier = (alpha * 0.8f) + (value * 0.2f * Mathf.PI);
				
				float baseTemperature = CalculateTemperature(Mathf.Sin(latitudeModifier));
				
				float altitudeFactor = (cell.Altitude / MaxAltitude) * 2.5f * TemperatureAltitudeFactor;
				
				cell.Temperature = baseTemperature * (1f - altitudeFactor);
			}
		}
	}
	
	private float CalculateRainfall (float value) {
		
		float span = MaxRainfall - MinRainfall;

		float rainfall = (value * span) + MinRainfall;

		rainfall = Mathf.Clamp(rainfall, 0, MaxRainfall);
		
		return rainfall;
	}
	
	private float CalculateTemperature (float value) {
		
		float span = MaxTemperature - MinTemperature;
		
		float temperature = (value * span) + MinTemperature;
		
		temperature = Mathf.Clamp(temperature, MinTemperature, MaxTemperature);
		
		return temperature;
	}
}
