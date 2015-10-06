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
	
	public const float PopulationConstant = 10;

	public const float TravelTimeFactor = 1;
	
	[XmlAttribute]
	public int Population;
	
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
	
	[XmlIgnore]
	public TerrainCell Cell;

	public CellGroup () {
	}
	
	public CellGroup (MigratingGroup migratingGroup, int splitPopulation) : this(migratingGroup.World, migratingGroup.TargetCell, splitPopulation) {

	}

	public CellGroup (World world, TerrainCell cell, int initialPopulation) : base(world) {

		Population = initialPopulation;
		
		LastUpdateDate = World.CurrentDate;

		Cell = cell;

		Cell.Groups.Add (this);

		Id = World.GenerateCellGroupId();
		
		NextUpdateDate = CalculateNextUpdateDate();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (World, NextUpdateDate, this));
		
		World.UpdateMostPopulousGroup (this);
		
		CalculateOptimalPopulation ();
	}

	public void MergeGroup (MigratingGroup group, int splitPopulation) {
		
		UpdatePopulation ();
	
		Population += splitPopulation;
		
		if (Population <= 0) {
			throw new System.Exception ("Population after migration merge shouldn't be 0 or less.");
		}
		
		SetupForNextUpdate ();
	}
	
	public int SplitGroup (MigratingGroup group) {
		
		UpdatePopulation ();

		int splitPopulation = (int)Mathf.Floor(Population * group.PercentPopulation);
		
		Population -= splitPopulation;
		
		SetupForNextUpdate ();

		return splitPopulation;
	}

	public void Update () {

		UpdatePopulation ();

		SetupForNextUpdate ();
	}

	public void SetupForNextUpdate () {
		
		if (Population <= 0) {
			World.AddGroupToRemove (this);
			return;
		}
		
		World.UpdateMostPopulousGroup (this);
		
		CalculateOptimalPopulation ();
		
		ConsiderMigration();
		
		//		if (IsTagged) {
		//		
		//			bool debug = true;
		//		}
		
		NextUpdateDate = CalculateNextUpdateDate();
		
		World.InsertEventToHappen (new UpdateCellGroupEvent (World, NextUpdateDate, this));
	}

	public void ConsiderMigration () {

		float percentToMigrate = 0.5f * Cell.GetNextLocalRandomFloat ();

		float score = Cell.GetNextLocalRandomFloat ();

		List<TerrainCell> possibleTargetCells = new List<TerrainCell> (Cell.Neighbors);
		possibleTargetCells.Add (Cell);

		float noMigrationPreference = 3f;

		TerrainCell targetCell = MathUtility.WeightedSelection (score, possibleTargetCells, (c) => {

			float areaFactor = Cell.Area / TerrainCell.MaxArea;
			float altitudeFactor = 1 - (c.Altitude / World.MaxPossibleAltitude);

			float stressFactor = 1 - c.CalculatePopulationStress();
			stressFactor *= stressFactor;
			stressFactor *= stressFactor;

			float survabilityFactor = c.Survivability; 

			float cellValue = survabilityFactor * altitudeFactor * areaFactor * stressFactor;

			if (c == Cell) {
				cellValue *= noMigrationPreference;
				cellValue += noMigrationPreference;
			}

			return cellValue;
		});

		if (targetCell == Cell)
			return;

		if (targetCell == null)
			return;

		if (targetCell.Survivability <= 0)
			return;

		//float survivabilityFactor = 0.75f + (targetCell.Survivability)/4f;
		float survivabilityFactor = targetCell.Survivability;

		int travelTime = (int)Mathf.Ceil(TravelTimeFactor / survivabilityFactor);
		
		int nextDate = World.CurrentDate + travelTime;

		MigratingGroup migratingGroup = new MigratingGroup (World, percentToMigrate, this, targetCell);
		
		World.InsertEventToHappen (new MigrateGroupEvent (World, nextDate, travelTime, migratingGroup));
	}

	public void Destroy () {

		Cell.Groups.Remove (this);
		World.RemoveGroup (this);

		StillPresent = false;
	}

	public void UpdatePopulation () {

		int dateSpan = World.CurrentDate - LastUpdateDate;

		PopulationAfterTime (dateSpan);
		
		LastUpdateDate = World.CurrentDate;
	}

	public int CalculateOptimalPopulation () {

		float populationCapacityFactor = PopulationConstant * Cell.Area * Cell.ForagingCapacity * Cell.Survivability;

		OptimalPopulation = (int)Mathf.Floor (populationCapacityFactor);

		return OptimalPopulation;
	}

	public int CalculateNextUpdateDate () {

		int populationFactor = 1 + Mathf.Abs (OptimalPopulation - Population);

		populationFactor = (int)Mathf.Max((1000 + OptimalPopulation) / (float)populationFactor, 1);

		return World.CurrentDate + GenerationSpan * populationFactor;
	}

	public int PopulationAfterTime (int time) { // in years

//		if (Cell.IsObserved) {
//		
//			bool debug = true;
//		}
		
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
