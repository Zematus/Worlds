using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class Faction : ISynchronizable {

	[XmlAttribute]
	public long Id;

	[XmlAttribute]
	public long GroupId;

	[XmlAttribute]
	public long PolityId;

	public Name Name;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup Group;

	[XmlIgnore]
	public Polity Polity;

	public Faction () {

	}

	public Faction (CellGroup group, Polity polity, Name name) {

		World = group.World;

		Group = group;
		GroupId = group.Id;

		Id = group.GenerateUniqueIdentifier ();

		PolityId = polity.Id;
		Polity = polity;

		Name = name;
	}

	public void Destroy () {
		
		Polity.RemoveFaction (this);
	}

	public void SetGroup (CellGroup group) {

		Group = group;
		GroupId = group.Id;
	}

	public void Update () {

		if (!Group.StillPresent) {
		
			Polity.RemoveFaction (this);

			return;
		}

		UpdateInternal ();
	}

	public abstract void UpdateInternal ();

	public virtual void Synchronize () {

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		Group = World.GetGroup (GroupId);
		Polity = World.GetPolity (PolityId);

		if (Group == null) {
			throw new System.Exception ("Missing Group with Id " + GroupId);
		}

		if (Polity == null) {
			throw new System.Exception ("Missing Polity with Id " + PolityId);
		}
	}
}
