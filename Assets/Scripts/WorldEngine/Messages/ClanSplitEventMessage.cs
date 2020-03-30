using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System;

[Obsolete]
public class ClanSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long OldClanId;

    public ClanSplitEventMessage()
    {

    }

    public ClanSplitEventMessage(Clan oldClan, Clan newClan, long date) : base(newClan, WorldEvent.ClanSplitDecisionEventId, date)
    {
        OldClanId = oldClan.Id;
    }

    protected override string GenerateMessage()
    {
        FactionInfo oldClanInfo = World.GetFactionInfo(OldClanId);

        return "A new clan, " + FactionInfo.Name.BoldText + ", has split from " + oldClanInfo.GetNameAndTypeStringBold();
    }
}
