using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using UnityEngine.Profiling;

public class CellGroup : HumanGroup {

	public const int GenerationTime = 25;

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)

	public const float NaturalGrowthRate = NaturalBirthRate - NaturalDeathRate;
	
	public const float PopulationForagingConstant = 10;
	public const float PopulationFarmingConstant = 40;

	public const float MinKnowledgeTransferValue = 0.25f;

	public const float SeaTravelBaseFactor = 0.025f;

	[XmlAttribute]
	public long Id;

	[XmlAttribute("PrevExPop")]
	public float PreviousExactPopulation;
	
	[XmlAttribute("ExPop")]
	public float ExactPopulation; // TODO: Get rid of 'float' population values
	
	[XmlAttribute("StilPres")]
	public bool StillPresent = true;
	
	[XmlAttribute("LastUpDate")]
	public int LastUpdateDate;
	
	[XmlAttribute("NextUpDate")]
	public int NextUpdateDate;
	
	[XmlAttribute("OptPop")]
	public int OptimalPopulation;
	
	[XmlAttribute("Lon")]
	public int Longitude;
	[XmlAttribute("Lat")]
	public int Latitude;
	
	[XmlAttribute("HasMigEv")]
	public bool HasMigrationEvent = false;

	[XmlAttribute("SeaTrFac")]
	public float SeaTravelFactor = 0;

	[XmlAttribute("TotalPolInfVal")]
	public float TotalPolityInfluenceValueFloat = 0;

	[XmlAttribute("MigVal")]
	public float MigrationValue;

	[XmlAttribute("TotalMigVal")]
	public float TotalMigrationValue;

	public Route SeaMigrationRoute = null;

	public List<string> Flags;

	public CellCulture Culture;

	public List<PolityInfluence> PolityInfluences;

	public static float TravelWidthFactor;

	[XmlIgnore]
	public float TotalPolityInfluenceValue {
		get {
			return TotalPolityInfluenceValueFloat;
		}
		set { 
			TotalPolityInfluenceValueFloat = MathUtility.RoundToSixDecimals (value);
		}
	}
	
	[XmlIgnore]
	public TerrainCell Cell;

	[XmlIgnore]
	public bool DebugTagged = false;

	[XmlIgnore]
	public Dictionary<string, BiomeSurvivalSkill> _biomeSurvivalSkills = new Dictionary<string, BiomeSurvivalSkill> (Biome.TypeCount);

	[XmlIgnore]
	public Dictionary<Direction, CellGroup> Neighbors;

	[XmlIgnore]
	public PolityInfluence HighestPolityInfluence = null;

	#if DEBUG
	[XmlIgnore]
	public bool RunningFunction_SetPolityInfluence = false;
	#endif

	private Dictionary<long, PolityInfluence> _polityInfluences = new Dictionary<long, PolityInfluence> ();

//	private Dictionary<int, WorldEvent> _associatedEvents = new Dictionary<int, WorldEvent> ();
	
	private HashSet<string> _flags = new HashSet<string> ();

	private float _noMigrationFactor = 0.01f;

	private bool _alreadyUpdated = false;

	private bool _destroyed = false;

//	Dictionary<TerrainCell, float> _cellMigrationValues = new Dictionary<TerrainCell, float> ();

	public int PreviousPopulation {

		get {
			return (int)Mathf.Floor(PreviousExactPopulation);
		}
	}

	public int Population {

		get {
			int population = (int)Mathf.Floor(ExactPopulation);

			#if DEBUG
			if (population < -1000) {
			
				Debug.Break ();
			}
			#endif

			return population;
		}
	}

	public CellGroup () {
		
		Manager.UpdateWorldLoadTrackEventCount ();
	}
	
	public CellGroup (MigratingGroup migratingGroup, int splitPopulation) : this(migratingGroup.World, migratingGroup.TargetCell, splitPopulation, migratingGroup.Culture) {

		foreach (PolityInfluence p in migratingGroup.PolityInfluences) {

			_polityInfluences.Add (p.PolityId, p);

			ValidateAndSetHighestPolityInfluence (p);
		}
	}

	public CellGroup (World world, TerrainCell cell, int initialPopulation, Culture baseCulture = null) : base(world) {

		PreviousExactPopulation = 0;
		ExactPopulation = initialPopulation;

		Cell = cell;
		Longitude = cell.Longitude;
		Latitude = cell.Latitude;

		Cell.Group = this;

//		Id = World.GenerateCellGroupId ();
		Id = Cell.GenerateUniqueIdentifier ();

		#if DEBUG
		if (Longitude > 1000) {
			Debug.LogError ("Longitude[" + Longitude + "] > 1000");
		}

		if (Latitude > 1000) {
			Debug.LogError ("Latitude[" + Latitude + "] > 1000");
		}
		#endif

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"CellGroup:constructor - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", initialPopulation: " + initialPopulation + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		bool initialGroup = false;

		if (baseCulture == null) {
			initialGroup = true;
			Culture = new CellCulture (this);
		} else {
			Culture = new CellCulture (this, baseCulture);
		}

		Neighbors = new Dictionary<Direction, CellGroup> (8);

		foreach (KeyValuePair<Direction, TerrainCell> pair in Cell.Neighbors) {
		
			if (pair.Value.Group != null) {

				CellGroup group = pair.Value.Group;
			
				Neighbors.Add (pair.Key, group);

				Direction dir = TerrainCell.ReverseDirection (pair.Key);

				group.AddNeighbor (dir, this);
			}
		}

		InitializeDefaultActivities (initialGroup);
		InitializeDefaultSkills (initialGroup);
		InitializeDefaultKnowledges (initialGroup);

		OptimalPopulation = CalculateOptimalPopulation (Cell);

		InitializeLocalMigrationValue ();
		
		NextUpdateDate = CalculateNextUpdateDate();

		LastUpdateDate = World.CurrentDate;
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (this, NextUpdateDate));
		
		World.UpdateMostPopulousGroup (this);

		InitializeDefaultEvents ();
	}

	public long GenerateUniqueIdentifier (long oom = 1, long offset = 0) {

		return Cell.GenerateUniqueIdentifier (oom, offset);
	}

	public bool ValidateAndSetHighestPolityInfluence (PolityInfluence influence) {

		if (HighestPolityInfluence == null) {
			SetHighestPolityInfluence (influence);
			return true;
		}

		if (HighestPolityInfluence.Value < influence.Value) {
			SetHighestPolityInfluence (influence);
			return true;
		}

		return false;
	}

	public void SetHighestPolityInfluence (PolityInfluence influence) {

		if (HighestPolityInfluence != null) {
			HighestPolityInfluence.Polity.Territory.RemoveCell (Cell);
		}

		HighestPolityInfluence = influence;

		if (influence != null) {
			influence.Polity.Territory.AddCell (Cell);
		}
	}

	public void InitializeDefaultEvents () {

		if (BoatMakingDiscoveryEvent.CanSpawnIn (this)) {

			int triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate (this);

			if (triggerDate == int.MinValue)
				return;

			World.InsertEventToHappen (new BoatMakingDiscoveryEvent (this, triggerDate));
		}

		if (PlantCultivationDiscoveryEvent.CanSpawnIn (this)) {

			int triggerDate = PlantCultivationDiscoveryEvent.CalculateTriggerDate (this);

			if (triggerDate == int.MinValue)
				return;

			World.InsertEventToHappen (new PlantCultivationDiscoveryEvent (this, triggerDate));
		}
	}

	public void InitializeDefaultActivities (bool initialGroup) {

		if (initialGroup) {
			Culture.AddActivityToPerform (CellCulturalActivity.CreateForagingActivity (this, 1f, 1f));
		}
	}

	public void InitializeDefaultKnowledges (bool initialGroup) {

		if (initialGroup) {
			Culture.AddKnowledgeToLearn (new SocialOrganizationKnowledge (this));
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

//	public void AddAssociatedEvent (WorldEvent e) {
//
////		_associatedEvents.Add (e.Id, e);
//	}
//	
//	public WorldEvent GetAssociatedEvent (int id) {
//
//		WorldEvent e;
//
//		if (!_associatedEvents.TryGetValue (id, out e))
//			return null;
//
//		return e;
//	}
//	
//	public IEnumerable<WorldEvent> GetAssociatedEvents (System.Type eventType) {
//
//		foreach (WorldEvent e in _associatedEvents.Values) {
//
//			if (e.GetType () != eventType) continue;
//		
//			yield return e;
//		}
//		
//		//return _associatedEvents.Process (p => p.Value).FindAll (e => e.GetType () == eventType);
//	}
//
//	public void RemoveAssociatedEvent (int id) {
//	
////		if (!_associatedEvents.ContainsKey (id))
////			return;
////
////		_associatedEvents.Remove (id);
//	}

	public int GetNextLocalRandomInt (int iterationOffset, int maxValue) {// = PerlinNoise.MaxPermutationValue) {
	
		return Cell.GetNextLocalRandomInt (iterationOffset, maxValue);
	}

	public float GetNextLocalRandomFloat (int iterationOffset) {

		return Cell.GetNextLocalRandomFloat (iterationOffset);
	}

//	public int GetNextLocalRandomIntNoIteration (int iterationOffset, int maxValue = PerlinNoise.MaxPermutationValue) {
//
//		return Cell.GetNextLocalRandomIntNoIteration (iterationOffset, maxValue);
//	}
//
//	public float GetNextLocalRandomFloatNoIteration (int iterationOffset) {
//
//		return Cell.GetNextLocalRandomFloatNoIteration (iterationOffset);
//	}

	public void AddNeighbor (Direction direction, CellGroup group) {

		if (group == null)
			return;

		if (!group.StillPresent)
			return;

		if (Neighbors.ContainsValue (group))
			return;
	
		Neighbors.Add (direction, group);
	}
	
	public void RemoveNeighbor (CellGroup group) {

		Direction? direction = null;

		bool found = false;

		foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors) {

			if (group == pair.Value) {
			
				direction = pair.Key;
				found = true;
				break;
			}
		}

		if (!found)
			return;
		
		Neighbors.Remove (direction.Value);
	}

	public void RemoveNeighbor (Direction direction) {

		Neighbors.Remove (direction);
	}

	public void InitializeDefaultSkills (bool initialGroup) {

		float baseValue = 0;
		if (initialGroup) {
			baseValue = 1f;
		}

		foreach (string biomeName in GetPresentBiomesInNeighborhood ()) {

			if (biomeName == Biome.Ocean.Name) {

				if (Culture.GetSkill (SeafaringSkill.SeafaringSkillId) == null) {

					Culture.AddSkillToLearn (new SeafaringSkill (this));
				}

			} else {

				Biome biome = Biome.Biomes[biomeName];

				string skillId = BiomeSurvivalSkill.GenerateId (biome);

				if (Culture.GetSkill (skillId) == null) {

					Culture.AddSkillToLearn (new BiomeSurvivalSkill (this, biome, baseValue));
				}
			}
		}
	}

	public void AddBiomeSurvivalSkill (BiomeSurvivalSkill skill) {

		if (_biomeSurvivalSkills.ContainsKey (skill.BiomeName)) {
		
			Debug.Break ();
		}
	
		_biomeSurvivalSkills.Add (skill.BiomeName, skill);
	}

	public HashSet<string> GetPresentBiomesInNeighborhood () {
	
		HashSet<string> biomeNames = new HashSet<string> ();
		
		foreach (string biomeName in Cell.PresentBiomeNames) {

			biomeNames.Add (biomeName);
		}

		foreach (TerrainCell neighborCell in Cell.Neighbors.Values) {
			
			foreach (string biomeName in neighborCell.PresentBiomeNames) {
				
				biomeNames.Add (biomeName);
			}
		}

		return biomeNames;
	}

	public void MergeGroup (MigratingGroup group) {

		float newPopulation = Population + group.Population;

		float percentage = group.Population / newPopulation;

		#if DEBUG
		float oldExactPopulation = ExactPopulation;
		#endif

		ExactPopulation = newPopulation;

		#if DEBUG
		if (Population < -1000) {

			Debug.Break ();
		}
		#endif

		Culture.MergeCulture (group.Culture, percentage);

		MergePolities (group.PolityInfluences, percentage);

		#if DEBUG
		if (Manager.RegisterDebugEvent != null) {
			if (Id == Manager.TracingData.GroupId) {
				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
					"MergeGroup - Group:" + groupId, 
					"CurrentDate: " + World.CurrentDate +
					", group.SourceGroupId: " + group.SourceGroupId + 
					", oldExactPopulation: " + oldExactPopulation + 
					", source group.Population: " + group.Population + 
					", newPopulation: " + newPopulation + 
					", group.PolityInfluences.Count: " + group.PolityInfluences.Count + 
					"");

				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
			}
		}
		#endif

		TriggerInterference ();
	}

	public void MergePolities (List <PolityInfluence> sourcePolityInfluences, float percentOfTarget) {

		List<PolityInfluence> polityInfluences = new List<PolityInfluence> (_polityInfluences.Values);

		foreach (PolityInfluence pInfluence in polityInfluences) {

			float influenceValue = pInfluence.Value;

			float newValue = influenceValue * (1 - percentOfTarget);

//			#if DEBUG
//			if (Manager.RegisterDebugEvent != null) {
//				if (Id == Manager.TracingData.GroupId) {
//
//					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//						"MergePolities:Rescale - Group:" + groupId + 
//						", pInfluence.PolityId: " + pInfluence.PolityId,
//						"CurrentDate: " + World.CurrentDate  +
//						", influenceValue: " + influenceValue + 
//						", Polity.TotalGroupInfluenceValue: " + pInfluence.Polity.TotalGroupInfluenceValue + 
//						", newInfluenceValue: " + newInfluenceValue + 
//						", percentOfTarget: " + percentOfTarget + 
//						"");
//
//					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//				}
//			}
//			#endif

			SetPolityInfluence (pInfluence.Polity, newValue);
		}

		foreach (PolityInfluence pInfluence in sourcePolityInfluences) {

			Polity polity = pInfluence.Polity;
			float influenceValue = pInfluence.Value;

			float currentValue = GetPolityInfluenceValue (polity);

			float newValue = currentValue + (influenceValue * percentOfTarget);

//			#if DEBUG
//			if (Manager.RegisterDebugEvent != null) {
//				if (Id == Manager.TracingData.GroupId) {
//					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//						"MergePolities:Add - Group:" + groupId + 
//						", pInfluence.PolityId: " + pInfluence.PolityId,
//						"CurrentDate: " + World.CurrentDate  +
//						", currentValue: " + currentValue +
//						", influenceValue: " + influenceValue +
//						", Polity.TotalGroupInfluenceValue: " + pInfluence.Polity.TotalGroupInfluenceValue + 
//						", newValue: " + newValue +
//						", percentOfTarget: " + percentOfTarget +
//						"");
//
//					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//				}
//			}
//			#endif

			SetPolityInfluence (polity, newValue);
		}
	}
	
	public int SplitGroup (MigratingGroup group) {

		int splitPopulation = (int)Mathf.Floor(Population * group.PercentPopulation);

//		#if DEBUG
//		float oldExactPopulation = ExactPopulation;
//		#endif

		ExactPopulation -= splitPopulation;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if ((Id == Manager.TracingData.GroupId) || 
//				((group.TargetCell.Group != null) && (group.TargetCell.Group.Id == Manager.TracingData.GroupId))) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//				string targetInfo = "Long:" + group.TargetCell.Longitude + "|Lat:" + group.TargetCell.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"SplitGroup - sourceGroup:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", targetInfo: " + targetInfo + 
//					", ExactPopulation: " + ExactPopulation + 
//					", oldExactPopulation: " + oldExactPopulation + 
//					", migratingGroup.PercentPopulation: " + group.PercentPopulation + 
//					", splitPopulation: " + splitPopulation + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		#if DEBUG
		if (Population < -1000) {
			Debug.Break ();
		}
		#endif

		return splitPopulation;
	}

//	public void PreUpdate () {
//
//	}

	public void PostUpdate () {

		_alreadyUpdated = false;

		if (Population < 2) {
			World.AddGroupToRemove (this);
			return;
		}

		Profiler.BeginSample ("Culture PostUpdate");
	
		Culture.PostUpdate ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Set Polity Updates");

		SetPolityUpdates ();

		Profiler.EndSample ();

		PostUpdatePolityInfluences ();

		UpdatePolityInfluenceAdministrativeCosts ();
	}

	public void SetupForNextUpdate () {

		if (_destroyed)
			return;
		
		World.UpdateMostPopulousGroup (this);

		Profiler.BeginSample ("Calculate Optimal Population");
		
		OptimalPopulation = CalculateOptimalPopulation (Cell);

		Profiler.EndSample ();

		Profiler.BeginSample ("Calculate Local Migration Value");

		CalculateLocalMigrationValue ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Consider Land Migration");
		
		ConsiderLandMigration ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Consider Sea Migration");

		ConsiderSeaMigration ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Calculate Next Update Date");
		
		NextUpdateDate = CalculateNextUpdateDate ();

		Profiler.EndSample ();

		LastUpdateDate = World.CurrentDate;
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (this, NextUpdateDate));
	}
	
	private float CalculateAltitudeDeltaMigrationFactor (TerrainCell targetCell) {

		float altitudeModifier = targetCell.Altitude / World.MaxPossibleAltitude;

		float altitudeDeltaModifier = 5 * altitudeModifier;
		float maxAltitudeDelta = Cell.Area / altitudeDeltaModifier;
		float minAltitudeDelta = -Cell.Area / (altitudeDeltaModifier * 5);
		float altitudeDelta = Mathf.Clamp (targetCell.Altitude - Cell.Altitude, minAltitudeDelta, maxAltitudeDelta);
		float altitudeDeltaFactor = 1 - ((altitudeDelta - minAltitudeDelta) / (maxAltitudeDelta - minAltitudeDelta));
		
		return altitudeDeltaFactor;
	}

	public float CalculateMigrationValue (TerrainCell cell) {

		#if DEBUG
		if (cell.IsSelected) {
			if (_polityInfluences.Count > 0) {
				bool debug = true;
			}
		}
		#endif
		
		float areaFactor = cell.Area / TerrainCell.MaxArea;

		Profiler.BeginSample ("Calculate Altitude Delta Migration Factor");

		float altitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (cell);
		altitudeDeltaFactor = Mathf.Pow (altitudeDeltaFactor, 4);

		Profiler.EndSample ();

		int existingPopulation = 0;

		float popDifferenceFactor = 1;

		if (cell.Group != null) {
			existingPopulation = cell.Group.Population;

			popDifferenceFactor = (float)Population / (float)(Population + existingPopulation);
			popDifferenceFactor = Mathf.Pow (popDifferenceFactor, 4);
		}

		popDifferenceFactor *= 10;

		float polityInfluenceFactor = 1;

		if (_polityInfluences.Count > 0) {
			polityInfluenceFactor = 0;

			foreach (PolityInfluence polityInfluence in _polityInfluences.Values) {
				
				Profiler.BeginSample ("Polity Migration Value");

				float influenceFactor = polityInfluence.Polity.MigrationValue (this, cell, polityInfluence.Value);
				influenceFactor = Mathf.Pow (influenceFactor, 8);

				Profiler.EndSample ();

				polityInfluenceFactor += influenceFactor;

//				#if DEBUG
//				if (Manager.RegisterDebugEvent != null) {
//					if (Id == Manager.TracingData.GroupId) {
////						if ((Longitude == cell.Longitude) && (Latitude == cell.Latitude)) {
//							string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//							string targetCellLoc = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
//
//							SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//								"CalculateMigrationValue:InfluenceFactor - Group:" + groupId + 
//								", polityInfluence.PolityId: " + polityInfluence.PolityId + 
//								", targetCell: " + targetCellLoc,
//								"CurrentDate: " + World.CurrentDate + 
//								", polityInfluence.Value: " + polityInfluence.Value + 
//								", influenceFactor: " + influenceFactor + 
//								"");
//
//							Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
////						}
//					}
//				}
//				#endif
			}
		}

		polityInfluenceFactor *= 10;

		float noMigrationFactor = 1;

		float optimalPopulation = OptimalPopulation;

		if (cell != Cell) {
			noMigrationFactor = _noMigrationFactor;

			Profiler.BeginSample ("Calculate Optimal Population");

			optimalPopulation = CalculateOptimalPopulation (cell);

			Profiler.EndSample ();
		}

		float optimalPopulationFactor = 0;

		if (optimalPopulation > 0) {
			optimalPopulationFactor = optimalPopulation / (existingPopulation + optimalPopulation);
		}

		float secondaryOptimalPopulationFactor = 0;

		if (optimalPopulation > 0) {
			secondaryOptimalPopulationFactor = optimalPopulation / (OptimalPopulation + optimalPopulation);
		}

		float cellValue = altitudeDeltaFactor * areaFactor * popDifferenceFactor * noMigrationFactor * polityInfluenceFactor * optimalPopulationFactor * secondaryOptimalPopulationFactor;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
////				if ((Longitude == cell.Longitude) && (Latitude == cell.Latitude)) {
//					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//					string targetCellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
//
//					if (cell.Group != null) {
//						targetCellInfo = "Id:" + cell.Group.Id + "|" + targetCellInfo;
//					}
//
//					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//						"CalculateMigrationValue - Group:" + groupId + 
//						", targetCell: " + targetCellInfo,
//						", CurrentDate: " + World.CurrentDate + 
////						", altitudeDeltaFactor: " + altitudeDeltaFactor + 
//						", ExactPopulation: " + ExactPopulation + 
//						", target existingPopulation: " + existingPopulation + 
//						", polityInfluenceFactor: " + polityInfluenceFactor + 
//						", OptimalPopulation: " + OptimalPopulation + 
//						", target optimalPopulation: " + optimalPopulation + 
//						", secondaryOptimalPopulationFactor: " + secondaryOptimalPopulationFactor + 
//						"");
//
//					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
////				}
//			}
//		}
//		#endif

		return cellValue;
	}

	public void TriggerInterference () {
	
		EraseSeaMigrationRoute ();
	}

	public void EraseSeaMigrationRoute () {
	
		if (SeaMigrationRoute == null)
			return;

		SeaMigrationRoute.Destroy ();
		SeaMigrationRoute = null;
	}

	public void GenerateSeaMigrationRoute () {

		if (!Cell.IsPartOfCoastline)
			return;

		Route route = new Route (Cell);

		bool invalidRoute = false;

		if (route.LastCell == null)
			invalidRoute = true;

		if (route.LastCell == route.FirstCell)
			invalidRoute = true;

		if (route.FirstCell.Neighbors.ContainsValue (route.LastCell))
			invalidRoute = true;

		if (invalidRoute) {
		
			route.Destroy ();
			return;
		}

		SeaMigrationRoute = route;
		SeaMigrationRoute.Consolidate ();
	}

	public void InitializeLocalMigrationValue () {

		MigrationValue = 1;

		TotalMigrationValue = 1;
	}

	public void CalculateLocalMigrationValue () {

		MigrationValue = CalculateMigrationValue (Cell);

		TotalMigrationValue = MigrationValue;
	}

	private class CellMigrationValue : CollectionUtility.ElementWeightPair<TerrainCell> {

		public CellMigrationValue (TerrainCell cell, float weight) : base (cell, weight) {
			
		}
	}
	
	public void ConsiderLandMigration () {

		if (HasMigrationEvent)
			return;

		List<CellMigrationValue> cellMigrationValues = new List<CellMigrationValue> (Cell.Neighbors.Count + 1);
		cellMigrationValues.Add (new CellMigrationValue (Cell, MigrationValue));

		foreach (TerrainCell c in Cell.Neighbors.Values) {

			Profiler.BeginSample ("Calculate Migration Value");
			
			float cellValue = CalculateMigrationValue (c);

			Profiler.EndSample ();
			
			TotalMigrationValue += cellValue;

			cellMigrationValues.Add (new CellMigrationValue (c, cellValue));
		}

		Profiler.BeginSample ("Land Migration Weighted Selection");

		TerrainCell targetCell = CollectionUtility.WeightedSelection (cellMigrationValues.ToArray (), TotalMigrationValue, () => Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CONSIDER_LAND_MIGRATION));

		Profiler.EndSample ();

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				string cellInfo = "No target cell";
//
//				if (targetCell != null) {
//					cellInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
//				}
//
//				string cellMigrationValuesStr = "";
//
//				foreach (CellMigrationValue pair in cellMigrationValues) {
//
//					cellMigrationValuesStr += "\n\t Long:" + pair.Value.Longitude + " Lat:" + pair.Value.Latitude + " Value:" + pair.Weight;
//				}
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ConsiderLandMigration - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", target cell: " + cellInfo + 
//					", migration values: " + cellMigrationValuesStr + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		if (targetCell == Cell)
			return;

		if (targetCell == null)
			return;
		
		float cellSurvivability = 0;
		float cellForagingCapacity = 0;

		Profiler.BeginSample ("Calculate Adaption To Cell");
		
		CalculateAdaptionToCell (targetCell, out cellForagingCapacity, out cellSurvivability);

		Profiler.EndSample ();

		if (cellSurvivability <= 0)
			return;

		Profiler.BeginSample ("Calculate Altitude Delta Migration Factor");

		float cellAltitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (targetCell);

		Profiler.EndSample ();

		float travelFactor = 
			cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
			cellSurvivability * cellSurvivability * targetCell.Accessibility;

		travelFactor = Mathf.Clamp (travelFactor, 0.0001f, 1);

		int travelTime = (int)Mathf.Ceil(Cell.Width / (TravelWidthFactor * travelFactor));
		
		int nextDate = World.CurrentDate + travelTime;
		
		World.InsertEventToHappen (new MigrateGroupEvent (this, targetCell, nextDate));

		HasMigrationEvent = true;
	}

	public void ConsiderSeaMigration () {

		if (SeaTravelFactor <= 0)
			return;

		if (HasMigrationEvent)
			return;

		if (SeaMigrationRoute == null) {
		
			GenerateSeaMigrationRoute ();

			if (SeaMigrationRoute == null)
				return;
		}

		TerrainCell targetCell = SeaMigrationRoute.LastCell;

		if (targetCell == Cell)
			return;

		if (targetCell == null)
			return;

		TotalMigrationValue += CalculateMigrationValue (targetCell);

		float cellSurvivability = 0;
		float cellForagingCapacity = 0;

		CalculateAdaptionToCell (targetCell, out cellForagingCapacity, out cellSurvivability);

		if (cellSurvivability <= 0)
			return;

		float routeLength = SeaMigrationRoute.Length;

		float successChance = SeaTravelFactor / (SeaTravelFactor + routeLength);

		float attemptValue = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CONSIDER_SEA_MIGRATION);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				string cellInfo = "No target cell";
//
//				if (targetCell != null) {
//					cellInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
//				}
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ConsiderSeaMigration - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", target cell: " + cellInfo + 
//					", attemptValue: " + attemptValue + 
//					", successChance: " + successChance + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		if (attemptValue >= successChance)
			return;

		int travelTime = (int)Mathf.Ceil(routeLength / SeaTravelFactor);

		int nextDate = World.CurrentDate + travelTime;

		World.InsertEventToHappen (new MigrateGroupEvent (this, targetCell, nextDate));

		HasMigrationEvent = true;
	}

	public void Destroy () {

		_destroyed = true;

		RemovePolityInfluences ();

		Cell.Group = null;
		World.RemoveGroup (this);

		foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors) {
			pair.Value.RemoveNeighbor (TerrainCell.ReverseDirection(pair.Key));
		}

		EraseSeaMigrationRoute ();

		StillPresent = false;

		if (FarmDegradationEvent.CanSpawnIn (Cell)) {

			int triggerDate = FarmDegradationEvent.CalculateTriggerDate (Cell);

			World.InsertEventToHappen (new FarmDegradationEvent (Cell, triggerDate));
		}
	}

	public void RemovePolityInfluences () {

		// Make sure all influencing polities get updated
		SetPolityUpdates (true);

		PolityInfluence[] polityInfluences = new PolityInfluence[_polityInfluences.Count];
		_polityInfluences.Values.CopyTo (polityInfluences, 0);

		foreach (PolityInfluence polityInfluence in polityInfluences) {

			Polity polity = polityInfluence.Polity;

			SetPolityInfluence (polity, 0);
		}
	}

	#if DEBUG

	public delegate void UpdateCalledDelegate ();

	public static UpdateCalledDelegate UpdateCalled = null; 

	#endif

	public void Update () {

		if (_alreadyUpdated)
			return;

		#if DEBUG
		if (UpdateCalled != null) {
		
			UpdateCalled ();
		}
		#endif

		_alreadyUpdated = true;

		PreviousExactPopulation = ExactPopulation;
		
		int timeSpan = World.CurrentDate - LastUpdateDate;

		if (timeSpan <= 0)
			return;

		Profiler.BeginSample ("Update Terrain Farmland Percentage");

		UpdateTerrainFarmlandPercentage (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Population");

		UpdatePopulation (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Culture");

		UpdateCulture (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Polities Cultural Influence");

		PolitiesCulturalInfluence (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Polity Update Effects");

		PolityUpdateEffects (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Travel Factors");

		UpdateTravelFactors ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Shortest Polity Influence Core Distances");

		PrepareUpdateShortestPolityInfluenceCoreDistances ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Add Updated Group");
		
		World.AddUpdatedGroup (this);

		Profiler.EndSample ();
	}

	private void SetPolityUpdates (bool forceUpdate = false) {
	
		foreach (PolityInfluence pi in _polityInfluences.Values) {
		
			SetPolityUpdate (pi, forceUpdate);
		}
	}

	public void SetPolityUpdate (PolityInfluence pi, bool forceUpdate) {

		Polity p = pi.Polity;
		float influenceValue = pi.Value;

		if (p.WillBeUpdated)
			return;

		if (forceUpdate || (p.CoreGroup == this)) {

			World.AddPolityToUpdate (p);
			return;
		}

		if (p.TotalGroupInfluenceValue <= 0)
			return;

		// If group is not the core group then there's a chance no polity update will happen

		float chanceFactor = influenceValue / p.TotalGroupInfluenceValue;

		float rollValue = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_SET_POLITY_UPDATE + (int)p.Id);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//				"SetPolityUpdate - Group:" + Id,
//				"CurrentDate: " + World.CurrentDate + 
//				", influenceValue: " + influenceValue + 
//				", p.TotalGroupInfluenceValue: " + p.TotalGroupInfluenceValue + 
//				", p.Id: " + p.Id + 
//				", chanceFactor: " + chanceFactor + 
//				", rollValue: " + rollValue + 
//				"");
//
//			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//		}
//		#endif

		if (rollValue <= chanceFactor)
			World.AddPolityToUpdate (p);
	}
	
	private void UpdatePopulation (int timeSpan) {
		
		ExactPopulation = PopulationAfterTime (timeSpan);
	}
	
	private void UpdateCulture (int timeSpan) {
		
		Culture.Update (timeSpan);
	}

	private void PolitiesCulturalInfluence (int timeSpan) {
	
		foreach (PolityInfluence pi in _polityInfluences.Values) {
		
			Culture.PolityCulturalInfluence (pi, timeSpan);
		}
	}

	private void PolityUpdateEffects (int timeSpan) {

		PolityInfluence[] polityInfluences = new PolityInfluence[_polityInfluences.Count];
		_polityInfluences.Values.CopyTo (polityInfluences, 0);

		float totalInfluenceValue = TotalPolityInfluenceValue;

		foreach (PolityInfluence polityInfluence in polityInfluences) {
		
			Polity polity = polityInfluence.Polity;
			float influence = polityInfluence.Value;

			polity.UpdateEffects (this, influence, totalInfluenceValue, timeSpan);
		}

		if (TribeFormationEvent.CanSpawnIn (this)) {

			int triggerDate = TribeFormationEvent.CalculateTriggerDate (this);

			if (triggerDate == int.MinValue)
				return;

			World.InsertEventToHappen (new TribeFormationEvent (this, triggerDate));
		}
	}

	private float GetActivityContribution (string activityId) {
	
		CellCulturalActivity activity = Culture.GetActivity (activityId) as CellCulturalActivity;

		if (activity == null)
			return 0;

		return activity.Contribution;
	}

	private void UpdateTerrainFarmlandPercentage (int timeSpan) {

		#if DEBUG
		if (Cell.IsSelected) {
		
			bool debug = true;
		}
		#endif

		CulturalKnowledge agricultureKnowledge = Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId);

		if (agricultureKnowledge == null) {

			return;
		}

		float farmlandPercentage = Cell.FarmlandPercentage;

		float knowledgeValue = agricultureKnowledge.ScaledValue;

		float techValue = Mathf.Sqrt(knowledgeValue);

		float areaPerFarmWorker = techValue / 5f;

		float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn (Cell);

		float farmingPopulation = GetActivityContribution (CellCulturalActivity.FarmingActivityId) * Population;

		float maxWorkableFarmlandArea = areaPerFarmWorker * farmingPopulation;

		float maxPossibleFarmlandArea = Cell.Area * terrainFactor;

		float maxFarmlandArea = Mathf.Min (maxPossibleFarmlandArea, maxWorkableFarmlandArea);

		float farmlandArea = farmlandPercentage * Cell.Area;

		float necessaryAreaToFarm = maxFarmlandArea - farmlandArea;

		if (necessaryAreaToFarm <= 0)
			return;

		float techGenerationFactor = techValue / 5f;

		float farmlandGenerationFactor = farmingPopulation * techGenerationFactor * timeSpan;

		float actualGeneratedPercentage = farmlandGenerationFactor / (necessaryAreaToFarm + farmlandGenerationFactor);

		float actualGeneratedFarmlandArea = necessaryAreaToFarm * actualGeneratedPercentage;

		float percentageToAdd = actualGeneratedFarmlandArea / Cell.Area;

		farmlandPercentage += percentageToAdd;

		if (farmlandPercentage > 1) {
		
			throw new System.Exception ("farmlandPercentage greater than 1");
		}

		#if DEBUG
		if (float.IsNaN(farmlandPercentage)) {

			Debug.Break ();
		}
		#endif

		Cell.FarmlandPercentage = farmlandPercentage;
	}

	public void UpdateTravelFactors () {

		float seafaringValue = 0;
		float shipbuildingValue = 0;

		foreach (CellCulturalSkill skill in Culture.Skills) {

			if (skill is SeafaringSkill) {

				seafaringValue = skill.Value;
			}
		}

		foreach (CellCulturalKnowledge knowledge in Culture.Knowledges) {

			if (knowledge is ShipbuildingKnowledge) {

				shipbuildingValue = knowledge.ScaledValue;
			}
		}

		SeaTravelFactor = SeaTravelBaseFactor * seafaringValue * shipbuildingValue * TravelWidthFactor;
	}

	public int CalculateOptimalPopulation (TerrainCell cell) {

		int optimalPopulation = 0;

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

		Profiler.BeginSample ("Get Activity Contribution");

		float foragingContribution = GetActivityContribution (CellCulturalActivity.ForagingActivityId);

		Profiler.EndSample ();

		Profiler.BeginSample ("Calculate Adaption To Cell");

		CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

		Profiler.EndSample ();

		float populationCapacityByForaging = foragingContribution * PopulationForagingConstant * cell.Area * modifiedForagingCapacity;

		Profiler.BeginSample ("Get Activity Contribution");

		float farmingContribution = GetActivityContribution (CellCulturalActivity.FarmingActivityId);
		float populationCapacityByFarming = 0;

		Profiler.EndSample ();

		if (farmingContribution > 0) {

			Profiler.BeginSample ("Calculate Farming Capacity");

			float farmingCapacity = CalculateFarmingCapacity (cell);

			Profiler.EndSample ();

			populationCapacityByFarming = farmingContribution * PopulationFarmingConstant * cell.Area * farmingCapacity;
		}

		float accesibilityFactor = 0.25f + 0.75f * cell.Accessibility; 

		float populationCapacity = (populationCapacityByForaging + populationCapacityByFarming) * modifiedSurvivability * accesibilityFactor;

		optimalPopulation = (int)Mathf.Floor (populationCapacity);

		#if DEBUG
		if (optimalPopulation < -1000) {

			Debug.Break ();
		}
		#endif

		#if DEBUG
		if (Manager.RegisterDebugEvent != null) {
			if (Id == Manager.TracingData.GroupId) {
				if ((cell.Longitude == Longitude) && (cell.Latitude == Latitude)) {
					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
					string cellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;

					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
						"CalculateOptimalPopulation - Group:" + groupId,
						"CurrentDate: " + World.CurrentDate + 
						", target cellInfo: " + cellInfo + 
						", foragingContribution: " + foragingContribution + 
//						", Area: " + cell.Area + 
						", modifiedForagingCapacity: " + modifiedForagingCapacity + 
						", modifiedSurvivability: " + modifiedSurvivability + 
						", accesibilityFactor: " + accesibilityFactor + 
						", optimalPopulation: " + optimalPopulation + 
						"");

					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
				}
			}
		}
		#endif

		return optimalPopulation;
	}

	public float CalculateFarmingCapacity (TerrainCell cell) {

		float capacityFactor = 0;
	
		CulturalKnowledge agricultureKnowledge = Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId);

		if (agricultureKnowledge == null)
			return capacityFactor;

		float value = agricultureKnowledge.ScaledValue;

		float techFactor = Mathf.Sqrt(value);

		capacityFactor = cell.FarmlandPercentage * techFactor;

		return capacityFactor;
	}

	public void CalculateAdaptionToCell (TerrainCell cell, out float foragingCapacity, out float survivability) {

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

		//		#if DEBUG
		//		string biomeData = "";
		//		#endif

		Profiler.BeginSample ("Get Skill Values");

		foreach (string biomeName in cell.PresentBiomeNames) {

			float biomePresence = cell.GetBiomePresence(biomeName);

			BiomeSurvivalSkill skill = null;

			if (_biomeSurvivalSkills.TryGetValue (biomeName, out skill)) {

				Profiler.BeginSample ("Evaluate Biome Survival Skill");

				float skillValuePresence = skill.Value * biomePresence;

				Biome biome = Biome.Biomes[biomeName];

				modifiedForagingCapacity += biome.ForagingCapacity * skillValuePresence;
				modifiedSurvivability += (biome.Survivability + skillValuePresence * (1 - biome.Survivability));

//				#if DEBUG
//
//				if (Manager.RegisterDebugEvent != null) {
//					biomeData += "\n\tBiome: " + biomeName + 
//						" ForagingCapacity: " + biome.ForagingCapacity + 
//						" skillValue: " + skillValue + 
//						" biomePresence: " + biomePresence;
//				}
//
//				#endif

				Profiler.EndSample ();
			}
		}

		Profiler.EndSample ();

		float altitudeSurvivabilityFactor = 1 - (cell.Altitude / World.MaxPossibleAltitude);

		modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;

		foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
		survivability = modifiedSurvivability * altitudeSurvivabilityFactor;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				if ((cell.Longitude == Longitude) && (cell.Latitude == Latitude)) {
//					System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
//
//					System.Reflection.MethodBase method = stackTrace.GetFrame(2).GetMethod();
//					string callingMethod = method.Name;
//
////					if (callingMethod.Contains ("CalculateMigrationValue")) {
//						string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//						string cellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
//
//						SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//							"CalculateAdaptionToCell - Group:" + groupId,
//							"CurrentDate: " + World.CurrentDate + 
//							", callingMethod(2): " + callingMethod + 
//							", target cell: " + cellInfo + 
//							", cell.FarmlandPercentage: " + cell.FarmlandPercentage + 
//							", foragingCapacity: " + foragingCapacity + 
//							", survivability: " + survivability + 
//							", biomeData: " + biomeData + 
//							"");
//
//						Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
////					}
//				}
//			}
//		}
//		#endif
	}

//	public void CalculateAdaptionToCell (TerrainCell cell, out float foragingCapacity, out float survivability) {
//
//		float modifiedForagingCapacity = 0;
//		float modifiedSurvivability = 0;
//
////		#if DEBUG
////		string biomeData = "";
////		#endif
//
//		Profiler.BeginSample ("Get Skill Values");
//		
//		foreach (CellCulturalSkill skill in Culture.Skills) {
//			
//			float skillValue = skill.Value;
//			
//			if (skill is BiomeSurvivalSkill) {
//
//				Profiler.BeginSample ("Evaluate Biome Survival Skill");
//				
//				BiomeSurvivalSkill biomeSurvivalSkill = skill as BiomeSurvivalSkill;
//				
//				string biomeName = biomeSurvivalSkill.BiomeName;
//				
//				float biomePresence = cell.GetBiomePresence(biomeName);
//				
//				if (biomePresence > 0)
//				{
//					Biome biome = Biome.Biomes[biomeName];
//					
//					modifiedForagingCapacity += biome.ForagingCapacity * skillValue * biomePresence;
//					modifiedSurvivability += (biome.Survivability + skillValue * (1 - biome.Survivability)) * biomePresence;
//
////					#if DEBUG
////
////					if (Manager.RegisterDebugEvent != null) {
////						biomeData += "\n\tBiome: " + biomeName + 
////							" ForagingCapacity: " + biome.ForagingCapacity + 
////							" skillValue: " + skillValue + 
////							" biomePresence: " + biomePresence;
////					}
////
////					#endif
//				}
//
//				Profiler.EndSample ();
//			}
//		}
//
//		Profiler.EndSample ();
//		
//		float altitudeSurvivabilityFactor = 1 - (cell.Altitude / World.MaxPossibleAltitude);
//
//		modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;
//
//		foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
//		survivability = modifiedSurvivability * altitudeSurvivabilityFactor;
//
////		#if DEBUG
////		if (Manager.RegisterDebugEvent != null) {
////			if (Id == Manager.TracingData.GroupId) {
////				if ((cell.Longitude == Longitude) && (cell.Latitude == Latitude)) {
////					System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
////
////					System.Reflection.MethodBase method = stackTrace.GetFrame(2).GetMethod();
////					string callingMethod = method.Name;
////
//////					if (callingMethod.Contains ("CalculateMigrationValue")) {
////						string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
////						string cellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
////
////						SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
////							"CalculateAdaptionToCell - Group:" + groupId,
////							"CurrentDate: " + World.CurrentDate + 
////							", callingMethod(2): " + callingMethod + 
////							", target cell: " + cellInfo + 
////							", cell.FarmlandPercentage: " + cell.FarmlandPercentage + 
////							", foragingCapacity: " + foragingCapacity + 
////							", survivability: " + survivability + 
////							", biomeData: " + biomeData + 
////							"");
////
////						Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//////					}
////				}
////			}
////		}
////		#endif
//	}

	public int CalculateNextUpdateDate () {

		#if DEBUG
		if (Cell.IsSelected) {
			bool debug = true;
		}
		#endif

		float randomFactor = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CALCULATE_NEXT_UPDATE);
		randomFactor = 1f - Mathf.Pow (randomFactor, 4);

		float migrationFactor = 0;

		if (TotalMigrationValue > 0) {
			migrationFactor = MigrationValue / TotalMigrationValue;
			migrationFactor = Mathf.Pow (migrationFactor, 4);
		}

		float skillLevelFactor = Culture.MinimumSkillAdaptationLevel ();
		
		float knowledgeLevelFactor = Culture.MinimumKnowledgeProgressLevel ();

		float populationFactor = 1 + Mathf.Abs (OptimalPopulation - Population);
		populationFactor = (10000 + OptimalPopulation) / populationFactor;

		float mixFactor = randomFactor * migrationFactor * skillLevelFactor * knowledgeLevelFactor * populationFactor;

		int finalFactor = (int)Mathf.Max(mixFactor, 1);

		#if DEBUG
		if (Manager.RegisterDebugEvent != null) {
			if (Id == Manager.TracingData.GroupId) {
				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
					"CalculateNextUpdateDate - Group:" + groupId, 
					"CurrentDate: " + World.CurrentDate + 
					", MigrationValue: " + MigrationValue + 
					", TotalMigrationValue: " + TotalMigrationValue + 
					", OptimalPopulation: " + OptimalPopulation + 
					", ExactPopulation: " + ExactPopulation + 
					", randomFactor: " + randomFactor +
					", LastUpdateDate: " + LastUpdateDate +
					"");

				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
			}
		}
		#endif

		return World.CurrentDate + GenerationTime * finalFactor;
	}

	public float PopulationAfterTime (int time) { // in years

		float population = ExactPopulation;
		
		if (population == OptimalPopulation)
			return population;
		
		float timeFactor = NaturalGrowthRate * time / (float)GenerationTime;

		if (population < OptimalPopulation) {
			
			float geometricTimeFactor = Mathf.Pow(2, timeFactor);
			float populationFactor = 1 - ExactPopulation/(float)OptimalPopulation;

			population = OptimalPopulation * MathUtility.RoundToSixDecimals (1 - Mathf.Pow(populationFactor, geometricTimeFactor));

			#if DEBUG
			if ((int)population < -1000) {

				Debug.Break ();
			}
			#endif

			#if DEBUG
			if (Manager.RegisterDebugEvent != null) {
				if (Id == Manager.TracingData.GroupId) {
					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
						"PopulationAfterTime:increase - Group:" + groupId,
						"CurrentDate: " + World.CurrentDate + 
						", OptimalPopulation: " + OptimalPopulation + 
						", ExactPopulation: " + ExactPopulation + 
						", new population: " + population + 
						"");

					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
				}
			}
			#endif

			return population;
		}

		if (population > OptimalPopulation) {

			population = OptimalPopulation + (ExactPopulation - OptimalPopulation) * MathUtility.RoundToSixDecimals (Mathf.Exp (-timeFactor));

			#if DEBUG
			if ((int)population < -1000) {

				Debug.Break ();
			}
			#endif

			#if DEBUG
			if (Manager.RegisterDebugEvent != null) {
				if (Id == Manager.TracingData.GroupId) {
					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
						"PopulationAfterTime:decrease - Group:" + groupId,
						"CurrentDate: " + World.CurrentDate + 
						", OptimalPopulation: " + OptimalPopulation + 
						", ExactPopulation: " + ExactPopulation + 
						", new population: " + population + 
						"");

					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
				}
			}
			#endif
			
			return population;
		}

		return 0;
	}

	public int GetPolityInfluencesCount () {

		return _polityInfluences.Count;
	}

	public List<PolityInfluence> GetPolityInfluences () {

		return new List<PolityInfluence> (_polityInfluences.Values);
	}

	public float GetPolityInfluenceValue (Polity polity) {

		PolityInfluence polityInfluence;

		if (!_polityInfluences.TryGetValue (polity.Id, out polityInfluence))
			return 0;

		return polityInfluence.Value;
	}

	public float GetPolityInfluenceCoreDistance (Polity polity) {

		PolityInfluence polityInfluence;

		if (!_polityInfluences.TryGetValue (polity.Id, out polityInfluence))
			return float.MaxValue;

		return polityInfluence.CoreDistance;
	}

	private float CalculateShortestPolityInfluenceCoreDistance (PolityInfluence pi) {

		if (pi.Polity.CoreGroup == this)
			return 0;

		float shortestDistance = float.MaxValue;
	
		foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors) {
		
			float distanceToCoreFromNeighbor = pair.Value.GetPolityInfluenceCoreDistance (pi.Polity);

			if (distanceToCoreFromNeighbor >= float.MaxValue)
				continue;
			
			float neighborDistance = Cell.NeighborDistances[pair.Key];

			float totalDistance = distanceToCoreFromNeighbor + neighborDistance;

			if (totalDistance < 0)
				continue;

			if (totalDistance < shortestDistance)
				shortestDistance = totalDistance;
		}

		return shortestDistance;
	}

	private void PrepareUpdateShortestPolityInfluenceCoreDistances () {
	
		foreach (PolityInfluence pi in _polityInfluences.Values) {
		
			pi.NewCoreDistance = CalculateShortestPolityInfluenceCoreDistance (pi);
		}
	}

	private float CalculatePolityInfluenceAdministrativeCost (PolityInfluence pi) {

		float influencedPopulation = Population * pi.Value;
		float distanceFactor = 500 + pi.CoreDistance;

		return influencedPopulation * distanceFactor * 0.001f;
	}

	private void UpdatePolityInfluenceAdministrativeCosts () {

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			pi.Polity.TotalAdministrativeCost -= pi.AdiministrativeCost;
			pi.AdiministrativeCost = CalculatePolityInfluenceAdministrativeCost (pi);
			pi.Polity.TotalAdministrativeCost += pi.AdiministrativeCost;
		}
	}

	private void PostUpdatePolityInfluences () {

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			pi.PostUpdate ();
		}
	}

	public void SetPolityInfluence (Polity polity, float newInfluenceValue) {

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if ((Id == Manager.TracingData.GroupId) || (polity.Id == Manager.TracingData.PolityId)) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
//
//				System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
//				string callingMethod = method.Name;
//
//				string callingClass = method.DeclaringType.ToString();
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"SetPolityInfluenceValue - Group:" + groupId + 
//					", polity.Id: " + polity.Id, 
//					"CurrentDate: " + World.CurrentDate + 
//					", polity.TotalGroupInfluenceValue: " + polity.TotalGroupInfluenceValue + 
//					", newInfluenceValue: " + newInfluenceValue + 
//					", caller: " + callingClass + ":" + callingMethod + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		#if DEBUG
		RunningFunction_SetPolityInfluence = true;

		if (Cell.IsSelected) {

			bool debug = true;
		}
		#endif

		PolityInfluence polityInfluence;

		_polityInfluences.TryGetValue (polity.Id, out polityInfluence);

		if (polityInfluence == null) {
			if (newInfluenceValue > Polity.MinPolityInfluence) {

				polityInfluence = new PolityInfluence (polity, newInfluenceValue);

				_polityInfluences.Add (polity.Id, polityInfluence);

				// We want to update the polity if a group is added.
				SetPolityUpdate (polityInfluence, true);

				ValidateAndSetHighestPolityInfluence (polityInfluence);

				TotalPolityInfluenceValue += newInfluenceValue;
				polity.TotalGroupInfluenceValue += newInfluenceValue;

				if (TotalPolityInfluenceValue > 1f) {
				
					throw new System.Exception ("Total influence value greater than 1: " + TotalPolityInfluenceValue);
				}

				polity.AddInfluencedGroup (this);
			}

			#if DEBUG
			RunningFunction_SetPolityInfluence = false;
			#endif

			return;
		}

		float oldInfluenceValue = polityInfluence.Value;

		if (newInfluenceValue <= Polity.MinPolityInfluence) {

			polityInfluence.Destroy ();
			
			_polityInfluences.Remove (polityInfluence.PolityId);

			polity.RemoveInfluencedGroup (this);

			// We want to update the polity if a group is removed.
			SetPolityUpdate (polityInfluence, true);

			if (polityInfluence == HighestPolityInfluence)
				FindHighestPolityInfluence ();

			TotalPolityInfluenceValue -= oldInfluenceValue;
			polity.TotalGroupInfluenceValue -= oldInfluenceValue;

			#if DEBUG
			RunningFunction_SetPolityInfluence = false;
			#endif

			return;
		}

		TotalPolityInfluenceValue -= oldInfluenceValue;
		polity.TotalGroupInfluenceValue -= oldInfluenceValue;

		polityInfluence.Value = newInfluenceValue;

		TotalPolityInfluenceValue += newInfluenceValue;
		polity.TotalGroupInfluenceValue += newInfluenceValue;

		if (TotalPolityInfluenceValue > 1f) {

			throw new System.Exception ("Total influence value greater than 1: " + TotalPolityInfluenceValue);
		}

		if (!(ValidateAndSetHighestPolityInfluence (polityInfluence)) && 
			(polityInfluence == HighestPolityInfluence) && 
			(oldInfluenceValue > newInfluenceValue))
			FindHighestPolityInfluence ();

		#if DEBUG
		RunningFunction_SetPolityInfluence = false;
		#endif
	}

	public void FindHighestPolityInfluence () {

		float highestInfluenceValue = float.MinValue;
		PolityInfluence highestInfluence = null;

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			if (pi.Value > highestInfluenceValue) {
			
				highestInfluenceValue = pi.Value;
				highestInfluence = pi;
			}
		}

		SetHighestPolityInfluence (highestInfluence);
	}

	public void RemovePolityInfluence (Polity polity) {

		PolityInfluence pi = null;

		if (!_polityInfluences.TryGetValue (polity.Id, out pi)) {
		
			throw new System.Exception ("Polity not actually influencing group");
		}

		_polityInfluences.Remove (polity.Id);

		TotalPolityInfluenceValue -= pi.Value;
		polity.TotalGroupInfluenceValue -= pi.Value;
	}

	public override void Synchronize () {

		Flags = new List<string> (_flags);

		Culture.Synchronize ();

		PolityInfluences = new List<PolityInfluence> (_polityInfluences.Values);
		
		base.Synchronize ();
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Flags.ForEach (f => _flags.Add (f));

		Cell = World.GetCell (Longitude, Latitude);

		Cell.Group = this;

		Neighbors = new Dictionary<Direction, CellGroup> (8);

		foreach (KeyValuePair<Direction,TerrainCell> pair in Cell.Neighbors) {
		
			if (pair.Value.Group != null) {
			
				CellGroup group = pair.Value.Group;

				Neighbors.Add (pair.Key, group);

				Direction dir = TerrainCell.ReverseDirection (pair.Key);

				group.AddNeighbor (dir, this);
			}
		}
		
		World.UpdateMostPopulousGroup (this);

		Culture.World = World;
		Culture.Group = this;
		Culture.FinalizeLoad ();

		if (SeaMigrationRoute != null) {
			
			SeaMigrationRoute.World = World;
			SeaMigrationRoute.FinalizeLoad ();
		}

		foreach (PolityInfluence p in PolityInfluences) {
		
			p.Polity = World.GetPolity (p.PolityId);

			if (p.Polity == null) { 
				throw new System.Exception ("Missing polity with id:" + p.PolityId);
			}

			_polityInfluences.Add (p.PolityId, p);

			if ((HighestPolityInfluence == null)  || (HighestPolityInfluence.Value < p.Value)) {
				HighestPolityInfluence = p;
			}
		}
	}
}
