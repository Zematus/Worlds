using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class Biome {
	
	public static Biome Ocean = new Biome(
		"Ocean", 
		World.MinPossibleAltitude, 
		0, 
		World.MinPossibleRainfall,
		World.MaxPossibleRainfall,
		-10f,
		World.MaxPossibleTemperature);

	public static Biome IceCap = new Biome(
		"Ice Cap", 
		World.MinPossibleAltitude, 
		World.MaxPossibleAltitude, 
		World.MinPossibleRainfall,
		World.MaxPossibleRainfall,
		World.MinPossibleTemperature,
		-10f);
	
	public static Biome Grassland = new Biome(
		"Grassland", 
		0, 
		World.MaxPossibleAltitude, 
		World.MinPossibleRainfall,
		World.MaxPossibleRainfall,
		-10f,
		World.MaxPossibleTemperature);

	public static Biome[] Biomes = new Biome[] {

		IceCap,
		Ocean,
		Grassland
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

	public Biome () {
	}

	public Biome (string name, float minAltitude, float maxAltitude, float minRainfall, float maxRainfall, float minTemperature, float maxTemperature) {

		Name = name;

		MinAltitude = minAltitude;
		MaxAltitude = maxAltitude;
		MinRainfall = minRainfall;
		MaxRainfall = maxRainfall;
		MinTemperature = minTemperature;
		MaxTemperature = maxTemperature;
	}
}
