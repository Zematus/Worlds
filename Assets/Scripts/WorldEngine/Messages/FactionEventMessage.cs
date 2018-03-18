using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class FactionEventMessage : CellEventMessage {

	[XmlAttribute]
	public long FactionId;

	[XmlIgnore]
	public Faction Faction {
		get { return World.GetFaction (FactionId); }
	}

	public FactionEventMessage () {

	}

	public FactionEventMessage (Faction faction, long id, long date) : base (faction.CoreGroup.Cell, id, date) {

		FactionId = faction.Id;
	}
}
