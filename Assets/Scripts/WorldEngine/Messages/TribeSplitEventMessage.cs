using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribeSplitEventMessage : FactionEventMessage
{
    public Identifier TribeId;
    public Identifier NewTribeId;

    public TribeSplitEventMessage()
    {

    }

    public TribeSplitEventMessage(Clan splitClan, Tribe tribe, Tribe newTribe, long date) :
        base(splitClan, WorldEvent.TribeSplitDecisionEventId, date)
    {
        TribeId = tribe.Id;
        NewTribeId = newTribe.Id;
    }

    protected override string GenerateMessage()
    {
        PolityInfo tribeInfo = World.GetPolityInfo(TribeId);
        PolityInfo newTribeInfo = World.GetPolityInfo(NewTribeId);

        return "A new tribe, " + newTribeInfo.Name.BoldText + ", formed by " +
            FactionInfo.GetNameAndTypeStringBold() + ", has split from " + tribeInfo.GetNameAndTypeStringBold();
    }
}
