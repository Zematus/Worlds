using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class AvoidFosterRelationshipEventMessage : PolityEventMessage
{
    public Identifier AgentId;
    public Identifier SourceTribeId;
    public Identifier TargetTribeId;

    public AvoidFosterRelationshipEventMessage()
    {

    }

    public AvoidFosterRelationshipEventMessage(
        Tribe sourceTribe, Tribe targetTribe, Agent agent, long date) :
        base(sourceTribe, WorldEvent.AvoidFosterTribeRelationDecisionEventId, date)
    {
        sourceTribe.World.AddMemorableAgent(agent);

        AgentId = agent.Id;
        SourceTribeId = sourceTribe.Id;
        TargetTribeId = targetTribe.Id;
    }

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(SourceTribeId);
        PolityInfo targetTribeInfo = World.GetPolityInfo(TargetTribeId);

        return leader.Name.BoldText + ", leader of " + sourceTribeInfo.GetNameAndTypeStringBold() +
            ", has avoided fostering the relationship with " + targetTribeInfo.GetNameAndTypeStringBold();
    }
}
