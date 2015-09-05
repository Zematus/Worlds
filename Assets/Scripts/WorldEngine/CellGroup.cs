using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroup : HumanGroup {

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float NaturalChangeFactor = 1 - CellGroup.NaturalBirthRate + CellGroup.NaturalDeathRate;
	
	[XmlAttribute]
	public int Id;
	
	[XmlAttribute]
	public bool StillPresent = true;
	
	[XmlAttribute]
	public float Stress = 0;
	
	[XmlIgnore]
	public TerrainCell Cell;

	[XmlIgnore]
	public static float InitialPopulationFactor = NaturalChangeFactor / Biome.Grassland.ForagingCapacity;

	public CellGroup () {
	}
	
	public CellGroup (MigratingGroup migratingGroup) : this(migratingGroup.World, migratingGroup.TargetCell, migratingGroup.Population) {

	}

	public CellGroup (World world, TerrainCell cell, int initialPopulation) : base(world, initialPopulation) {

		Cell = cell;

		Cell.Groups.Add (this);

		Id = World.GenerateGroupId();
		
		int nextDate = World.CurrentDate + 20 + Cell.GetNextLocalRandomInt (40);
		
		World.InsertEventToHappen (new UpdateGroupEvent (World, nextDate, this));
		
		World.UpdateMostPopulousGroup (this);
	}

	public void MergeGroup (MigratingGroup group) {
	
		Population += group.Population;
		
		World.UpdateMostPopulousGroup (this);
	}

	public void Update () {

		ModifyPopulation ();

		if (Population <= 0) {
			World.AddGroupToRemove (this);
			return;
		}

//		//float areaFactor = 1 - Cell.Area / TerrainCell.MaxArea;
//
//		float nomadismPreferenceFactor = 0.1f;
//		float stressFactor = Stress * 1;
//
//		//float chanceOfMigration = Mathf.Clamp (nomadismPreferenceFactor + Stress + areaFactor, 0, 1);
//		float chanceOfMigration = Mathf.Clamp (nomadismPreferenceFactor + stressFactor, 0, 1);
//
//		float migrationRoll = Cell.GetNextLocalRandomFloat ();
//
//		if (migrationRoll < chanceOfMigration) {

		PerformMigration();
		
		if (Population <= 0) {
			World.AddGroupToRemove (this);
			return;
		}
//		}

		int nextDate = World.CurrentDate + 20 + Cell.GetNextLocalRandomInt (40);

		World.InsertEventToHappen (new UpdateGroupEvent (World, nextDate, this));
		
		World.UpdateMostPopulousGroup (this);
	}

	public void PerformMigration () {

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

		Population -= popToMigrate;

		World.AddMigratingGroup (new MigratingGroup (World, popToMigrate, targetCell));
	}

	public void Destroy () {

		Cell.Groups.Remove (this);
		World.RemoveGroup (this);

		StillPresent = false;
	}

	public float GetProductionFactor () {

		if (Population == 0)
			return 0;

		float factor = Cell.MaxForage * Cell.ForagingCapacity / Population;

		return factor;
	}
	
	public float GetPopulationChangeRate () {

		float productionFactor = GetProductionFactor ();

		float survivabilityFactor = Cell.Survivability * (productionFactor / NaturalChangeFactor);

		float survivabilityDeathRate = Mathf.Max(0, 1 - survivabilityFactor);

		float starvationDeathRate = Mathf.Max(0, 1 - productionFactor); // productionFactor should be 1 - (NaturalBirthRate - NaturalDeathRate)

		float unnaturalDeathRate = starvationDeathRate + survivabilityDeathRate;
		
		Stress = Mathf.Clamp(unnaturalDeathRate, 0, 1);
		
		// Death rate can't be lower than the natural death rate
		return NaturalBirthRate - NaturalDeathRate - unnaturalDeathRate;
	}

	public void ModifyPopulation () {

		float popChangeRate = GetPopulationChangeRate ();

		float changeRate = Mathf.Max(MinChangeRate, popChangeRate);

		Population += Mathf.CeilToInt(Population * changeRate);
	}
}
