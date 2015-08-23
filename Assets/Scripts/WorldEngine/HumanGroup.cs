using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public abstract class HumanGroup {
	
	[XmlAttribute]
	public int Population;

	[XmlIgnore]
	public World World;

	public HumanGroup () {
	}

	public HumanGroup (World world, int population) {

		World = world;
	
		Population = population;
	}
}
