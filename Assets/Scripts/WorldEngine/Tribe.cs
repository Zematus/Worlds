using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Tribe : Polity {

	public const float BaseCoreInfluence = 0.5f;

	public Tribe () : base () {

	}

	private Tribe (CellGroup coreGroup, float coreGroupInfluence) : base (coreGroup, coreGroupInfluence) {

	}

	public static Tribe GenerateNewTribe (CellGroup coreGroup) {

		float randomValue = coreGroup.Cell.GetNextLocalRandomFloat ();
		float coreInfluence = BaseCoreInfluence + randomValue * (1 - BaseCoreInfluence);
	
		Tribe newTribe = new Tribe (coreGroup, coreInfluence);

		return newTribe;
	}

	public override float MigrationValue (TerrainCell targetCell, float sourceRelativeInfluence)
	{
		sourceRelativeInfluence = Mathf.Max (sourceRelativeInfluence, 0);

		CellGroup targetGroup = targetCell.Group;

		float groupRelativeInfluence = 0.0001f;

		if (targetGroup != null) {

			groupRelativeInfluence = Mathf.Max (targetGroup.GetRelativePolityInfluence (this), groupRelativeInfluence);
		}

		float influenceFactor = sourceRelativeInfluence / (groupRelativeInfluence + sourceRelativeInfluence);

		return Mathf.Clamp01 (influenceFactor);
	}

	public override void MergingEffects (CellGroup targetGroup, float sourceInfluence, float percentOfTarget) {

		float currentInfluence = targetGroup.GetPolityInfluence (this);

		float newInfluence = (currentInfluence * (1 - percentOfTarget)) + (sourceInfluence * percentOfTarget);

		targetGroup.SetPolityInfluence (this, newInfluence);
	}

	public override void UpdateEffects (CellGroup group, float influence, int timeSpan) {
		
	}
}
