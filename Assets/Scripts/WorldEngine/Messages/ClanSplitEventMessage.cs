using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long OldClanId;

	public ClanSplitEventMessage () {

	}

	public ClanSplitEventMessage (Clan oldClan, Clan newClan, long date) : base (newClan, WorldEvent.ClanSplitDecisionEventId, date) {

		OldClanId = oldClan.Id;
	}

	protected override string GenerateMessage ()
	{
		Faction oldClan = World.GetFaction (OldClanId);

		return "A new clan, " + Faction.Name.BoldText + ", has split from clan " +  oldClan.Name.BoldText;
	}
}
