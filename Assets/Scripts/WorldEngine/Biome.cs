using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Biome {

	public static float MinBiomeTemperature = World.MinPossibleTemperature*3 - 1;
	public static float MaxBiomeTemperature = World.MaxPossibleTemperature*3 + 1;
	
	public static float MinBiomeRainfall = World.MinPossibleRainfall*3 - 1;
	public static float MaxBiomeRainfall = World.MaxPossibleRainfall*3 + 1;
	
	public static float MinBiomeAltitude = World.MinPossibleAltitude*3 - 1;
	public static float MaxBiomeAltitude = World.MaxPossibleAltitude*3 + 1;

	public static Biome IceCap = new Biome(
		"Ice Cap", 
		0,
		MinBiomeAltitude, 
		MaxBiomeAltitude, 
		MinBiomeRainfall,
		MaxBiomeRainfall,
		MinBiomeTemperature,
		-15f,
		0.1f,
		0.02f);
	
	public static Biome Ocean = new Biome(
		"Ocean",
		1,
		MinBiomeAltitude, 
		0.01f, 
		MinBiomeRainfall,
		MaxBiomeRainfall,
		-15f,
		MaxBiomeTemperature,
		0.0f,
		0.0f);
	
	public static Biome Grassland = new Biome(
		"Grassland",
		2,
		0, 
		MaxBiomeAltitude, 
		15f,
		1575f,
		-5f,
		MaxBiomeTemperature,
		1.0f,
		0.4f);
	
	public static Biome Forest = new Biome(
		"Forest", 
		3,
		0, 
		MaxBiomeAltitude, 
		1375f,
		2975f,
		-5f,
		MaxBiomeTemperature,
		0.8f,
		0.6f);
	
	public static Biome Taiga = new Biome(
		"Taiga", 
		4,
		0, 
		MaxBiomeAltitude,
		275f,
		MaxBiomeRainfall,
		-20f,
		-0f,
		0.6f,
		0.4f);
	
	public static Biome Tundra = new Biome(
		"Tundra", 
		5,
		0, 
		MaxBiomeAltitude, 
		MinBiomeRainfall,
		1275f,
		-20f,
		-0f,
		0.3f,
		0.2f);
	
	public static Biome Desert = new Biome(
		"Desert", 
		6,
		0, 
		MaxBiomeAltitude, 
		MinBiomeRainfall,
		675f,
		-5f,
		MaxBiomeTemperature,
		0.2f,
		0.1f);
	
	public static Biome Rainforest = new Biome(
		"Rainforest", 
		7,
		0, 
		MaxBiomeAltitude, 
		1775f,
		MaxBiomeRainfall,
		-5f,
		MaxBiomeTemperature,
		0.6f,
		0.8f);

	public static Dictionary<string, Biome> Biomes = new Dictionary<string, Biome>() {

		{"Ice Cap", IceCap},
		{"Ocean", Ocean},
		{"Grassland", Grassland},
		{"Forest", Forest},
		{"Taiga", Taiga},
		{"Tundra", Tundra},
		{"Desert", Desert},
		{"Rainforest", Rainforest}
	};

	public string Name;

	public float MinAltitude;
	public float MaxAltitude;

	public float MinRainfall;
	public float MaxRainfall;

	public float MinTemperature;
	public float MaxTemperature;

	public float Survivability;
	public float ForagingCapacity;

	public int ColorId;

	public Biome () {
	}

	public Biome (
		string name, 
		int colorId, 
		float minAltitude, 
		float maxAltitude, 
		float minRainfall, 
		float maxRainfall, 
		float minTemperature, 
		float maxTemperature,
		float survivability,
		float foragingCapacity) {

		Name = name;

		ColorId = colorId;

		MinAltitude = minAltitude;
		MaxAltitude = maxAltitude;
		MinRainfall = minRainfall;
		MaxRainfall = maxRainfall;
		MinTemperature = minTemperature;
		MaxTemperature = maxTemperature;
		Survivability = survivability;
		ForagingCapacity = foragingCapacity;
	}
}
