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
	
	public const float PopulationConstant = 10;

	public const float TravelTimeFactor = 1;

	public const float MinKnowledgeTransferValue = 0.25f;
	
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

	public CellCulture Culture;
	
	[XmlIgnore]
	public TerrainCell Cell;
	
	[XmlIgnore]
	public float CellMigrationValue;
	
	[XmlIgnore]
	public bool DebugTagged = false;
	
	[XmlIgnore]
	public List<CellGroup> Neighbors;

	private Dictionary<int, WorldEvent> _associatedEvents = new Dictionary<int, WorldEvent> ();
	
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
		
		Neighbors = cell.Neighbors.FindAll (c => c.Group != null).Process (c => c.Group);

		Neighbors.ForEach (g => g.AddNeighbor (this));

		InitializeBiomeSurvivalSkills ();
		
		NextUpdateDate = CalculateNextUpdateDate();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (this, NextUpdateDate));
		
		World.UpdateMostPopulousGroup (this);
		
		OptimalPopulation = CalculateOptimalPopulation (Cell);

		if (BoatMakingDiscoveryEvent.CanSpawnIn (this)) {

			int triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate (this);
			
			World.InsertEventToHappen (new BoatMakingDiscoveryEvent (this, triggerDate));
		}
	}

	public void AddAssociatedEvent (WorldEvent e) {

		_associatedEvents.Add (e.Id, e);
	}
	
	public WorldEvent GetAssociatedEvent (int id) {

		WorldEvent e;

		if (!_associatedEvents.TryGetValue (id, out e))
			return null;

		return e;
	}
	
	public List<WorldEvent> GetAssociatedEvents (System.Type eventType) {
		
		return _associatedEvents.Process (p => p.Value).FindAll (e => e.GetType () == eventType);
	}

	public void RemoveAssociatedEvent (int id) {
	
		if (!_associatedEvents.ContainsKey (id))
			return;

		_associatedEvents.Remove (id);
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

	public void InitializeBiomeSurvivalSkills () {

		foreach (string biomeName in GetPresentBiomesInNeighborhood ()) {
			
			if (biomeName == "Ocean") continue;

			Biome biome = Biome.Biomes[biomeName];
		
			string skillId = BiomeSurvivalSkill.GenerateId (biome);

			if (Culture.GetSkill (skillId) == null) {
				
				Culture.AddSkillToLearn (new BiomeSurvivalSkill (this, biome));
			}
		}
	}

	public HashSet<string> GetPresentBiomesInNeighborhood () {
	
		HashSet<string> biomeNames = new HashSet<string> ();
		
		foreach (string biomeName in Cell.PresentBiomeNames) {

			biomeNames.Add (biomeName);
		}

		foreach (TerrainCell neighborCell in Cell.Neighbors) {
			
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
		
		ConsiderMigration ();

		ConsiderKnowledgeTransfer ();
		
		NextUpdateDate = CalculateNextUpdateDate ();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (this, NextUpdateDate));
	}

	public void ConsiderKnowledgeTransfer () {

		if (HasKnowledgeTransferEvent)
			return;
	
		float transferValue = 0;

		CellGroup targetGroup = KnowledgeTransferEvent.DiscoverTargetGroup (this, out transferValue);

		if (targetGroup == null)
			return;

		if (transferValue <= 0)
			return;

		int triggerDate = KnowledgeTransferEvent.CalculateTriggerDate (this, transferValue);

		World.InsertEventToHappen (new KnowledgeTransferEvent (this, targetGroup, triggerDate));

		HasKnowledgeTransferEvent = true;
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

	public void ConsiderMigration () {

		if (HasMigrationEvent)
			return;

		List<TerrainCell> possibleTargetCells = new List<TerrainCell> (Cell.Neighbors);
		possibleTargetCells.Add (Cell);

		float noMigrationPreference = 5f;

		CellMigrationValue = 0;
		float totalMigrationValue = 0;

		TerrainCell targetCell = CollectionUtility.WeightedSelection (possibleTargetCells, (c) => {

			float areaFactor = Cell.Area / TerrainCell.MaxArea;

			float altitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (c);
			altitudeDeltaFactor *= altitudeDeltaFactor;
			altitudeDeltaFactor *= altitudeDeltaFactor;

			float stressFactor = 1 - c.CalculatePopulationStress();
			stressFactor *= stressFactor;
			stressFactor *= stressFactor;

			float cSurvivability = 0;
			float cForagingCapacity = 0;

			CalculateAdaptionToCell (c, out cForagingCapacity, out cSurvivability);

			float adaptionFactor = cSurvivability * cForagingCapacity; 

			float cellValue = adaptionFactor * altitudeDeltaFactor * areaFactor * stressFactor;

			if (c == Cell) {
				cellValue *= noMigrationPreference;
				cellValue += noMigrationPreference;

				cellValue *= cForagingCapacity;

				CellMigrationValue += cellValue;
			}

			totalMigrationValue += cellValue;

			return cellValue;
		}, Cell.GetNextLocalRandomFloat);
		
		if (totalMigrationValue <= 0) {
			CellMigrationValue = 1;
		} else {
			CellMigrationValue /= totalMigrationValue;
		}
		
		float percentToMigrate = (1 - CellMigrationValue) * Cell.GetNextLocalRandomFloat ();

		if (targetCell == Cell)
			return;

		if (targetCell == null)
			return;
		
		float cellSurvivability = 0;
		float cellForagingCapacity = 0;
		
		CalculateAdaptionToCell (targetCell, out cellForagingCapacity, out cellSurvivability);

		float cellAltitudeDeltaFactor = CalculateAltitudeDeltaMigrationFactor (targetCell);

		float travelFactor = 
			cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
			cellSurvivability * cellSurvivability *  targetCell.Accessibility;

		travelFactor = Mathf.Clamp (travelFactor, 0.0001f, 1);

		if (cellSurvivability <= 0)
			return;

		int travelTime = (int)Mathf.Ceil(TravelTimeFactor / travelFactor);
		
		int nextDate = World.CurrentDate + travelTime;

		MigratingGroup migratingGroup = new MigratingGroup (World, percentToMigrate, this, targetCell);
		
		World.InsertEventToHappen (new MigrateGroupEvent (World, nextDate, travelTime, migratingGroup));

		HasMigrationEvent = true;
	}

	public void Destroy () {

		Cell.Group = null;
		World.RemoveGroup (this);

		Neighbors.ForEach (g => RemoveNeighbor (this));

		StillPresent = false;
	}

	private void UpdateInternal () {
		
		int timeSpan = World.CurrentDate - LastUpdateDate;

		if (timeSpan <= 0)
			return;

		UpdatePopulation (timeSpan);
		UpdateCulture (timeSpan);
		
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

//	private void AbsorbKnowledgeFromNeighbors () {
//	
//		Neighbors.ForEach (g => AbsorbKnowledgeFrom (g));
//	}

	public void AbsorbKnowledgeFrom (CellGroup group) {
		
		float populationFactor = Mathf.Min (1, group.Population / (float)Population);
		
		group.Culture.Knowledges.ForEach (k => {
			
			Culture.TransferKnowledge (k, populationFactor);
		});

		group.Culture.Discoveries.ForEach (d => {
			
			Culture.TransferDiscovery (d);
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

		float randomFactor = Cell.GetNextLocalRandomFloat ();

		float migrationFactor = CellMigrationValue;

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

		Cell = World.GetCell (CellLongitude, CellLatitude);

		Cell.Group = this;

		Neighbors = Cell.Neighbors.FindAll (c => c.Group != null).Process (c => c.Group);
		
		Neighbors.ForEach (g => g.AddNeighbor (this));
		
		World.UpdateMostPopulousGroup (this);

		Culture.Group = this;
		Culture.FinalizeLoad ();
	}
}
