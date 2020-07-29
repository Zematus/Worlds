using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class AvoidMergeTribesAttemptEventMessage : PolityEventMessage
{
    public Identifier AgentId;
    public Identifier SourceTribeId;
    public Identifier TargetTribeId;

    public AvoidMergeTribesAttemptEventMessage()
    {

    }

    public AvoidMergeTribesAttemptEventMessage(
        Tribe sourceTribe, Tribe targetTribe, Agent agent, long date) :
        base(sourceTribe, WorldEvent.AvoidMergeTribesAttemptDecisionEventId, date)
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
            ", has decided not to propose " + targetTribeInfo.GetNameAndTypeStringBold() +
            " merge with " + leader.PossessiveNoun + " tribe";
    }
}
