using UnityEngine;
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
	
	public const float PopulationForagingConstant = 10;
	public const float PopulationFarmingConstant = 40;

	public const float MinKnowledgeTransferValue = 0.25f;

	public const float SeaTravelBaseFactor = 0.025f;

	[XmlAttribute]
	public float PreviousExactPopulation;
	
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

	[XmlAttribute]
	public float TotalPolityInfluence = 0;

	public Route SeaMigrationRoute = null;

	public List<string> Flags = new List<string> ();

	public CellCulture Culture;

	public List<int> InfluencingPolityIds;
	public List<float> PolityInfluenceValues;

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

	[XmlIgnore]
	public Dictionary<Polity, float> PolityInfluences = new Dictionary<Polity, float> ();

//	private Dictionary<int, WorldEvent> _associatedEvents = new Dictionary<int, WorldEvent> ();
	
	private HashSet<string> _flags = new HashSet<string> ();

	private float _noMigrationFactor = 0.002f;

	Dictionary<TerrainCell, float> _cellMigrationValues = new Dictionary<TerrainCell, float> ();

	public int PreviousPopulation {

		get {
			return (int)Mathf.Floor(PreviousExactPopulation);
		}
	}

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

		PreviousExactPopulation = 0;
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

		InitializeDefaultActivities ();
		InitializeDefaultSkills ();
		InitializeDefaultKnowledges ();
		
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

	public void InitializeDefaultActivities () {

		Culture.AddActivityToPerform (CulturalActivity.CreateForagingActivity (this));
	}

	public void InitializeDefaultKnowledges () {

		Culture.AddKnowledgeToLearn (new SocialOrganizationKnowledge (this));
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

	public void MergeGroup (MigratingGroup group) {
		
		UpdateInternal ();

		float newPopulation = Population + group.Population;
		
		if (newPopulation <= 0) {
			throw new System.Exception ("Population after migration merge shouldn't be 0 or less.");
		}

		float percentage = group.Population / newPopulation;

		ExactPopulation = newPopulation;

		Culture.MergeCulture (group.Culture, percentage);

		foreach (KeyValuePair <Polity, float> pair in group.PolityInfluences) {
		
			Polity polity = pair.Key;
			float influence = pair.Value;

			polity.MergingEffects (this, influence, percentage);
		}

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

		if (Cell.IsSelected) {
		
			bool debug = true;
		}
		
		float areaFactor = cell.Area / TerrainCell.MaxArea;

		float altitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (cell);
		altitudeDeltaFactor *= altitudeDeltaFactor;
		altitudeDeltaFactor *= altitudeDeltaFactor;

		int existingPopulation = 0;

		float popDifferenceFactor = 1;

		if (cell.Group != null) {
			existingPopulation = cell.Group.Population;

			popDifferenceFactor = (float)Population / (float)(Population + existingPopulation);
			popDifferenceFactor *= popDifferenceFactor;
			popDifferenceFactor *= popDifferenceFactor;

		}

		popDifferenceFactor *= 10;

		float polityInfluenceFactor = 1;

		foreach (KeyValuePair<Polity, float> pair in PolityInfluences) {

			float influenceFactor = pair.Key.MigrationValue (cell, pair.Value) * 50;

			polityInfluenceFactor += influenceFactor;
		}

		float noMigrationFactor = 1;

		if (cell != Cell) {
			noMigrationFactor = _noMigrationFactor;
		}

		float cellValue = altitudeDeltaFactor * areaFactor * popDifferenceFactor * noMigrationFactor * polityInfluenceFactor;

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

		TotalMigrationValue = CellMigrationValue;
	}
	
	public void ConsiderLandMigration () {

		if (HasMigrationEvent)
			return;
		
		_cellMigrationValues [Cell] = CellMigrationValue;

		foreach (TerrainCell c in Cell.Neighbors.Values) {
			
			float cellValue = CalculateMigrationValue (c);
			
			TotalMigrationValue += cellValue;

			_cellMigrationValues [c] = cellValue;
		}

		TerrainCell targetCell = CollectionUtility.WeightedSelection (_cellMigrationValues, TotalMigrationValue, Cell.GetNextLocalRandomFloat);

		if (targetCell == Cell)
			return;

		if (targetCell == null)
			return;

		float percentToMigrate = Cell.GetNextLocalRandomFloat ();
		percentToMigrate *= percentToMigrate;

		if (TotalMigrationValue > 0) {
			percentToMigrate *= (1 - CellMigrationValue / TotalMigrationValue);
		}
		
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

		if (FarmDegradationEvent.CanSpawnIn (Cell)) {

			int triggerDate = FarmDegradationEvent.CalculateTriggerDate (Cell);

			World.InsertEventToHappen (new FarmDegradationEvent (Cell, triggerDate));
		}
	}

	private void UpdateInternal () {

		PreviousExactPopulation = ExactPopulation;
		
		int timeSpan = World.CurrentDate - LastUpdateDate;

		if (timeSpan <= 0)
			return;

		UpdateTerrainFarmlandPercentage (timeSpan);
		UpdatePopulation (timeSpan);
		UpdateCulture (timeSpan);
		UpdatePolities (timeSpan);

		UpdateTravelFactors ();
		
//		AbsorbKnowledgeFromNeighbors ();
		
		LastUpdateDate = World.CurrentDate;
		
		World.AddUpdatedGroup (this);
	}
	
	private void UpdatePopulation (int timeSpan) {
		
		ExactPopulation = PopulationAfterTime (timeSpan);
	}
	
	private void UpdateCulture (int timeSpan) {
		
		Culture.Update (timeSpan);
	}

	private void UpdatePolities (int timeSpan) {

		foreach (KeyValuePair <Polity, float> pair in PolityInfluences) {
		
			Polity polity = pair.Key;
			float influence = pair.Value;

			polity.UpdateEffects (this, influence, timeSpan);
		}
	}

	public void LostTribalism () {

		List <Polity> polities = new List<Polity> (PolityInfluences.Keys);

		foreach (Polity polity in polities) {

			if (polity is Tribe) {
				SetPolityInfluence (polity, 0);
			}
		}
	}

	private float GetActivityContribution (string activityId) {
	
		CulturalActivity activity = Culture.GetActivity (activityId);

		if (activity == null)
			return 0;

		return activity.Contribution;
	}

	private void UpdateTerrainFarmlandPercentage (int timeSpan) {

		if (Cell.IsSelected) {
		
			bool debug = true;
		}

		CulturalKnowledge agricultureKnowledge = Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId);

		if (agricultureKnowledge == null) {

			return;
		}

		float farmlandPercentage = Cell.FarmlandPercentage;

		float techValue = Mathf.Sqrt(agricultureKnowledge.Value);

		float areaPerFarmWorker = techValue / 5f;

		float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn (Cell);

		float farmingPopulation = GetActivityContribution (CulturalActivity.FarmingActivityId) * Population;

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

//	public void AbsorbKnowledgeFrom (CellGroup group) {
//		
//		float populationFactor = Mathf.Min (1, group.Population / (float)Population);
//		
//		group.Culture.Knowledges.ForEach (k => {
//			
//			Culture.AbsorbKnowledgeFrom (k, populationFactor);
//		});
//
//		group.Culture.Discoveries.ForEach (d => {
//			
//			Culture.AbsorbDiscoveryFrom (d);
//		});
//	}

	public int CalculateOptimalPopulation (TerrainCell cell) {

		int optimalPopulation = 0;

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

		float foragingContribution = GetActivityContribution (CulturalActivity.ForagingActivityId);

		CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

		float populationCapacityByForaging = foragingContribution * PopulationForagingConstant * cell.Area * modifiedForagingCapacity;

		float farmingContribution = GetActivityContribution (CulturalActivity.FarmingActivityId);
		float populationCapacityByFarming = 0;

		if (farmingContribution > 0) {

			float farmingCapacity = CalculateFarmingCapacity (cell);

			populationCapacityByFarming = farmingContribution * PopulationFarmingConstant * cell.Area * farmingCapacity;
		}

		float populationCapacity = (populationCapacityByForaging + populationCapacityByFarming) * modifiedSurvivability;

		optimalPopulation = (int)Mathf.Floor (populationCapacity);

		return optimalPopulation;
	}

	public float CalculateFarmingCapacity (TerrainCell cell) {

		float capacityFactor = 0;
	
		CulturalKnowledge agricultureKnowledge = Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId);

		if (agricultureKnowledge == null)
			return capacityFactor;

		float techFactor = Mathf.Sqrt(agricultureKnowledge.Value);

		capacityFactor = cell.FarmlandPercentage * techFactor;

		return capacityFactor;
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

		modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;

		foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
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

	public float GetPolityInfluence (Polity polity) {

		float influence;

		PolityInfluences.TryGetValue (polity, out influence);

		return influence;
	}

	public void SetPolityInfluence (Polity polity, float influence) {

		float currentInfluence = GetPolityInfluence (polity);

		TotalPolityInfluence -= currentInfluence;

		if (influence <= 0) {
			PolityInfluences.Remove (polity);

			polity.RemoveInfluencedGroup (this);
		} else {
			PolityInfluences [polity] = influence;

			polity.AddInfluencedGroup (this);

			TotalPolityInfluence += influence;
		}
	}

	public float GetRelativePolityInfluence (Polity polity) {
	
		float absoluteInfluence = GetPolityInfluence (polity);

		if (TotalPolityInfluence <= 0)
			return 0;

		return absoluteInfluence / TotalPolityInfluence;
	}

	public override void Synchronize () {

		InfluencingPolityIds = new List<int> (PolityInfluences.Count);
		PolityInfluenceValues = new List<float> (PolityInfluences.Count);

		foreach (KeyValuePair <Polity, float> pair in PolityInfluences) {
		
			InfluencingPolityIds.Add (pair.Key.Id);
			PolityInfluenceValues.Add (pair.Value);
		} 
		
		base.Synchronize ();
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Flags.ForEach (f => _flags.Add (f));

		Cell = World.GetCell (CellLongitude, CellLatitude);

		Cell.Group = this;

		Neighbors = new List<CellGroup> (new List<TerrainCell>(Cell.Neighbors.Values).FindAll (c => c.Group != null).Process (c => c.Group));
		
		Neighbors.ForEach (g => g.AddNeighbor (this));
		
		World.UpdateMostPopulousGroup (this);

		Culture.World = World;
		Culture.Group = this;
		Culture.FinalizeLoad ();

		if (SeaMigrationRoute != null) {
			
			SeaMigrationRoute.World = World;
			SeaMigrationRoute.FinalizeLoad ();
		}

		for (int i = 0; i < InfluencingPolityIds.Count; i++) {

			int id = InfluencingPolityIds [i];
			float value = PolityInfluenceValues [i];

			Polity polity = World.GetPolity (id);

			if (polity == null)
				throw new System.Exception ("Unable to find Polity with Id " + id);

			PolityInfluences.Add (polity, value);
		}
	}
}
