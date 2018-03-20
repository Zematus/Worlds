using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PolityFormationEventMessage : CellEventMessage {

	[XmlAttribute]
	public bool First = false;

	[XmlAttribute]
	public long PolityId;

	public PolityFormationEventMessage () {

	}

	public PolityFormationEventMessage (Polity polity, long date) : base (polity.CoreGroup.Cell, WorldEvent.PolityFormationEventId, date) {

		PolityId = polity.Id;
	}

	protected override string GenerateMessage ()
	{
		Polity polity = World.GetPolity (PolityId);

		if (First) {
			return "The first polity, " + polity.Name.BoldText + ", formed at " + Position;
		} else {
			return "A new polity, " + polity.Name.BoldText + ", formed at " + Position;
		}
	}
}
