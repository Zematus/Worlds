using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public interface IGroupAction {

	void Perform ();
}

public class KnowledgeTransferAction : IGroupAction {

	public CellGroup SourceGroup;
	public CellGroup TargetGroup;

	public KnowledgeTransferAction (CellGroup sourceGroup, CellGroup targetGroup) {
	
		SourceGroup = sourceGroup;
		TargetGroup = targetGroup;
	}

	public void Perform () {

		float populationFactor = Mathf.Min (1, SourceGroup.Population / (float)TargetGroup.Population);

		foreach (CulturalKnowledge sourceKnowledge in SourceGroup.Culture.Knowledges) {
			
			TargetGroup.Culture.TransferKnowledge (sourceKnowledge, populationFactor);
		}
	}
}
