﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroup : HumanGroup {

	public const int GenerationTime = 25;

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)

	public const float NaturalGrowthRate = NaturalBirthRate - NaturalDeathRate;
	
	public const float PopulationConstant = 10;

	public const float MinKnowledgeTransferValue = 0.25f;

	public const float SeaTravelBaseFactor = 0.025f;
	
	[XmlAttribute]
	public float ExactPopulation;
	
	[XmlAttribute]
	public int Id;
	
	[XmlAttribute]
	public bool StillPresent = true;
	
	[XmlAttribute]
	public int LastUpdateDate;
	
	[XmlAttribute]
	public int NextUpdateDate;
	
	[XmlAttribute]
	public int OptimalPopulation;
	
	[XmlAttribute]
	public int CellLongitude;
	[XmlAttribute]
	public int CellLatitude;
	
	[XmlAttribute]
	public bool HasMigrationEvent = false;
	[XmlAttribute]
	public bool HasKnowledgeTransferEvent = false;

	[XmlAttribute]
	public float SeaTravelFactor = 0;

	public Route SeaMigrationRoute = null;

	public List<string> Flags = new List<string> ();

	public CellCulture Culture;

	[XmlIgnore]
	public float EffectivePopulation = 0; // The population that tentatively would have been productive between an especific timespan

	[XmlIgnore]
	public static float TravelWidthFactor;
	
	[XmlIgnore]
	public TerrainCell Cell;
	
	[XmlIgnore]
	public float CellMigrationValue;

	[XmlIgnore]
	public float TotalMigrationValue;
	
	[XmlIgnore]
	public bool DebugTagged = false;
	
	[XmlIgnore]
	public List<CellGroup> Neighbors;

//	private Dictionary<int, WorldEvent> _associatedEvents = new Dictionary<int, WorldEvent> ();
	
	private HashSet<string> _flags = new HashSet<string> ();

	private float _noMigrationPreference = 5f;
	
	[XmlIgnore]
	public int Population {

		get {
			return (int)Mathf.Floor(ExactPopulation);
		}
	}

	public CellGroup () {
		
		Manager.UpdateWorldLoadTrackEventCount ();
	}
	
	public CellGroup (MigratingGroup migratingGroup, int splitPopulation, CellCulture splitCulture) : this(migratingGroup.World, migratingGroup.TargetCell, splitPopulation, splitCulture) {
	}

	public CellGroup (World world, TerrainCell cell, int initialPopulation, CellCulture baseCulture = null) : base(world) {

		ExactPopulation = initialPopulation;
		
		LastUpdateDate = World.CurrentDate;

		Cell = cell;
		CellLongitude = cell.Longitude;
		CellLatitude = cell.Latitude;

		Cell.Group = this;

		Id = World.GenerateCellGroupId();

		if (baseCulture == null) {
			Culture = new CellCulture (this);
		} else {
			Culture = new CellCulture (this, baseCulture);
		}
		
		Neighbors = new List<CellGroup>(new List<TerrainCell>(cell.Neighbors.Values).FindAll (c => c.Group != null).Process (c => c.Group));

		Neighbors.ForEach (g => g.AddNeighbor (this));

		InitializeDefaultSkills ();
		
		NextUpdateDate = CalculateNextUpdateDate();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (this, NextUpdateDate));
		
		World.UpdateMostPopulousGroup (this);
		
		OptimalPopulation = CalculateOptimalPopulation (Cell);

		InitializeDefaultEvents ();
	}

	public void InitializeDefaultEvents () {

		if (BoatMakingDiscoveryEvent.CanSpawnIn (this)) {

			int triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate (this);

			World.InsertEventToHappen (new BoatMakingDiscoveryEvent (this, triggerDate));
		}

		if (PlantCultivationDiscoveryEvent.CanSpawnIn (this)) {

			int triggerDate = PlantCultivationDiscoveryEvent.CalculateTriggerDate (this);

			World.InsertEventToHappen (new PlantCultivationDiscoveryEvent (this, triggerDate));
		}
	}

	public void SetFlag (string flag) {
	
		if (_flags.Contains (flag))
			return;

		_flags.Add (flag);
		Flags.Add (flag);
	}

	public bool IsFlagSet (string flag) {
	
		return _flags.Contains (flag);
	}

	public void UnsetFlag (string flag) {
	
		if (!_flags.Contains (flag))
			return;

		_flags.Remove (flag);
		Flags.Remove (flag);
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

	public void AddNeighbor (CellGroup group) {

		if (group == null)
			return;

		if (!group.StillPresent)
			return;

		if (Neighbors.Contains (group))
			return;
	
		Neighbors.Add (group);
	}
	
	public void RemoveNeighbor (CellGroup group) {
		
		if (!Neighbors.Contains (group))
			return;
		
		Neighbors.Remove (group);
	}

	public void InitializeDefaultSkills () {

		foreach (string biomeName in GetPresentBiomesInNeighborhood ()) {
			
			if (biomeName == Biome.Ocean.Name) {

				if (Culture.GetSkill (SeafaringSkill.SeafaringSkillId) == null) {

					Culture.AddSkillToLearn (new SeafaringSkill (this));
				}

			} else {
				
				Biome biome = Biome.Biomes[biomeName];

				string skillId = BiomeSurvivalSkill.GenerateId (biome);

				if (Culture.GetSkill (skillId) == null) {

					Culture.AddSkillToLearn (new BiomeSurvivalSkill (this, biome));
				}
			}
		}
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

	public void MergeGroup (MigratingGroup group, int splitPopulation, CellCulture splitCulture) {
		
		UpdateInternal ();

		float newPopulation = Population + splitPopulation;
		
		if (newPopulation <= 0) {
			throw new System.Exception ("Population after migration merge shouldn't be 0 or less.");
		}

		float percentage = splitPopulation / newPopulation;
	
		ExactPopulation = newPopulation;

		Culture.MergeCulture (splitCulture, percentage);

		TriggerInterference ();
	}
	
	public int SplitGroup (MigratingGroup group) {
		
		UpdateInternal ();

		int splitPopulation = (int)Mathf.Floor(Population * group.PercentPopulation);
		
		ExactPopulation -= splitPopulation;

		return splitPopulation;
	}

	public void Update () {

		UpdateInternal ();
	}

	public void PostUpdate () {
	
		Culture.PostUpdate ();
	}

	public void SetupForNextUpdate () {
		
		if (Population < 2) {
			World.AddGroupToRemove (this);
			return;
		}
		
		World.UpdateMostPopulousGroup (this);
		
		OptimalPopulation = CalculateOptimalPopulation (Cell);

		CalculateLocalMigrationValue ();
		
		ConsiderLandMigration ();
		ConsiderSeaMigration ();

//		ConsiderKnowledgeTransfer ();
		
		NextUpdateDate = CalculateNextUpdateDate ();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (this, NextUpdateDate));
	}

//	public void ConsiderKnowledgeTransfer () {
//
//		if (HasKnowledgeTransferEvent)
//			return;
//	
//		float transferValue = 0;
//
//		CellGroup targetGroup = KnowledgeTransferEvent.DiscoverTargetGroup (this, out transferValue);
//
//		if (targetGroup == null)
//			return;
//
//		if (transferValue <= 0)
//			return;
//
//		int triggerDate = KnowledgeTransferEvent.CalculateTriggerDate (this, transferValue);
//
//		World.InsertEventToHappen (new KnowledgeTransferEvent (this, targetGroup, triggerDate));
//
//		HasKnowledgeTransferEvent = true;
//	}
	
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
		
		float areaFactor = cell.Area / TerrainCell.MaxArea;
		
		float altitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (cell);
		altitudeDeltaFactor *= altitudeDeltaFactor;
		altitudeDeltaFactor *= altitudeDeltaFactor;
		
		float stressFactor = 1 - cell.CalculatePopulationStress();
		stressFactor *= stressFactor;
		stressFactor *= stressFactor;
		
		float cSurvivability = 0;
		float cForagingCapacity = 0;
		
		CalculateAdaptionToCell (cell, out cForagingCapacity, out cSurvivability);
		
		float adaptionFactor = cSurvivability * cForagingCapacity; 
		
		float cellValue = adaptionFactor * altitudeDeltaFactor * areaFactor * stressFactor;

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

	public void CalculateLocalMigrationValue () {

		CellMigrationValue = CalculateMigrationValue (Cell);
		CellMigrationValue *= _noMigrationPreference;
		CellMigrationValue += _noMigrationPreference;

		TotalMigrationValue = CellMigrationValue;
	}
	
	public void ConsiderLandMigration () {

		if (HasMigrationEvent)
			return;
		
		Dictionary<TerrainCell, float> cellValuePairs = new Dictionary<TerrainCell, float> ();

		cellValuePairs.Add (Cell, CellMigrationValue);

		foreach (TerrainCell c in Cell.Neighbors.Values) {
			
			float cellValue = CalculateMigrationValue (c);
			
			TotalMigrationValue += cellValue;
			
			cellValuePairs.Add (c, cellValue);
		}

		TerrainCell targetCell = CollectionUtility.WeightedSelection (cellValuePairs, TotalMigrationValue, Cell.GetNextLocalRandomFloat);

		if (targetCell == Cell)
			return;

		if (targetCell == null)
			return;
		
		float percentToMigrate = (1 - CellMigrationValue/TotalMigrationValue) * Cell.GetNextLocalRandomFloat ();
		
		float cellSurvivability = 0;
		float cellForagingCapacity = 0;
		
		CalculateAdaptionToCell (targetCell, out cellForagingCapacity, out cellSurvivability);

		if (cellSurvivability <= 0)
			return;

		float cellAltitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (targetCell);

		float travelFactor = 
			cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
			cellSurvivability * cellSurvivability *  targetCell.Accessibility;

		travelFactor = Mathf.Clamp (travelFactor, 0.0001f, 1);

		int travelTime = (int)Mathf.Ceil(Cell.Width / (TravelWidthFactor * travelFactor));
		
		int nextDate = World.CurrentDate + travelTime;

		MigratingGroup migratingGroup = new MigratingGroup (World, percentToMigrate, this, targetCell);
		
		World.InsertEventToHappen (new MigrateGroupEvent (World, nextDate, travelTime, migratingGroup));

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

		float percentToMigrate = (1 - CellMigrationValue/TotalMigrationValue) * Cell.GetNextLocalRandomFloat ();

		float cellSurvivability = 0;
		float cellForagingCapacity = 0;

		CalculateAdaptionToCell (targetCell, out cellForagingCapacity, out cellSurvivability);

		if (cellSurvivability <= 0)
			return;

		float routeLength = SeaMigrationRoute.Length;

		float successChance = SeaTravelFactor / (SeaTravelFactor + routeLength);

		float attemptValue = Cell.GetNextLocalRandomFloat ();

		if (attemptValue >= successChance)
			return;

		int travelTime = (int)Mathf.Ceil(routeLength / SeaTravelFactor);

		int nextDate = World.CurrentDate + travelTime;

		MigratingGroup migratingGroup = new MigratingGroup (World, percentToMigrate, this, targetCell);

		World.InsertEventToHappen (new MigrateGroupEvent (World, nextDate, travelTime, migratingGroup));

		HasMigrationEvent = true;
	}

	public void Destroy () {

		Cell.Group = null;
		World.RemoveGroup (this);

		Neighbors.ForEach (g => RemoveNeighbor (this));

		EraseSeaMigrationRoute ();

		StillPresent = false;
	}

	private void UpdateInternal () {
		
		int timeSpan = World.CurrentDate - LastUpdateDate;

		if (timeSpan <= 0)
			return;

		UpdatePopulation (timeSpan);
		UpdateCulture (timeSpan);
		UpdateTerrainFarmlandPercentage (timeSpan);

		UpdateTravelFactors ();
		
//		AbsorbKnowledgeFromNeighbors ();
		
		LastUpdateDate = World.CurrentDate;
		
		World.AddUpdatedGroup (this);
	}
	
	private void UpdatePopulation (int timeSpan) {

		float prevPopulation = ExactPopulation;
		
		ExactPopulation = PopulationAfterTime (timeSpan);

		// This is a very rough estimate, it assume linear growth and ignores any other internal or external factor
		EffectivePopulation = Mathf.Min (ExactPopulation, prevPopulation) + 0.5f * (Mathf.Abs (ExactPopulation - prevPopulation));
	}
	
	private void UpdateCulture (int timeSpan) {
		
		Culture.Update (timeSpan);
	}

	private void UpdateTerrainFarmlandPercentage (int timeSpan) {

		CulturalKnowledge agriculturalKnowledge = Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId);

		float farmlandPercentage = Cell.FarmlandPercentage;

//		float farmlandDegradation = 0.01f * farmlandPercentage;
//
//		farmlandPercentage -= farmlandDegradation;

		if (agriculturalKnowledge == null) {

//			Cell.FarmlandPercentage = farmlandPercentage;

			return;
		}

		float farmlandArea = farmlandPercentage * Cell.Area;

		float maxPossibleFarmlandArea = Cell.Area * Cell.Accessibility;

		float availableAreaToFarm = maxPossibleFarmlandArea - farmlandArea;

		if (availableAreaToFarm <= 0)
			return;

		float techEfficiencyFactor = agriculturalKnowledge.Value / 10000f;

		float maxGeneratedFarmlandAreaPossible = EffectivePopulation * techEfficiencyFactor * timeSpan;

		float actualGeneratedPercentage = maxGeneratedFarmlandAreaPossible / (availableAreaToFarm + maxGeneratedFarmlandAreaPossible);

		float actualGeneratedFarmlandArea = availableAreaToFarm * actualGeneratedPercentage;

		float percentageToAdd = actualGeneratedFarmlandArea / Cell.Area;

		farmlandPercentage += percentageToAdd;

		if (farmlandPercentage > 1) {
		
			throw System.Exception ("farmlandPercentage greater than 1");
		}

		Cell.FarmlandPercentage = farmlandPercentage;
	}

	public void UpdateTravelFactors () {

		float seafaringValue = 0;
		float shipbuildingValue = 0;

		foreach (CulturalSkill skill in Culture.Skills) {

			if (skill is SeafaringSkill) {

				seafaringValue = skill.Value;
			}
		}

		foreach (CulturalKnowledge knowledge in Culture.Knowledges) {

			if (knowledge is ShipbuildingKnowledge) {

				shipbuildingValue = knowledge.Value;
			}
		}

		SeaTravelFactor = SeaTravelBaseFactor * seafaringValue * shipbuildingValue * TravelWidthFactor;
	}

//	private void AbsorbKnowledgeFromNeighbors () {
//	
//		Neighbors.ForEach (g => AbsorbKnowledgeFrom (g));
//	}

	public void AbsorbKnowledgeFrom (CellGroup group) {
		
		float populationFactor = Mathf.Min (1, group.Population / (float)Population);
		
		group.Culture.Knowledges.ForEach (k => {
			
			Culture.AbsorbKnowledgeFrom (k, populationFactor);
		});

		group.Culture.Discoveries.ForEach (d => {
			
			Culture.AbsorbDiscoveryFrom (d);
		});
	}

	public int CalculateOptimalPopulation (TerrainCell cell) {

		int optimalPopulation = 0;

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

		CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

		float populationCapacityFactor = PopulationConstant * cell.Area * modifiedForagingCapacity * modifiedSurvivability;

		optimalPopulation = (int)Mathf.Floor (populationCapacityFactor);

		return optimalPopulation;
	}

	public void CalculateAdaptionToCell (TerrainCell cell, out float foragingCapacity, out float survivability) {

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;
		
		foreach (CulturalSkill skill in Culture.Skills) {
			
			float skillValue = skill.Value;
			
			if (skill is BiomeSurvivalSkill) {
				
				BiomeSurvivalSkill biomeSurvivalSkill = skill as BiomeSurvivalSkill;
				
				string biomeName = biomeSurvivalSkill.BiomeName;
				
				float biomePresence = cell.GetBiomePresence(biomeName);
				
				if (biomePresence > 0)
				{
					Biome biome = Biome.Biomes[biomeName];
					
					modifiedForagingCapacity += biome.ForagingCapacity * skillValue * biomePresence;
					modifiedSurvivability += (biome.Survivability + skillValue * (1 - biome.Survivability)) * biomePresence;
				}
			}
		}
		
		float altitudeSurvivabilityFactor = 1 - (cell.Altitude / World.MaxPossibleAltitude);

		foragingCapacity = modifiedForagingCapacity;
		survivability = modifiedSurvivability * altitudeSurvivabilityFactor;
	}

	public int CalculateNextUpdateDate () {

		if (Cell.IsSelected) {
		
			bool debug = true;
		}

		float randomFactor = Cell.GetNextLocalRandomFloat ();

		float migrationFactor = CellMigrationValue/TotalMigrationValue;

		float skillLevelFactor = Culture.MinimumSkillAdaptationLevel ();
		
		float knowledgeLevelFactor = Culture.MinimumKnowledgeProgressLevel ();

		float populationFactor = 1 + Mathf.Abs (OptimalPopulation - Population);
		populationFactor = (10000 + OptimalPopulation) / populationFactor;

		float mixFactor = randomFactor * migrationFactor * skillLevelFactor * knowledgeLevelFactor * populationFactor;

		int finalFactor = (int)Mathf.Max(mixFactor, 1);

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

			population = OptimalPopulation * (1 - Mathf.Pow(populationFactor, geometricTimeFactor));

			return population;
		}

		if (population > OptimalPopulation) {

			population = OptimalPopulation + (ExactPopulation - OptimalPopulation) * Mathf.Exp (-timeFactor);
			
			return population;
		}

		return 0;
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Flags.ForEach (f => _flags.Add (f));

		Cell = World.GetCell (CellLongitude, CellLatitude);

		Cell.Group = this;

		Neighbors = new List<CellGroup> (new List<TerrainCell>(Cell.Neighbors.Values).FindAll (c => c.Group != null).Process (c => c.Group));
		
		Neighbors.ForEach (g => g.AddNeighbor (this));
		
		World.UpdateMostPopulousGroup (this);

		Culture.Group = this;
		Culture.FinalizeLoad ();

		if (SeaMigrationRoute != null) {
			
			SeaMigrationRoute.World = World;
			SeaMigrationRoute.FinalizeLoad ();
		}
	}
}
