using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class AcceptedFosterTribeRelationDecision : PolityDecision {

	private Tribe _sourceTribe;
	private Tribe _targetTribe;

	public AcceptedFosterTribeRelationDecision (Tribe sourceTribe, Tribe targetTribe, long eventId) : base (sourceTribe, eventId) {

		Description = "The leader of " + targetTribe.GetNameAndTypeStringBold () + ", " + targetTribe.CurrentLeader.Name.BoldText + ", has accepted the offer from " + sourceTribe.GetNameAndTypeStringBold () + 
			" to improve the relationship between the tribes";

		_targetTribe = targetTribe;
		_sourceTribe = sourceTribe;
	}

	private string GenerateAcceptedOfferResultEffectsString () {

		return 
			"\t• " + GenerateResultEffectsString_IncreaseRelationship (_targetTribe, _sourceTribe) + "\n" + 
			"\t• " + GenerateResultEffectsString_DecreasePreference (_targetTribe, CulturalPreference.IsolationPreferenceId);
	}

	public static void TargetTribeAcceptedOffer (Tribe sourceTribe, Tribe targetTribe) {

		sourceTribe.DominantFaction.SetToUpdate ();
		targetTribe.DominantFaction.SetToUpdate ();

		WorldEventMessage message = new AcceptedFosterRelationshipAttemptEventMessage (sourceTribe, targetTribe, targetTribe.CurrentLeader, sourceTribe.World.CurrentDate);

		sourceTribe.AddEventMessage (message);
		targetTribe.AddEventMessage (message);
	}

	private void AcceptedOffer () {

		TargetTribeAcceptedOffer (_sourceTribe, _targetTribe);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Of course they would!", "Effects:\n" + GenerateAcceptedOfferResultEffectsString (), AcceptedOffer)
		};
	}

	public override void ExecutePreferredOption ()
	{
		AcceptedOffer ();
	}
}
	