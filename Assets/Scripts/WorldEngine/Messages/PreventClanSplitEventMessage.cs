using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[System.Obsolete]
public class PreventClanSplitEventMessage : FactionEventMessage
{
    public Identifier AgentId;

    public PreventClanSplitEventMessage()
    {

    }

    public PreventClanSplitEventMessage(Faction faction, Agent agent, long date) :
        base(faction, WorldEvent.PreventClanSplitEventId, date)
    {
        faction.World.AddMemorableAgent(agent);

        AgentId = agent.Id;
    }

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);

        return leader.Name.BoldText + " has prevented clan " +
            FactionInfo.Name.BoldText + " from splitting";
    }
}
