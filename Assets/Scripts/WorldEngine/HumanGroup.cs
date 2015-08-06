using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class WorldEvent {


}

public class HumanGroup {
	
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
}
