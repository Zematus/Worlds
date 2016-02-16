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
		"Ice_Cap",
		0,
		MinBiomeAltitude,
		0.0001f,
		MinBiomeRainfall,
		MaxBiomeRainfall,
		MinBiomeTemperature,
		-15f,
		0.0f,
		0.01f,
		0.5f,
		0.0f);

	public static Biome Glacier = new Biome(
		"Glacier",
		"Glacier",
		0,
		0, 
		MaxBiomeAltitude, 
		15f,
		MaxBiomeRainfall,
		MinBiomeTemperature,
		-15f,
		0.0f,
		0.01f,
		0.5f,
		0.0f);
	
	public static Biome Ocean = new Biome(
		"Ocean",
		"Ocean",
		1,
		MinBiomeAltitude,
		0.0001f,
		MinBiomeRainfall,
		MaxBiomeRainfall,
		-15f,
		MaxBiomeTemperature,
		0.0f,
		0.0f,
		0.0f,
		0.0f);
	
	public static Biome Grassland = new Biome(
		"Grassland",
		"Grassland",
		2,
		0, 
		MaxBiomeAltitude, 
		15f,
		1575f,
		-10f,
		MaxBiomeTemperature,
		1.0f,
		0.4f,
		1.0f,
		1.0f);
	
	public static Biome Forest = new Biome(
		"Forest",
		"Forest",
		3,
		0, 
		MaxBiomeAltitude, 
		1375f,
		2975f,
		-10f,
		MaxBiomeTemperature,
		0.4f,
		0.6f,
		0.6f,
		0.3f);
	
	public static Biome Taiga = new Biome(
		"Taiga",
		"Taiga",
		4,
		0, 
		MaxBiomeAltitude,
		1375f,
		MaxBiomeRainfall,
		-20f,
		-0f,
		0.2f,
		0.4f,
		0.4f,
		0.2f);
	
	public static Biome Tundra = new Biome(
		"Tundra",
		"Tundra",
		5,
		0, 
		MaxBiomeAltitude, 
		15f,
		1575f,
		-20f,
		-0f,
		0.0f,
		0.1f,
		1.0f,
		0.0f);
	
	public static Biome DeserticTundra = new Biome(
		"Desertic Tundra",
		"Desertic_Tundra",
		8,
		0, 
		MaxBiomeAltitude, 
		MinBiomeRainfall,
		675f,
		MinBiomeTemperature,
		-0f,
		0.0f,
		0.04f,
		0.8f,
		0.0f);
	
	public static Biome Desert = new Biome(
		"Desert",
		"Desert",
		6,
		0, 
		MaxBiomeAltitude, 
		MinBiomeRainfall,
		675f,
		-10f,
		MaxBiomeTemperature,
		0.0f,
		0.02f,
		0.8f,
		0.0f);
	
	public static Biome Rainforest = new Biome(
		"Rainforest",
		"Rainforest",
		7,
		0, 
		MaxBiomeAltitude, 
		1775f,
		MaxBiomeRainfall,
		-5f,
		MaxBiomeTemperature,
		0.2f,
		0.8f,
		0.2f,
		0.1f);

	public static Dictionary<string, Biome> Biomes = new Dictionary<string, Biome>() {
		
		{"Glacier", Glacier},
		{"Ice Cap", IceCap},
		{"Ocean", Ocean},
		{"Grassland", Grassland},
		{"Forest", Forest},
		{"Taiga", Taiga},
		{"Tundra", Tundra},
		{"Desertic Tundra", DeserticTundra},
		{"Desert", Desert},
		{"Rainforest", Rainforest}
	};

	public string Name;
	public string Id;

	public float MinAltitude;
	public float MaxAltitude;

	public float MinRainfall;
	public float MaxRainfall;

	public float MinTemperature;
	public float MaxTemperature;

	public float Survivability;
	public float ForagingCapacity;
	public float Accessibility;
	public float Arability;

	public int ColorId;

	public Biome () {
	}

	public Biome (
		string name, 
		string id, 
		int colorId, 
		float minAltitude, 
		float maxAltitude, 
		float minRainfall, 
		float maxRainfall, 
		float minTemperature, 
		float maxTemperature,
		float survivability,
		float foragingCapacity,
		float accessibility,
		float arability) {

		Name = name;
		Id = id;

		ColorId = colorId;

		MinAltitude = minAltitude;
		MaxAltitude = maxAltitude;
		MinRainfall = minRainfall;
		MaxRainfall = maxRainfall;
		MinTemperature = minTemperature;
		MaxTemperature = maxTemperature;
		Survivability = survivability;
		ForagingCapacity = foragingCapacity;
		Accessibility = accessibility;
		Arability = arability;
	}
}
