using UnityEngine;
using System.Collections;
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
		-15f);
	
	public static Biome Ocean = new Biome(
		"Ocean",
		1,
		World.MinPossibleAltitude - 1f, 
		0, 
		World.MinPossibleRainfall - 1f,
		World.MaxPossibleRainfall + 1f,
		-15f,
		World.MaxPossibleTemperature + 1f);
	
	public static Biome Grassland = new Biome(
		"Grassland",
		2,
		0, 
		World.MaxPossibleAltitude + 1f, 
		25f,
		1475f,
		-5f,
		World.MaxPossibleTemperature + 1f);
	
	public static Biome Forest = new Biome(
		"Forest", 
		3,
		0, 
		World.MaxPossibleAltitude + 1f, 
		975f,
		2475f,
		-5f,
		World.MaxPossibleTemperature + 1f);
	
	public static Biome Taiga = new Biome(
		"Taiga", 
		4,
		0, 
		World.MaxPossibleAltitude + 1f,
		475f,
		World.MaxPossibleRainfall + 1f,
		-15f,
		-0f);
	
	public static Biome Tundra = new Biome(
		"Tundra", 
		5,
		0, 
		World.MaxPossibleAltitude + 1f, 
		World.MinPossibleRainfall - 1f,
		725f,
		-20f,
		-0f);
	
	public static Biome Desert = new Biome(
		"Desert", 
		6,
		0, 
		World.MaxPossibleAltitude + 1f, 
		World.MinPossibleRainfall - 1f,
		275f,
		-5f,
		World.MaxPossibleTemperature + 1f);
	
	public static Biome Rainforest = new Biome(
		"Rainforest", 
		7,
		0, 
		World.MaxPossibleAltitude + 1f, 
		1975f,
		World.MaxPossibleRainfall + 1f,
		-5f,
		World.MaxPossibleTemperature + 1f);

	public static Biome[] Biomes = new Biome[] {

		IceCap,
		Ocean,
		Grassland,
		Forest,
		Taiga,
		Tundra,
		Desert,
		Rainforest
	};
	
	[XmlAttribute]
	public string Name;

	[XmlAttribute]
	public float MinAltitude;
	[XmlAttribute]
	public float MaxAltitude;

	[XmlAttribute]
	public float MinRainfall;
	[XmlAttribute]
	public float MaxRainfall;

	[XmlAttribute]
	public float MinTemperature;
	[XmlAttribute]
	public float MaxTemperature;

	public int ColorId;

	public Biome () {
	}

	public Biome (string name, int colorId, float minAltitude, float maxAltitude, float minRainfall, float maxRainfall, float minTemperature, float maxTemperature) {

		Name = name;

		ColorId = colorId;

		MinAltitude = minAltitude;
		MaxAltitude = maxAltitude;
		MinRainfall = minRainfall;
		MaxRainfall = maxRainfall;
		MinTemperature = minTemperature;
		MaxTemperature = maxTemperature;
	}
}
