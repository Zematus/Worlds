using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class CellGroup : HumanGroup {

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float MinChangeRate = -0.99f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float NaturalChangeFactor = 1 - CellGroup.NaturalBirthRate + CellGroup.NaturalDeathRate;
	
	[XmlAttribute]
	public int Id;
	
	[XmlAttribute]
	public bool StillPresent = true;
	
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

		float chanceOfMigration = 0.5f;

		float migrationRoll = Cell.GetNextLocalRandomFloat ();

		if (migrationRoll < chanceOfMigration) {

			PerformMigration();
		}

		int nextDate = World.CurrentDate + 20 + Cell.GetNextLocalRandomInt (40);

		World.InsertEventToHappen (new UpdateGroupEvent (World, nextDate, this));
		
		World.UpdateMostPopulousGroup (this);
	}

	public void PerformMigration () {

		float percentToMigrate = 0.5f * Cell.GetNextLocalRandomFloat ();
	
		int popToMigrate = (int)(percentToMigrate * Population);

		if (popToMigrate < 1)
			return;

		int index = Cell.GetNextLocalRandomInt (Cell.Neighbors.Count);
		
		TerrainCell targetCell = Cell.Neighbors[index];

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
		
		// Death rate can't be lower than the natural death rate
		return NaturalBirthRate - NaturalDeathRate - starvationDeathRate - survivabilityDeathRate;
	}

	public void ModifyPopulation () {

		float changeRate = Mathf.Max(MinChangeRate, GetPopulationChangeRate ());

		Population += (int)Mathf.Floor(Population * changeRate);
	}
}
