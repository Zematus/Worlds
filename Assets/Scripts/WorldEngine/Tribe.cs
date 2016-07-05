using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Tribe : Polity {

	public const float BaseCoreInfluence = 0.5f;

	public Tribe () {

	}

	private Tribe (CellGroup coreGroup, float coreGroupInfluence) : base (coreGroup, coreGroupInfluence) {

	}

	public static Tribe GenerateNewTribe (CellGroup coreGroup) {

		float randomValue = coreGroup.Cell.GetNextLocalRandomFloat ();
		float coreInfluence = BaseCoreInfluence + randomValue * (1 - BaseCoreInfluence);

		coreInfluence *= 1 - coreGroup.TotalPolityInfluenceValue;
	
		Tribe newTribe = new Tribe (coreGroup, coreInfluence);

		return newTribe;
	}

	public override void UpdateInternal ()
	{
		RelocateCore ();
	}

	public void RelocateCore () {

		CellGroup mostInfluencedPopGroup = GetGroupWithMostInfluencedPop ();

		SetCoreGroup (mostInfluencedPopGroup);
	}
}
