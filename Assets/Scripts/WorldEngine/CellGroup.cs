using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroup : HumanGroup {

	public const int GenerationSpan = 20;

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)

	public const float NaturalGrowthRate = NaturalBirthRate - NaturalDeathRate;
	
	public const float PopulationConstant = 100;

//	public const float NaturalChangeFactor = 1 - CellGroup.NaturalBirthRate + CellGroup.NaturalDeathRate;
	
	[XmlAttribute]
	public int Id;
	
	[XmlAttribute]
	public bool StillPresent = true;
	
	[XmlAttribute]
	public float Stress = 0;
	
	[XmlAttribute]
	public int LastUpdateDate;
	
//	[XmlIgnore]
//	public static float InitialPopulationFactor = NaturalChangeFactor / Biome.Grassland.ForagingCapacity;
	
	[XmlIgnore]
	public TerrainCell Cell;
	
	[XmlIgnore]
	public int OptimalPopulation;

	public CellGroup () {
	}
	
	public CellGroup (MigratingGroup migratingGroup) : this(migratingGroup.World, migratingGroup.TargetCell, migratingGroup.Population) {

		LastUpdateDate = World.CurrentDate;

		CalculateOptimalPopulation ();
	}

	public CellGroup (World world, TerrainCell cell, int initialPopulation) : base(world, initialPopulation) {
		
		LastUpdateDate = World.CurrentDate;

		Cell = cell;

		Cell.Groups.Add (this);

		Id = World.GenerateCellGroupId();
		
		int nextDate = World.CurrentDate + GenerationSpan;
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (World, nextDate, this));
		
		World.UpdateMostPopulousGroup (this);
		
		CalculateOptimalPopulation ();
	}

	public void MergeGroup (MigratingGroup group) {
	
		Population += group.Population;
		
		World.UpdateMostPopulousGroup (this);
		
		CalculateOptimalPopulation ();
	}

	public void Update () {

		ModifyPopulation ();

		if (Population <= 0) {
			World.AddGroupToRemove (this);
			return;
		}

		ConsiderMigration();

		int nextDate = World.CurrentDate + GenerationSpan;

//		if (IsTagged) {
//		
//			bool debug = true;
//		}

		World.InsertEventToHappen (new UpdateCellGroupEvent (World, nextDate, this));
		
		World.UpdateMostPopulousGroup (this);
	}

	public void ConsiderMigration () {

		float percentToMigrate = 0.1f * Cell.GetNextLocalRandomFloat ();
	
		int popToMigrate = (int)(percentToMigrate * Population);

		if (popToMigrate < 1)
			return;

		float score = Cell.GetNextLocalRandomFloat ();

		List<TerrainCell> possibleTargetCells = new List<TerrainCell> (Cell.Neighbors);
		possibleTargetCells.Add (Cell);

		float noMigrationPreference = 5;

		TerrainCell targetCell = MathUtility.WeightedSelection (score, possibleTargetCells, (x) => {

			float areaFactor = Cell.Area / TerrainCell.MaxArea;
			float altitudeFactor = 3 * (1 - (x.Altitude / World.MaxPossibleAltitude));
			float stressFactor = 5 * (1 - x.GetStress());
			float survabilityFactor = x.Survivability; 

			float cellValue = survabilityFactor * stressFactor * altitudeFactor * areaFactor;

			if (x == Cell) cellValue *= noMigrationPreference;

			return cellValue;
		});

		if (targetCell == Cell)
			return;

		if (targetCell == null)
			return;
		
		int nextDate = World.CurrentDate + targetCell.GetTravelTime();

		MigratingGroup migratingGroup = new MigratingGroup (World, popToMigrate, this, targetCell);
		
		World.InsertEventToHappen (new MigrateGroupEvent (World, nextDate, migratingGroup));
	}

	public void Destroy () {

		Cell.Groups.Remove (this);
		World.RemoveGroup (this);

		StillPresent = false;
	}

//	public float GetProductionFactor () {
//
//		if (Population == 0)
//			return 0;
//
//		float factor = Cell.MaxForage * Cell.ForagingCapacity / Population;
//
//		return factor;
//	}
//	
//	public float GetPopulationChangeRate () {
//
//		float productionFactor = GetProductionFactor ();
//
//		float survivabilityFactor = Cell.Survivability * (productionFactor / NaturalChangeFactor);
//
//		float survivabilityDeathRate = Mathf.Max(0, 1 - survivabilityFactor);
//
//		float starvationDeathRate = Mathf.Max(0, 1 - productionFactor); // productionFactor should be 1 - (NaturalBirthRate - NaturalDeathRate)
//
//		float unnaturalDeathRate = starvationDeathRate + survivabilityDeathRate;
//		
//		Stress = Mathf.Clamp(unnaturalDeathRate, 0, 1);
//		
//		// Death rate can't be lower than the natural death rate
//		return NaturalBirthRate - NaturalDeathRate - unnaturalDeathRate;
//	}

	public void ModifyPopulation () {

		int dateSpan = World.CurrentDate - LastUpdateDate;

		PopulationAfterTime (dateSpan);
		
//		OptimalPopulation = CalculateOptimalPopulation ();
//
//		float popChangeRate = GetPopulationChangeRate ();
//
//		float changeRate = Mathf.Max(MinChangeRate, popChangeRate);
//
//		Population += Mathf.CeilToInt(Population * changeRate);
	}

	public int CalculateOptimalPopulation () {

		float populationCapacityFactor = PopulationConstant * Cell.Area * Cell.ForagingCapacity * Cell.Survivability;

		OptimalPopulation = (int)Mathf.Floor (populationCapacityFactor);

//		float survivabilityFactor = Cell.Survivability * Cell.MaxForage * Cell.ForagingCapacity;
//
//		float changeRateFactor = 1 + NaturalDeathRate - NaturalBirthRate;
//
//		int optimalPopulation = (int)Mathf.Floor (survivabilityFactor / changeRateFactor);

		return OptimalPopulation;
	}

	public int PopulationAfterTime (int time) { // in years

		if (Population == OptimalPopulation)
			return Population;
		
		float timeFactor = NaturalGrowthRate * time / (float)GenerationSpan;

		if (Population < OptimalPopulation) {
			
			float geometricTimeFactor = Mathf.Pow(2, timeFactor);
			float populationFactor = 1 - Population/(float)OptimalPopulation;

			Population = (int)Mathf.Floor(OptimalPopulation * (1 - Mathf.Pow(populationFactor, geometricTimeFactor)));

			return Population;
		}

		if (Population > OptimalPopulation) {

			Population = (int)Mathf.Floor(OptimalPopulation + (Population - OptimalPopulation) * Mathf.Exp (-timeFactor));
			
			return Population;
		}

		return 0;
	}
}
