using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public abstract class HumanGroup {
	
	[XmlAttribute]
	public int Population;
	
	[XmlAttribute]
	public bool IsTagged;

	[XmlIgnore]
	public World World;

	public HumanGroup () {
	}

	public HumanGroup (World world, int population) {

		IsTagged = false;

		World = world;
	
		Population = population;
	}

	public virtual void FinalizeLoad () {

		if (IsTagged) {
		
			World.TagGroup (this);
		}
	}
}
