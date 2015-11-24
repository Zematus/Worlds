using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public interface IGroupAction {

	void Perform ();
}

public class KnowledgeTransferAction : IGroupAction {

	public const float BaseTransferFactor = 0.1f;

	public CellGroup SourceGroup;
	public CellGroup TargetGroup;

	public KnowledgeTransferAction (CellGroup sourceGroup, CellGroup targetGroup) {
	
		SourceGroup = sourceGroup;
		TargetGroup = targetGroup;
	}

	public void Perform () {

		float populationFactor = Mathf.Min (1, SourceGroup.Population / TargetGroup.Population);

		foreach (CulturalKnowledge sourceKnowledge in SourceGroup.Culture.Knowledges) {
			
			CulturalKnowledge targetKnowledge = TargetGroup.Culture.GetKnowledge (sourceKnowledge.Id);
			
			if (targetKnowledge == null) {

				targetKnowledge = sourceKnowledge.CopyWithGroup (SourceGroup);
				targetKnowledge.Value = 0;

				TargetGroup.Culture.AddKnowledgeToLearn (targetKnowledge);
			}

			float transferValueRate = targetKnowledge.Value / sourceKnowledge.Value;

			if (transferValueRate >= 1) continue;

			float transferFactor = BaseTransferFactor * (1 - transferValueRate);

			targetKnowledge.IncreaseValue (targetKnowledge.Value, transferFactor * populationFactor);
		}
	}
}
