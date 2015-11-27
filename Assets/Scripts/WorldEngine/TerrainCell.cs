using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TerrainCellChanges {

	[XmlAttribute]
	public int Longitude;
	[XmlAttribute]
	public int Latitude;
	[XmlAttribute]
	public int LocalIteration = 0;

	public TerrainCellChanges () {
		
		Manager.UpdateWorldLoadTrackEventCount ();
	}
	
	public TerrainCellChanges (TerrainCell cell) {

		Longitude = cell.Longitude;
		Latitude = cell.Latitude;
	}
}

public class TerrainCell {
	
	[XmlAttribute]
	public int Longitude;
	[XmlAttribute]
	public int Latitude;
	
	[XmlAttribute]
	public float Height;
	[XmlAttribute]
	public float Width;

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
	public float Accessibility;
	
	public List<string> PresentBiomeNames = new List<string>();
	public List<float> BiomePresences = new List<float>();
	
	public CellGroup Group;
	
	[XmlIgnore]
	public float Area;
	
	[XmlIgnore]
	public World World;
	
	[XmlIgnore]
	public static float MaxArea;
	
	[XmlIgnore]
	public bool IsObserved = false;
	
	[XmlIgnore]
	public List<TerrainCell> Neighbors { get; private set; }
	
	[XmlIgnore]
	private TerrainCellChanges _changes = null;

	public int LocalIteration {

		get {

			TerrainCellChanges changes = GetChanges ();

			return changes.LocalIteration;
		}

		set {
			
			TerrainCellChanges changes = GetChanges ();

			changes.LocalIteration = value;

			World.AddTerrainCellChanges (changes);
		}
	}

	public TerrainCell () {

	}

	public TerrainCell (World world, int longitude, int latitude, float height, float width) {
	
		World = world;
		Longitude = longitude;
		Latitude = latitude;

		Height = height;
		Width = width;

		Area = height * width;
	}

	public TerrainCellChanges GetChanges () {
		
		if (_changes == null) {
			
			_changes = World.GetTerrainCellChanges (this);
			
			if (_changes == null)
				_changes = new TerrainCellChanges (this);
		}

		return _changes;
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

		return GetBiomePresence (biome.Name);
	}
	
	public float GetBiomePresence (string biomeName) {
		
		for (int i = 0; i < PresentBiomeNames.Count; i++) {
			
			if (biomeName == PresentBiomeNames[i])
			{
				return BiomePresences[i];
			}
		}
		
		return 0;
	}

	public void FinalizeLoad () {
		
		InitializeNeighbors ();

		if (Group != null) {
		
			Group.World = World;
			Group.Cell = this;

			World.AddGroup(Group);

			Group.FinalizeLoad ();
		}
	}

	public float CalculatePopulationStress () {
		
		if ((Group != null) && (Group.StillPresent)) {

			float groupStress = 1;

			if (Group.OptimalPopulation > 0)
				groupStress = Group.Population / (float)Group.OptimalPopulation;

			groupStress = 0.50f + (0.50f * groupStress);

			return Mathf.Min (1, groupStress);
		}

		return 0;
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

		return World.TerrainCells [longitude] [latitude];
	}
	
	private List<TerrainCell> GetNeighborCells () {
		
		List<TerrainCell> neighbors = new List<TerrainCell> ();
		
		int wLongitude = (World.Width + Longitude - 1) % World.Width;
		int eLongitude = (Longitude + 1) % World.Width;
		
		if (Latitude < (World.Height - 1)) {
			
			neighbors.Add(World.TerrainCells[wLongitude][Latitude + 1]);
			neighbors.Add(World.TerrainCells[Longitude][Latitude + 1]);
			neighbors.Add(World.TerrainCells[eLongitude][Latitude + 1]);
		}
		
		neighbors.Add(World.TerrainCells[wLongitude][Latitude]);
		neighbors.Add(World.TerrainCells[eLongitude][Latitude]);
		
		if (Latitude > 0) {
			
			neighbors.Add(World.TerrainCells[wLongitude][Latitude - 1]);
			neighbors.Add(World.TerrainCells[Longitude][Latitude - 1]);
			neighbors.Add(World.TerrainCells[eLongitude][Latitude - 1]);
		}
		
		return neighbors;
	}
}
