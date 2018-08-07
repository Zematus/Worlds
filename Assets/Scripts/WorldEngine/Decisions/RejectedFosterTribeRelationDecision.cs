using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class RejectedFosterTribeRelationDecision : PolityDecision {

	private Tribe _sourceTribe;
	private Tribe _targetTribe;

	public RejectedFosterTribeRelationDecision (Tribe sourceTribe, Tribe targetTribe, long eventId) : base (sourceTribe, eventId) {

		Description = "The leader of " + targetTribe.GetNameAndTypeStringBold () + ", " + targetTribe.CurrentLeader.Name.BoldText + ", has rejected the attempt from " + sourceTribe.GetNameAndTypeStringBold () + 
			" to improve the relationship between the tribes";

		_targetTribe = targetTribe;
		_sourceTribe = sourceTribe;
	}

	private string GenerateRejectedOfferResultEffectsString () {

		return 
			"\t• " + GenerateResultEffectsString_DecreaseRelationship (_targetTribe, _sourceTribe) + "\n" + 
			"\t• " + GenerateResultEffectsString_IncreasePreference (_targetTribe, CulturalPreference.IsolationPreferenceId);
	}

	public static void TargetTribeRejectedOffer (Tribe sourceTribe, Tribe targetTribe) {

		sourceTribe.DominantFaction.SetToUpdate ();
		targetTribe.DominantFaction.SetToUpdate ();

		WorldEventMessage message = new RejectedFosterRelationshipAttemptEventMessage (sourceTribe, targetTribe, targetTribe.CurrentLeader, sourceTribe.World.CurrentDate);

		sourceTribe.AddEventMessage (message);
		targetTribe.AddEventMessage (message);
	}

	private void RejectedOffer () {

		TargetTribeRejectedOffer (_sourceTribe, _targetTribe);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Oh well...", "Effects:\n" + GenerateRejectedOfferResultEffectsString (), RejectedOffer)
		};
	}

	public override void ExecutePreferredOption ()
	{
		RejectedOffer ();
	}
}
	