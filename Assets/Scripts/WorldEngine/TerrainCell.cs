using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TerrainCell {

	public const float MaxForageFactor = 2/0.4f;

	[XmlIgnore]
	public World World;
	
	[XmlAttribute]
	public int Longitude;
	[XmlAttribute]
	public int Latitude;
	[XmlAttribute]
	public int LocalIteration;

	[XmlAttribute]
	public float Altitude;
	[XmlAttribute]
	public float Rainfall;
	[XmlAttribute]
	public float Temperature;

	[XmlAttribute]
	public float Survivability;
	[XmlAttribute]
	public float ForagingCapacity;
	[XmlAttribute]
	public float MaxForage;

	[XmlAttribute]
	public float Height;
	[XmlAttribute]
	public float Width;

	[XmlAttribute]
	public bool Ready;
	
	[XmlIgnore]
	public float Area;

	public List<string> PresentBiomeNames = new List<string>();
	public List<float> BiomePresences = new List<float>();

	public List<HumanGroup> HumanGroups = new List<HumanGroup>();

	public TerrainCell () {
	
		Manager.UpdateWorldLoadTrack ();
	}
	
	public TerrainCell (bool update) {
		
		if (update) Manager.UpdateWorldLoadTrack ();
	}

	public int GetNextLocalRandomInt () {

		int x = Mathf.Abs (World.Seed + Longitude);
		int y = Mathf.Abs (World.Seed + Latitude);
		int z = Mathf.Abs (World.Seed + World.Iteration + LocalIteration);

		LocalIteration++;

		return PerlinNoise.GetPermutationValue(x, y, z);
	}
	
	public float GetNextLocalRandomFloat () {
		
		return GetNextLocalRandomInt() / (float)PerlinNoise.MaxPermutationValue;
	}

	public float GetBiomePresence (Biome biome) {
		
		Area = Height * Width;

		for (int i = 0; i < PresentBiomeNames.Count; i++) {

			if (biome.Name == PresentBiomeNames[i])
			{
				return BiomePresences[i];
			}
		}

		return 0;
	}

	public void FinalizeLoad () {

		foreach (HumanGroup group in HumanGroups) {
		
			group.World = World;
			group.Cell = this;
		}
	}
}
