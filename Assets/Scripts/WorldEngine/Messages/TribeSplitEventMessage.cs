using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribeSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long TribeId;

	[XmlAttribute]
	public long NewTribeId;

	public TribeSplitEventMessage () {

	}

	public TribeSplitEventMessage (Clan splitClan, Tribe tribe, Tribe newTribe, long date) : base (splitClan, WorldEvent.TribeSplitDecisionEventId, date) {

		TribeId = tribe.Id;
		NewTribeId = newTribe.Id;
	}

	protected override string GenerateMessage ()
	{
		Polity tribe = World.GetPolity (TribeId);
		Polity newTribe = World.GetPolity (NewTribeId);

		return "A new tribe, " + newTribe.Name.BoldText + ", formed by clan " + Faction.Name.BoldText + ", has split from the " +  tribe.Name.BoldText + " Tribe";
	}
}
