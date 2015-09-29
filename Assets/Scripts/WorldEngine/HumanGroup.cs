using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public abstract class HumanGroup {
	
	[XmlAttribute]
	public bool IsTagged;

	[XmlIgnore]
	public World World;

	public HumanGroup () {
	}

	public HumanGroup (World world) {

		IsTagged = false;

		World = world;
	}

	public virtual void FinalizeLoad () {

		if (IsTagged) {
		
			World.TagGroup (this);
		}
	}
}
