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

	public CellCulture Culture;
	
	[XmlIgnore]
	public TerrainCell Cell;
	
	[XmlIgnore]
	public float CellMigrationValue;
	
	[XmlIgnore]
	public bool DebugTagged = false;

	//DEBUG stuff
	private static float _DEBUG_SmallestAltitudeDeltaFactor = float.MaxValue;
	private static float _DEBUG_targetCellAltitude;
	private static float _DEBUG_sourceCellAltitude;
	
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

		InitializeBiomeSurvivalSkills ();
		
		NextUpdateDate = CalculateNextUpdateDate();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (World, NextUpdateDate, this));
		
		World.UpdateMostPopulousGroup (this);
		
		OptimalPopulation = CalculateOptimalPopulation (Cell);
	}

	public void InitializeBiomeSurvivalSkills () {

		foreach (string biomeName in Cell.PresentBiomeNames) {

			Biome biome = Biome.Biomes[biomeName];
		
			string skillId = BiomeSurvivalSkill.GenerateId (biome);

			if (Culture.GetSkill (skillId) == null) {
				
				Culture.AddSkill (new BiomeSurvivalSkill (this, biome, 0.0f));
			}
		}
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

	public void SetupForNextUpdate () {
		
		if (Population < 2) {
			World.AddGroupToRemove (this);
			return;
		}
		
		World.UpdateMostPopulousGroup (this);
		
		OptimalPopulation = CalculateOptimalPopulation (Cell);
		
		ConsiderMigration();
		
		NextUpdateDate = CalculateNextUpdateDate();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (World, NextUpdateDate, this));
	}
	
	private float CalculateAltitudeDeltaMigrationFactor (TerrainCell targetCell) {

		float altitudeModifier = targetCell.Altitude / World.MaxPossibleAltitude;

		float altitudeDeltaModifier = 5 * altitudeModifier;
		float maxAltitudeDelta = Cell.Area / altitudeDeltaModifier;
		float minAltitudeDelta = -Cell.Area / (altitudeDeltaModifier * 5);
		float altitudeDelta = Mathf.Clamp (targetCell.Altitude - Cell.Altitude, minAltitudeDelta, maxAltitudeDelta);
		float altitudeDeltaFactor = 1 - ((altitudeDelta - minAltitudeDelta) / (maxAltitudeDelta - minAltitudeDelta));

		if (altitudeDeltaFactor < _DEBUG_SmallestAltitudeDeltaFactor) {

			_DEBUG_SmallestAltitudeDeltaFactor = altitudeDeltaFactor;

			_DEBUG_targetCellAltitude = targetCell.Altitude;
			_DEBUG_sourceCellAltitude = Cell.Altitude;
		}
		
		return altitudeDeltaFactor;
	}

	public void ConsiderMigration () {

		float score = Cell.GetNextLocalRandomFloat ();

		List<TerrainCell> possibleTargetCells = new List<TerrainCell> (Cell.Neighbors);
		possibleTargetCells.Add (Cell);

		float noMigrationPreference = 20f;

		CellMigrationValue = 0;
		float totalMigrationValue = 0;

		TerrainCell targetCell = MathUtility.WeightedSelection (score, possibleTargetCells, (c) => {

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
		});
		
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
	}

	public void Destroy () {

		Cell.Group = null;
		World.RemoveGroup (this);

		StillPresent = false;
	}

	private void UpdateInternal () {
		
		int timeSpan = World.CurrentDate - LastUpdateDate;

		if (timeSpan <= 0)
			return;

		UpdatePopulation (timeSpan);
		UpdateCulture (timeSpan);
		
		LastUpdateDate = World.CurrentDate;
		
		World.AddUpdatedGroup (this);
	}
	
	private void UpdatePopulation (int timeSpan) {
		
		ExactPopulation = PopulationAfterTime (timeSpan);
	}
	
	private void UpdateCulture (int timeSpan) {
		
		Culture.Update (timeSpan);
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

		float migrationFactor = 0.1f + (CellMigrationValue * 0.9f);

		float skillLevelFactor = (1 + 99 * Culture.MinimumSkillAdaptationLevel ()) / 100f;

		float populationFactor = 1 + Mathf.Abs (OptimalPopulation - Population);

		float mixFactor = randomFactor * migrationFactor * skillLevelFactor * (10000 + OptimalPopulation) / populationFactor;

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
		
		World.UpdateMostPopulousGroup (this);

		Culture.Group = this;
		Culture.FinalizeLoad ();
	}
}
