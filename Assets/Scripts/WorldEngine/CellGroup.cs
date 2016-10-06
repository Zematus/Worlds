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
	public float TotalPolityInfluenceValue = 0;

	[XmlAttribute("MigVal")]
	public float MigrationValue;

	[XmlAttribute("TotalMigVal")]
	public float TotalMigrationValue;

	public Route SeaMigrationRoute = null;

	public List<string> Flags = new List<string> ();

	public CellCulture Culture;

	public List<PolityInfluence> PolityInfluences;

	public static float TravelWidthFactor;
	
	[XmlIgnore]
	public TerrainCell Cell;

	[XmlIgnore]
	public bool DebugTagged = false;

	[XmlIgnore]
	public List<CellGroup> Neighbors;

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

	Dictionary<TerrainCell, float> _cellMigrationValues = new Dictionary<TerrainCell, float> ();

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
	
	public CellGroup (MigratingGroup migratingGroup, int splitPopulation, CellCulture splitCulture) : this(migratingGroup.World, migratingGroup.TargetCell, splitPopulation, splitCulture) {

		foreach (PolityInfluence p in migratingGroup.PolityInfluences) {

			_polityInfluences.Add (p.PolityId, p);

			ValidateAndSetHighestPolityInfluence (p);
		}
	}

	public CellGroup (World world, TerrainCell cell, int initialPopulation, CellCulture baseCulture = null) : base(world) {

		PreviousExactPopulation = 0;
		ExactPopulation = initialPopulation;
		
		LastUpdateDate = World.CurrentDate;

		Cell = cell;
		Longitude = cell.Longitude;
		Latitude = cell.Latitude;

		Cell.Group = this;

		Id = World.GenerateCellGroupId();

		bool initialGroup = false;

		if (baseCulture == null) {
			initialGroup = true;
			Culture = new CellCulture (this);
		} else {
			Culture = new CellCulture (this, baseCulture);
		}
		
		Neighbors = new List<CellGroup>(new List<TerrainCell>(cell.Neighbors.Values).FindAll (c => c.Group != null).Process (c => c.Group));

		Neighbors.ForEach (g => g.AddNeighbor (this));

		InitializeDefaultActivities (initialGroup);
		InitializeDefaultSkills (initialGroup);
		InitializeDefaultKnowledges (initialGroup);

		OptimalPopulation = CalculateOptimalPopulation (Cell);

		InitializeLocalMigrationValue ();
		
		NextUpdateDate = CalculateNextUpdateDate();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (this, NextUpdateDate));
		
		World.UpdateMostPopulousGroup (this);

		InitializeDefaultEvents ();
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

			World.InsertEventToHappen (new BoatMakingDiscoveryEvent (this, triggerDate));
		}

		if (PlantCultivationDiscoveryEvent.CanSpawnIn (this)) {

			int triggerDate = PlantCultivationDiscoveryEvent.CalculateTriggerDate (this);

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

	public int GetNextLocalRandomInt (int maxValue = PerlinNoise.MaxPermutationValue) {
	
		return Cell.GetNextLocalRandomInt (maxValue);
	}

	public float GetNextLocalRandomFloat () {

		return Cell.GetNextLocalRandomFloat ();
	}

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

		ExactPopulation = newPopulation;

		#if DEBUG
		if (Population < -1000) {

			Debug.Break ();
		}
		#endif

		Culture.MergeCulture (group.Culture, percentage);

		foreach (PolityInfluence p in group.PolityInfluences) {
		
			Polity polity = p.Polity;
			float influence = p.Value;

			polity.MergingEffects (this, influence, percentage);
		}

		TriggerInterference ();
	}
	
	public int SplitGroup (MigratingGroup group) {

		int splitPopulation = (int)Mathf.Floor(Population * group.PercentPopulation);
		
		ExactPopulation -= splitPopulation;

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
	
		Culture.PostUpdate ();
	}

	public void SetupForNextUpdate () {

		if (_destroyed)
			return;
		
		World.UpdateMostPopulousGroup (this);
		
		OptimalPopulation = CalculateOptimalPopulation (Cell);

		CalculateLocalMigrationValue ();
		
		ConsiderLandMigration ();
		ConsiderSeaMigration ();
		
		NextUpdateDate = CalculateNextUpdateDate ();
		
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

		float altitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (cell);
		altitudeDeltaFactor = Mathf.Pow (altitudeDeltaFactor, 4);

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

				float influenceFactor = polityInfluence.Polity.MigrationValue (cell, polityInfluence.Value);
				influenceFactor = Mathf.Pow (influenceFactor, 8);

				polityInfluenceFactor += influenceFactor;
			}
		}

		polityInfluenceFactor *= 10;

		float noMigrationFactor = 1;

		float optimalPopulation = OptimalPopulation;

		if (cell != Cell) {
			noMigrationFactor = _noMigrationFactor;

			optimalPopulation = CalculateOptimalPopulation (cell);
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
//			if ((Id == 1471) || (Id == 1622)) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//				string targetCellLoc = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
//
//				Manager.RegisterDebugEvent ("DebugMessage", 
//					"CalculateMigrationValue - Group:" + groupId + 
//					", targetCell: " + targetCellLoc + 
//					", CurrentDate: " + World.CurrentDate + 
//					", cellValue: " + cellValue + 
////					", altitudeDeltaFactor: " + altitudeDeltaFactor + 
////					", areaFactor: " + areaFactor + 
//					", Population: " + Population + 
//					", existingPopulation: " + existingPopulation + 
////					", popDifferenceFactor: " + popDifferenceFactor + 
////					", noMigrationFactor: " + noMigrationFactor + 
//					", polityInfluenceFactor: " + polityInfluenceFactor + 
//					", optimalPopulation: " + optimalPopulation + 
////					", optimalPopulationFactor: " + optimalPopulationFactor + 
//					", secondaryOptimalPopulationFactor: " + secondaryOptimalPopulationFactor + 
//					"");
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
	
	public void ConsiderLandMigration () {

		if (HasMigrationEvent)
			return;
		
		_cellMigrationValues [Cell] = MigrationValue;

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

		float attemptValue = Cell.GetNextLocalRandomFloat ();

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

		Neighbors.ForEach (g => RemoveNeighbor (this));

		EraseSeaMigrationRoute ();

		StillPresent = false;

		if (FarmDegradationEvent.CanSpawnIn (Cell)) {

			int triggerDate = FarmDegradationEvent.CalculateTriggerDate (Cell);

			World.InsertEventToHappen (new FarmDegradationEvent (Cell, triggerDate));
		}
	}

	public void RemovePolityInfluences () {

		// Make sure any influencing polity gets updated if necessary
		SetPolityUpdates ();

		PolityInfluence[] polityInfluences = new PolityInfluence[_polityInfluences.Count];
		_polityInfluences.Values.CopyTo (polityInfluences, 0);

		foreach (PolityInfluence polityInfluence in polityInfluences) {

			Polity polity = polityInfluence.Polity;

			SetPolityInfluenceValue (polity, 0);
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
		if (Manager.RecordingEnabled) {
			if (UpdateCalled != null) {
			
				UpdateCalled ();
			}
		}
		#endif

		_alreadyUpdated = true;

		PreviousExactPopulation = ExactPopulation;
		
		int timeSpan = World.CurrentDate - LastUpdateDate;

		if (timeSpan <= 0)
			return;

		Profiler.BeginSample ("Cell Group Update");

		// Should do this before modifying the polity influence (otherwise might never get the polity updated if the influence reaches zero)
		SetPolityUpdates ();

		UpdateTerrainFarmlandPercentage (timeSpan);
		UpdatePopulation (timeSpan);
		UpdateCulture (timeSpan);
		PolitiesCulturalInfluence (timeSpan);
		PolityUpdateEffects (timeSpan);

		UpdateTravelFactors ();
		
		LastUpdateDate = World.CurrentDate;
		
		World.AddUpdatedGroup (this);

		Profiler.EndSample ();
	}

	private void SetPolityUpdates () {
	
		foreach (PolityInfluence pi in _polityInfluences.Values) {
		
			SetPolityUpdate (pi.Polity, pi.Value);
		}
	}

//	public void SetPolityUpdate (Polity p) {
//
//		PolityInfluence pi;
//
//		if (!_polityInfluences.TryGetValue (p, out pi)) {
//		
//			return;
//		}
//
//		SetPolityUpdate (pi.Polity, pi.Value);
//	}

	public void SetPolityUpdate (Polity p, float influenceValue) {

		if (p.TotalGroupInfluenceValue <= 0)
			return;

		// If group is not the core group then there's a chance no polity update will happen
		if (p.CoreGroup != this) {

			float chanceFactor = influenceValue / p.TotalGroupInfluenceValue;

			float rollValue = Cell.GetNextLocalRandomFloat ();

			if (rollValue > chanceFactor)
				return;
		}

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

		foreach (PolityInfluence polityInfluence in polityInfluences) {
		
			Polity polity = polityInfluence.Polity;
			float influence = polityInfluence.Value;

			polity.UpdateEffects (this, influence, timeSpan);
		}

		if (TribeFormationEvent.CanSpawnIn (this)) {

			int triggerDate = TribeFormationEvent.CalculateTriggerDate (this);

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
		}
		#endif

		//TODO: Remove commented lines
//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == 1085) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				Manager.RegisterDebugEvent ("DebugMessage", 
//					"CalculateOptimalPopulation - Group:" + groupId + 
//					", CurrentDate: " + World.CurrentDate + 
//					", foragingContribution: " + foragingContribution + 
//					", Area: " + cell.Area + 
//					", modifiedForagingCapacity: " + modifiedForagingCapacity + 
//					", modifiedSurvivability: " + modifiedSurvivability + 
//					", accesibilityFactor: " + accesibilityFactor + 
//					", optimalPopulation: " + optimalPopulation);
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

		//TODO: Remove commented code
//		#if DEBUG
//		string biomeData = "";
//		#endif
		
		foreach (CellCulturalSkill skill in Culture.Skills) {
			
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

					//TODO: Remove commented code
//					#if DEBUG
//					biomeData += "\n\tBiome:" + biomeName + 
//						" ForagingCapacity:" + biome.ForagingCapacity + 
//						" skillValue:" + skillValue + 
//						" biomePresence:" + biomePresence;
//					#endif
				}
			}
		}
		
		float altitudeSurvivabilityFactor = 1 - (cell.Altitude / World.MaxPossibleAltitude);

		modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;

		foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
		survivability = modifiedSurvivability * altitudeSurvivabilityFactor;

		//TODO: Remove commented code
//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Id == 1085) {
//				System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
//
//				System.Reflection.MethodBase method = stackTrace.GetFrame(2).GetMethod();
//				string callingMethod = method.Name;
//
//				if (callingMethod.Contains ("SetupForNextUpdate")) {
//					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//					Manager.RegisterDebugEvent ("DebugMessage", 
//						"CalculateOptimalPopulation - Group:" + groupId + 
//						", CurrentDate: " + World.CurrentDate + 
//						", foragingCapacity: " + foragingCapacity + 
//						", biomeData: " + biomeData);
//				}
//			}
//		}
//		#endif
	}

	public int CalculateNextUpdateDate () {

		#if DEBUG
		if (Cell.IsSelected) {
			bool debug = true;
		}
		#endif

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if ((Id == 1471) || (Id == 1622)) {
//				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
//
//				Manager.RegisterDebugEvent ("DebugMessage", 
//					"CalculateNextUpdateDate - Group:" + groupId + 
//					", CurrentDate: " + World.CurrentDate + 
//					", MigrationValue: " + MigrationValue + 
//					", TotalMigrationValue: " + TotalMigrationValue + 
//					", OptimalPopulation: " + OptimalPopulation + 
//					", Population: " + Population);
//			}
//		}
//		#endif

		float randomFactor = Cell.GetNextLocalRandomFloat ();
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

			return population;
		}

		if (population > OptimalPopulation) {

			population = OptimalPopulation + (ExactPopulation - OptimalPopulation) * MathUtility.RoundToSixDecimals (Mathf.Exp (-timeFactor));

			#if DEBUG
			if ((int)population < -1000) {

				Debug.Break ();
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

	public void SetPolityInfluenceValue (Polity polity, float newInfluenceValue) {

		#if DEBUG
		RunningFunction_SetPolityInfluence = true;

		if (Cell.IsSelected) {

			bool debug = true;
		}
		#endif

		newInfluenceValue = MathUtility.RoundToSixDecimals (newInfluenceValue);

		PolityInfluence polityInfluence;

		_polityInfluences.TryGetValue (polity.Id, out polityInfluence);

		if (polityInfluence == null) {
			if (newInfluenceValue > Polity.MinPolityInfluence) {

				polityInfluence = new PolityInfluence (polity, newInfluenceValue);

				_polityInfluences.Add (polity.Id, polityInfluence);

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
			
			_polityInfluences.Remove (polityInfluence.PolityId);

			polity.RemoveInfluencedGroup (this);

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

		Culture.Synchronize ();

		PolityInfluences = new List<PolityInfluence> (_polityInfluences.Values);
		
		base.Synchronize ();
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Flags.ForEach (f => _flags.Add (f));

		Cell = World.GetCell (Longitude, Latitude);

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
