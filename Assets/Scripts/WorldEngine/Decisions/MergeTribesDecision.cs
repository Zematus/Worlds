using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class MergeTribesDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.05f;
	public const float BaseMaxPreferencePercentChange = 0.15f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

//	public const float BaseMinInfluencePercentChange = 0.05f;
//	public const float BaseMaxInfluencePercentChange = 0.15f;

	private bool _makeAttempt = true;

	private float _chanceOfRejecting;

	private Tribe _sourceTribe;
	private Tribe _targetTribe;

	public MergeTribesDecision (Tribe sourceTribe, Tribe targetTribe, bool makeAttempt, float chanceOfRejecting) : base (sourceTribe) {

		_sourceTribe = sourceTribe;
		_targetTribe = targetTribe;

		_chanceOfRejecting = chanceOfRejecting;

		Description = targetTribe.GetNameAndTypeStringBold ().FirstLetterToUpper () + " has a strong relationship with " + sourceTribe.GetNameAndTypeStringBold () + 
			" and people from both tribes seem to intermix frequently regardless of clan affiliation.\n\n" +
			"Should " + sourceTribe.CurrentLeader.Name.BoldText + " propose " + targetTribe.GetNameAndTypeStringBold () + " merge into " + sourceTribe.GetNameAndTypeStringBold () + "?";

		_makeAttempt = makeAttempt;
	}

	private string GenerateAvoidMergeTribesAttemptResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_DecreasePreference (_sourceTribe, CulturalPreference.CohesionPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange);
	}

	public static void LeaderAvoidsMergeTribesAttempt (Tribe sourceTribe, Tribe targetTribe) {

		int rngOffset = RngOffsets.MERGE_TRIBES_EVENT_SOURCETRIBE_LEADER_AVOIDS_ATTEMPT_MODIFY_ATTRIBUTE;

		Effect_IncreasePreference (sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);
		Effect_DecreasePreference (sourceTribe, CulturalPreference.CohesionPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);

		Clan sourceDominantClan = sourceTribe.DominantFaction as Clan;

		sourceDominantClan.SetToUpdate ();

		sourceTribe.AddEventMessage (new AvoidMergeTribesAttemptEventMessage (sourceTribe, targetTribe, sourceTribe.CurrentLeader, sourceTribe.World.CurrentDate));
	}

	private void AvoidMergeTribesAttempt () {

		LeaderAvoidsMergeTribesAttempt (_sourceTribe, _targetTribe);
	}

	private string GenerateAttemptMergeTribesResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_DecreasePreference (_sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_IncreasePreference (_sourceTribe, CulturalPreference.CohesionPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• The current leader of " + _targetTribe.GetNameAndTypeStringBold () + " will receive an offer to merge their tribe into " + _sourceTribe.GetNameAndTypeStringBold ();
	}

	public static void LeaderAttemptsMergeTribes_TriggerRejectDecision (Tribe sourceTribe, Tribe targetTribe, float chanceOfRejecting) {

		World world = sourceTribe.World;

		bool acceptOffer = targetTribe.GetNextLocalRandomFloat (RngOffsets.MERGE_TRIBES_EVENT_TARGETTRIBE_LEADER_ACCEPT_OFFER) > chanceOfRejecting;

		Clan targetDominantClan = targetTribe.DominantFaction as Clan;

		if (targetTribe.IsUnderPlayerFocus || targetDominantClan.IsUnderPlayerGuidance) {

			Decision handleOfferDecision;

			handleOfferDecision = new HandleMergeTribesOfferDecision (sourceTribe, targetTribe, acceptOffer); // Give player options

			if (targetDominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (handleOfferDecision);

			} else {

				handleOfferDecision.ExecutePreferredOption ();
			}

		} else if (acceptOffer) {

			HandleMergeTribesOfferDecision.LeaderAcceptsOffer (sourceTribe, targetTribe);

		} else {

			HandleMergeTribesOfferDecision.LeaderRejectsOffer (sourceTribe, targetTribe);
		}
	}

	public static void LeaderAttemptsMergeTribes (Tribe sourceTribe, Tribe targetTribe, float chanceOfRejecting) {

		int rngOffset = RngOffsets.MERGE_TRIBES_EVENT_SOURCETRIBE_LEADER_MAKES_ATTEMPT_MODIFY_ATTRIBUTE;

		Effect_DecreasePreference (sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);

		LeaderAttemptsMergeTribes_TriggerRejectDecision (sourceTribe, targetTribe, chanceOfRejecting);
	}

	private void AttemptToMergeTribes () {

		LeaderAttemptsMergeTribes (_sourceTribe, _targetTribe, _chanceOfRejecting);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Attempt to merge tribes...", "Effects:\n" + GenerateAttemptMergeTribesResultEffectsString (), AttemptToMergeTribes),
			new Option ("We better stay separated...", "Effects:\n" + GenerateAvoidMergeTribesAttemptResultEffectsString (), AvoidMergeTribesAttempt)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_makeAttempt)
			AttemptToMergeTribes ();
		else
			AvoidMergeTribesAttempt ();
	}
}
