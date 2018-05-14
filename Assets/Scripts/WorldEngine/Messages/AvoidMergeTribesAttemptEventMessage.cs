using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class AvoidMergeTribesAttemptEventMessage : PolityEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long SourceTribeId;

	[XmlAttribute]
	public long TargetTribeId;

	public AvoidMergeTribesAttemptEventMessage () {

	}

	public AvoidMergeTribesAttemptEventMessage (Tribe sourceTribe, Tribe targetTribe, Agent agent, long date) : base (sourceTribe, WorldEvent.AvoidMergeTribesAttemptDecisionEventId, date) {

		sourceTribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		SourceTribeId = sourceTribe.Id;
		TargetTribeId = targetTribe.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);
		Tribe sourceTribe = World.GetPolity (SourceTribeId) as Tribe;
		Tribe targetTribe = World.GetPolity (TargetTribeId) as Tribe;

		return leader.Name.BoldText + ", leader of " + sourceTribe.GetNameAndTypeStringBold () + ", has decided not to propose " + 
			targetTribe.GetNameAndTypeStringBold () + " merge with " + leader.PossessiveNoun + " tribe";
	}
}
