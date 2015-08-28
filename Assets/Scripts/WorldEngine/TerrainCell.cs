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
	
	[XmlIgnore]
	public static float MaxArea;
	
	[XmlIgnore]
	public List<TerrainCell> Neighbors { get; private set; }

	public List<string> PresentBiomeNames = new List<string>();
	public List<float> BiomePresences = new List<float>();

	public List<CellGroup> Groups = new List<CellGroup>();

	public TerrainCell () {
	
		Manager.UpdateWorldLoadTrack ();
	}
	
	public TerrainCell (bool update) {
		
		if (update) Manager.UpdateWorldLoadTrack ();
	}

	public int GetNextLocalRandomInt (int maxValue = PerlinNoise.MaxPermutationValue) {

		maxValue = Mathf.Min (PerlinNoise.MaxPermutationValue, maxValue);

		int x = Mathf.Abs (World.Seed + Longitude);
		int y = Mathf.Abs (World.Seed + Latitude);
		int z = Mathf.Abs (World.Seed + World.CurrentDate + LocalIteration);

		LocalIteration++;

		return PerlinNoise.GetPermutationValue(x, y, z) % maxValue;
	}
	
	public float GetNextLocalRandomFloat () {

		int value = GetNextLocalRandomInt ();
		
		return value / (float)PerlinNoise.MaxPermutationValue;
	}

	public float GetBiomePresence (Biome biome) {

		for (int i = 0; i < PresentBiomeNames.Count; i++) {

			if (biome.Name == PresentBiomeNames[i])
			{
				return BiomePresences[i];
			}
		}

		return 0;
	}

	public float GetStress () {
		
		if (Groups.Count <= 0)
			return 0;

		float totalStress = 0;
		
		foreach (CellGroup group in Groups) {
			
			totalStress += group.Stress;
		}

		totalStress /= (float)Groups.Count;

		return totalStress;
	}

	public void FinalizeLoad () {
		
		InitializeNeighbors ();

		foreach (CellGroup group in Groups) {
		
			group.World = World;
			group.Cell = this;

			World.AddGroup(group);
		}
	}

	public void InitializeNeighbors () {
		
		Neighbors = GetNeighborCells ();
	}

	private TerrainCell GetNeighborCell (int direction) {

		int latitude = Latitude;
		int longitude = Longitude;

		switch (direction) {

		case 0:
			latitude++;
			break;
		case 1:
			latitude++;
			longitude++;
			break;
		case 2:
			longitude++;
			break;
		case 3:
			latitude--;
			longitude++;
			break;
		case 4:
			latitude--;
			break;
		case 5:
			latitude--;
			longitude--;
			break;
		case 6:
			longitude--;
			break;
		case 7:
			latitude++;
			longitude--;
			break;

		default:
			throw new System.Exception ("Unexpected direction: " + direction);
		}

		if ((latitude < 0) || (latitude >= World.Height)) {
		
			return null;
		}

		longitude = (int)Mathf.Repeat (longitude, World.Width);

		return World.Terrain [longitude] [latitude];
	}
	
	public List<TerrainCell> GetNeighborCells () {
		
		List<TerrainCell> neighbors = new List<TerrainCell> ();
		
		int wLongitude = (World.Width + Longitude - 1) % World.Width;
		int eLongitude = (Longitude + 1) % World.Width;
		
		if (Latitude < (World.Height - 1)) {
			
			neighbors.Add(World.Terrain[wLongitude][Latitude + 1]);
			neighbors.Add(World.Terrain[Longitude][Latitude + 1]);
			neighbors.Add(World.Terrain[eLongitude][Latitude + 1]);
		}
		
		neighbors.Add(World.Terrain[wLongitude][Latitude]);
		neighbors.Add(World.Terrain[eLongitude][Latitude]);
		
		if (Latitude > 0) {
			
			neighbors.Add(World.Terrain[wLongitude][Latitude - 1]);
			neighbors.Add(World.Terrain[Longitude][Latitude - 1]);
			neighbors.Add(World.Terrain[eLongitude][Latitude - 1]);
		}
		
		return neighbors;
	}
}
