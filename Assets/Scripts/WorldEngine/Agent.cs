using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class Agent : ISynchronizable {

	public const int MaxLifespan = 113; // Prime number to hide birthdate cycle artifacts

	[XmlAttribute("Type")]
	public string Type;

	[XmlAttribute]
	public long Id;

	[XmlAttribute("Birth")]
	public int BirthDate;

	[XmlAttribute("GrpId")]
	public long GroupId;

	[XmlAttribute("StilPres")]
	public bool StillPresent = true;

	[XmlAttribute("IterOff")]
	public int IterationOffset;

	public Name Name = null;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup Group;

	public Agent () {

	}

	public Agent (string type, CellGroup group, int birthDate, int iterationOffset = 0) {

		Type = type;

		World = group.World;

		GroupId = group.Id;
		Group = group;

		BirthDate = birthDate;

		IterationOffset = iterationOffset;

		GenerateName ();
	}

	public void Destroy () {

		StillPresent = false;
	}

	protected abstract void GenerateName ();

	public void Update () {

		if (!Group.StillPresent) {

			return;
		}

		UpdateInternal ();
	}

	protected abstract void UpdateInternal ();

	public virtual void Synchronize () {

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		Group = World.GetGroup (GroupId);

		if (Group == null) {
			throw new System.Exception ("Missing Polity with Id " + GroupId);
		}
	}
}