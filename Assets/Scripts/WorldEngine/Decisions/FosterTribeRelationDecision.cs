using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FosterTribeRelationDecision : PolityDecision {

	public const float BaseMinIsolationPreferencePercentIncrease = 0.05f;
	public const float BaseMaxIsolationPreferencePercentIncrease = 0.15f;

	public const float BaseMinIsolationPreferencePercentDecrease = 0.10f;
	public const float BaseMaxIsolationPreferencePercentDecrease = 0.30f;

	private bool _makeAttempt = true;

	private float _chanceOfRejecting;

	private Tribe _sourceTribe;
	private Tribe _targetTribe;

	public FosterTribeRelationDecision (Tribe sourceTribe, Tribe targetTribe, bool makeAttempt, float chanceOfRejecting, long eventId) : base (sourceTribe, eventId) {

		_sourceTribe = sourceTribe;
		_targetTribe = targetTribe;

		_chanceOfRejecting = chanceOfRejecting;

		Description = targetTribe.GetNameAndTypeStringBold ().FirstLetterToUpper () + " has had a long contact with " + sourceTribe.GetNameAndTypeStringBold () + 
			", but the relationship between them could be improved upon.\n\n" +
			"Should " + sourceTribe.CurrentLeader.Name.BoldText + " attempt to foster our relationship with " + targetTribe.GetNameAndTypeStringBold () + "?";

		_makeAttempt = makeAttempt;
	}

	private string GenerateAvoidFosteringRelationshipResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinIsolationPreferencePercentIncrease, BaseMaxIsolationPreferencePercentIncrease);
	}

	public static void LeaderAvoidsFosteringRelationship (Tribe sourceTribe, Tribe targetTribe) {

		int rngOffset = RngOffsets.FOSTER_TRIBE_RELATION_EVENT_SOURCETRIBE_LEADER_AVOIDS_ATTEMPT_MODIFY_ATTRIBUTE;

		Effect_IncreasePreference (sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinIsolationPreferencePercentIncrease, BaseMaxIsolationPreferencePercentIncrease, rngOffset++);

		Clan sourceDominantClan = sourceTribe.DominantFaction as Clan;

		sourceDominantClan.SetToUpdate ();

		sourceTribe.AddEventMessage (new AvoidFosterRelationshipEventMessage (sourceTribe, targetTribe, sourceTribe.CurrentLeader, sourceTribe.World.CurrentDate));
	}

	private void AvoidFosteringRelationship () {

		LeaderAvoidsFosteringRelationship (_sourceTribe, _targetTribe);
	}

	private string GenerateAttemptFosterRelationshipResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_DecreasePreference (_sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinIsolationPreferencePercentDecrease, BaseMaxIsolationPreferencePercentDecrease) + "\n" +
			"\t• The current leader of " + _targetTribe.GetNameAndTypeStringBold () + " will receive an offer to foster the relationship with " + _sourceTribe.GetNameAndTypeStringBold ();
	}

	public static void LeaderAttemptsFosterRelationship_TriggerRejectDecision (Tribe sourceTribe, Tribe targetTribe, float chanceOfRejecting, long eventId) {

		World world = sourceTribe.World;

		bool acceptOffer = targetTribe.GetNextLocalRandomFloat (RngOffsets.FOSTER_TRIBE_RELATION_EVENT_TARGETTRIBE_LEADER_ACCEPT_OFFER) > chanceOfRejecting;

		Clan targetDominantClan = targetTribe.DominantFaction as Clan;

		if (targetTribe.IsUnderPlayerFocus || targetDominantClan.IsUnderPlayerGuidance) {

			Decision handleOfferDecision;

			handleOfferDecision = new HandleFosterTribeRelationAttemptDecision (sourceTribe, targetTribe, acceptOffer, eventId); // Give player options

			if (targetDominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (handleOfferDecision);

			} else {

				handleOfferDecision.ExecutePreferredOption ();
			}

		} else if (acceptOffer) {

			HandleFosterTribeRelationAttemptDecision.LeaderAcceptsOffer (sourceTribe, targetTribe, eventId);

		} else {

			HandleFosterTribeRelationAttemptDecision.LeaderRejectsOffer (sourceTribe, targetTribe, eventId);
		}
	}

	public static void LeaderAttemptsFosterRelationship (Tribe sourceTribe, Tribe targetTribe, float chanceOfRejecting, long eventId) {

		int rngOffset = RngOffsets.FOSTER_TRIBE_RELATION_EVENT_SOURCETRIBE_LEADER_MAKES_ATTEMPT_MODIFY_ATTRIBUTE;

		Effect_DecreasePreference (sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinIsolationPreferencePercentDecrease, BaseMaxIsolationPreferencePercentDecrease, rngOffset++);

		LeaderAttemptsFosterRelationship_TriggerRejectDecision (sourceTribe, targetTribe, chanceOfRejecting, eventId);
	}

	private void AttemptToFosterRelationship () {

		LeaderAttemptsFosterRelationship (_sourceTribe, _targetTribe, _chanceOfRejecting, _eventId);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Attempt to foster relationship...", "Effects:\n" + GenerateAttemptFosterRelationshipResultEffectsString (), AttemptToFosterRelationship),
			new Option ("Don't waste time with that...", "Effects:\n" + GenerateAvoidFosteringRelationshipResultEffectsString (), AvoidFosteringRelationship)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_makeAttempt)
			AttemptToFosterRelationship ();
		else
			AvoidFosteringRelationship ();
	}
}
