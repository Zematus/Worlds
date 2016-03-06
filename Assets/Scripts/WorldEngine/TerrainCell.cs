using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public enum Direction {

	North = 0,
	Northeast = 1,
	East = 2,
	Southeast = 3,
	South = 4,
	Southwest = 5,
	West = 6,
	Northwest = 7
}

public struct WorldPosition {

	[XmlAttribute]
	public int Longitude;
	[XmlAttribute]
	public int Latitude;

	public WorldPosition (int longitude, int latitude) {

		Longitude = longitude;
		Latitude = latitude;
	}
}

public class TerrainCellChanges {

	[XmlAttribute]
	public int Longitude;
	[XmlAttribute]
	public int Latitude;

	[XmlAttribute]
	public int LocalIteration = 0;
	[XmlAttribute]
	public float FarmlandPercentage = 0;

	public List<string> Flags = new List<string> ();

	public TerrainCellChanges () {
		
		Manager.UpdateWorldLoadTrackEventCount ();
	}
	
	public TerrainCellChanges (TerrainCell cell) {

		Longitude = cell.Longitude;
		Latitude = cell.Latitude;

		LocalIteration = cell.LocalIteration;
		FarmlandPercentage = cell.FarmlandPercentage;
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
	[XmlAttribute]
	public float Arability;

	[XmlAttribute]
	public bool IsPartOfCoastline;

	[XmlAttribute]
	public int LocalIteration = 0;
	[XmlAttribute]
	public float FarmlandPercentage = 0;

	public static float MaxArea;

	public static float MaxWidth;

	public List<string> PresentBiomeNames = new List<string>();
	public List<float> BiomePresences = new List<float>();
	
	public CellGroup Group;

	[XmlIgnore]
	public List<Route> CrossingRoutes = new List<Route>();

	[XmlIgnore]
	public bool HasCrossingRoutes = false;
	
	[XmlIgnore]
	public float Area;
	
	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public bool IsSelected = false;
	
	[XmlIgnore]
	public bool IsObserved = false;
	
	[XmlIgnore]
	public Dictionary<Direction, TerrainCell> Neighbors { get; private set; }

	private HashSet<string> _flags = new HashSet<string> ();

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

	public WorldPosition Position {

		get { 
		
			return new WorldPosition (Longitude, Latitude);
		}
	}

	public TerrainCellChanges GetChanges () {

		// If there where no changes there's no need to create a TerrainCellChanges object
		if ((LocalIteration == 0) && 
			(FarmlandPercentage == 0) && 
			(_flags.Count == 0))
			return null;

		TerrainCellChanges changes = new TerrainCellChanges (this);

		changes.Flags.AddRange (_flags);

		return changes;
	}

	public void SetChanges (TerrainCellChanges changes) {

		LocalIteration = changes.LocalIteration;
		FarmlandPercentage = changes.FarmlandPercentage;

		foreach (string flag in changes.Flags) {
		
			_flags.Add (flag);
		}
	}

	public void SetFlag (string flag) {

		if (_flags.Contains (flag))
			return;

		_flags.Add (flag);
	}

	public bool IsFlagSet (string flag) {

		return _flags.Contains (flag);
	}

	public void UnsetFlag (string flag) {

		if (!_flags.Contains (flag))
			return;

		_flags.Remove (flag);
	}

	public void AddCrossingRoute (Route route) {
	
		CrossingRoutes.Add (route);

		HasCrossingRoutes = true;
	}

	public void RemoveCrossingRoute (Route route) {

		CrossingRoutes.Remove (route);

		HasCrossingRoutes = CrossingRoutes.Count > 0;
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

//	public float CalculatePopulationStress () {
//		
//		if ((Group != null) && (Group.StillPresent)) {
//
//			float groupStress = 1;
//
//			if (Group.OptimalPopulation > 0)
//				groupStress = Group.Population / (float)Group.OptimalPopulation;
//
//			groupStress = 0.50f + (0.50f * groupStress);
//
//			return Mathf.Min (1, groupStress);
//		}
//
//		return 0;
//	}

	public void InitializeNeighbors () {
		
		Neighbors = FindNeighborCells ();
	}

	public void InitializeMiscellaneous () {

		IsPartOfCoastline = FindIfCoastline ();
	}

	public TerrainCell GetNeighborCell (Direction direction) {

		TerrainCell nCell;

		if (!Neighbors.TryGetValue (direction, out nCell))
			return null;

		return nCell;
	}
	
	private Dictionary<Direction,TerrainCell> FindNeighborCells () {
		
		Dictionary<Direction,TerrainCell> neighbors = new Dictionary<Direction,TerrainCell> ();
		
		int wLongitude = (World.Width + Longitude - 1) % World.Width;
		int eLongitude = (Longitude + 1) % World.Width;
		
		if (Latitude < (World.Height - 1)) {
			
			neighbors.Add(Direction.Northwest, World.TerrainCells[wLongitude][Latitude + 1]);
			neighbors.Add(Direction.North, World.TerrainCells[Longitude][Latitude + 1]);
			neighbors.Add(Direction.Northeast, World.TerrainCells[eLongitude][Latitude + 1]);
		}
		
		neighbors.Add(Direction.West, World.TerrainCells[wLongitude][Latitude]);
		neighbors.Add(Direction.East, World.TerrainCells[eLongitude][Latitude]);
		
		if (Latitude > 0) {
			
			neighbors.Add(Direction.Southwest, World.TerrainCells[wLongitude][Latitude - 1]);
			neighbors.Add(Direction.South, World.TerrainCells[Longitude][Latitude - 1]);
			neighbors.Add(Direction.Southeast, World.TerrainCells[eLongitude][Latitude - 1]);
		}
		
		return neighbors;
	}

	private bool FindIfCoastline () {

		if (Altitude <= 0) {

			foreach (TerrainCell nCell in Neighbors.Values) {
				
				if (nCell.Altitude > 0)
					return true;
			}

		} else {

			foreach (TerrainCell nCell in Neighbors.Values) {

				if (nCell.Altitude <= 0)
					return true;
			}
		}

		return false;
	}
}
