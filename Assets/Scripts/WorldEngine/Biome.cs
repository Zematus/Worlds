using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class Biome {
	
	public static Biome Ocean = new Biome(
		"Ocean",
		Color.blue,
		World.MinPossibleAltitude, 
		0, 
		World.MinPossibleRainfall,
		World.MaxPossibleRainfall,
		-10f,
		World.MaxPossibleTemperature);

	public static Biome IceCap = new Biome(
		"Ice Cap", 
		Color.white,
		World.MinPossibleAltitude, 
		World.MaxPossibleAltitude, 
		World.MinPossibleRainfall,
		World.MaxPossibleRainfall,
		World.MinPossibleTemperature,
		-10f);
	
	public static Biome Grassland = new Biome(
		"Grassland", 
		Color.green,
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

	public Color Color;

	public Biome () {
	}

	public Biome (string name, Color color, float minAltitude, float maxAltitude, float minRainfall, float maxRainfall, float minTemperature, float maxTemperature) {

		Name = name;

		Color = color;

		MinAltitude = minAltitude;
		MaxAltitude = maxAltitude;
		MinRainfall = minRainfall;
		MaxRainfall = maxRainfall;
		MinTemperature = minTemperature;
		MaxTemperature = maxTemperature;
	}
}
