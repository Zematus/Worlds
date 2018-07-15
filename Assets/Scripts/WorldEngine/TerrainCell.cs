using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public enum Direction {

	Null = -1,
	North = 0,
	Northeast = 1,
	East = 2,
	Southeast = 3,
	South = 4,
	Southwest = 5,
	West = 6,
	Northwest = 7
}

public enum CellUpdateType {

	None = 0x0,
	Cell = 0x1,
	Group = 0x2,
	Region = 0x4,
	Territory = 0x8,
	Route = 0x10,
    All = 0xFF
}

public struct WorldPosition {

	[XmlAttribute("Lon")]
	public int Longitude;
	[XmlAttribute("Lat")]
	public int Latitude;

	public WorldPosition (int longitude, int latitude) {

		Longitude = longitude;
		Latitude = latitude;
	}

	public override string ToString ()
	{
		return string.Format ("[" + Longitude + "," + Latitude + "]");
	}

	public bool Equals (int longitude, int latitude) {

		return ((Longitude == longitude) && (Latitude == latitude));
	}

	public bool Equals (WorldPosition p) {

		return Equals (p.Longitude, p.Latitude);
	}

	public override bool Equals (object p) {

		if (p is WorldPosition)
			return Equals ((WorldPosition)p);
		
		return false;
	}

	public override int GetHashCode ()
	{
		int hash = 91 + Longitude.GetHashCode();
		hash = (hash * 7) + Latitude.GetHashCode();

		return hash;
	}

	public static bool operator ==(WorldPosition p1, WorldPosition p2) 
	{
		return p1.Equals(p2);
	}

	public static bool operator !=(WorldPosition p1, WorldPosition p2) 
	{
		return !p1.Equals(p2);
	}
}

public class TerrainCellChanges {

	[XmlAttribute("Lon")]
	public int Longitude;
	[XmlAttribute("Lat")]
	public int Latitude;

//	[XmlAttribute("It")]
//	public int LocalIteration = 0;
	[XmlAttribute("Fp")]
	public float FarmlandPercentage = 0;

	public List<string> Flags = new List<string> ();

	public TerrainCellChanges () {
		
		Manager.UpdateWorldLoadTrackEventCount ();
	}
	
	public TerrainCellChanges (TerrainCell cell) {

		Longitude = cell.Longitude;
		Latitude = cell.Latitude;

//		LocalIteration = cell.LocalIteration;
		FarmlandPercentage = cell.FarmlandPercentage;
	}
}

public class TerrainCell : ISynchronizable {

	#if DEBUG

	public delegate void GetNextLocalRandomCalledDelegate (string callerMethod);

//	public static int LastRandomInteger = 0; 
	public static GetNextLocalRandomCalledDelegate GetNextLocalRandomCalled = null; 

	#endif

	public const int MaxNeighborDirections = 8;
	public const int NeighborSearchOffset = 3;
	
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
//
//	[XmlAttribute]
//	public int LocalIteration = 0;
	[XmlAttribute]
	public float FarmlandPercentage = 0;

	[XmlAttribute]
	public string BiomeWithMostPresence = null;
	[XmlAttribute]
	public float MostBiomePresence = 0;

	public static float MaxArea;

	public static float MaxWidth;

	public List<string> PresentBiomeNames = new List<string>();
	public List<float> BiomePresences = new List<float>();
	
	public CellGroup Group;

	[XmlIgnore]
	public WorldPosition Position;

	[XmlIgnore]
	public Region Region = null;

	[XmlIgnore]
	public Territory EncompassingTerritory = null;

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
	public Dictionary<Direction, TerrainCell> Neighbors { get; private set; }

	[XmlIgnore]
	public Dictionary<Direction, float> NeighborDistances { get; private set; }

	private HashSet<string> _flags = new HashSet<string> ();

	private Dictionary<string, float> _biomePresences = new Dictionary<string, float> ();

	public TerrainCell () {
		
	}

	public TerrainCell (World world, int longitude, int latitude, float height, float width) {

		Position = new WorldPosition (longitude, latitude);
	
		World = world;
		Longitude = longitude;
		Latitude = latitude;

		Height = height;
		Width = width;

		Area = height * width;
	}

	public static Direction ReverseDirection (Direction dir) {

		return (Direction)(((int)dir + 4) % 8);
	}

	public Direction GetDirection (TerrainCell targetCell) {

		foreach (KeyValuePair<Direction, TerrainCell> pair in Neighbors) {

			if (pair.Value == targetCell) {
			
				return pair.Key;
			}
		}

		return Direction.Null;
	}

	public Direction TryGetNeighborDirection (int offset) {

		if (Neighbors.Count <= 0)
			return Direction.Null;

		int dir = (int)Mathf.Repeat (offset, MaxNeighborDirections);

		while (true) {
			if (Neighbors.ContainsKey ((Direction)dir))
				return (Direction)dir;

			dir = (dir + NeighborSearchOffset) % MaxNeighborDirections;
		}
	}

	public TerrainCell TryGetNeighborCell (int offset) {

		return Neighbors[TryGetNeighborDirection (offset)];
	}

	public long GenerateUniqueIdentifier (long date, long oom = 1L, long offset = 0L) {

		#if DEBUG
		if (oom > 1000L) {
			Debug.LogWarning ("'oom' shouldn't be greater than 1000 (oom = " + oom + ")");
		}

		if (date >= World.MaxSupportedDate) {
			Debug.LogWarning ("'date' shouldn't be greater than " + World.MaxSupportedDate + " (date = " + date + ")");
		}
		#endif

		return (((date * 1000000) + ((long)Longitude * 1000) + (long)Latitude) * oom) + (offset % oom);
	}

	public TerrainCellChanges GetChanges () {

		// If there where no changes there's no need to create a TerrainCellChanges object
		if ((FarmlandPercentage == 0) && 
//			(LocalIteration == 0) && 
			(_flags.Count == 0))
			return null;

		TerrainCellChanges changes = new TerrainCellChanges (this);

		changes.Flags.AddRange (_flags);

		return changes;
	}

	public void SetChanges (TerrainCellChanges changes) {

//		LocalIteration = changes.LocalIteration;
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

	public int GetNextLocalRandomInt(int queryOffset, int maxValue)
    {
        int value = GetLocalRandomInt(World.CurrentDate, queryOffset, maxValue);

#if DEBUG
        if (GetNextLocalRandomCalled != null)
        {
            if (Manager.TrackGenRandomCallers)
            {

                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
                string callingMethod = method.Name;

                int frame = 2;
                while (callingMethod.Contains("GetNextLocalRandom")
                    || callingMethod.Contains("GetNextRandom"))
                {
                    method = stackTrace.GetFrame(frame).GetMethod();
                    callingMethod = method.Name;

                    frame++;
                }

                string callingClass = method.DeclaringType.ToString();

                GetNextLocalRandomCalled(callingClass + ":" + callingMethod);

            }
            else
            {
                GetNextLocalRandomCalled(null);
            }
        }
#endif

        //		#if DEBUG
        //		if (Manager.RecordingEnabled) {
        //			LastRandomInteger = value;
        //			string key = "Long:" + Longitude + "-Lat:" + Latitude + "-Date:" + date + "-LocalIteration:" + LocalIteration;

        //			Manager.Recorder.Record (key, "LastRandomInteger:" + value);
        //		}
        //		#endif

        return value;
    }

    public int GetLocalRandomInt(long date, int queryOffset, int maxValue)
    {
        if (maxValue < 2) return 0;

        maxValue = Mathf.Min(PerlinNoise.MaxPermutationValue, maxValue);

        long dateFactor = (date % 256) + (date % 7843); // This operation will reduce to zero or almost zero the number of artifacts resulting from (date & 255) being a constant value in some circumstances

        int x = Mathf.Abs(World.Seed + Longitude + queryOffset);
        int y = Mathf.Abs(World.Seed + Latitude + queryOffset);
        int z = Mathf.Abs(World.Seed + (int)dateFactor + queryOffset);

        int value = PerlinNoise.GetPermutationValue(x, y, z) % maxValue;

        return value;
    }

    public float GetNextLocalRandomFloat (int queryOffset) {

		int value = GetNextLocalRandomInt (queryOffset, PerlinNoise.MaxPermutationValue);

		return value / (float)PerlinNoise.MaxPermutationValue;
	}
	
	public float GetLocalRandomFloat (long date, int queryOffset) {

		int value = GetLocalRandomInt (date, queryOffset, PerlinNoise.MaxPermutationValue);
		
		return value / (float)PerlinNoise.MaxPermutationValue;
	}

	public float GetBiomePresence (Biome biome) {

		return GetBiomePresence (biome.Name);
	}

	public void AddBiomePresence (string biomeName, float presence) {

		PresentBiomeNames.Add (biomeName);
		BiomePresences.Add (presence);

		_biomePresences [biomeName] = presence;

		if (MostBiomePresence < presence) {
		
			MostBiomePresence = presence;
			BiomeWithMostPresence = biomeName;
		}
	}
	
	public float GetBiomePresence (string biomeName) {

		float value = 0;

		if (!_biomePresences.TryGetValue (biomeName, out value))
			return 0;
		
		return value;
	}

	public void Synchronize () {
	}

	public void FinalizeLoad () {

		Position = new WorldPosition (Longitude, Latitude);

		for (int i = 0; i < BiomePresences.Count; i++) {
		
			_biomePresences [PresentBiomeNames [i]] = BiomePresences [i];
		}
		
		InitializeNeighbors ();

		if (Group != null) {
		
			Group.World = World;
			Group.Cell = this;

			World.AddGroup(Group);

			Group.FinalizeLoad ();
		}
	}

	public void InitializeNeighbors () {
		
		SetNeighborCells ();

		NeighborDistances = new Dictionary<Direction, float> (Neighbors.Count);

		foreach (KeyValuePair<Direction, TerrainCell> pair in Neighbors) {

			float distance = CalculateDistance (pair.Value, pair.Key);

			NeighborDistances.Add (pair.Key, distance);
		}
	}

	public float CalculateDistance (TerrainCell cell, Direction direction) {

		float distance = TerrainCell.MaxWidth;

		if ((direction == Direction.Northeast) ||
			(direction == Direction.Northwest) ||
			(direction == Direction.Southeast) ||
			(direction == Direction.Southwest)) {

			float sqMaxWidth = TerrainCell.MaxWidth * TerrainCell.MaxWidth;

			float widthSum = (Width + cell.Width) / 2f;
			float sqCellWidth = widthSum * widthSum;

			distance = Mathf.Sqrt (sqMaxWidth + sqCellWidth);

		} else if ((direction == Direction.East) ||
			(direction == Direction.West)) {

			distance = cell.Width;
		}

		float altitudeDiff = Altitude - cell.Altitude;
		float sqAltDif = altitudeDiff * altitudeDiff;
		float sqDistance = distance * distance;

		distance = Mathf.Sqrt (sqAltDif + sqDistance);

		return distance;
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
	
	private void SetNeighborCells () {
		
		Neighbors = new Dictionary<Direction,TerrainCell> (8);
		
		int wLongitude = (World.Width + Longitude - 1) % World.Width;
		int eLongitude = (Longitude + 1) % World.Width;
		
		if (Latitude < (World.Height - 1)) {
			
			Neighbors.Add (Direction.Northwest, World.TerrainCells[wLongitude][Latitude + 1]);
			Neighbors.Add (Direction.North, World.TerrainCells[Longitude][Latitude + 1]);
			Neighbors.Add (Direction.Northeast, World.TerrainCells[eLongitude][Latitude + 1]);
		}
		
		Neighbors.Add (Direction.West, World.TerrainCells[wLongitude][Latitude]);
		Neighbors.Add (Direction.East, World.TerrainCells[eLongitude][Latitude]);
		
		if (Latitude > 0) {
			
			Neighbors.Add (Direction.Southwest, World.TerrainCells[wLongitude][Latitude - 1]);
			Neighbors.Add (Direction.South, World.TerrainCells[Longitude][Latitude - 1]);
			Neighbors.Add (Direction.Southeast, World.TerrainCells[eLongitude][Latitude - 1]);
		}
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
