using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public delegate void ProgressCastDelegate (float value, string message = null);

[XmlRoot]
public class World {
	
	public const float Circumference = 40075; // In kilometers;

	public const int NumContinents = 12;
	public const float ContinentMinWidthFactor = 5.7f;
	public const float ContinentMaxWidthFactor = 8.7f;

	public const float MinPossibleAltitude = -15000;
	public const float MaxPossibleAltitude = 15000;
	
	public const float MinPossibleRainfall = 0;
	public const float MaxPossibleRainfall = 13000;
	public const float RainfallDrynessOffsetFactor = 0.005f;
	
	public const float MinPossibleTemperature = -40;
	public const float MaxPossibleTemperature = 50;

	public const float StartPopulationDensity = 0.5f;
	
	public float MinPossibleAltitudeWithOffset = MinPossibleAltitude - Manager.SeaLevelOffset;
	public float MaxPossibleAltitudeWithOffset = MaxPossibleAltitude - Manager.SeaLevelOffset;
	
	public float MinPossibleRainfallWithOffset = MinPossibleRainfall + Manager.RainfallOffset;
	public float MaxPossibleRainfallWithOffset = MaxPossibleRainfall + Manager.RainfallOffset;
	
	public float MinPossibleTemperatureWithOffset = MinPossibleTemperature + Manager.TemperatureOffset;
	public float MaxPossibleTemperatureWithOffset = MaxPossibleTemperature + Manager.TemperatureOffset;

	public float MaxAltitude = float.MinValue;
	public float MinAltitude = float.MaxValue;
	
	public float MaxRainfall = float.MinValue;
	public float MinRainfall = float.MaxValue;

	public float MaxTemperature = float.MinValue;
	public float MinTemperature = float.MaxValue;

	public int MostPopulousGroupId = -1;
	
	[XmlIgnore]
	public CellGroup MostPopulousGroup = null;
	
	[XmlIgnore]
	public bool Ready { get; private set; }
	
	[XmlIgnore]
	public ProgressCastDelegate ProgressCastMethod { get; set; }
	
	[XmlIgnore]
	public HumanGroup TaggedGroup = null;
	
	[XmlIgnore]
	public TerrainCell ObservedCell = null;
	
	[XmlAttribute]
	public int CurrentDate { get; private set; }
	
	[XmlAttribute]
	public int Width { get; private set; }
	[XmlAttribute]
	public int Height { get; private set; }

	[XmlAttribute]
	public int EventsToHappenCount { get; private set; }
	
	[XmlAttribute]
	public int Seed { get; private set; }

	public int CurrentCellGroupId { get; private set; }

	public TerrainCell[][] Terrain;

	public List<WorldEvent> EventsToHappen = new List<WorldEvent> ();
	
	private HashSet<CellGroup> _updatedGroups = new HashSet<CellGroup>();
		
	private const float _progressIncrement = 0.25f;
	
	private List<CellGroup> _groups = new List<CellGroup>();
	
	private List<CellGroup> _groupsToUpdate = new List<CellGroup>();
	private List<CellGroup> _groupsToRemove = new List<CellGroup>();

	private List<MigratingGroup> _migratingGroups = new List<MigratingGroup> ();

	private Vector2[] _continentOffsets;
	private float[] _continentWidths;
	private float[] _continentHeights;

	private bool _initialized = false;

	private float _accumulatedProgress = 0;

	private float _cellMaxSideLength;

	public World () {
		
		ProgressCastMethod = (value, message) => {};

		Manager.LoadingWorld = this;

		Ready = false;
	}

	public World (int width, int height, int seed) {

		ProgressCastMethod = (value, message) => {};

		Ready = false;
	
		Width = width;
		Height = height;
		Seed = seed;

		CurrentCellGroupId = 0;

		_cellMaxSideLength = Circumference / Width;
		TerrainCell.MaxArea = _cellMaxSideLength * _cellMaxSideLength;
		
		CurrentDate = 0;

		Terrain = new TerrainCell[width][];

		for (int i = 0; i < width; i++)
		{
			TerrainCell[] column = new TerrainCell[height];

			for (int j = 0; j < height; j++)
			{
				float alpha = (j / (float)height) * Mathf.PI;

				TerrainCell cell = new TerrainCell (false);
				cell.World = this;
				cell.Longitude = i;
				cell.Latitude = j;
				cell.LocalIteration = 0;

				cell.Height = _cellMaxSideLength;
				cell.Width = Mathf.Sin(alpha) * _cellMaxSideLength;

				cell.Area = cell.Height * cell.Width;

				cell.Ready = false;

				column[j] = cell;
			}

			Terrain[i] = column;
		}
		
		for (int i = 0; i < width; i++) {

			for (int j = 0; j < height; j++) {

				Terrain[i][j].InitializeNeighbors();
			}
		}

		_continentOffsets = new Vector2[NumContinents];
		_continentHeights = new float[NumContinents];
		_continentWidths = new float[NumContinents];
	}

	public void UpdateMostPopulousGroup (CellGroup contenderGroup) {
	
		if (MostPopulousGroup == null) {

			MostPopulousGroup = contenderGroup;

		} else if (MostPopulousGroup.Population < contenderGroup.Population) {
			
			MostPopulousGroup = contenderGroup;
		}
		
		if (MostPopulousGroup != null) {
			MostPopulousGroupId = MostPopulousGroup.Id;
		} else {
			MostPopulousGroupId = -1;
		}
	}
	
	public void AddUpdatedGroup (CellGroup group) {
		
		_updatedGroups.Add (group);
	}

	public void Iterate () {
		
		//
		// Evaluate Events that will happen currently
		//

		int DateToSkipTo = CurrentDate + 1;

		while (true) {

			if (EventsToHappen.Count <= 0) break;
		
			WorldEvent eventToHappen = EventsToHappen[0];

			if (eventToHappen.TriggerDate > CurrentDate) {

				DateToSkipTo = eventToHappen.TriggerDate;
				break;
			}

			EventsToHappen.Remove(eventToHappen);
			EventsToHappenCount--;

			if (eventToHappen.CanTrigger())
			{
				eventToHappen.Trigger();
			}
		}
		
		//
		// Update Human Groups
		//

		CellGroup[] currentGroupsToUpdate = _groupsToUpdate.ToArray();
		
		_groupsToUpdate.Clear ();
	
		foreach (CellGroup group in currentGroupsToUpdate) {
		
			group.Update();
		}
		
		//
		// Migrate Human Groups
		//
		
		MigratingGroup[] currentGroupsToMigrate = _migratingGroups.ToArray();
		
		_migratingGroups.Clear ();
		
		foreach (MigratingGroup group in currentGroupsToMigrate) {
			
			group.SplitFromSourceGroup();
		}
		
		foreach (MigratingGroup group in currentGroupsToMigrate) {

			group.MoveToCell();
		}
		
		//
		// Set next group updates
		//
		
		foreach (CellGroup group in _updatedGroups) {
			
			group.SetupForNextUpdate ();
			Manager.AddUpdatedCell (group.Cell);
		}

		_updatedGroups.Clear ();
		
		//
		// Remove Human Groups
		//

		foreach (CellGroup group in _groupsToRemove) {
			
			group.Destroy();
		}

		_groupsToRemove.Clear ();

		//
		// Skip to Next Event's Date
		//

		if (EventsToHappen.Count > 0) {

			WorldEvent futureEventToHappen = EventsToHappen [0];
			
			if (futureEventToHappen.TriggerDate > DateToSkipTo) {
				
				DateToSkipTo = futureEventToHappen.TriggerDate;
			}
		}

		CurrentDate = DateToSkipTo;
	}

	public void InsertEventToHappen (WorldEvent eventToHappen) {

		EventsToHappenCount++;

		if (EventsToHappen.Count == 0) {

			EventsToHappen.Add(eventToHappen);
			return;
		}

		for (int i = EventsToHappen.Count - 1; i >= 0; i--) {
		
			if (eventToHappen.TriggerDate >= EventsToHappen[i].TriggerDate) {

				EventsToHappen.Insert(i + 1, eventToHappen);
				return;
			}
		}
		
		EventsToHappen.Insert(0, eventToHappen);
	}
	
	public void AddMigratingGroup (MigratingGroup group) {
		
		_migratingGroups.Add (group);
	}
	
	public void AddGroup (CellGroup group) {
		
		_groups.Add (group);

		Manager.AddUpdatedCell (group.Cell);
	}
	
	public void RemoveGroup (CellGroup group) {
		
		_groups.Remove (group);
	}
	
	public CellGroup FindCellGroup (int id) {

		foreach (CellGroup group in _groups) {
		
			if (group.Id == id) return group;
		}

		return null;
	}

	public void AddGroupToUpdate (CellGroup group) {

		if (_groupsToUpdate.Contains (group))
			return;
	
		_groupsToUpdate.Add (group);
	}
	
	public void AddGroupToRemove (CellGroup group) {
		
		if (_groupsToRemove.Contains (group))
			return;
		
		_groupsToRemove.Add (group);
	}

	public void FinalizeLoad () {
		
		_cellMaxSideLength = Circumference / Width;
		TerrainCell.MaxArea = _cellMaxSideLength * _cellMaxSideLength;
		
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				TerrainCell cell = Terrain[i][j];

				cell.World = this;
				
				cell.Area = cell.Height * cell.Width;

				cell.FinalizeLoad();
			}
		}

		foreach (WorldEvent eventToHappen in EventsToHappen) {
		
			eventToHappen.World = this;

			eventToHappen.FinalizeLoad();
		}

		EventsToHappen.Sort ((a, b) => {

			if (a.TriggerDate < b.TriggerDate) return -1;
			if (a.TriggerDate > b.TriggerDate) return 1;

			return 0;
		});

		if (MostPopulousGroupId > -1) {
		
			MostPopulousGroup = FindCellGroup(MostPopulousGroupId);

			if (MostPopulousGroup == null)
				MostPopulousGroupId = -1;
		}

		Ready = true;
	}

	public void TagGroup (HumanGroup group) {
	
		UntagGroup ();
		
		TaggedGroup = group;

		group.IsTagged = true;
	}
	
	public void UntagGroup () {
		
		if (TaggedGroup != null)
			TaggedGroup.IsTagged = false;
	}
	
	public void SetObservedCell (TerrainCell cell) {
		
		if (ObservedCell != null)
			ObservedCell.IsObserved = false;
		
		ObservedCell = cell;
		
		if (cell != null)
			cell.IsObserved = true;
	}
	
	public void FinalizeGeneration () {
		
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				TerrainCell cell = Terrain[i][j];

				cell.Ready = true;
			}
		}
		
		Ready = true;
	}

	public void Initialize () {

		if (_initialized)
			return;

		_initialized = true;

		Manager.EnqueueTaskAndWait (() => {

			Random.seed = Seed;
			return true;
		});
	}

	public void Generate () {

		ProgressCastMethod (_accumulatedProgress, "Generating Terrain...");

		GenerateTerrainAltitude ();
		
		ProgressCastMethod (_accumulatedProgress, "Calculating Rainfall...");

		GenerateTerrainRainfall ();
		
//		ProgressCastMethod (_accumulatedProgress, "Generating Rivers...");
//		
//		GenerateTerrainRivers ();
		
		ProgressCastMethod (_accumulatedProgress, "Calculating Temperatures...");

		GenerateTerrainTemperature ();
		
		ProgressCastMethod (_accumulatedProgress, "Generating Biomes...");

		GenerateTerrainBiomes ();
		
		ProgressCastMethod (_accumulatedProgress, "Setting Initial Human Groups...");

		SetInitialHumanGroups ();
		
		ProgressCastMethod (_accumulatedProgress, "Finalizing...");

		Ready = true;
	}

	private void GenerateContinents () {
		
		float longitudeFactor = 15f;
		float latitudeFactor = 6f;
		
		Manager.EnqueueTaskAndWait (() => {
			
			Vector2 prevPos = new Vector2(Random.Range(0, Width * (longitudeFactor - 1f) / longitudeFactor),
			                              Random.Range(Height / latitudeFactor, Height * (latitudeFactor - 1f) / latitudeFactor));

			for (int i = 0; i < NumContinents; i++) {

				int widthOff = Random.Range(0, 2) * 3;
				
				_continentOffsets[i] = prevPos;
				_continentWidths[i] = Random.Range(ContinentMinWidthFactor + widthOff, ContinentMaxWidthFactor + widthOff);
				_continentHeights[i] = Random.Range(ContinentMinWidthFactor + widthOff, ContinentMaxWidthFactor + widthOff);

				float yPos = Random.Range(Height / latitudeFactor, Height * (latitudeFactor - 1f) / latitudeFactor);

				Vector2 newVector = new Vector2(
					Mathf.Repeat(prevPos.x + Random.Range(Width / longitudeFactor, Width * 2 / longitudeFactor), Width),
					yPos);
				
				if (i % 3 == 2) {
					newVector = new Vector2(
						Mathf.Repeat(prevPos.x + Random.Range(Width * 4 / longitudeFactor, Width * 6 / longitudeFactor), Width),
						yPos);
				}

				prevPos = newVector;
			}
			
//			for (int i = 0; i < NumContinents; i++) {
//				
//				_continentOffsets[i] = new Vector2(
//						Mathf.Repeat(Random.Range(Width*i/longitudeFactor, Width*(i + 2)/longitudeFactor), Width),
//						Random.Range(Height / latitudeFactor, Height * (latitudeFactor - 1f) / latitudeFactor));
//				_continentWidths[i] = Random.Range(ContinentMinWidthFactor, ContinentMaxWidthFactor);
//				_continentHeights[i] = Random.Range(ContinentMinWidthFactor, ContinentMaxWidthFactor) / 2f;
//			}
			
			return true;
		});
	}
	
	private float GetContinentModifier (int x, int y) {

		float maxValue = 0;

		for (int i = 0; i < NumContinents; i++)
		{
			float dist = GetContinentDistance(i, x, y);

			float value = Mathf.Clamp(1f - dist/((float)Width), 0 , 1);

			float otherValue = value;

			if (maxValue < value) {

				otherValue = maxValue;
				maxValue = value;
			}

			float valueMod = otherValue;
			otherValue *= 2;
			otherValue = Mathf.Min(1, otherValue);

			maxValue = MathUtility.MixValues(maxValue, otherValue, valueMod);
		}

		return maxValue;
	}

	private float GetContinentDistance (int id, int x, int y) {
		
		float betaFactor = Mathf.Sin(Mathf.PI * y / Height);

		Vector2 continentOffset = _continentOffsets[id];
		float contX = continentOffset.x;
		float contY = continentOffset.y;
		
		float distX = Mathf.Min(Mathf.Abs(contX - x), Mathf.Abs(Width + contX - x));
		distX = Mathf.Min(distX, Mathf.Abs(contX - x - Width));
		distX *= betaFactor;
		
		float distY = Mathf.Abs(contY - y);
		
		float continentWidth = _continentWidths[id];
		float continentHeight = _continentHeights[id];
		
		return new Vector2(distX*continentWidth, distY*continentHeight).magnitude;
	}

	private void GenerateTerrainAltitude () {

		GenerateContinents();
		
		int sizeX = Width;
		int sizeY = Height;

		float radius1 = 0.75f;
		float radius1b = 1.25f;
		float radius2 = 8f;
		float radius3 = 4f;
		float radius4 = 8f;
		float radius5 = 16f;
		float radius6 = 64f;
		float radius7 = 128f;
		float radius8 = 1.5f;
		float radius9 = 1f;

		ManagerTask<Vector3> offset1 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset2 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset1b = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset2b = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset3 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset4 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset5 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset6 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset7 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset8 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset9 = GenerateRandomOffsetVector();
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				float alpha = (j / (float)sizeY) * Mathf.PI;

				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);
				float value1b = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1b, offset1b);
				float value2b = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2b);
				float value3 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius3, offset3);
				float value4 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius4, offset4);
				float value5 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius5, offset5);
				float value6 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius6, offset6);
				float value7 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius7, offset7);
				float value8 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius8, offset8);
				float value9 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius9, offset9);

				value8 = value8 * 1.5f + 0.25f;

				float valueA = GetContinentModifier(i, j);
				valueA = MathUtility.MixValues(valueA, value3, 0.22f * value8);
				valueA = MathUtility.MixValues(valueA, value4, 0.15f * value8);
				valueA = MathUtility.MixValues(valueA, value5, 0.1f * value8);
				valueA = MathUtility.MixValues(valueA, value6, 0.03f * value8);
				valueA = MathUtility.MixValues(valueA, value7, 0.005f * value8);
				
				float valueC = MathUtility.MixValues(value1, value9, 0.5f * value8);
				valueC = MathUtility.MixValues(valueC, value2, 0.04f * value8);
				valueC = GetMountainRangeNoiseFromRandomNoise(valueC, 25);
				float valueCb = MathUtility.MixValues(value1b, value9, 0.5f * value8);
				valueCb = MathUtility.MixValues(valueCb, value2b, 0.04f * value8);
				valueCb = GetMountainRangeNoiseFromRandomNoise(valueCb, 25);
				valueC = MathUtility.MixValues(valueC, valueCb, 0.5f * value8);

				valueC = MathUtility.MixValues(valueC, value3, 0.45f * value8);
				valueC = MathUtility.MixValues(valueC, value4, 0.075f);
				valueC = MathUtility.MixValues(valueC, value5, 0.05f);
				valueC = MathUtility.MixValues(valueC, value6, 0.02f);
				valueC = MathUtility.MixValues(valueC, value7, 0.01f);
				
				float valueB = MathUtility.MixValues(valueA, (valueA * 0.02f) + 0.49f, valueA - Mathf.Max(0, (2 * valueC) - 1));

				float valueD = MathUtility.MixValues(valueB, valueC, 0.225f * value8);

				CalculateAndSetAltitude(i, j, valueD);
//				CalculateAndSetAltitude(i, j, valueA);
			}

			ProgressCastMethod (_accumulatedProgress + _progressIncrement * (i + 1)/(float)sizeX);
		}

		_accumulatedProgress += _progressIncrement;
	}
	
//	private void GenerateTerrainRivers () {
//
//		int sizeX = Width;
//		int sizeY = Height;
//		
//		float radius1 = 4f;
//		float radius2 = 12f;
//		
//		ManagerTask<Vector3> offset1 = GenerateRandomOffsetVector();
//		ManagerTask<Vector3> offset2 = GenerateRandomOffsetVector();
//		
//		for (int i = 0; i < sizeX; i++) {
//
//			float beta = (i / (float)sizeX) * Mathf.PI * 2;
//			
//			for (int j = 0; j < sizeY; j++) {
//
//				TerrainCell cell = Terrain[i][j];
//				
//				float alpha = (j / (float)sizeY) * Mathf.PI;
//				
//				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
//				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);
//				
//				float altitudeValue = cell.Altitude;
//				float rainfallValue = cell.Rainfall;
//
//				float altitudeFactor = Mathf.Max(0, altitudeValue / MaxPossibleAltitude);
//				float depthFactor = Mathf.Max(0, altitudeValue / MinPossibleAltitude);
//				float altitudeFactor1 = Mathf.Clamp(10f * altitudeFactor, 0.25f , 1);
//				float rainfallFactor = Mathf.Max(0, rainfallValue / MaxPossibleRainfall);
//
//				float valueA = MathUtility.MixValues(value2, value1, altitudeFactor1);
//				valueA = GetRiverNoiseFromRandomNoise(valueA, 25);
//
//				if (altitudeValue >= 0) {
//					valueA = valueA * Mathf.Max(1 - altitudeFactor * rainfallFactor * 2.5f, 0);
//				} else {
//					valueA = valueA * Mathf.Max(1 - depthFactor * 8, 0);
//				}
//
//				float altitudeMod = valueA * MaxPossibleAltitude * 0.1f;
//
//				cell.Altitude -= altitudeMod;
//			}
//			
//			ProgressCastMethod (_accumulatedProgress + _progressIncrement * (i + 1)/(float)sizeX);
//		}
//		
//		_accumulatedProgress += _progressIncrement;
//	}
	
	private ManagerTask<int> GenerateRandomInteger (int min, int max) {
		
		return Manager.EnqueueTask (() => Random.Range(min, max));
	}

	private ManagerTask<Vector3> GenerateRandomOffsetVector () {

		return Manager.EnqueueTask (() => Random.insideUnitSphere * 1000);
	}

	private float GetRandomNoiseFromPolarCoordinates (float alpha, float beta, float radius, Vector3 offset) {

		Vector3 pos = MathUtility.GetCartesianCoordinates(alpha,beta,radius) + offset;
		
		return PerlinNoise.GetValue(pos.x, pos.y, pos.z);
	}
	
	private float GetMountainRangeNoiseFromRandomNoise(float noise, float widthFactor) {

		noise = (noise * 2) - 1;
		
		float value1 = -Mathf.Exp (-Mathf.Pow (noise * widthFactor + 1f, 2));
		float value2 = Mathf.Exp(-Mathf.Pow(noise * widthFactor - 1f, 2));

		float value = (value1 + value2 + 1) / 2f;
		
		return value;
	}
	
	private float GetRiverNoiseFromRandomNoise(float noise, float widthFactor) {
		
		noise = (noise * 2) - 1;

		float value = Mathf.Exp(-Mathf.Pow(noise * widthFactor, 2));
		
		value = (value + 1) / 2f;
		
		return value;
	}

	private float CalculateAltitude (float value) {
	
		float span = MaxPossibleAltitude - MinPossibleAltitude;

		float altitude = (value * span) + MinPossibleAltitude;

		altitude -= Manager.SeaLevelOffset;

		altitude = Mathf.Clamp (altitude, MinPossibleAltitudeWithOffset, MaxPossibleAltitudeWithOffset);

		return altitude;
	}
	
	private void CalculateAndSetAltitude (int longitude, int latitude, float value) {
		
		float altitude = CalculateAltitude(value);
		Terrain[longitude][latitude].Altitude = altitude;
		
		if (altitude > MaxAltitude) MaxAltitude = altitude;
		if (altitude < MinAltitude) MinAltitude = altitude;
	}
	
	private void GenerateTerrainRainfall () {
		
		int sizeX = Width;
		int sizeY = Height;
		
		float radius1 = 2f;
		float radius2 = 1f;
		float radius3 = 16f;
		
		ManagerTask<Vector3> offset1 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset2 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset3 = GenerateRandomOffsetVector();
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				TerrainCell cell = Terrain[i][j];
				
				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);
				float value3 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius3, offset3);

				value2 = value2 * 1.5f + 0.25f;

				float valueA = MathUtility.MixValues(value1, value3, 0.15f);

				float latitudeFactor = alpha + (((valueA * 2) - 1f) * Mathf.PI * 0.2f);
				float latitudeModifier1 = (1.5f * Mathf.Sin(latitudeFactor)) - 0.5f;
				float latitudeModifier2 = Mathf.Cos(latitudeFactor);

				int offCellX = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/40f)) % Width;
				int offCellX2 = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/20f)) % Width;
				int offCellX3 = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/10f)) % Width;
				int offCellX4 = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/5f)) % Width;
				int offCellY = j + (int)Mathf.Floor(latitudeModifier2 * Height/10f);

				TerrainCell offCell = Terrain[offCellX][j];
				TerrainCell offCell2 = Terrain[offCellX2][j];
				TerrainCell offCell3 = Terrain[offCellX3][j];
				TerrainCell offCell4 = Terrain[offCellX4][j];
				TerrainCell offCell5 = Terrain[i][offCellY];

				float altitudeValue = Mathf.Max(0, cell.Altitude);
				float offAltitude = Mathf.Max(0, offCell.Altitude);
				float offAltitude2 = Mathf.Max(0, offCell2.Altitude);
				float offAltitude3 = Mathf.Max(0, offCell3.Altitude);
				float offAltitude4 = Mathf.Max(0, offCell4.Altitude);
				float offAltitude5 = Mathf.Max(0, offCell5.Altitude);

				float altitudeModifier = (altitudeValue - 
				                          (offAltitude * 0.7f) - 
				                          (offAltitude2 * 0.6f) -  
				                          (offAltitude3 * 0.5f) -
				                          (offAltitude4 * 0.4f) - 
				                          (offAltitude5 * 0.5f) + 
				                          (MaxPossibleAltitude * 0.17f * value2) -
				                          (altitudeValue * 0.25f)) / MaxPossibleAltitude;
				float rainfallValue = MathUtility.MixValues(latitudeModifier1, altitudeModifier, 0.85f);
				rainfallValue = MathUtility.MixValues(Mathf.Abs(rainfallValue) * rainfallValue, rainfallValue, 0.75f);

				float rainfall = Mathf.Min(MaxPossibleRainfall, CalculateRainfall(rainfallValue));
				cell.Rainfall = rainfall;

				if (rainfall > MaxRainfall) MaxRainfall = rainfall;
				if (rainfall < MinRainfall) MinRainfall = rainfall;
			}
			
			ProgressCastMethod (_accumulatedProgress + _progressIncrement * (i + 1)/(float)sizeX);
		}
		
		_accumulatedProgress += _progressIncrement;
	}
	
	private void GenerateTerrainTemperature () {
		
		int sizeX = Width;
		int sizeY = Height;
		
		float radius1 = 2f;
		float radius2 = 16f;
		
		ManagerTask<Vector3> offset1 = GenerateRandomOffsetVector();
		ManagerTask<Vector3> offset2 = GenerateRandomOffsetVector();
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				TerrainCell cell = Terrain[i][j];
				
				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);

				float latitudeModifier = (alpha * 0.9f) + ((value1 + value2) * 0.05f * Mathf.PI);

				float altitudeSpan = MaxPossibleAltitude - MinPossibleAltitude;

				float absAltitude = cell.Altitude - MinPossibleAltitudeWithOffset;
				
				float altitudeFactor1 = (absAltitude / altitudeSpan) * 0.7f;
				float altitudeFactor2 = Mathf.Max(0, (cell.Altitude / MaxPossibleAltitude) * 1.3f);
				float altitudeFactor3 = -0.18f;
				
				float temperature = CalculateTemperature(Mathf.Sin(latitudeModifier) - altitudeFactor1 - altitudeFactor2 - altitudeFactor3);

				cell.Temperature = temperature;
				
				if (temperature > MaxTemperature) MaxTemperature = temperature;
				if (temperature < MinTemperature) MinTemperature = temperature;
			}
			
			ProgressCastMethod (_accumulatedProgress + _progressIncrement * (i + 1)/(float)sizeX);
		}
		
		_accumulatedProgress += _progressIncrement;
	}

	private void GenerateTerrainBiomes () {
		
		int sizeX = Width;
		int sizeY = Height;

		//float initialPopulationFactor = (1 - HumanGroup.NaturalBirthRate + HumanGroup.NaturalDeathRate) / Biome.Grassland.ForagingCapacity;
		
		for (int i = 0; i < sizeX; i++) {

			for (int j = 0; j < sizeY; j++) {

				TerrainCell cell = Terrain[i][j];

				float totalPresence = 0;

				Dictionary<string, float> biomePresences = new Dictionary<string, float> ();

				foreach (Biome biome in Biome.Biomes.Values) {

					float presence = CalculateBiomePresence (cell, biome);

					if (presence <= 0) continue;

					biomePresences.Add(biome.Name, presence);

					totalPresence += presence;
				}

				cell.Survivability = 0;
				cell.ForagingCapacity = 0;

				foreach (Biome biome in Biome.Biomes.Values)
				{
					float presence = 0;

					if (biomePresences.TryGetValue(biome.Name, out presence))
					{
						cell.PresentBiomeNames.Add(biome.Name);

						presence = presence/totalPresence;

						cell.BiomePresences.Add(presence);

						cell.Survivability += biome.Survivability * presence;
						cell.ForagingCapacity += biome.ForagingCapacity * presence;
					}
				}

//				cell.MaxForage = cell.Area * TerrainCell.MaxForageFactor * cell.ForagingCapacity * CellGroup.InitialPopulationFactor;
			}
			
			ProgressCastMethod (_accumulatedProgress + 0.20f * (i + 1)/(float)sizeX);
		}
		
		_accumulatedProgress += 0.20f;
	}
	
	private void SetInitialHumanGroups () {

		int maxGroups = 1;

		float minPresence = 0.50f;
		
		int sizeX = Width;
		int sizeY = Height;

		List<TerrainCell> SuitableCells = new List<TerrainCell> ();
		
		for (int i = 0; i < sizeX; i++) {
			
			for (int j = 0; j < sizeY; j++) {
				
				TerrainCell cell = Terrain [i] [j];
				
				float biomePresence = cell.GetBiomePresence(Biome.Grassland);
				
				if (biomePresence < minPresence) continue;

				SuitableCells.Add(cell);
			}
			
			ProgressCastMethod (_accumulatedProgress + 0.05f * (i + 1)/(float)sizeX);
		}
		
		_accumulatedProgress += 0.05f;

		if (SuitableCells.Count <= 0)
			return;

		bool first = true;

		for (int i = 0; i < maxGroups; i++) {
			
			ManagerTask<int> n = GenerateRandomInteger(0, SuitableCells.Count);

			TerrainCell cell = SuitableCells[n];

			int population = (int)Mathf.Floor(StartPopulationDensity * cell.Area);

			Culture baseCulture = new Culture();

			for (int j = 0; j < cell.PresentBiomeNames.Count; j++) {

				string biome = cell.PresentBiomeNames[j];
				float presence = cell.BiomePresences[j];

				baseCulture.Skills.Add (new BiomeSurvivalSkill (biome, presence));
			} 

			CellGroup group = new CellGroup(this, cell, population, baseCulture);

			AddGroup(group);

			if (first) {
				TagGroup(group);

				first = false;
			}
		}
	}

	public int GenerateCellGroupId () {
	
		return ++CurrentCellGroupId;
	}

	private float CalculateBiomePresence (TerrainCell cell, Biome biome) {

		float presence = 1f;

		// Altitude

		float altitudeSpan = biome.MaxAltitude - biome.MinAltitude;


		float altitudeDiff = cell.Altitude - biome.MinAltitude;

		if (altitudeDiff < 0)
			return -1f;

		float altitudeFactor = altitudeDiff / altitudeSpan;

		if (float.IsInfinity (altitudeSpan)) {

			altitudeFactor = 0.5f;
		}
		
		if (altitudeFactor > 1)
			return -1f;

		if (altitudeFactor > 0.5f)
			altitudeFactor = 1f - altitudeFactor;

		presence *= altitudeFactor*2;

		// Rainfall
		
		float rainfallSpan = biome.MaxRainfall - biome.MinRainfall;
		
		float rainfallDiff = cell.Rainfall - biome.MinRainfall;
		
		if (rainfallDiff < 0)
			return -1f;
		
		float rainfallFactor = rainfallDiff / rainfallSpan;
		
		if (float.IsInfinity (rainfallSpan)) {
			
			rainfallFactor = 0.5f;
		}
		
		if (rainfallFactor > 1)
			return -1f;
		
		if (rainfallFactor > 0.5f)
			rainfallFactor = 1f - rainfallFactor;
		
		presence *= rainfallFactor*2;
		
		// Temperature
		
		float temperatureSpan = biome.MaxTemperature - biome.MinTemperature;
		
		float temperatureDiff = cell.Temperature - biome.MinTemperature;
		
		if (temperatureDiff < 0)
			return -1f;
		
		float temperatureFactor = temperatureDiff / temperatureSpan;
		
		if (float.IsInfinity (temperatureSpan)) {
			
			temperatureFactor = 0.5f;
		}
		
		if (temperatureFactor > 1)
			return -1f;
		
		if (temperatureFactor > 0.5f)
			temperatureFactor = 1f - temperatureFactor;
		
		presence *= temperatureFactor*2;

		return presence;
	}
	
	private float CalculateRainfall (float value) {

		float drynessOffset = MaxPossibleRainfall * RainfallDrynessOffsetFactor;
		
		float span = MaxPossibleRainfall - MinPossibleRainfall + drynessOffset;

		float rainfall = (value * span) + MinPossibleRainfall - drynessOffset;

		rainfall += Manager.RainfallOffset;

		rainfall = Mathf.Clamp(rainfall, 0, MaxPossibleRainfallWithOffset);
		
		return rainfall;
	}
	
	private float CalculateTemperature (float value) {
		
		float span = MaxPossibleTemperature - MinPossibleTemperature;
		
		float temperature = (value * span) + MinPossibleTemperature;

		temperature += Manager.TemperatureOffset;
		
		temperature = Mathf.Clamp(temperature, MinPossibleTemperatureWithOffset, MaxPossibleTemperatureWithOffset);
		
		return temperature;
	}
}
