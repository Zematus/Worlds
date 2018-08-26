using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PreventTribeSplitEventMessage : PolityEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long SplitClanId;

	public PreventTribeSplitEventMessage()
    {

    }

    public PreventTribeSplitEventMessage(Tribe tribe, Clan splitClan, Agent agent, long date) : base(tribe, WorldEvent.PreventTribeSplitEventId, date)
    {
        tribe.World.AddMemorableAgent(agent);

        AgentId = agent.Id;
        SplitClanId = splitClan.Id;
    }

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        FactionInfo splitClan = World.GetFactionInfo(SplitClanId);

        return leader.Name.BoldText + ", leader of " + PolityInfo.GetNameAndTypeStringBold() + ", has prevented " + splitClan.GetNameAndTypeString() + " from leaving the tribe";
    }
}
