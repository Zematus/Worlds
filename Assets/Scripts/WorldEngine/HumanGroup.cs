using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public abstract class HumanGroup {
	
	[XmlAttribute]
	public bool MigrationTagged;

	[XmlIgnore]
	public World World;

	public HumanGroup () {
	}

	public HumanGroup (World world) {

		MigrationTagged = false;

		World = world;
	}

	public virtual void FinalizeLoad () {

		if (MigrationTagged) {
		
			World.MigrationTagGroup (this);
		}
	}
}
