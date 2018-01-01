using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.Profiling;

public delegate void ProgressCastDelegate (float value, string message = null, bool reset = false);

public interface ISynchronizable {

	void Synchronize ();
	void FinalizeLoad ();
}

public static class RngOffsets {

	public const int CELL_GROUP_CONSIDER_LAND_MIGRATION_TARGET = 0;
	public const int CELL_GROUP_CONSIDER_LAND_MIGRATION_CHANCE = 1;

	public const int CELL_GROUP_CONSIDER_SEA_MIGRATION = 2;

	public const int CELL_GROUP_CALCULATE_NEXT_UPDATE = 3;

	public const int CELL_GROUP_SET_POLITY_UPDATE = 4;

	public const int CELL_GROUP_CONSIDER_POLITY_INFLUENCE_EXPANSION_POLITY = 5;
	public const int CELL_GROUP_CONSIDER_POLITY_INFLUENCE_EXPANSION_TARGET = 6;
	public const int CELL_GROUP_CONSIDER_POLITY_INFLUENCE_EXPANSION_CHANCE = 7;

	public const int CELL_GROUP_UPDATE_MIGRATION_DIRECTION = 8;
	public const int CELL_GROUP_GENERATE_GROUP_MIGRATION_DIRECTION = 9;
	public const int CELL_GROUP_GENERATE_INFLUENCE_TRANSFER_DIRECTION = 10;
	public const int CELL_GROUP_GENERATE_CORE_MIGRATION_DIRECTION = 11;

	public const int ACTIVITY_UPDATE = 10000;
	public const int ACTIVITY_POLITY_INFLUENCE = 10001;

	public const int KNOWLEDGE_MERGE = 20000;
	public const int KNOWLEDGE_MODIFY_VALUE = 20001;
	public const int KNOWLEDGE_UPDATE_VALUE_INTERNAL = 20002;
	public const int KNOWLEDGE_UPDATE_VALUE_INTERNAL_2 = 20003;
	public const int KNOWLEDGE_POLITY_INFLUENCE = 20004;
	public const int KNOWLEDGE_POLITY_INFLUENCE_2 = 20005;

	public const int SKILL_UPDATE = 30000;
	public const int SKILL_POLITY_INFLUENCE = 30001;

	public const int POLITY_CULTURE_NORMALIZE_ATTRIBUTE_VALUES = 40000;
	public const int POLITY_CULTURE_GENERATE_NEW_LANGUAGE = 40001;

	public const int POLITY_UPDATE_EFFECTS = 50000;

	public const int REGION_GENERATE_NAME = 60000;
	public const int REGION_SELECT_BORDER_REGION = 61000;

	public const int TRIBE_GENERATE_NEW_TRIBE = 70000;
	public const int TRIBE_GENERATE_NAME = 71000;

	public const int CLAN_GENERATE_NAME = 80000;
	public const int CLAN_CHOOSE_CORE_GROUP = 81000;
	public const int CLAN_CHOOSE_TARGET_GROUP = 82000;
	public const int CLAN_LEADER_GEN_OFFSET = 83000;

	public const int AGENT_GENERATE_BIO = 90000;
	public const int AGENT_GENERATE_NAME = 91000;

	public const int ROUTE_CHOOSE_NEXT_DEPTH_SEA_CELL = 100000;
	public const int ROUTE_CHOOSE_NEXT_COASTAL_CELL = 110000;
	public const int ROUTE_CHOOSE_NEXT_COASTAL_CELL_2 = 120000;

	public const int FARM_DEGRADATION_EVENT_CALCULATE_TRIGGER_DATE = 900000;
	public const int SAILING_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE = 900001;
	public const int TRIBALISM_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE = 900002;
	public const int TRIBE_FORMATION_EVENT_CALCULATE_TRIGGER_DATE = 900003;
	public const int BOAT_MAKING_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE = 900004;
	public const int PLANT_CULTIVATION_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE = 900005;
	public const int CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE = 900006;
	public const int TRIBE_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE = 900007;
	public const int CLAN_CORE_MIGRATION_EVENT_CALCULATE_TRIGGER_DATE = 900008;

	public const int EVENT_TRIGGER = 1000000;
	public const int EVENT_CAN_TRIGGER = 1100000;

	public const int MIGRATING_GROUP_MOVE_FACTION_CORE = 2000000;
	public const int EXPAND_POLITY_MOVE_FACTION_CORE = 2100000;
}

[XmlRoot]
public class World : ISynchronizable {

	public const int MaxPossibleYearsToSkip = int.MaxValue / 100;
	
	public const float Circumference = 40075; // In kilometers;
	
	public const int NumContinents =12;
	public const float ContinentMinWidthFactor = 5.7f;
	public const float ContinentMaxWidthFactor = 8.7f;

	public const float AvgPossibleRainfall = 990f;
	public const float AvgPossibleTemperature = 13.7f;
	
	public const float MinPossibleAltitude = -15000;
	public const float MaxPossibleAltitude = 15000;
	
	public const float MinPossibleRainfall = 0;
	public const float MaxPossibleRainfall = 13000;
	
	public const float MinPossibleTemperature = -40 - AvgPossibleTemperature;
	public const float MaxPossibleTemperature = 50 - AvgPossibleTemperature;

	public const float OptimalRainfallForArability = 1000;
	public const float OptimalTemperatureForArability = 30;
	public const float MaxRainfallForArability = 7000;
	public const float MinRainfallForArability = 0;
	public const float MinTemperatureForArability = -15;
	
	public const float StartPopulationDensity = 0.5f;
	
	public const int MinStartingPopulation = 100;
	public const int MaxStartingPopulation = 100000;
	
	[XmlAttribute]
	public int Width { get; private set; }
	[XmlAttribute]
	public int Height { get; private set; }

	[XmlAttribute]
	public int Seed { get; private set; }
	
	[XmlAttribute]
	public int CurrentDate { get; private set; }

	[XmlAttribute]
	public int MaxYearsToSkip { get; private set; }
	
	[XmlAttribute]
	public int CellGroupCount { get; private set; }

	[XmlAttribute]
	public int MemorableAgentCount { get; private set; }

	[XmlAttribute]
	public int FactionCount { get; private set; }

	[XmlAttribute]
	public int PolityCount { get; private set; }

	[XmlAttribute]
	public int RegionCount { get; private set; }

	[XmlAttribute]
	public int LanguageCount { get; private set; }

	[XmlAttribute]
	public int TerrainCellChangesListCount { get; private set; }

	[XmlAttribute]
	public float SeaLevelOffset { get; private set; }

	[XmlAttribute]
	public float RainfallOffset { get; private set; }

	[XmlAttribute]
	public float TemperatureOffset { get; private set; }

	// Start wonky segment (save failures might happen here)

	[XmlArrayItem (Type = typeof(UpdateCellGroupEvent)),
		XmlArrayItem (Type = typeof(MigrateGroupEvent)),
		XmlArrayItem (Type = typeof(ExpandPolityInfluenceEvent)),
		XmlArrayItem (Type = typeof(TribeFormationEvent)),
		XmlArrayItem (Type = typeof(SailingDiscoveryEvent)),
		XmlArrayItem (Type = typeof(BoatMakingDiscoveryEvent)),
		XmlArrayItem (Type = typeof(TribalismDiscoveryEvent)),
		XmlArrayItem (Type = typeof(PlantCultivationDiscoveryEvent)),
		XmlArrayItem (Type = typeof(ClanSplitEvent)),
		XmlArrayItem (Type = typeof(TribeSplitEvent)),
		XmlArrayItem (Type = typeof(ClanCoreMigrationEvent))]
	public List<WorldEvent> EventsToHappen;

	public List<TerrainCellChanges> TerrainCellChangesList = new List<TerrainCellChanges> ();

	public List<CulturalActivityInfo> CulturalActivityInfoList = new List<CulturalActivityInfo> ();
	public List<CulturalSkillInfo> CulturalSkillInfoList = new List<CulturalSkillInfo> ();
	public List<CulturalKnowledgeInfo> CulturalKnowledgeInfoList = new List<CulturalKnowledgeInfo> ();
	public List<CulturalDiscovery> CulturalDiscoveryInfoList = new List<CulturalDiscovery> ();

	public List<CellGroup> CellGroups;

	[XmlArrayItem (Type = typeof(Agent))]
	public List<Agent> MemorableAgents;

	[XmlArrayItem (Type = typeof(Clan))]
	public List<Faction> Factions;

	[XmlArrayItem (Type = typeof(Tribe))]
	public List<Polity> Polities;

	[XmlArrayItem (Type = typeof(CellRegion))]
	public List<Region> Regions;

	public List<Language> Languages;

	public List<long> EventMessageIds;

	// End wonky segment 

	[XmlIgnore]
	public int EventsToHappenCount { get; private set; }

	[XmlIgnore]
	public TerrainCell SelectedCell = null;
	[XmlIgnore]
	public Region SelectedRegion = null;
	[XmlIgnore]
	public Territory SelectedTerritory = null;
	[XmlIgnore]
	public Polity FocusedPolity = null;
	
	[XmlIgnore]
	public float MinPossibleAltitudeWithOffset = MinPossibleAltitude - Manager.SeaLevelOffset;
	[XmlIgnore]
	public float MaxPossibleAltitudeWithOffset = MaxPossibleAltitude - Manager.SeaLevelOffset;
	
	[XmlIgnore]
	public float MinPossibleRainfallWithOffset = MinPossibleRainfall;
	[XmlIgnore]
	public float MaxPossibleRainfallWithOffset = MaxPossibleRainfall * Manager.RainfallOffset / AvgPossibleRainfall;
	
	[XmlIgnore]
	public float MinPossibleTemperatureWithOffset = MinPossibleTemperature + Manager.TemperatureOffset;
	[XmlIgnore]
	public float MaxPossibleTemperatureWithOffset = MaxPossibleTemperature + Manager.TemperatureOffset;
	
	[XmlIgnore]
	public float MaxAltitude = float.MinValue;
	[XmlIgnore]
	public float MinAltitude = float.MaxValue;
	
	[XmlIgnore]
	public float MaxRainfall = float.MinValue;
	[XmlIgnore]
	public float MinRainfall = float.MaxValue;
	
	[XmlIgnore]
	public float MaxTemperature = float.MinValue;
	[XmlIgnore]
	public float MinTemperature = float.MaxValue;
	
	[XmlIgnore]
	public TerrainCell[][] TerrainCells;
	
	[XmlIgnore]
	public CellGroup MostPopulousGroup = null;
	
	[XmlIgnore]
	public ProgressCastDelegate ProgressCastMethod { get; set; }
	
	[XmlIgnore]
	public HumanGroup MigrationTaggedGroup = null;

	private BinaryTree<int, WorldEvent> _eventsToHappen = new BinaryTree<int, WorldEvent> ();
	
//	private List<IGroupAction> _groupActionsToPerform = new List<IGroupAction> ();

	private HashSet<int> _terrainCellChangesListIndexes = new HashSet<int> ();

	private HashSet<string> _culturalActivityIdList = new HashSet<string> ();
	private HashSet<string> _culturalSkillIdList = new HashSet<string> ();
	private HashSet<string> _culturalKnowledgeIdList = new HashSet<string> ();
	private HashSet<string> _culturalDiscoveryIdList = new HashSet<string> ();

	private Dictionary<long, CellGroup> _cellGroups = new Dictionary<long, CellGroup> ();
	
	private HashSet<CellGroup> _updatedGroups = new HashSet<CellGroup> ();
	private HashSet<CellGroup> _groupsToPostUpdate_afterPolityUpdates = new HashSet<CellGroup> ();

	private HashSet<CellGroup> _groupsToUpdate = new HashSet<CellGroup>();
	private HashSet<CellGroup> _groupsToRemove = new HashSet<CellGroup>();

	private List<MigratingGroup> _migratingGroups = new List<MigratingGroup> ();

	private Dictionary<long, Agent> _memorableAgents = new Dictionary<long, Agent> ();

	private Dictionary<long, Faction> _factions = new Dictionary<long, Faction> ();

	private HashSet<Faction> _factionsToSplit = new HashSet<Faction>();
	private HashSet<Faction> _factionsToUpdate = new HashSet<Faction>();
	private HashSet<Faction> _factionsToRemove = new HashSet<Faction>();

	private Dictionary<long, Polity> _polities = new Dictionary<long, Polity> ();

	private HashSet<Polity> _politiesToUpdate = new HashSet<Polity>();
	private HashSet<Polity> _politiesToRemove = new HashSet<Polity>();

	private Dictionary<long, Region> _regions = new Dictionary<long, Region> ();

	private Dictionary<long, Language> _languages = new Dictionary<long, Language> ();

	private HashSet<long> _eventMessageIds = new HashSet<long> ();
	private Queue<WorldEventMessage> _eventMessagesToShow = new Queue<WorldEventMessage> ();

	private Queue<Decision> _decisionsToResolve = new Queue<Decision> ();

	private Vector2[] _continentOffsets;
	private float[] _continentWidths;
	private float[] _continentHeights;
	
	private float _progressIncrement = 0.25f;

	private float _accumulatedProgress = 0;

	private float _cellMaxSideLength;

	private int _dateToSkipTo;

	public World () {

		Manager.WorldBeingLoaded = this;

		ProgressCastMethod = (value, message, reset) => {};
	}

	public World (int width, int height, int seed) {

		ProgressCastMethod = (value, message, reset) => {};
		
		Width = width;
		Height = height;
		Seed = seed;
		
		CurrentDate = 0;
		MaxYearsToSkip = MaxPossibleYearsToSkip;
		EventsToHappenCount = 0;
		CellGroupCount = 0;
		PolityCount = 0;
		RegionCount = 0;
		TerrainCellChangesListCount = 0;

		SeaLevelOffset = Manager.SeaLevelOffset;
		RainfallOffset = Manager.RainfallOffset;
		TemperatureOffset = Manager.TemperatureOffset;
	}
	
	public void StartInitialization (float acumulatedProgress, float progressIncrement) {

		Manager.SeaLevelOffset = SeaLevelOffset;
		Manager.RainfallOffset = RainfallOffset;
		Manager.TemperatureOffset = TemperatureOffset;

		_accumulatedProgress = acumulatedProgress;
		_progressIncrement = progressIncrement;
		
		_cellMaxSideLength = Circumference / Width;
		TerrainCell.MaxArea = _cellMaxSideLength * _cellMaxSideLength;
		TerrainCell.MaxWidth = _cellMaxSideLength;
		CellGroup.TravelWidthFactor = _cellMaxSideLength;
		
		TerrainCells = new TerrainCell[Width][];
		
		for (int i = 0; i < Width; i++)
		{
			TerrainCell[] column = new TerrainCell[Height];
			
			for (int j = 0; j < Height; j++)
			{
				float alpha = (j / (float)Height) * Mathf.PI;

				float cellHeight = _cellMaxSideLength;
				float cellWidth = Mathf.Sin(alpha) * _cellMaxSideLength;

				TerrainCell cell = new TerrainCell (this, i, j, cellHeight, cellWidth);
				
				column[j] = cell;
			}
			
			TerrainCells[i] = column;
		}
		
		for (int i = 0; i < Width; i++) {
			
			for (int j = 0; j < Height; j++) {

				TerrainCell cell = TerrainCells [i] [j];
				
				cell.InitializeNeighbors();
			}
		}
		
		_continentOffsets = new Vector2[NumContinents];
		_continentHeights = new float[NumContinents];
		_continentWidths = new float[NumContinents];
		
		Manager.EnqueueTaskAndWait (() => {
			
			Random.InitState(Seed);
			return true;
		});
	}

	public void FinishInitialization () {

		for (int i = 0; i < Width; i++) {

			for (int j = 0; j < Height; j++) {

				TerrainCell cell = TerrainCells [i] [j];

				cell.InitializeMiscellaneous();
			}
		}

		foreach (TerrainCellChanges changes in TerrainCellChangesList) {

			SetTerrainCellChanges (changes);
		}
	}

//	public List<WorldEvent> GetValidEventsToHappen () {
//	
//		return _eventsToHappen.GetValues (ValidateEventsToHappenNode);
//	}

	public List<WorldEvent> GetFilteredEventsToHappenForSerialization () {

		return _eventsToHappen.GetValues (FilterEventsToHappenNodeForSerialization);
	}

	public void Synchronize () {

		EventsToHappen = _eventsToHappen.GetValues (FilterEventsToHappenNodeForSerialization, FilterEventsToHappenNodeEffect, true);

		#if DEBUG
		Dictionary<System.Type, int> eventTypes = new Dictionary<System.Type, int> ();
		#endif

		foreach (WorldEvent e in EventsToHappen) {

			#if DEBUG
			System.Type type = e.GetType ();

			if (!eventTypes.ContainsKey (type)) {
			
				eventTypes.Add (type, 1);
			} else {
			
				eventTypes [type]++;
			}
			#endif

			e.Synchronize ();
		}

		#if DEBUG
		string debugMsg = "Total Groups: " + _cellGroups.Count + "\nSerialized event types:";

		foreach (KeyValuePair<System.Type, int> pair in eventTypes) {

			debugMsg += "\n\t" + pair.Key + " : " + pair.Value;
		}

		Debug.Log (debugMsg);
		#endif

		CellGroups = new List<CellGroup> (_cellGroups.Values);

		foreach (CellGroup g in CellGroups) {

			g.Synchronize ();
		}

		MemorableAgents = new List<Agent> (_memorableAgents.Values);

		foreach (Agent a in MemorableAgents) {

			a.Synchronize ();
		}

		Factions = new List<Faction> (_factions.Values);

		foreach (Faction f in Factions) {

			f.Synchronize ();
		}

		Polities = new List<Polity> (_polities.Values);

		foreach (Polity p in Polities) {
		
			p.Synchronize ();
		}

		Regions = new List<Region> (_regions.Values);

		foreach (Region r in Regions) {

			r.Synchronize ();
		}

		Languages = new List<Language> (_languages.Values);

		foreach (Language l in Languages) {

			l.Synchronize ();
		}

		TerrainCellChangesList.Clear ();
		TerrainCellChangesListCount = 0;

		for (int i = 0; i < Width; i++) {

			for (int j = 0; j < Height; j++) {

				TerrainCell cell = TerrainCells [i] [j];

				GetTerrainCellChanges (cell);
			}
		}

		EventMessageIds = new List<long> (_eventMessageIds);
	}

	public void GetTerrainCellChanges (TerrainCell cell) {

		TerrainCellChanges changes = cell.GetChanges ();

		if (changes == null)
			return;
	
		int index = changes.Longitude + (changes.Latitude * Width);

		if (!_terrainCellChangesListIndexes.Add (index))
			return;

		TerrainCellChangesList.Add (changes);

		TerrainCellChangesListCount++;
	}

	public void SetTerrainCellChanges (TerrainCellChanges changes) {

		TerrainCell cell = TerrainCells [changes.Longitude] [changes.Latitude];

		cell.SetChanges (changes);
	}

//	public void AddGroupActionToPerform (KnowledgeTransferAction action) {
//	
//		_groupActionsToPerform.Add (action);
//	}

	public void AddExistingCulturalActivityInfo (CulturalActivityInfo baseInfo) {

		if (_culturalActivityIdList.Contains (baseInfo.Id))
			return;

		CulturalActivityInfoList.Add (new CulturalActivityInfo (baseInfo));
		_culturalActivityIdList.Add (baseInfo.Id);
	}

	public void AddExistingCulturalSkillInfo (CulturalSkillInfo baseInfo) {

		if (_culturalSkillIdList.Contains (baseInfo.Id))
			return;
	
		CulturalSkillInfoList.Add (new CulturalSkillInfo (baseInfo));
		_culturalSkillIdList.Add (baseInfo.Id);
	}
	
	public void AddExistingCulturalKnowledgeInfo (CulturalKnowledgeInfo baseInfo) {
		
		if (_culturalKnowledgeIdList.Contains (baseInfo.Id))
			return;
		
		CulturalKnowledgeInfoList.Add (new CulturalKnowledgeInfo (baseInfo));
		_culturalKnowledgeIdList.Add (baseInfo.Id);
	}
	
	public void AddExistingCulturalDiscoveryInfo (CulturalDiscovery baseInfo) {
		
		if (_culturalDiscoveryIdList.Contains (baseInfo.Id))
			return;
		
		CulturalDiscoveryInfoList.Add (new CulturalDiscovery (baseInfo));
		_culturalDiscoveryIdList.Add (baseInfo.Id);
	}

	public void UpdateMostPopulousGroup (CellGroup contenderGroup) {
	
		if (MostPopulousGroup == null) {

			MostPopulousGroup = contenderGroup;

		} else if (MostPopulousGroup.Population < contenderGroup.Population) {
			
			MostPopulousGroup = contenderGroup;
		}
	}
	
	public void AddUpdatedGroup (CellGroup group) {
		
		_updatedGroups.Add (group);
	}

	public void AddGroupToPostUpdate_AfterPolityUpdate (CellGroup group) {

		_groupsToPostUpdate_afterPolityUpdates.Add (group);
	}

	public TerrainCell GetCell (WorldPosition position) {
	
		return GetCell (position.Longitude, position.Latitude);
	}
	
	public TerrainCell GetCell (int longitude, int latitude) {

		if ((longitude < 0) || (longitude >= Width))
			return null;
		
		if ((latitude < 0) || (latitude >= Height))
			return null;

		return TerrainCells[longitude][latitude];
	}

	public void SetMaxYearsToSkip (int value) {
	
		MaxYearsToSkip = Mathf.Max (value, 1);
	}

	private bool ValidateEventsToHappenNode (BinaryTreeNode<int, WorldEvent> node) {

		if (!node.Valid) {

			node.MarkedForRemoval = true;
			return false;
		}

		if (!node.Value.IsStillValid ()) {

			node.MarkedForRemoval = true;
			return false;
		}

		if (node.Key != node.Value.TriggerDate) {
		
			node.MarkedForRemoval = true;
			return false;
		}

		return true;
	}

	private bool FilterEventsToHappenNodeForSerialization (BinaryTreeNode<int, WorldEvent> node) {

		if (ValidateEventsToHappenNode(node)) {
			
			return !node.Value.DoNotSerialize;
		}

		return false;
	}

	private void InvalidEventsToHappenNodeEffect (BinaryTreeNode<int, WorldEvent> node) {

		EventsToHappenCount--;

		//		#if DEBUG
		//		if (Manager.RegisterDebugEvent != null) {
		//			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Event Removal", "Removal");
		//
		//			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
		//		}
		//		#endif
	}

	private void FilterEventsToHappenNodeEffect (BinaryTreeNode<int, WorldEvent> node) {

		if (node.MarkedForRemoval) {
			InvalidEventsToHappenNodeEffect (node);
		}
	}

	private void UpdateGroups () {

		foreach (CellGroup group in _groupsToUpdate) {

			Profiler.BeginSample ("Group Update");

			group.Update ();

			Profiler.EndSample ();
		}

		_groupsToUpdate.Clear ();
	}

	private void MigrateGroups () {

		MigratingGroup[] currentGroupsToMigrate = _migratingGroups.ToArray();

		_migratingGroups.Clear ();

		foreach (MigratingGroup group in currentGroupsToMigrate) {

			group.SplitFromSourceGroup();
		}

		foreach (MigratingGroup group in currentGroupsToMigrate) {

			group.MoveToCell();
		}
	}

	//	private void PerformGroupActions () {
	//
	//		//
	//		// Perform Group Actions
	//		//
	//
	//		foreach (IGroupAction action in _groupActionsToPerform) {
	//		
	//			action.Perform ();
	//		}
	//
	//		_groupActionsToPerform.Clear ();
	//	}

	private void PostUpdateGroups_BeforePolityUpdates () {

		foreach (CellGroup group in _updatedGroups) {

			Profiler.BeginSample ("Cell Group Postupdate Before Polity Updates");

			group.PostUpdate_BeforePolityUpdates ();

			Profiler.EndSample ();
		}
	}

	private void RemoveGroups () {

		foreach (CellGroup group in _groupsToRemove) {

			Profiler.BeginSample ("Destroy Group");

			group.Destroy ();

			Profiler.EndSample ();
		}

		_groupsToRemove.Clear ();
	}

	private void SetNextGroupUpdates () {

		foreach (CellGroup group in _updatedGroups) {

			Profiler.BeginSample ("Cell Group Setup for Next Update");

			group.SetupForNextUpdate ();
			Manager.AddUpdatedCell (group.Cell, CellUpdateType.Group);

			Profiler.EndSample ();
		}

		_updatedGroups.Clear ();
	}

	private void PostUpdateGroups_AfterPolityUpdates () {

		//TODO: This function should not exist. Think of a way to remove it

		foreach (CellGroup group in _groupsToPostUpdate_afterPolityUpdates) {

			Profiler.BeginSample ("Cell Group Postupdate After Polity Updates");

			group.PostUpdate_AfterPolityUpdates ();

			Profiler.EndSample ();
		}

		_groupsToPostUpdate_afterPolityUpdates.Clear ();
	}

	private void SplitFactions () {

		foreach (Faction faction in _factionsToSplit) {

			Profiler.BeginSample ("Split Faction");

			faction.Split ();

			Profiler.EndSample ();
		}

		_factionsToSplit.Clear ();
	}

	private void UpdateFactions () {

		foreach (Faction faction in _factionsToUpdate) {

			Profiler.BeginSample ("Update Faction");

			faction.Update ();

			Profiler.EndSample ();
		}

		_factionsToUpdate.Clear ();
	}

	private void RemoveFactions () {

		foreach (Faction faction in _factionsToRemove) {

			Profiler.BeginSample ("Destroy Faction");

			faction.Destroy ();

			Profiler.EndSample ();
		}

		_factionsToRemove.Clear ();
	}

	private void UpdatePolities () {

		foreach (Polity polity in _politiesToUpdate) {

			Profiler.BeginSample ("Update Polity");

			polity.Update ();

			Profiler.EndSample ();
		}

		_politiesToUpdate.Clear ();
	}

	private void RemovePolities () {

		foreach (Polity polity in _politiesToRemove) {

			Profiler.BeginSample ("Destroy Polity");

			polity.Destroy ();

			Profiler.EndSample ();
		}

		_politiesToRemove.Clear ();
	}

	public int Iterate () {
	
		EvaluateEventsToHappen ();

		return Update ();
	}

	public bool EvaluateEventsToHappen () {

		if (CellGroupCount <= 0)
			return false;
		
		//
		// Evaluate Events that will happen at the current date
		//

		_dateToSkipTo = CurrentDate + 1;

		Profiler.BeginSample ("Evaluate Events");

		while (true) {

			if (_eventsToHappen.Count <= 0) break;

			_eventsToHappen.FindLeftmost (ValidateEventsToHappenNode, InvalidEventsToHappenNodeEffect);
		
			WorldEvent eventToHappen = _eventsToHappen.Leftmost;

			if (eventToHappen.TriggerDate < 0) {
				Debug.Break ();
				throw new System.Exception ("eventToHappen.TriggerDate less than zero: " + eventToHappen);
			}

			if (eventToHappen.TriggerDate > CurrentDate) {

				int maxDate = CurrentDate + MaxYearsToSkip;

				if (maxDate < 0) {
					Debug.Break ();
					throw new System.Exception ("Surpassed date limit (Int32.MaxValue)");
				}

				_dateToSkipTo = Mathf.Min (eventToHappen.TriggerDate, maxDate);
				break;
			}

			_eventsToHappen.RemoveLeftmost ();
			EventsToHappenCount--;

//			#if DEBUG
//			if (Manager.RegisterDebugEvent != null) {
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Event Being Triggered", "Triggering");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//			#endif

			Profiler.BeginSample ("Event Trigger");

			if (eventToHappen.CanTrigger ()) {
				
				eventToHappen.Trigger ();

			} else {
				
				eventToHappen.FailedToTrigger = true;
			}

			Profiler.EndSample ();

			eventToHappen.Destroy ();
		}

		Profiler.EndSample ();

		return true;
	}

	public int Update () {

		if (CellGroupCount <= 0)
			return 0;

		UpdateGroups ();

		MigrateGroups ();

		//		PerformGroupActions ();

		PostUpdateGroups_BeforePolityUpdates ();

		RemoveGroups ();

		SetNextGroupUpdates ();

		SplitFactions ();

		UpdateFactions ();

		RemoveFactions ();

		UpdatePolities ();

		RemovePolities ();

		PostUpdateGroups_AfterPolityUpdates ();

		//
		// Skip to Next Event's Date
		//

		if (_eventsToHappen.Count > 0) {

			WorldEvent futureEventToHappen = _eventsToHappen.Leftmost;

			if (futureEventToHappen.TriggerDate < _dateToSkipTo) {

				_dateToSkipTo = futureEventToHappen.TriggerDate;
			}
		}

		int dateSpan = _dateToSkipTo - CurrentDate;

		CurrentDate = _dateToSkipTo;

		return dateSpan;
	}

	public List<WorldEvent> GetEventsToHappen () {
	
		return _eventsToHappen.Values;
	}

	public void InsertEventToHappen (WorldEvent eventToHappen) {

//		Profiler.BeginSample ("Insert Event To Happen");

		EventsToHappenCount++;

		_eventsToHappen.Insert (eventToHappen.TriggerDate, eventToHappen, eventToHappen.AssociateNode);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Event Added - Id: " + eventToHappen.Id, "TriggerDate: " + eventToHappen.TriggerDate);
//
//			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//		}
//		#endif

//		Profiler.EndSample ();
	}

	#if DEBUG

	public delegate void AddMigratingGroupCalledDelegate ();

	public static AddMigratingGroupCalledDelegate AddMigratingGroupCalled = null; 

	#endif
	
	public void AddMigratingGroup (MigratingGroup group) {

		#if DEBUG
		if (AddMigratingGroupCalled != null) {
			AddMigratingGroupCalled ();
		}
		#endif
		
		_migratingGroups.Add (group);

		if (!group.SourceGroup.StillPresent) {
			Debug.LogWarning ("Sourcegroup is no longer present. Group Id: " + group.SourceGroup.Id);
		}

		// Source Group needs to be updated
		_groupsToUpdate.Add (group.SourceGroup);

		// If Target Group is present, it also needs to be updated
		if ((group.TargetCell.Group != null) && (group.TargetCell.Group.StillPresent)) {

			_groupsToUpdate.Add (group.TargetCell.Group);
		}
	}
	
	public void AddGroup (CellGroup group) {

		_cellGroups.Add (group.Id, group);

		Manager.AddUpdatedCell (group.Cell, CellUpdateType.Group);

		CellGroupCount++;
	}
	
	public void RemoveGroup (CellGroup group) {

		_cellGroups.Remove (group.Id);
		
		CellGroupCount--;
	}
	
	public CellGroup GetGroup (long id) {

		CellGroup group;

		_cellGroups.TryGetValue (id, out group);

		return group;
	}

	#if DEBUG

	public delegate void AddGroupToUpdateCalledDelegate (string callingMethod);

	public static AddGroupToUpdateCalledDelegate AddGroupToUpdateCalled = null; 

	#endif

	public void AddGroupToUpdate (CellGroup group) {

		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (group.Id == Manager.TracingData.GroupId) {
//				
//				System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
//
//				System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
//				string callingMethod = method.Name;
//				string callingClass = method.DeclaringType.ToString();
//
//				string groupId = "Id:" + group.Id + "|Long:" + group.Longitude + "|Lat:" + group.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"AddGroupToUpdate - Group:" + groupId,
//					"CurrentDate: " + CurrentDate + 
//					", Call: " + callingClass + ":" + callingMethod +
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}

		if (AddGroupToUpdateCalled != null) {

//				System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
//
//				System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
//				string callingMethod = method.Name;
//				string callingClass = method.DeclaringType.ToString();
//
//				AddGroupToUpdateCalled (callingClass + ":" + callingMethod);
			AddGroupToUpdateCalled (null);
		}
		#endif

		if (!group.StillPresent) {
			Debug.LogWarning ("Group to update is no longer present. Id: " + group.Id);
		}

		_groupsToUpdate.Add (group);
	}
	
	public void AddGroupToRemove (CellGroup group) {
		
		_groupsToRemove.Add (group);
	}

	public void AddLanguage (Language language) {

		_languages.Add (language.Id, language);

		LanguageCount++;
	}

	public void RemoveLanguage (Region language) {

		_languages.Remove (language.Id);

		LanguageCount--;
	}

	public Language GetLanguage (long id) {

		Language language;

		_languages.TryGetValue (id, out language);

		return language;
	}

	public void AddRegion (Region region) {

		_regions.Add (region.Id, region);

		RegionCount++;
	}

	public void RemoveRegion (Region region) {

		_regions.Remove (region.Id);

		RegionCount--;
	}

	public Region GetRegion (long id) {

		Region region;

		_regions.TryGetValue (id, out region);

		return region;
	}

	public void AddMemorableAgent (Agent agent) {

		if (!_memorableAgents.ContainsKey (agent.Id)) {
			_memorableAgents.Add (agent.Id, agent);

			MemorableAgentCount++;
		}
	}

	public Agent GetMemorableAgent (long id) {

		Agent agent;

		_memorableAgents.TryGetValue (id, out agent);

		return agent;
	}

	public void AddFaction (Faction faction) {

		_factions.Add (faction.Id, faction);

		FactionCount++;
	}

	public void RemoveFaction (Faction faction) {

		_factions.Remove (faction.Id);

		FactionCount--;
	}

	public Faction GetFaction (long id) {

		Faction faction;

		_factions.TryGetValue (id, out faction);

		return faction;
	}

	public bool ContainsFaction (long id) {

		return _factions.ContainsKey (id);
	}

	public void AddFactionToSplit (Faction faction) {

		_factionsToSplit.Add (faction);
	}

	public void AddFactionToUpdate (Faction faction) {

		_factionsToUpdate.Add (faction);
	}

	public void AddFactionToRemove (Faction faction) {

		_factionsToRemove.Add (faction);
	}

	public void AddPolity (Polity polity) {

		_polities.Add (polity.Id, polity);

		PolityCount++;
	}

	public void RemovePolity (Polity polity) {

		_polities.Remove (polity.Id);

		PolityCount--;
	}

	public Polity GetPolity (long id) {

		Polity polity;

		_polities.TryGetValue (id, out polity);

		return polity;
	}

	public void AddPolityToUpdate (Polity polity) {

		_politiesToUpdate.Add (polity);
		polity.WillBeUpdated = true;
	}

	public void AddPolityToRemove (Polity polity) {

		_politiesToRemove.Add (polity);
	}

	public void AddDecisionToResolve (Decision decision) {
	
		_decisionsToResolve.Enqueue (decision);
	}

	public bool HasDecisionsToResolve () {
	
		return _decisionsToResolve.Count > 0;
	}

	public Decision PullDecisionToResolve () {

		return _decisionsToResolve.Dequeue ();
	}

	public void AddEventMessage (WorldEventMessage eventMessage) {

		_eventMessagesToShow.Enqueue (eventMessage);

		_eventMessageIds.Add (eventMessage.Id);
	}

	public void AddEventMessageToShow (WorldEventMessage eventMessage) {

		if (_eventMessagesToShow.Contains (eventMessage))
			return;

		_eventMessagesToShow.Enqueue (eventMessage);
	}

	public bool HasEventMessage (long id) {

		return _eventMessageIds.Contains (id);
	}

	public WorldEventMessage GetNextMessageToShow () {
	
		return _eventMessagesToShow.Dequeue ();
	}

	public int EventMessagesLeftToShow () {

		return _eventMessagesToShow.Count;
	}

	public void FinalizeLoad (float startProgressValue, float endProgressValue, ProgressCastDelegate castProgress) {

		if (castProgress == null)
			castProgress = (value, message, reset) => {};

		float progressFactor = 1 / (endProgressValue - startProgressValue);

		// Segment 1

		foreach (long messageId in EventMessageIds) {

			_eventMessageIds.Add (messageId);
		}

		TerrainCellChangesList.ForEach (c => {

			int index = c.Longitude + c.Latitude * Width;

			_terrainCellChangesListIndexes.Add (index);
		});

		Languages.ForEach (l => {

			_languages.Add (l.Id, l);
		});

		Regions.ForEach (r => {

			r.World = this;

			_regions.Add (r.Id, r);
		});

		Polities.ForEach (p => {

			p.World = this;

			_polities.Add (p.Id, p);
		});

		Factions.ForEach (f => {

			f.World = this;

			_factions.Add (f.Id, f);
		});

		CellGroups.ForEach (g => {

			g.World = this;

			_cellGroups.Add (g.Id, g);
		});

		// Segment 2

		int elementCount = 0;
		float totalElementsFactor = progressFactor * (Languages.Count + Regions.Count + Factions.Count + Polities.Count + CellGroups.Count + EventsToHappen.Count);

		foreach (Language l in Languages) {

			l.FinalizeLoad ();

			castProgress (startProgressValue + (++elementCount/totalElementsFactor), "Initializing Languages...");
		}

		// Segment 3

		foreach (Region r in Regions) {

			r.FinalizeLoad ();

			castProgress (startProgressValue + (++elementCount/totalElementsFactor), "Initializing Regions...");
		}

		// Segment 5

		foreach (Polity p in Polities) {

			p.FinalizeLoad ();

			castProgress (startProgressValue + (++elementCount/totalElementsFactor), "Initializing Polities...");
		}

		// Segment 4

		foreach (Faction f in Factions) {

			f.FinalizeLoad ();

			castProgress (startProgressValue + (++elementCount/totalElementsFactor), "Initializing Factions...");
		}

		// Segment 6

		foreach (CellGroup g in CellGroups) {

			try {
				g.FinalizeLoad ();

			} catch (System.Exception e) {
				bool debug = true;
			}

			castProgress (startProgressValue + (++elementCount/totalElementsFactor), "Initializing Cell Groups...");
		}

		// Segment 7

		foreach (WorldEvent e in EventsToHappen) {

			e.World = this;
			e.FinalizeLoad ();

			InsertEventToHappen (e);
//			_eventsToHappen.Insert (e.TriggerDate, e);

			castProgress (startProgressValue + (++elementCount/totalElementsFactor), "Initializing Events...");
		}

		// Segment 8

		CulturalActivityInfoList.ForEach (a => _culturalActivityIdList.Add (a.Id));
		CulturalSkillInfoList.ForEach (s => _culturalSkillIdList.Add (s.Id));
		CulturalKnowledgeInfoList.ForEach (k => _culturalKnowledgeIdList.Add (k.Id));
		CulturalDiscoveryInfoList.ForEach (d => _culturalDiscoveryIdList.Add (d.Id));
	}

	public void FinalizeLoad () {

		FinalizeLoad (0, 1, null);
	}

	public void MigrationTagGroup (HumanGroup group) {
	
		MigrationUntagGroup ();
		
		MigrationTaggedGroup = group;

		group.MigrationTagged = true;
	}
	
	public void MigrationUntagGroup () {
		
		if (MigrationTaggedGroup != null)
			MigrationTaggedGroup.MigrationTagged = false;
	}
	
	public void GenerateTerrain () {
		
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

		ProgressCastMethod (_accumulatedProgress, "Generating Arability...");

		GenerateTerrainArability ();
	}

	public void Generate () {

		GenerateTerrain ();
		
		ProgressCastMethod (_accumulatedProgress, "Finalizing...");
	}
	
	public void GenerateHumanGroup (int longitude, int latitude, int initialPopulation) {
			
		TerrainCell cell = GetCell (longitude, latitude);
		
		CellGroup group = new CellGroup(this, cell, initialPopulation);
		
		AddGroup(group);
	}

	public void GenerateRandomHumanGroups (int maxGroups, int initialPopulation) {

		ProgressCastMethod (_accumulatedProgress, "Adding Random Human Groups...");
		
		float minPresence = 0.50f;
		
		int sizeX = Width;
		int sizeY = Height;
		
		List<TerrainCell> SuitableCells = new List<TerrainCell> ();
		
		for (int i = 0; i < sizeX; i++) {
			
			for (int j = 0; j < sizeY; j++) {
				
				TerrainCell cell = TerrainCells [i] [j];
				
				float biomePresence = cell.GetBiomePresence(Biome.Grassland);
				
				if (biomePresence < minPresence) continue;
				
				SuitableCells.Add(cell);
			}

			ProgressCastMethod (_accumulatedProgress + _progressIncrement * (i + 1)/(float)sizeX);
		}
		
		_accumulatedProgress += _progressIncrement;

		maxGroups = Mathf.Min (SuitableCells.Count, maxGroups);
		
		bool first = true;
		
		for (int i = 0; i < maxGroups; i++) {
			
			ManagerTask<int> n = GenerateRandomInteger(0, SuitableCells.Count);

//			Debug.Log ("Selected suitable cell index (from " + SuitableCells.Count + "):" + (int)n);
			
			TerrainCell cell = SuitableCells[n];
			
			CellGroup group = new CellGroup(this, cell, initialPopulation);
			
			AddGroup(group);
			
			if (first) {
				MigrationTagGroup(group);
				
				first = false;
			}
		}
	}

	private void GenerateContinents () {
		
		float longitudeFactor = 15f;
		float latitudeFactor = 6f;

		float minLatitude = Height / latitudeFactor;
		float maxLatitude = Height * (latitudeFactor - 1f) / latitudeFactor;
		
		Manager.EnqueueTaskAndWait (() => {
			
			Vector2 prevPos = new Vector2(
				RandomUtility.Range(0f, Width),
				RandomUtility.Range(minLatitude, maxLatitude));

			//Vector2 prevPrevPos = prevPos;

			for (int i = 0; i < NumContinents; i++) {

				int widthOff = Random.Range(0, 2) * 3;
				
				_continentOffsets[i] = prevPos;
				_continentWidths[i] = RandomUtility.Range(ContinentMinWidthFactor + widthOff, ContinentMaxWidthFactor + widthOff);
				_continentHeights[i] = RandomUtility.Range(ContinentMinWidthFactor + widthOff, ContinentMaxWidthFactor + widthOff);

				float xPos = Mathf.Repeat(prevPos.x + RandomUtility.Range(Width / longitudeFactor, Width * 2 / longitudeFactor), Width);
				float yPos = RandomUtility.Range(minLatitude, maxLatitude);

				if (i % 3 == 2) {
					xPos = Mathf.Repeat(prevPos.x + RandomUtility.Range(Width * 4 / longitudeFactor, Width * 5 / longitudeFactor), Width);
				}

				Vector2 newPos = new Vector2(xPos, yPos);

				//prevPrevPos = prevPos;
				prevPos = newPos;
			}
			
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

//				valueC = MathUtility.MixValues(valueC, value3, 0.3f * value8);
				valueC = MathUtility.MixValues(valueC, value3, 0.45f * value8);
//				valueC = MathUtility.MixValues(valueC, value3, 0.55f * value8);
				valueC = MathUtility.MixValues(valueC, value4, 0.075f);
				valueC = MathUtility.MixValues(valueC, value5, 0.05f);
				valueC = MathUtility.MixValues(valueC, value6, 0.02f);
				valueC = MathUtility.MixValues(valueC, value7, 0.01f);
				
//				float valueB = MathUtility.MixValues (valueA, (valueA * 0.02f) + 0.49f, Mathf.Max(0, 0.9f * valueA - Mathf.Max(0, (2f * valueC) - 1)));
//
//				float valueD = MathUtility.MixValues (valueB, valueC, 0.225f * value8);

				float valueB = MathUtility.MixValues (valueA, valueC, 0.35f * value8);

				float valueD = MathUtility.MixValues (valueB, (valueA * 0.02f) + 0.49f, Mathf.Clamp01(1.3f * valueA - Mathf.Max(0, (2.5f * valueC) - 1)));

				CalculateAndSetAltitude(i, j, valueD);
//				CalculateAndSetAltitude(i, j, valueC);
//				CalculateAndSetAltitude(i, j, valueB);
//				CalculateAndSetAltitude(i, j, valueCb);
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

		return Manager.EnqueueTask (() => {
//			Vector3 randVector = Random.insideUnitSphere;
//
//			randVector.x = (float)System.Math.Round (randVector.x, 4);
//			randVector.y = (float)System.Math.Round (randVector.y, 4);
//			randVector.z = (float)System.Math.Round (randVector.z, 4);

			return RandomUtility.insideUnitSphere * 1000;
		});
	}

	// Returns a value between 0 and 1
	private float GetRandomNoiseFromPolarCoordinates (float alpha, float beta, float radius, Vector3 offset) {

		Vector3 pos = MathUtility.GetCartesianCoordinates(alpha,beta,radius) + offset;
		
		return PerlinNoise.GetValue(pos.x, pos.y, pos.z);
	}
	
	private float GetMountainRangeNoiseFromRandomNoise(float noise, float widthFactor) {

		noise = (noise * 2) - 1;
		
		float value1 = -Mathf.Exp (-Mathf.Pow (noise * widthFactor + 1f, 2));
		float value2 = Mathf.Exp (-Mathf.Pow(noise * widthFactor - 1f, 2));

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
		TerrainCells[longitude][latitude].Altitude = altitude;
		
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
				TerrainCell cell = TerrainCells[i][j];
				
				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);
				float value3 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius3, offset3);

				value2 = value2 * 1.5f + 0.25f;

				float valueA = MathUtility.MixValues(value1, value3, 0.15f);

//				float latitudeFactor = alpha + (((valueA * 2) - 1f) * Mathf.PI * 0.2f);
				float latitudeFactor = alpha + (((valueA * 2) - 1f) * Mathf.PI * 0.15f);
				float latitudeModifier1 = (1.5f * Mathf.Sin(latitudeFactor)) - 0.5f;
//				float latitudeModifier2 = Mathf.Cos(latitudeFactor);
				float latitudeFactor2 = (latitudeFactor * 3) - (Mathf.PI / 2f);
				float latitudeModifier2 = Mathf.Sin(latitudeFactor2);
				float latitudeFactor3 = (latitudeFactor * 6) + (Mathf.PI / 4f);
				float latitudeModifier3 = Mathf.Cos(latitudeFactor3);

				int offCellX = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/40f)) % Width;
				int offCellX2 = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/20f)) % Width;
				int offCellX3 = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/10f)) % Width;
				int offCellX4 = (Width + i + (int)Mathf.Floor(latitudeModifier2 * Width/5f)) % Width;
//				int offCellY = j + (int)Mathf.Floor(latitudeModifier2 * Height/10f);
				int offCellY = (int)Mathf.Clamp(j + Mathf.Floor(latitudeModifier3 * Height/20f), 0, Height);
				offCellY = (offCellY == Height) ? offCellY - 1 : offCellY;
//				int offCellY = j;

				TerrainCell offCell = TerrainCells[offCellX][j];
				TerrainCell offCell2 = TerrainCells[offCellX2][j];
				TerrainCell offCell3 = TerrainCells[offCellX3][j];
				TerrainCell offCell4 = TerrainCells[offCellX4][j];
				TerrainCell offCell5 = TerrainCells[i][offCellY];

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
				TerrainCell cell = TerrainCells[i][j];
				
				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, offset1);
				float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, offset2);

				float latitudeModifier = (alpha * 0.9f) + ((value1 + value2) * 0.05f * Mathf.PI);

				float altitudeSpan = MaxPossibleAltitude - MinPossibleAltitude;

				float absAltitude = cell.Altitude - MinPossibleAltitudeWithOffset;
				
				float altitudeFactor1 = (absAltitude / altitudeSpan) * 0.7f;
				float altitudeFactor2 = (Mathf.Clamp01 (cell.Altitude / MaxPossibleAltitude) * 1.3f);
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

	private void GenerateTerrainArability () {

		int sizeX = Width;
		int sizeY = Height;

		float radius = 2f;

		ManagerTask<Vector3> offset = GenerateRandomOffsetVector();

		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;

			for (int j = 0; j < sizeY; j++)
			{
				TerrainCell cell = TerrainCells[i][j];

				float alpha = (j / (float)sizeY) * Mathf.PI;

				float baseArability = CalculateCellBaseArability (cell);

				cell.Arability = 0;

				if (baseArability <= 0)
					continue;

				// This simulates things like stoniness, impracticality of drainage, excessive salts, etc.
				float noiseFactor = 0.0f + 1.0f * GetRandomNoiseFromPolarCoordinates(alpha, beta, radius, offset);

				cell.Arability = baseArability * noiseFactor;
			}

			ProgressCastMethod (_accumulatedProgress + _progressIncrement * (i + 1)/(float)sizeX);
		}

		_accumulatedProgress += _progressIncrement;
	}

	private void GenerateTerrainBiomes () {
		
		int sizeX = Width;
		int sizeY = Height;
		
		for (int i = 0; i < sizeX; i++) {

			for (int j = 0; j < sizeY; j++) {

				TerrainCell cell = TerrainCells[i][j];

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
				cell.Accessibility = 0;

				foreach (Biome biome in Biome.Biomes.Values)
				{
					float presence = 0;

					if (biomePresences.TryGetValue(biome.Name, out presence))
					{
						presence = presence/totalPresence;

						cell.AddBiomePresence (biome.Name, presence);

						cell.Survivability += biome.Survivability * presence;
						cell.ForagingCapacity += biome.ForagingCapacity * presence;
						cell.Accessibility += biome.Accessibility * presence;
					}
				}

				float altitudeSurvivabilityFactor = 1 - Mathf.Clamp01 (cell.Altitude / MaxPossibleAltitude);

				cell.Survivability *= altitudeSurvivabilityFactor;
			}
			
			ProgressCastMethod (_accumulatedProgress + _progressIncrement * (i + 1)/(float)sizeX);
		}
		
		_accumulatedProgress += _progressIncrement;
	}

	private float CalculateCellBaseArability (TerrainCell cell) {

		float landFactor = 1 - cell.GetBiomePresence (Biome.Ocean);

		if (landFactor == 0)
			return 0;

		float rainfallFactor = 0;

		if (cell.Rainfall > OptimalRainfallForArability) {
		
			rainfallFactor = (MaxRainfallForArability - cell.Rainfall) / (MaxRainfallForArability - OptimalRainfallForArability);

		} else {
			
			rainfallFactor = (cell.Rainfall - MinRainfallForArability) / (OptimalRainfallForArability - MinRainfallForArability);
		}

		rainfallFactor = Mathf.Clamp01 (rainfallFactor);

		float temperatureFactor = (cell.Temperature - MinTemperatureForArability) / (OptimalTemperatureForArability - MinTemperatureForArability);
		temperatureFactor = Mathf.Clamp01 (temperatureFactor);

		return rainfallFactor * temperatureFactor * landFactor;
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
		
		float span = MaxPossibleRainfallWithOffset - MinPossibleRainfallWithOffset;

		float rainfall = (value * span) + MinPossibleRainfallWithOffset;

		float minRainfall = Mathf.Max (0, MinPossibleRainfallWithOffset);

		rainfall = Mathf.Clamp(rainfall, minRainfall, MaxPossibleRainfallWithOffset);
		
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
