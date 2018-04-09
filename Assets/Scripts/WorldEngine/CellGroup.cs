using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using UnityEngine.Profiling;

public class CellGroupSnapshot {

	public long Id;

	public bool HasMigrationEvent;
	public long MigrationEventDate;
	public int MigrationTargetLongitude;
	public int MigrationTargetLatitude;

	public bool HasPolityExpansionEvent;
	public long PolityExpansionEventDate;
	public long ExpansionTargetGroupId;
	public long ExpandingPolityId;

	public bool HasTribeFormationEvent;
	public long TribeFormationEventDate;

	public CellGroupSnapshot (CellGroup c) {

		Id = c.Id;

		HasMigrationEvent = c.HasMigrationEvent;
		MigrationEventDate = c.MigrationEventDate;
		MigrationTargetLongitude = c.MigrationTargetLongitude;
		MigrationTargetLatitude = c.MigrationTargetLatitude;

		HasPolityExpansionEvent = c.HasPolityExpansionEvent;
		PolityExpansionEventDate = c.PolityExpansionEventDate;
		ExpansionTargetGroupId = c.ExpansionTargetGroupId;
		ExpandingPolityId = c.ExpandingPolityId;

		HasTribeFormationEvent = c.HasTribeFormationEvent;
		TribeFormationEventDate = c.TribeFormationEventDate;
	}
}

public class CellGroup : HumanGroup {

	public const long GenerationSpan = 25 * World.YearLength;

	public const long MaxUpdateSpan = GenerationSpan * 8000;

	public const float MaxUpdateSpanFactor = MaxUpdateSpan / GenerationSpan;

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)

	public const float NaturalGrowthRate = NaturalBirthRate - NaturalDeathRate;
	
	public const float PopulationForagingConstant = 10;
	public const float PopulationFarmingConstant = 40;

	public const float MinKnowledgeTransferValue = 0.25f;

	public const float SeaTravelBaseFactor = 500f;

	public const float MigrationFactor = 0.1f;

	public const float MaxMigrationAltitudeDelta = 1f; // in meters

	public const float MaxCoreDistance = 1000000000000f;

	public static float TravelWidthFactor;

	[XmlAttribute]
	public long Id;

	[XmlAttribute("PrefMigDir")]
	public int PreferredMigrationDirectionInt;

	[XmlAttribute("PrevExPop")]
	public float PreviousExactPopulation;

	[XmlAttribute("ExPop")]
	public float ExactPopulation; // TODO: Get rid of 'float' population values
	
	[XmlAttribute("StilPres")]
	public bool StillPresent = true;

	[XmlAttribute("InDate")]
	public long InitDate;
	
	[XmlAttribute("LastUpDate")]
	public long LastUpdateDate;
	
	[XmlAttribute("NextUpDate")]
	public long NextUpdateDate;
	
	[XmlAttribute("OptPop")]
	public int OptimalPopulation;
	
	[XmlAttribute("Lon")]
	public int Longitude;
	[XmlAttribute("Lat")]
	public int Latitude;

	[XmlAttribute("SeaTrFac")]
	public float SeaTravelFactor = 0;

	[XmlAttribute("TotalPolInfVal")]
	public float TotalPolityInfluenceValueFloat = 0;

	[XmlAttribute("MigVal")]
	public float MigrationValue;

	[XmlAttribute("TotalMigVal")]
	public float TotalMigrationValue;

	[XmlAttribute("PolExpVal")]
	public float PolityExpansionValue;

	[XmlAttribute("TotalPolExpVal")]
	public float TotalPolityExpansionValue;

	[XmlAttribute("HasMigEv")]
	public bool HasMigrationEvent = false;
	[XmlAttribute("MigDate")]
	public long MigrationEventDate;
	[XmlAttribute("MigLon")]
	public int MigrationTargetLongitude;
	[XmlAttribute("MigLat")]
	public int MigrationTargetLatitude;
	[XmlAttribute("MigEvDir")]
	public int MigrationEventDirectionInt;

	[XmlAttribute("HasExpEv")]
	public bool HasPolityExpansionEvent = false;
	[XmlAttribute("PolExpDate")]
	public long PolityExpansionEventDate;
	[XmlAttribute("ExpTgtGrpId")]
	public long ExpansionTargetGroupId;
	[XmlAttribute("ExpPolId")]
	public long ExpandingPolityId;

	[XmlAttribute("HasTrbFrmEv")]
	public bool HasTribeFormationEvent = false;
	[XmlAttribute("TrbFrmDate")]
	public long TribeFormationEventDate;

	public Route SeaMigrationRoute = null;

	public List<string> Flags;

	public CellCulture Culture;

	public List<PolityInfluence> PolityInfluences;

	public List<long> FactionCoreIds;

	[XmlIgnore]
	public WorldPosition Position {

		get { 
			return Cell.Position;
		}
	}

	[XmlIgnore]
	public float TotalPolityInfluenceValue {

		get {
			return TotalPolityInfluenceValueFloat;
		}
		set { 
			TotalPolityInfluenceValueFloat = MathUtility.RoundToSixDecimals (Mathf.Clamp01 (value));
		}
	}

	[XmlIgnore]
	public Direction PreferredMigrationDirection;

	[XmlIgnore]
	public Dictionary<long, Faction> FactionCores = new Dictionary<long, Faction> ();

	[XmlIgnore]
	public UpdateCellGroupEvent UpdateEvent;

	[XmlIgnore]
	public MigrateGroupEvent MigrationEvent;

	[XmlIgnore]
	public ExpandPolityInfluenceEvent PolityExpansionEvent;

	[XmlIgnore]
	public TribeFormationEvent TribeCreationEvent;
	
	[XmlIgnore]
	public TerrainCell Cell;

	#if DEBUG
	[XmlIgnore]
	public bool DebugTagged = false;
	#endif

	[XmlIgnore]
	public Dictionary<string, BiomeSurvivalSkill> _biomeSurvivalSkills = new Dictionary<string, BiomeSurvivalSkill> (Biome.TypeCount);

	[XmlIgnore]
	public Dictionary<Direction, CellGroup> Neighbors;

	[XmlIgnore]
	public PolityInfluence HighestPolityInfluence = null;

	private Dictionary<long, PolityInfluence> _polityInfluences = new Dictionary<long, PolityInfluence> ();
	private HashSet<long> _polityInfluencesToRemove = new HashSet<long> ();
	private Dictionary<long, PolityInfluence> _polityInfluencesToAdd = new Dictionary<long, PolityInfluence> ();

//	private Dictionary<int, WorldEvent> _associatedEvents = new Dictionary<int, WorldEvent> ();
	
	private HashSet<string> _flags = new HashSet<string> ();

	private bool _alreadyUpdated = false;

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
				throw new System.Exception ("Debug.Break");
			}
			#endif

			return population;
		}
	}

	public CellGroup () {
		
		Manager.UpdateWorldLoadTrackEventCount ();
	}
	
	public CellGroup (MigratingGroup migratingGroup, int splitPopulation) : this (migratingGroup.World, migratingGroup.TargetCell, splitPopulation, migratingGroup.Culture, migratingGroup.MigrationDirection) {

		foreach (PolityInfluence p in migratingGroup.PolityInfluences) {

			_polityInfluencesToAdd.Add (p.PolityId, p);

			if (p.NewFactionCoreDistance == -1) {
				p.NewFactionCoreDistance = CalculateShortestFactionCoreDistance (p.Polity);
			}

			if (p.NewPolityCoreDistance == -1) {
				p.NewPolityCoreDistance = CalculateShortestPolityCoreDistance (p.Polity);
			}
		}
	}

	public CellGroup (World world, TerrainCell cell, int initialPopulation, Culture baseCulture = null, Direction migrationDirection = Direction.Null) : base(world) {

		InitDate = World.CurrentDate;
		LastUpdateDate = InitDate;

		PreviousExactPopulation = 0;
		ExactPopulation = initialPopulation;

		Cell = cell;
		Longitude = cell.Longitude;
		Latitude = cell.Latitude;

		Cell.Group = this;

		Id = Cell.GenerateUniqueIdentifier (World.CurrentDate, 1L, 0);

		if (migrationDirection == Direction.Null) {
			int offset = Cell.GetNextLocalRandomInt (RngOffsets.CELL_GROUP_UPDATE_MIGRATION_DIRECTION, TerrainCell.MaxNeighborDirections);

			PreferredMigrationDirection = Cell.TryGetNeighborDirection (offset);

		} else {
			PreferredMigrationDirection = migrationDirection;
		}

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

		InitializeDefaultPreferences (initialGroup);
		InitializeDefaultActivities (initialGroup);
		InitializeDefaultSkills (initialGroup);
		InitializeDefaultKnowledges (initialGroup);

		InitializeDefaultEvents ();

		World.AddUpdatedGroup (this);
	}

	public void UpdatePreferredMigrationDirection () {
	
		int dir = ((int)PreferredMigrationDirection) + RandomUtility.NoOffsetRange (Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_UPDATE_MIGRATION_DIRECTION));

		PreferredMigrationDirection = Cell.TryGetNeighborDirection (dir);
	}

	public Direction GenerateCoreMigrationDirection () {

		int dir = (int)PreferredMigrationDirection;

		float fDir = RandomUtility.PseudoNormalRepeatDistribution (Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_GENERATE_CORE_MIGRATION_DIRECTION), 0.05f, dir, TerrainCell.MaxNeighborDirections);

		return TryGetNeighborDirection ((int)fDir);
	}

	public Direction GeneratePolityExpansionDirection () {

		int dir = (int)PreferredMigrationDirection;

		float fDir = RandomUtility.PseudoNormalRepeatDistribution (Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_GENERATE_INFLUENCE_TRANSFER_DIRECTION), 0.05f, dir, TerrainCell.MaxNeighborDirections);

		return TryGetNeighborDirection ((int)fDir);
	}

	public Direction GenerateGroupMigrationDirection () {

		int dir = (int)PreferredMigrationDirection;

		float fDir = RandomUtility.PseudoNormalRepeatDistribution (Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_GENERATE_GROUP_MIGRATION_DIRECTION), 0.05f, dir, TerrainCell.MaxNeighborDirections);

		return Cell.TryGetNeighborDirection ((int)fDir);
	}

	public void AddFactionCore (Faction faction) {

		if (!FactionCores.ContainsKey (faction.Id)) {

			FactionCores.Add (faction.Id, faction);
		}
	}

	public void RemoveFactionCore (Faction faction) {

		if (FactionCores.ContainsKey (faction.Id)) {

			FactionCores.Remove (faction.Id);
		}
	}

	public bool FactionHasCoreHere (Faction faction) {

		return FactionCores.ContainsKey (faction.Id);
	}

	public ICollection<Faction> GetFactionCores () {

		return FactionCores.Values;
	}

	public CellGroupSnapshot GetSnapshot () {
	
		return new CellGroupSnapshot (this);
	}

	public long GenerateUniqueIdentifier (long date, long oom = 1L, long offset = 0L) {

		return Cell.GenerateUniqueIdentifier (date, oom, offset);
	}

	public void SetHighestPolityInfluence (PolityInfluence influence) {

		if (HighestPolityInfluence == influence)
			return;

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

			long triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate (this);

			if (triggerDate > World.MaxSupportedDate)
				return;

			if (triggerDate == long.MinValue)
				return;

			World.InsertEventToHappen (new BoatMakingDiscoveryEvent (this, triggerDate));
		}

		if (PlantCultivationDiscoveryEvent.CanSpawnIn (this)) {

			long triggerDate = PlantCultivationDiscoveryEvent.CalculateTriggerDate (this);

			if (triggerDate > World.MaxSupportedDate)
				return;

			if (triggerDate == long.MinValue)
				return;

			World.InsertEventToHappen (new PlantCultivationDiscoveryEvent (this, triggerDate));
		}
	}

	public void InitializeDefaultPreferences (bool initialGroup) {

		if (initialGroup) {
			Culture.AddPreferenceToAcquire (CellCulturalPreference.CreateAuthorityPreference (this, 0.5f));
			Culture.AddPreferenceToAcquire (CellCulturalPreference.CreateCohesivenessPreference (this, 0.5f));
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

	public int GetNextLocalRandomInt (int iterationOffset, int maxValue) {
	
		return Cell.GetNextLocalRandomInt (iterationOffset, maxValue);
	}

	public int GetLocalRandomInt (long date, int iterationOffset, int maxValue) {

		return Cell.GetLocalRandomInt (date, iterationOffset, maxValue);
	}

	public float GetNextLocalRandomFloat (int iterationOffset) {

		return Cell.GetNextLocalRandomFloat (iterationOffset);
	}

	public float GetLocalRandomFloat (long date, int iterationOffset) {

		return Cell.GetLocalRandomFloat (date, iterationOffset);
	}

	public void AddNeighbor (Direction direction, CellGroup group) {

		if (group == null)
			return;

		if (!group.StillPresent)
			return;

		if (Neighbors.ContainsValue (group))
			return;
	
		Neighbors.Add (direction, group);
	}
	
//	public void RemoveNeighbor (CellGroup group) {
//
//		Direction? direction = null;
//
//		bool found = false;
//
//		foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors) {
//
//			if (group == pair.Value) {
//			
//				direction = pair.Key;
//				found = true;
//				break;
//			}
//		}
//
//		if (!found)
//			return;
//		
//		Neighbors.Remove (direction.Value);
//	}

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
			throw new System.Exception ("Debug.Break");
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

//		#if DEBUG
//		float oldExactPopulation = ExactPopulation;
//		#endif

		ExactPopulation = newPopulation;

		#if DEBUG
		if (Population < -1000) {

			Debug.Break ();
			throw new System.Exception ("Debug.Break");
		}
		#endif

		Culture.MergeCulture (group.Culture, percentage);

		MergePolityInfluences (group.PolityInfluences, percentage);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"MergeGroup - Group:" + groupId, 
//					"CurrentDate: " + World.CurrentDate +
//					", group.SourceGroupId: " + group.SourceGroupId + 
//					", oldExactPopulation: " + oldExactPopulation + 
//					", source group.Population: " + group.Population + 
//					", newPopulation: " + newPopulation + 
//					", group.PolityInfluences.Count: " + group.PolityInfluences.Count + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		TriggerInterference ();
	}

	public void MergePolityInfluence (PolityInfluence sourcePolityInfluence, float percentOfTarget) {

		MergePolityInfluences (new List<PolityInfluence> { sourcePolityInfluence }, percentOfTarget);
	}

	public void MergePolityInfluences (List <PolityInfluence> sourcePolityInfluences, float percentOfTarget) {

		Dictionary<long, PolityInfluence> targetPolityInfluences = new Dictionary<long, PolityInfluence> (_polityInfluences);

		foreach (PolityInfluence pi in _polityInfluencesToAdd.Values) {
		
			targetPolityInfluences.Add (pi.PolityId, pi);
		}

		foreach (PolityInfluence pInfluence in sourcePolityInfluences) {

			Polity polity = pInfluence.Polity;
			float influenceValue = pInfluence.Value;

			float currentNewValue = 0;

			PolityInfluence pTargetPolityInfluence = null;

			if (targetPolityInfluences.TryGetValue (pInfluence.PolityId, out pTargetPolityInfluence)) {
			
				currentNewValue = pTargetPolityInfluence.NewValue;
				targetPolityInfluences.Remove (pTargetPolityInfluence.PolityId);
			}

			float newValue = (currentNewValue * (1 - percentOfTarget)) + (influenceValue * percentOfTarget);

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

		foreach (PolityInfluence pInfluence in targetPolityInfluences.Values) {

			float influenceValue = pInfluence.NewValue;

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
			throw new System.Exception ("Debug.Break");
		}
		#endif

		return splitPopulation;
	}

	public void PostUpdate_BeforePolityUpdates () {

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"PostUpdate - Group:" + Id,
//					"CurrentDate: " + World.CurrentDate + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		_alreadyUpdated = false;

		if (Population < 2) {
			World.AddGroupToRemove (this);
			return;
		}

		Profiler.BeginSample ("Update Terrain Farmland Percentage");

		UpdateTerrainFarmlandPercentage ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Culture PostUpdate");
	
		Culture.PostUpdate ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Set Faction Updates");

		SetFactionUpdates ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Set Polity Updates");

		SetPolityUpdates ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Post Update Polity Influences");

		PostUpdatePolityInfluences_BeforePolityUpdates ();

		Profiler.EndSample ();

		Profiler.BeginSample ("PostUpdate Polity Cultural Influences");

		PostUpdatePolityCulturalInfluences ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Polity Influence Administrative Costs");

		UpdatePolityInfluenceAdministrativeCosts ();

		Profiler.EndSample ();
	}

	public void PostUpdate_AfterPolityUpdates () {

		PostUpdatePolityInfluences_AfterPolityUpdates ();
	}

	public bool InfluencingPolityHasKnowledge(string id) {

		foreach (PolityInfluence pi in _polityInfluences.Values) {
			if (pi.Polity.Culture.GetKnowledge (id) != null) {
				return true;
			}
		}

		return false;
	}

	public void SetupForNextUpdate () {

		if (!StillPresent)
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

		Profiler.BeginSample ("Consider Influence Expansion");

		ConsiderPolityInfluenceExpansion ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Calculate Next Update Date");
		
		NextUpdateDate = CalculateNextUpdateDate ();

		Profiler.EndSample ();

		LastUpdateDate = World.CurrentDate;

		if (UpdateEvent == null) {
			UpdateEvent = new UpdateCellGroupEvent (this, NextUpdateDate);
		} else {
			UpdateEvent.Reset (NextUpdateDate);
		}

		World.InsertEventToHappen (UpdateEvent);
	}

	public float CalculateAltitudeDeltaFactor (TerrainCell targetCell) {

		if (targetCell == Cell)
			return 0.5f;

		float altitudeChange = Mathf.Max(0, targetCell.Altitude) - Mathf.Max(0, Cell.Altitude);
		float altitudeDelta = 2 * altitudeChange / (Cell.Area + targetCell.Area);

		float altitudeDeltaFactor = 1 - (Mathf.Clamp (altitudeDelta, -MaxMigrationAltitudeDelta, MaxMigrationAltitudeDelta) + MaxMigrationAltitudeDelta) / 2 * MaxMigrationAltitudeDelta;
		
		return altitudeDeltaFactor;
	}

	public float CalculateMigrationValue (TerrainCell cell) {

//		#if DEBUG
//		if (cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif
		
		float areaFactor = cell.Area / TerrainCell.MaxArea;

//		Profiler.BeginSample ("Calculate Altitude Delta Migration Factor");

		float altitudeDeltaFactor = CalculateAltitudeDeltaFactor (cell);
		float altitudeDeltaFactorPow = Mathf.Pow (altitudeDeltaFactor, 4);

		#if DEBUG
		if (float.IsNaN (altitudeDeltaFactorPow)) {
			throw new System.Exception ("float.IsNaN (altitudeDeltaFactorPow)");
		}
		#endif

//		Profiler.EndSample ();

		int existingPopulation = 0;

		float popDifferenceFactor = 1;

		if (cell.Group != null) {
			existingPopulation = cell.Group.Population;

			popDifferenceFactor = (float)Population / (float)(Population + existingPopulation);
			popDifferenceFactor = Mathf.Pow (popDifferenceFactor, 4);
		}

		float noMigrationFactor = 1;

		float optimalPopulation = OptimalPopulation;

		if (cell != Cell) {
			noMigrationFactor = MigrationFactor;

//			Profiler.BeginSample ("Calculate Optimal Population");

			optimalPopulation = CalculateOptimalPopulation (cell);

//			Profiler.EndSample ();
		}

		float targetOptimalPopulationFactor = 0;

		if (optimalPopulation > 0) {
			targetOptimalPopulationFactor = optimalPopulation / (existingPopulation + optimalPopulation);
//			targetOptimalPopulationFactor = Mathf.Pow (targetOptimalPopulationFactor, 4);
		}

		float cellValue = altitudeDeltaFactorPow * areaFactor * popDifferenceFactor * noMigrationFactor * targetOptimalPopulationFactor;

		#if DEBUG
		if (float.IsNaN (cellValue)) {

			Debug.Break ();
			throw new System.Exception ("float.IsNaN (cellValue)");
		}
		#endif

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
//						", altitudeDeltaFactor: " + altitudeDeltaFactor + 
//						", ExactPopulation: " + ExactPopulation + 
//						", target existingPopulation: " + existingPopulation + 
//						", popDifferenceFactor: " + popDifferenceFactor + 
//						", OptimalPopulation: " + OptimalPopulation + 
//						", target optimalPopulation: " + optimalPopulation + 
//						", targetOptimalPopulationFactor: " + targetOptimalPopulationFactor + 
//						"");
//
//					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
////				}
//			}
//		}
//		#endif

		return cellValue;
	}

	public long GeneratePastSpawnDate (long baseDate, int cycleLength, int offset = 0) {

		long currentDate = World.CurrentDate;

		long startCycleDate = baseDate + GetLocalRandomInt (baseDate, offset, cycleLength);

		long currentCycleDate = currentDate - (currentDate - startCycleDate) % cycleLength;

		long spawnDate = currentCycleDate + GetLocalRandomInt (currentCycleDate, offset, cycleLength);

		if (currentDate < spawnDate) {

			if (currentCycleDate == startCycleDate) {

				return baseDate;
			}

			long prevCycleDate = currentCycleDate - cycleLength;

			long prevSpawnDate = prevCycleDate + GetLocalRandomInt (prevCycleDate, offset, cycleLength);

			return prevSpawnDate;
		}

		return spawnDate;
	}

	public long GenerateFutureSpawnDate (long baseDate, int cycleLength, int offset = 0) {

		long currentDate = World.CurrentDate;

		long startCycleDate = baseDate + GetLocalRandomInt (baseDate, offset, cycleLength);

		long currentCycleDate = currentDate - (currentDate - startCycleDate) % cycleLength;

		long spawnDate = currentCycleDate + GetLocalRandomInt (currentCycleDate, offset, cycleLength);

		if (currentDate >= spawnDate) {

			long nextCycleDate = currentCycleDate + cycleLength;

			long nextSpawnDate = nextCycleDate + GetLocalRandomInt (nextCycleDate, offset, cycleLength);

			return nextSpawnDate;
		}

		return spawnDate;
	}

	public void TriggerInterference () {
	
		ResetSeaMigrationRoute ();
	}

	public void ResetSeaMigrationRoute () {
	
		if (SeaMigrationRoute == null)
			return;

		SeaMigrationRoute.Reset ();
	}

	public void DestroySeaMigrationRoute () {

		if (SeaMigrationRoute == null)
			return;

		SeaMigrationRoute.Destroy ();
		SeaMigrationRoute = null;
	}

	public void GenerateSeaMigrationRoute () {

		if (!Cell.IsPartOfCoastline)
			return;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				bool routePresent = SeaMigrationRoute == null;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"GenerateSeaMigrationRoute - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", route present: " + routePresent + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		if (SeaMigrationRoute == null) {
			SeaMigrationRoute = new Route (Cell);

		} else {
			if (SeaMigrationRoute.FirstCell == null) {
				Debug.LogError ("SeaMigrationRoute.FirstCell is null at " + Cell.Position);
			}

			SeaMigrationRoute.Reset ();
			SeaMigrationRoute.Build ();
		}

		bool invalidRoute = false;

		if (SeaMigrationRoute.LastCell == null)
			invalidRoute = true;

		if (SeaMigrationRoute.LastCell == SeaMigrationRoute.FirstCell)
			invalidRoute = true;

		if (SeaMigrationRoute.MigrationDirection == Direction.Null)
			invalidRoute = true;

		if (SeaMigrationRoute.FirstCell.Neighbors.ContainsValue (SeaMigrationRoute.LastCell))
			invalidRoute = true;

		if (invalidRoute) {
		
//			SeaMigrationRoute.Destroy ();
			return;
		}

		SeaMigrationRoute.Consolidate ();

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				TerrainCell targetCell = SeaMigrationRoute.LastCell;
//
//				string cellInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"GenerateSeaMigrationRoute - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", target cell: " + cellInfo + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif
	}

//	public void InitializeLocalMigrationValue () {
//
//		MigrationValue = 1;
//
//		TotalMigrationValue = 1;
//	}

	public void CalculateLocalMigrationValue () {

		MigrationValue = CalculateMigrationValue (Cell);

		TotalMigrationValue = MigrationValue;

		#if DEBUG
		if (float.IsNaN (TotalMigrationValue)) {

			throw new System.Exception ("float.IsNaN (TotalMigrationValue)");
		}
		#endif
	}

	private class CellWeight : CollectionUtility.ElementWeightPair<TerrainCell> {

		public CellWeight (TerrainCell cell, float weight) : base (cell, weight) {
			
		}
	}

	private class GroupWeight : CollectionUtility.ElementWeightPair<CellGroup> {

		public GroupWeight (CellGroup group, float weight) : base (group, weight) {

		}
	}

	private class PolityInfluenceWeight : CollectionUtility.ElementWeightPair<PolityInfluence> {

		public PolityInfluenceWeight (PolityInfluence polityInfluence, float weight) : base (polityInfluence, weight) {

		}
	}
	
	public void ConsiderLandMigration () {

//		#if DEBUG
//		if (Cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		if (HasMigrationEvent)
			return;

//		Profiler.BeginSample ("Select Random Target Cell For Migration");

		UpdatePreferredMigrationDirection ();

//		int targetCellIndex = Cell.GetNextLocalRandomInt (RngOffsets.CELL_GROUP_CONSIDER_LAND_MIGRATION_TARGET, Cell.Neighbors.Count);
//
//		TerrainCell targetCell = Cell.Neighbors.Values.ElementAt (targetCellIndex);

		Direction migrationDirection = GenerateGroupMigrationDirection ();

		TerrainCell targetCell = Cell.Neighbors [migrationDirection];

//		#if DEBUG
//		if (Cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif

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
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

//		Profiler.EndSample ();

//		Profiler.BeginSample ("Calculate Migration Value");

		float cellValue = CalculateMigrationValue (targetCell);

		TotalMigrationValue += cellValue;

		#if DEBUG
		if (float.IsNaN (TotalMigrationValue)) {

			throw new System.Exception ("float.IsNaN (TotalMigrationValue)");
		}
		#endif

//		Profiler.EndSample ();

		float migrationChance = cellValue / TotalMigrationValue;

		float rollValue = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CONSIDER_LAND_MIGRATION_CHANCE);

		if (rollValue > migrationChance)
			return;
		
		float cellSurvivability = 0;
		float cellForagingCapacity = 0;

//		Profiler.BeginSample ("Calculate Adaption To Cell");
		
		CalculateAdaptionToCell (targetCell, out cellForagingCapacity, out cellSurvivability);

//		Profiler.EndSample ();

		if (cellSurvivability <= 0)
			return;

//		Profiler.BeginSample ("Calculate Altitude Delta Migration Factor");

		float cellAltitudeDeltaFactor = CalculateAltitudeDeltaFactor (targetCell);

//		Profiler.EndSample ();

		float travelFactor = 
			cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
			cellSurvivability * cellSurvivability * targetCell.Accessibility;

		travelFactor = Mathf.Clamp (travelFactor, 0.0001f, 1);

		int travelTime = (int)Mathf.Ceil(World.YearLength * Cell.Width / (TravelWidthFactor * travelFactor));
		
		long nextDate = World.CurrentDate + travelTime;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ConsiderLandMigration - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		SetMigrationEvent (targetCell, migrationDirection, nextDate);
	}

	public void ConsiderSeaMigration () {

		if (SeaTravelFactor <= 0)
			return;

		if (HasMigrationEvent)
			return;

//		#if DEBUG
//		bool hadMigrationRoute = SeaMigrationRoute != null;
//
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ConsiderSeaMigration - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", has migration route: " + hadMigrationRoute + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		if ((SeaMigrationRoute == null) ||
			(!SeaMigrationRoute.Consolidated)) {
		
			GenerateSeaMigrationRoute ();

			if ((SeaMigrationRoute == null) ||
				(!SeaMigrationRoute.Consolidated))
				return;
		}

		TerrainCell targetCell = SeaMigrationRoute.LastCell;
		Direction migrationDirection = SeaMigrationRoute.MigrationDirection;

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

		TotalMigrationValue += CalculateMigrationValue (targetCell);

		if (float.IsNaN (TotalMigrationValue)) {
			throw new System.Exception ("float.IsNaN (TotalMigrationValue)");
		}

		float cellSurvivability = 0;
		float cellForagingCapacity = 0;

		CalculateAdaptionToCell (targetCell, out cellForagingCapacity, out cellSurvivability);

		if (cellSurvivability <= 0)
			return;

		float routeLength = SeaMigrationRoute.Length;
		float routeLengthFactor = Mathf.Pow (routeLength, 2);

		float successChance = SeaTravelFactor / (SeaTravelFactor + routeLengthFactor);

		float attemptValue = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CONSIDER_SEA_MIGRATION);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ConsiderSeaMigration - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
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

		int travelTime = (int)Mathf.Ceil(World.YearLength * routeLength / SeaTravelFactor);

		long nextDate = World.CurrentDate + travelTime;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ConsiderSeaMigration - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		SetMigrationEvent (targetCell, migrationDirection, nextDate);
	}

	private void SetMigrationEvent (TerrainCell targetCell, Direction migrationDirection, long nextDate) {

		if (MigrationEvent == null) {
			MigrationEvent = new MigrateGroupEvent (this, targetCell, migrationDirection, nextDate);
		} else {
			MigrationEvent.Reset (targetCell, migrationDirection, nextDate);
		}

		World.InsertEventToHappen (MigrationEvent);

		HasMigrationEvent = true;

		MigrationEventDate = nextDate;
		MigrationTargetLongitude = targetCell.Longitude;
		MigrationTargetLatitude = targetCell.Latitude;
		MigrationEventDirectionInt = (int)migrationDirection;
	}

	public Direction TryGetNeighborDirection (int offset) {

		if (Neighbors.Count <= 0)
			return Direction.Null;

		int dir = (int)Mathf.Repeat (offset, TerrainCell.MaxNeighborDirections);

		while (true) {
			if (Neighbors.ContainsKey ((Direction)dir))
				return (Direction)dir;

			dir = (dir + TerrainCell.NeighborSearchOffset) % TerrainCell.MaxNeighborDirections;
		}
	}

	public void ConsiderPolityInfluenceExpansion () {

//		#if DEBUG
//		if (Cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		PolityExpansionValue = 0;
		TotalPolityExpansionValue = 0;

		if (_polityInfluences.Count <= 0)
			return;

		if (Neighbors.Count <= 0)
			return;

		if (HasPolityExpansionEvent)
			return;

//		Profiler.BeginSample ("Select Random Polity Influence");

		List<PolityInfluenceWeight> polityInfluenceWeights = new List<PolityInfluenceWeight> (_polityInfluences.Count);

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			polityInfluenceWeights.Add (new PolityInfluenceWeight (pi, pi.Value));
		}

		PolityInfluence selectedPi = CollectionUtility.WeightedSelection (
			polityInfluenceWeights.ToArray (), TotalPolityInfluenceValue, () => Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CONSIDER_POLITY_INFLUENCE_EXPANSION_POLITY));

//		Profiler.EndSample ();

		PolityExpansionValue = 1;
		TotalPolityExpansionValue = 1;

//		Profiler.BeginSample ("Select Random Target Group for Polity Expansion");

//		int targetGroupIndex = Cell.GetNextLocalRandomInt (RngOffsets.CELL_GROUP_CONSIDER_POLITY_INFLUENCE_EXPANSION_TARGET, TerrainCell.MaxNeighborDirections);
//
//		CellGroup targetGroup = GetNeighborGroup (targetGroupIndex);

		Direction expansionDirection = GeneratePolityExpansionDirection ();

		if (expansionDirection == Direction.Null)
			return;

		CellGroup targetGroup = Neighbors [expansionDirection];

//		Profiler.EndSample ();

		if (!targetGroup.StillPresent)
			return;

//		Profiler.BeginSample ("Calculate Polity Expansion Value");

		float groupValue = selectedPi.Polity.CalculateGroupInfluenceExpansionValue (this, targetGroup, selectedPi.Value);

		if (groupValue <= 0)
			return;

		TotalPolityExpansionValue += groupValue;

//		Profiler.EndSample ();

		float expansionChance = groupValue / TotalPolityExpansionValue;

		float rollValue = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CONSIDER_POLITY_INFLUENCE_EXPANSION_CHANCE);

		if (rollValue > expansionChance)
			return;

		float cellSurvivability = 0;
		float cellForagingCapacity = 0;

		CalculateAdaptionToCell (targetGroup.Cell, out cellForagingCapacity, out cellSurvivability);

		if (cellSurvivability <= 0)
			return;

		float cellAltitudeDeltaFactor = CalculateAltitudeDeltaFactor (targetGroup.Cell);

		float travelFactor = 
			cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
			cellSurvivability * cellSurvivability * targetGroup.Cell.Accessibility;

		travelFactor = Mathf.Clamp (travelFactor, 0.0001f, 1);

		int travelTime = (int)Mathf.Ceil(World.YearLength * Cell.Width / (TravelWidthFactor * travelFactor));

		long nextDate = World.CurrentDate + travelTime;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//				string targetGroupId = "Id:" + targetGroup.Id + "|Long:" + targetGroup.Longitude + "|Lat:" + targetGroup.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ConsiderPolityInfluenceExpansion - Group:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					"' Neighbors.Count: " + Neighbors.Count +
//					", groupValue: " + groupValue +  
//					", TotalPolityExpansionValue: " + TotalPolityExpansionValue + 
//					", rollValue: " + rollValue + 
//					", travelFactor: " + travelFactor + 
//					", nextDate: " + nextDate + 
//					", selectedPi.PolityId: " + selectedPi.PolityId + 
//					", targetGroup: " + targetGroupId + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		if (PolityExpansionEvent == null) {
			PolityExpansionEvent = new ExpandPolityInfluenceEvent (this, selectedPi.Polity, targetGroup, nextDate);
		} else {
			PolityExpansionEvent.Reset (selectedPi.Polity, targetGroup, nextDate);
		}

		World.InsertEventToHappen (PolityExpansionEvent);

		HasPolityExpansionEvent = true;

		PolityExpansionEventDate = nextDate;
		ExpandingPolityId = selectedPi.PolityId;
		ExpansionTargetGroupId = targetGroup.Id;
	}

	public void Destroy () {

		StillPresent = false;

		foreach (Faction faction in GetFactionCores ()) {

			#if DEBUG
			Debug.Log ("Faction will be removed due to core group dissapearing. faction id: " + faction.Id + ", group id:" + Id);
			#endif

			World.AddFactionToRemove (faction);
		}

		RemovePolityInfluences ();

		Cell.Group = null;
		World.RemoveGroup (this);

		foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors) {
			pair.Value.RemoveNeighbor (TerrainCell.ReverseDirection(pair.Key));
		}

		DestroySeaMigrationRoute ();

		Cell.FarmlandPercentage = 0;
	}

	public void RemovePolityInfluences () {

		// Make sure all influencing polities get updated
		SetPolityUpdates (true);

		PolityInfluence[] polityInfluences = new PolityInfluence[_polityInfluences.Count];
		_polityInfluences.Values.CopyTo (polityInfluences, 0);

		foreach (PolityInfluence polityInfluence in polityInfluences) {

			Polity polity = polityInfluence.Polity;

			polity.RemoveInfluencedGroup (this);

			// We want to update the polity if a group is removed.
			SetPolityUpdate (polityInfluence, true);
		}

		SetHighestPolityInfluence (null);
	}

	#if DEBUG

	public delegate void UpdateCalledDelegate ();

	public static UpdateCalledDelegate UpdateCalled = null; 

	#endif

	public void Update () {

		if (!StillPresent) {
			Debug.LogWarning ("Group is no longer present");
			return;
		}

		if (_alreadyUpdated)
			return;

		PreviousExactPopulation = ExactPopulation;
		
		long timeSpan = World.CurrentDate - LastUpdateDate;

		if (timeSpan <= 0)
			return;

		#if DEBUG
		if (UpdateCalled != null) {

			UpdateCalled ();
		}
		#endif

		_alreadyUpdated = true;

		Profiler.BeginSample ("Update Population");

		UpdatePopulation (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Culture");

		UpdateCulture (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Polity Cultural Influences");

		UpdatePolityCulturalInfluences (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Polity Update Effects");

		PolityUpdateEffects (timeSpan);

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Travel Factors");

		UpdateTravelFactors ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Shortest Polity Core Distances");

		UpdateShortestPolityCoreDistances ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Shortest Faction Core Distances");

		UpdateShortestFactionCoreDistances ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Add Updated Group");
		
		World.AddUpdatedGroup (this);

		Profiler.EndSample ();
	}

	private void SetFactionUpdates () {

		foreach (Faction faction in FactionCores.Values) {

			World.AddFactionToUpdate (faction);
		}
	}

	private void SetPolityUpdates (bool forceUpdate = false) {
	
		foreach (PolityInfluence pi in _polityInfluences.Values) {
		
			SetPolityUpdate (pi, forceUpdate);
		}
	}

	public void SetPolityUpdate (PolityInfluence pi, bool forceUpdate) {

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//
//				System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
//
//				System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
//				string callingMethod = method.Name;
//
////				int frame = 2;
////				while (callingMethod.Contains ("GetNextLocalRandom") || callingMethod.Contains ("GetNextRandom")) {
////					method = stackTrace.GetFrame(frame).GetMethod();
////					callingMethod = method.Name;
////
////					frame++;
////				}
//
//				string callingClass = method.DeclaringType.ToString();
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"SetPolityUpdate - Group:" + Id,
//					"CurrentDate: " + World.CurrentDate + 
//					"forceUpdate: " + forceUpdate + 
//					"caller: " + callingClass + "::" + callingMethod +
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		Polity p = pi.Polity;

		if (p.WillBeUpdated)
			return;

		if (forceUpdate || (p.CoreGroup == this)) {

			World.AddPolityToUpdate (p);
			return;
		}

		int groupCount = p.InfluencedGroups.Count;

		if (groupCount <= 0)
			return;

		// If group is not the core group then there's a chance no polity update will happen

		float chanceFactor = 1f / (float)groupCount;

		float rollValue = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_SET_POLITY_UPDATE + (int)p.Id);

		#if DEBUG
		if (Manager.RegisterDebugEvent != null) {

			System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

			System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
			string callingMethod = method.Name;

			//				int frame = 2;
			//				while (callingMethod.Contains ("GetNextLocalRandom") || callingMethod.Contains ("GetNextRandom")) {
			//					method = stackTrace.GetFrame(frame).GetMethod();
			//					callingMethod = method.Name;
			//
			//					frame++;
			//				}

			string callingClass = method.DeclaringType.ToString();

			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
				"SetPolityUpdate - After roll - Group:" + Id,
				"CurrentDate: " + World.CurrentDate + 
				", polity Id: " + p.Id + 
				", chanceFactor: " + chanceFactor + 
				", rollValue: " + rollValue + 
				", forceUpdate: " + forceUpdate + 
				", caller: " + callingClass + "::" + callingMethod +
				"");

			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
		}
		#endif

		if (rollValue <= chanceFactor)
			World.AddPolityToUpdate (p);
	}
	
	private void UpdatePopulation (long timeSpan) {
		
		ExactPopulation = PopulationAfterTime (timeSpan);
	}
	
	private void UpdateCulture (long timeSpan) {
		
		Culture.Update (timeSpan);
	}

	private void UpdatePolityCulturalInfluences (long timeSpan) {
	
		foreach (PolityInfluence pi in _polityInfluences.Values) {
		
			Culture.UpdatePolityCulturalInfluence (pi, timeSpan);
		}
	}

	private void PostUpdatePolityCulturalInfluences () {

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			Culture.PostUpdatePolityCulturalInfluence (pi);
		}
	}

	private void PolityUpdateEffects (long timeSpan) {

//		#if DEBUG
//		if ((Cell.Longitude == 229) && (Cell.Latitude == 120)) {
//			if ((_polityInfluences.Count + _polityInfluencesToAdd.Count) == 2) {
//				bool debug = true;
//			}
//		}
//		#endif

		foreach (PolityInfluence polityInfluence in _polityInfluences.Values) {

			if (_polityInfluencesToRemove.Contains (polityInfluence.PolityId))
				continue;
		
			Polity polity = polityInfluence.Polity;
			float influenceValue = polityInfluence.NewValue;

			polity.GroupUpdateEffects (this, influenceValue, TotalPolityInfluenceValue, timeSpan);
		}

		if (HasTribeFormationEvent)
			return;

		if (TribeFormationEvent.CanSpawnIn (this)) {

			long triggerDate = TribeFormationEvent.CalculateTriggerDate (this);

			if (triggerDate == int.MinValue)
				return;

			if (TribeCreationEvent == null) {
				TribeCreationEvent = new TribeFormationEvent (this, triggerDate);
			} else {
				TribeCreationEvent.Reset (triggerDate);
			}

			World.InsertEventToHappen (TribeCreationEvent);

			HasTribeFormationEvent = true;

			TribeFormationEventDate = triggerDate;
		}
	}

	private float GetActivityContribution (string activityId) {
	
		CellCulturalActivity activity = Culture.GetActivity (activityId) as CellCulturalActivity;

		if (activity == null)
			return 0;

		return activity.Contribution;
	}

	private void UpdateTerrainFarmlandPercentage () {

//		#if DEBUG
//		if (Cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		CulturalKnowledge agricultureKnowledge = Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId);

		if (agricultureKnowledge == null) {

			return;
		}

		float knowledgeValue = agricultureKnowledge.ScaledValue;

		float techValue = Mathf.Sqrt(knowledgeValue);

		float areaPerFarmWorker = techValue / 5f;

		float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn (Cell);

		float farmingPopulation = GetActivityContribution (CellCulturalActivity.FarmingActivityId) * Population;

		float maxWorkableArea = areaPerFarmWorker * farmingPopulation;

		float availableArea = Cell.Area * terrainFactor;

		float farmlandPercentage = 0;

		if ((maxWorkableArea > 0) && (availableArea > 0)) {

			float farmlandPercentageAvailableArea = maxWorkableArea / (maxWorkableArea + availableArea);

			farmlandPercentage = farmlandPercentageAvailableArea * terrainFactor;
		}

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

//		#if DEBUG
//		if (Cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		SeaTravelFactor = SeaTravelBaseFactor * seafaringValue * shipbuildingValue * TravelWidthFactor;
	}

	public int CalculateOptimalPopulation (TerrainCell cell) {

		int optimalPopulation = 0;

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

		float foragingContribution = GetActivityContribution (CellCulturalActivity.ForagingActivityId);

		CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

		float populationCapacityByForaging = foragingContribution * PopulationForagingConstant * cell.Area * modifiedForagingCapacity;

		float farmingContribution = GetActivityContribution (CellCulturalActivity.FarmingActivityId);
		float populationCapacityByFarming = 0;

		if (farmingContribution > 0) {

			float farmingCapacity = CalculateFarmingCapacity (cell);

			populationCapacityByFarming = farmingContribution * PopulationFarmingConstant * cell.Area * farmingCapacity;
		}

		float accesibilityFactor = 0.25f + 0.75f * cell.Accessibility; 

		float populationCapacity = (populationCapacityByForaging + populationCapacityByFarming) * modifiedSurvivability * accesibilityFactor;

		optimalPopulation = (int)Mathf.Floor (populationCapacity);

		#if DEBUG
		if (optimalPopulation < -1000) {

			Debug.Break ();
			throw new System.Exception ("Debug.Break");
		}
		#endif

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == Manager.TracingData.GroupId) {
//				if ((cell.Longitude == Longitude) && (cell.Latitude == Latitude)) {
//					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//					string cellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
//
//					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//						"CalculateOptimalPopulation - Group:" + groupId,
//						"CurrentDate: " + World.CurrentDate + 
//						", target cellInfo: " + cellInfo + 
//						", foragingContribution: " + foragingContribution + 
////						", Area: " + cell.Area + 
//						", modifiedForagingCapacity: " + modifiedForagingCapacity + 
//						", modifiedSurvivability: " + modifiedSurvivability + 
//						", accesibilityFactor: " + accesibilityFactor + 
//						", optimalPopulation: " + optimalPopulation + 
//						"");
//
//					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//				}
//			}
//		}
//		#endif

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

//		Profiler.BeginSample ("Get Group Skill Values");

		foreach (string biomeName in cell.PresentBiomeNames) {

//			Profiler.BeginSample ("Try Get Group Biome Survival Skill");

			float biomePresence = cell.GetBiomePresence(biomeName);

			BiomeSurvivalSkill skill = null;

			Biome biome = Biome.Biomes[biomeName];

			if (_biomeSurvivalSkills.TryGetValue (biomeName, out skill)) {

//				Profiler.BeginSample ("Evaluate Group Biome Survival Skill");

				modifiedForagingCapacity += biomePresence * biome.ForagingCapacity * skill.Value;
				modifiedSurvivability += biomePresence * (biome.Survivability + skill.Value * (1 - biome.Survivability));

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

//				Profiler.EndSample ();

			} else {
			
				modifiedSurvivability += biomePresence * biome.Survivability;
			}

//			Profiler.EndSample ();
		}

//		Profiler.EndSample ();

		float altitudeSurvivabilityFactor = 1 - Mathf.Clamp01 (cell.Altitude / World.MaxPossibleAltitude);

		modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;

		foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
		survivability = modifiedSurvivability * altitudeSurvivabilityFactor;

		if (survivability > 1) {
			throw new System.Exception ("Modified survivability greater than 1: " + survivability);
		}

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

	public long CalculateNextUpdateDate () {

//		#if DEBUG
//		if (Cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		#if DEBUG
		if (FactionCores.Count > 0) {
			foreach (Faction faction in FactionCores.Values) {
				if (faction.CoreGroupId != Id) {
					Debug.LogError ("Group identifies as faction core when it no longer is. Id: " + Id + ", CoreId: " + faction.CoreGroupId + ", current date: " + World.CurrentDate);
				}
			}
		}
		#endif

		float randomFactor = Cell.GetNextLocalRandomFloat (RngOffsets.CELL_GROUP_CALCULATE_NEXT_UPDATE);
		randomFactor = 1f - Mathf.Pow (randomFactor, 4);

		float migrationFactor = 1;

		if (TotalMigrationValue > 0) {
			migrationFactor = MigrationValue / TotalMigrationValue;
			migrationFactor = Mathf.Pow (migrationFactor, 4);
		}

		float polityExpansionFactor = 1;

		if (TotalPolityExpansionValue > 0) {
			polityExpansionFactor = PolityExpansionValue / TotalPolityExpansionValue;
			polityExpansionFactor = Mathf.Pow (polityExpansionFactor, 4);
		}

		float skillLevelFactor = Culture.MinimumSkillAdaptationLevel ();
		float knowledgeLevelFactor = Culture.MinimumKnowledgeProgressLevel ();

		float populationFactor = 0.0001f + Mathf.Abs (OptimalPopulation - Population);
		populationFactor = 100 * OptimalPopulation / populationFactor;

		populationFactor = Mathf.Min(populationFactor, MaxUpdateSpanFactor);

		float mixFactor = randomFactor * migrationFactor * polityExpansionFactor * skillLevelFactor * knowledgeLevelFactor * populationFactor;

		long updateSpan = GenerationSpan * (int)mixFactor;

		if (updateSpan < 0)
			updateSpan = MaxUpdateSpan;

		updateSpan = (updateSpan < GenerationSpan) ? GenerationSpan : updateSpan;
		updateSpan = (updateSpan > MaxUpdateSpan) ? MaxUpdateSpan : updateSpan;

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

		return World.CurrentDate + updateSpan;
	}

	public float PopulationAfterTime (long time) { // in years

		float population = ExactPopulation;
		
		if (population == OptimalPopulation)
			return population;
		
		float timeFactor = NaturalGrowthRate * time / (float)GenerationSpan;

		if (population < OptimalPopulation) {
			
			float geometricTimeFactor = Mathf.Pow(2, timeFactor);
			float populationFactor = 1 - ExactPopulation/(float)OptimalPopulation;

			population = OptimalPopulation * MathUtility.RoundToSixDecimals (1 - Mathf.Pow(populationFactor, geometricTimeFactor));

			#if DEBUG
			if ((int)population < -1000) {

				Debug.Break ();
				throw new System.Exception ("Debug.Break");
			}
			#endif

//			#if DEBUG
//			if (Manager.RegisterDebugEvent != null) {
//				if (Id == Manager.TracingData.GroupId) {
//					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//						"PopulationAfterTime:increase - Group:" + groupId,
//						"CurrentDate: " + World.CurrentDate + 
//						", OptimalPopulation: " + OptimalPopulation + 
//						", ExactPopulation: " + ExactPopulation + 
//						", new population: " + population + 
//						"");
//
//					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//				}
//			}
//			#endif

			return population;
		}

		if (population > OptimalPopulation) {

			population = OptimalPopulation + (ExactPopulation - OptimalPopulation) * MathUtility.RoundToSixDecimals (Mathf.Exp (-timeFactor));

			#if DEBUG
			if ((int)population < -1000) {

				Debug.Break ();
				throw new System.Exception ("Debug.Break");
			}
			#endif

//			#if DEBUG
//			if (Manager.RegisterDebugEvent != null) {
//				if (Id == Manager.TracingData.GroupId) {
//					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//						"PopulationAfterTime:decrease - Group:" + groupId,
//						"CurrentDate: " + World.CurrentDate + 
//						", OptimalPopulation: " + OptimalPopulation + 
//						", ExactPopulation: " + ExactPopulation + 
//						", new population: " + population + 
//						"");
//
//					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//				}
//			}
//			#endif
			
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

	public PolityInfluence GetPolityInfluence (Polity polity) {

		PolityInfluence polityInfluence;

		if (!_polityInfluences.TryGetValue (polity.Id, out polityInfluence))
			return null;

		return polityInfluence;
	}

	public float GetPolityInfluenceValue (Polity polity) {

		PolityInfluence polityInfluence;

		if (!_polityInfluences.TryGetValue (polity.Id, out polityInfluence))
			return 0;

		return polityInfluence.Value;
	}

	public float GetFactionCoreDistance (Polity polity) {

		PolityInfluence polityInfluence;

		if (!_polityInfluences.TryGetValue (polity.Id, out polityInfluence)) {
			return float.MaxValue;
		}

		return polityInfluence.FactionCoreDistance;
	}

	public float GetPolityCoreDistance (Polity polity) {

		PolityInfluence polityInfluence;

		if (!_polityInfluences.TryGetValue (polity.Id, out polityInfluence)) {
			return float.MaxValue;
		}

		return polityInfluence.PolityCoreDistance;
	}

	private float CalculateShortestFactionCoreDistance (Polity polity) {

		foreach (Faction faction in polity.GetFactions ()) {
			if (faction.CoreGroup == this)
				return 0;
		}

		float shortestDistance = MaxCoreDistance;
	
		foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors) {
		
			float distanceToCoreFromNeighbor = pair.Value.GetFactionCoreDistance (polity);

			if (distanceToCoreFromNeighbor == float.MaxValue)
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

	private float CalculateShortestPolityCoreDistance (Polity polity) {

		if (polity.CoreGroup == this)
			return 0;

		float shortestDistance = MaxCoreDistance;

		foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors) {

			float distanceToCoreFromNeighbor = pair.Value.GetPolityCoreDistance (polity);

			if (distanceToCoreFromNeighbor == float.MaxValue)
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

	private void UpdateShortestFactionCoreDistances () {
	
		foreach (PolityInfluence pi in _polityInfluences.Values) {
		
			pi.NewFactionCoreDistance = CalculateShortestFactionCoreDistance (pi.Polity);
		}
	}

	private void UpdateShortestPolityCoreDistances () {

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			pi.NewPolityCoreDistance = CalculateShortestPolityCoreDistance (pi.Polity);
		}
	}

	private float CalculatePolityInfluenceAdministrativeCost (PolityInfluence pi) {

		float influencedPopulation = Population * pi.Value;

		float distanceFactor = 500 + pi.FactionCoreDistance;

		float cost = influencedPopulation * distanceFactor * 0.001f;

		if (cost < 0)
			return float.MaxValue;

		return cost;
	}

	private void UpdatePolityInfluenceAdministrativeCosts () {

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			pi.AdiministrativeCost = CalculatePolityInfluenceAdministrativeCost (pi);
		}
	}

	public void PostUpdatePolityInfluences_BeforePolityUpdates () {

		TotalPolityInfluenceValue = 0;

		foreach (long polityId in _polityInfluencesToRemove) {

			PolityInfluence pi;

			if (!_polityInfluences.TryGetValue (polityId, out pi)) {
				if (!_polityInfluencesToAdd.TryGetValue (polityId, out pi)) {
				
					Debug.LogWarning ("Trying to remove nonexisting PolityInfluence with id: " + polityId + " from group with id: " + Id);
				}

				_polityInfluencesToAdd.Remove (pi.PolityId);

			} else {

				_polityInfluences.Remove (pi.PolityId);

				// Decreate polity contacts
				foreach (PolityInfluence epi in _polityInfluences.Values) {

					Polity.DecreaseContactGroupCount (pi.Polity, epi.Polity);
				}

				// Remove all polity faction cores from group
				foreach (Faction faction in GetFactionCores ()) {

					if (faction.PolityId == pi.PolityId) {

						#if DEBUG
						Debug.Log ("Faction will be removed due to total loss of polity influence. faction id: " + faction.Id + ", group id:" + Id);
						#endif

						World.AddFactionToRemove (faction);
					}
				}

				#if DEBUG
				if (this == pi.Polity.CoreGroup) {
					Debug.LogWarning ("Polity has lost it's core group. Group Id: " + Id + ", Polity Id: " + pi.Polity.Id);
				}
				#endif

				pi.Polity.RemoveInfluencedGroup (this);

				// We want to update the polity if a group is removed.
				SetPolityUpdate (pi, true);
			}
		}

		_polityInfluencesToRemove.Clear ();

		foreach (PolityInfluence pi in _polityInfluencesToAdd.Values) {

			// Increase polity contacts
			foreach (PolityInfluence epi in _polityInfluences.Values) {
			
				Polity.IncreaseContactGroupCount (pi.Polity, epi.Polity);
			}
		
			_polityInfluences.Add (pi.PolityId, pi);

			// We want to update the polity if a group is added.
			SetPolityUpdate (pi, true);

			pi.Polity.AddInfluencedGroup (this);
		}

		_polityInfluencesToAdd.Clear ();

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			pi.PostUpdate ();

			TotalPolityInfluenceValue += pi.Value;
		}

		#if DEBUG
		if (TotalPolityInfluenceValue > 1.0) {
		
			Debug.LogWarning ("Total Polity Influence Value greater than 1: " + TotalPolityInfluenceValue);
		}
		#endif

		#if DEBUG
		if (TotalPolityInfluenceValue <= 0) {

			if (GetFactionCores ().Count > 0) {

				Debug.LogWarning ("Group with no polity influence has faction cores - Id: " + Id);
			}
		}
		#endif

		FindHighestPolityInfluence ();
	}

	public void PostUpdatePolityInfluences_AfterPolityUpdates () {

		TotalPolityInfluenceValue = 0;

		foreach (long polityId in _polityInfluencesToRemove) {

			PolityInfluence pi;

			if (!_polityInfluences.TryGetValue (polityId, out pi)) {
				if (!_polityInfluencesToAdd.TryGetValue (polityId, out pi)) {

					Debug.LogWarning ("Trying to remove nonexisting PolityInfluence with id: " + polityId + " from group with id: " + Id);
				}

			} else {

				_polityInfluences.Remove (pi.PolityId);
			}
		}

		_polityInfluencesToRemove.Clear ();

		foreach (PolityInfluence pi in _polityInfluences.Values) {

			TotalPolityInfluenceValue += pi.Value;
		}

		#if DEBUG
		if (TotalPolityInfluenceValue > 1.0) {

			Debug.LogWarning ("Total Polity Influence Value greater than 1: " + TotalPolityInfluenceValue);
		}
		#endif

		#if DEBUG
		if (TotalPolityInfluenceValue <= 0) {

			if (GetFactionCores ().Count > 0) {

				Debug.LogWarning ("Group with no polity influence has faction cores - Id: " + Id);
			}
		}
		#endif

		FindHighestPolityInfluence ();
	}

	public PolityInfluence SetPolityInfluence (Polity polity, float newInfluenceValue, float polityCoreDistance = -1, float factionCoreDistance = -1) {

		newInfluenceValue = MathUtility.RoundToSixDecimals (newInfluenceValue);

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

//		#if DEBUG
//		if (Cell.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		PolityInfluence polityInfluence;

		_polityInfluencesToRemove.Remove (polity.Id);

		if (!_polityInfluences.TryGetValue (polity.Id, out polityInfluence)) {
			_polityInfluencesToAdd.TryGetValue (polity.Id, out polityInfluence);
		}

		if (polityInfluence == null) {
			if (newInfluenceValue > Polity.MinPolityInfluence) {

				if (polityCoreDistance == -1) {
					polityCoreDistance = CalculateShortestPolityCoreDistance (polity);
				}

				if (factionCoreDistance == -1) {
					factionCoreDistance = CalculateShortestFactionCoreDistance (polity);
				}

				polityInfluence = new PolityInfluence (polity, newInfluenceValue, polityCoreDistance, factionCoreDistance);

				_polityInfluencesToAdd.Add (polity.Id, polityInfluence);
			}

			return polityInfluence;
		}

		if (newInfluenceValue <= Polity.MinPolityInfluence) {

//			#if DEBUG
//			foreach (Faction faction in GetFactionCores ()) {
//
//				if (faction.PolityId == polityInfluence.PolityId) {
//
//					Debug.LogWarning ("Faction belonging to polity to remove has core in cell - group Id: " + Id + " - polity Id: " + polityInfluence.PolityId);
//				}
//			}
//			#endif
			
			_polityInfluencesToRemove.Add (polityInfluence.PolityId);

			return null;
		}

		polityInfluence.NewValue = newInfluenceValue;

		return polityInfluence;
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

//		#if DEBUG
//		foreach (Faction faction in GetFactionCores ()) {
//
//			if (faction.PolityId == polity.Id) {
//
//				Debug.LogWarning ("Faction belonging to polity to remove has core in cell - group Id: " + Id + " - polity Id: " + polity.Id);
//			}
//		}
//		#endif

		_polityInfluencesToRemove.Add (polity.Id);
	}

	public override void Synchronize () {

		Flags = new List<string> (_flags);

		Culture.Synchronize ();

		PolityInfluences = new List<PolityInfluence> (_polityInfluences.Values);

		if (SeaMigrationRoute != null) {
			if (!SeaMigrationRoute.Consolidated) {
				SeaMigrationRoute = null;
			} else {
				SeaMigrationRoute.Synchronize ();
			}
		}

		FactionCoreIds = new List<long> (FactionCores.Keys);

		PreferredMigrationDirectionInt = (int)PreferredMigrationDirection;
		
		base.Synchronize ();
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		PreferredMigrationDirection = (Direction)PreferredMigrationDirectionInt;

		foreach (long id in FactionCoreIds) {
		
			Faction faction = World.GetFaction (id);

			if (faction == null) {
				throw new System.Exception ("Missing faction with id: " + id);
			}

			FactionCores.Add (id, faction);
		}

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

		if (Cell == null) {
			Debug.LogError ("Cell is null");
		}

		if (SeaMigrationRoute != null) {

			SeaMigrationRoute.World = World;

			if (SeaMigrationRoute.Consolidated) {
				SeaMigrationRoute.FinalizeLoad ();
			} else {
				SeaMigrationRoute.FirstCell = Cell;
			}
		}

		foreach (PolityInfluence p in PolityInfluences) {

			p.Polity = World.GetPolity (p.PolityId);
			p.NewValue = p.Value;

			if (p.Polity == null) { 
				throw new System.Exception ("Missing polity with id:" + p.PolityId);
			}

			_polityInfluences.Add (p.PolityId, p);

			if ((HighestPolityInfluence == null)  || (HighestPolityInfluence.Value < p.Value)) {
				HighestPolityInfluence = p;
			}
		}

		// Generate Update Event

		UpdateEvent = new UpdateCellGroupEvent (this, NextUpdateDate);
		World.InsertEventToHappen (UpdateEvent);

		// Generate Migration Event

		if (HasMigrationEvent) {
		
			TerrainCell targetCell = World.GetCell (MigrationTargetLongitude, MigrationTargetLatitude);

			MigrationEvent = new MigrateGroupEvent (this, targetCell, (Direction)MigrationEventDirectionInt, MigrationEventDate);
			World.InsertEventToHappen (MigrationEvent);
		}

		// Generate Polity Expansion Event

		if (HasPolityExpansionEvent) {

			Polity expandingPolity = World.GetPolity (ExpandingPolityId);
			CellGroup targetGroup = World.GetGroup (ExpansionTargetGroupId);
		
			PolityExpansionEvent = new ExpandPolityInfluenceEvent (this, expandingPolity, targetGroup, PolityExpansionEventDate);
			World.InsertEventToHappen (PolityExpansionEvent);
		}

		// Generate Tribe Formation Event

		if (HasTribeFormationEvent) {

			TribeCreationEvent = new TribeFormationEvent (this, TribeFormationEventDate);
			World.InsertEventToHappen (TribeCreationEvent);
		}
	}
}
