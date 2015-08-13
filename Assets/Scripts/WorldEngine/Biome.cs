using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Biome {

	public static Biome IceCap = new Biome(
		"Ice Cap", 
		0,
		World.MinPossibleAltitude - 1f, 
		World.MaxPossibleAltitude + 1f, 
		World.MinPossibleRainfall - 1f,
		World.MaxPossibleRainfall + 1f,
		World.MinPossibleTemperature - 1f,
		-15f,
		0.1f,
		0.02f);
	
	public static Biome Ocean = new Biome(
		"Ocean",
		1,
		World.MinPossibleAltitude - 1f, 
		0.01f, 
		World.MinPossibleRainfall - 1f,
		World.MaxPossibleRainfall + 1f,
		-15f,
		World.MaxPossibleTemperature + 1f,
		0.0f,
		0.0f);
	
	public static Biome Grassland = new Biome(
		"Grassland",
		2,
		0, 
		World.MaxPossibleAltitude + 1f, 
		15f,
		1575f,
		-5f,
		World.MaxPossibleTemperature + 1f,
		1.0f,
		0.4f);
	
	public static Biome Forest = new Biome(
		"Forest", 
		3,
		0, 
		World.MaxPossibleAltitude + 1f, 
		1375f,
		2975f,
		-5f,
		World.MaxPossibleTemperature + 1f,
		0.8f,
		0.6f);
	
	public static Biome Taiga = new Biome(
		"Taiga", 
		4,
		0, 
		World.MaxPossibleAltitude + 1f,
		275f,
		World.MaxPossibleRainfall + 1f,
		-20f,
		-0f,
		0.6f,
		0.4f);
	
	public static Biome Tundra = new Biome(
		"Tundra", 
		5,
		0, 
		World.MaxPossibleAltitude + 1f, 
		World.MinPossibleRainfall - 1f,
		1275f,
		-20f,
		-0f,
		0.3f,
		0.2f);
	
	public static Biome Desert = new Biome(
		"Desert", 
		6,
		0, 
		World.MaxPossibleAltitude + 1f, 
		World.MinPossibleRainfall - 1f,
		675f,
		-5f,
		World.MaxPossibleTemperature + 1f,
		0.2f,
		0.1f);
	
	public static Biome Rainforest = new Biome(
		"Rainforest", 
		7,
		0, 
		World.MaxPossibleAltitude + 1f, 
		1775f,
		World.MaxPossibleRainfall + 1f,
		-5f,
		World.MaxPossibleTemperature + 1f,
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
