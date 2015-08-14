using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class WorldEvent {

}

public class HumanGroup {

	public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
	public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float MinChangeRate = -0.99f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
	public const float NaturalChangeFactor = 1 - HumanGroup.NaturalBirthRate + HumanGroup.NaturalDeathRate;
	
	[XmlAttribute]
	public int Id;
	
	[XmlAttribute]
	public int Population;
	
	[XmlIgnore]
	public TerrainCell Cell;
	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public static float InitialPopulationFactor = NaturalChangeFactor / Biome.Grassland.ForagingCapacity;

	public HumanGroup () {
	}

	public HumanGroup (World world, TerrainCell cell, int id, int initialPopulation) {

		World = world;
		Cell = cell;

		Id = id;
	
		Population = initialPopulation;
	}

	public void Update () {

		ModifyPopulation ();

		if (Population > 0) {
			World.AddGroupToUpdate (this);
		} else {
			World.AddGroupToRemove (this);
		}
	}

	public void Destroy () {

		Cell.HumanGroups.Remove (this);
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
