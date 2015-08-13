using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class WorldEvent {

}

public class HumanGroup {

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	
	[XmlAttribute]
	public int Id;
	
	[XmlAttribute]
	public int Population;
	
	[XmlIgnore]
	public TerrainCell Cell;
	[XmlIgnore]
	public World World;

	public HumanGroup () {
	}

	public HumanGroup (World world, TerrainCell cell, int id, int initialPopulation) {

		World = world;
		Cell = cell;

		Id = id;
	
		Population = initialPopulation;
	}

	public void Update () {

		World.AddGroupToUpdate (this);
	}

	public float GetProductionFactor () {

		float baseForage = Population * Cell.ForagingCapacity;

		float factor = Cell.ForagingCapacity * (1 + 2 * Cell.MaxForage) / (1 + Cell.MaxForage + baseForage);

		// Pop * FCapacity * 2 * MaxForage / (MaxForage + Pop * FCapacity)

		return factor;
	}
	
	public float GetPopulationChangeRate () {

		//death rate modifier from resource availability = resources/population

		float productionFactor = GetProductionFactor ();

		float deathRateModifiers = Cell.Survivability * productionFactor;
		
		return NaturalBirthRate - NaturalDeathRate / deathRateModifiers;
	}
}
