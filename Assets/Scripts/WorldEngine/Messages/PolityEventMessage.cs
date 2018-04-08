using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class PolityEventMessage : WorldEventMessage {

	[XmlAttribute]
	public long PolityId;

	[XmlIgnore]
	public Polity Polity {
		get { return World.GetPolity (PolityId); }
	}

	public PolityEventMessage () {

	}

	public PolityEventMessage (Polity polity, long id, long date) : base (polity.World, id, date) {

		PolityId = polity.Id;
	}
}
