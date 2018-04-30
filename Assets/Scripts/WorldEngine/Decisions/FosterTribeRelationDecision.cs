using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FosterTribeRelationDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinInfluencePercentChange = 0.05f;
	public const float BaseMaxInfluencePercentChange = 0.15f;

	private bool _makeAttempt = true;

	private float _chanceOfRejecting;

	private Tribe _sourceTribe;
	private Tribe _targetTribe;

	private Clan _sourceDominantClan;
	private Clan _targetDominantClan;

	public FosterTribeRelationDecision (Tribe sourceTribe, Tribe targetTribe, bool makeAttempt, float chanceOfRejecting) : base (sourceTribe) {

		_sourceTribe = sourceTribe;
		_targetTribe = targetTribe;

		_sourceDominantClan = sourceTribe.DominantFaction as Clan;
		_targetDominantClan = targetTribe.DominantFaction as Clan;

		_chanceOfRejecting = chanceOfRejecting;

		Description = targetTribe.GetNameAndTypeStringBold ().FirstLetterToUpper () + " has had a long contact with our tribe, " + sourceTribe.Name.BoldText + 
			", but our relationship with them is not very strong.\n\n" +
			"Should " + sourceTribe.CurrentLeader.Name.BoldText + " attempt to foster our relationship with " + targetTribe.GetNameAndTypeStringBold () + "?";

		_makeAttempt = makeAttempt;
	}

	private string GenerateAvoidFosteringRelationshipResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange);
	}

	public static void LeaderAvoidsFosteringRelationship (Tribe sourceTribe, Tribe targetTribe) {

		int rngOffset = RngOffsets.FOSTER_TRIBE_RELATION_EVENT_SOURCETRIBE_LEADER_AVOIDS_ATTEMPT_MODIFY_ATTRIBUTE;

		Effect_IncreasePreference (sourceTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);

		Clan sourceDominantClan = sourceTribe.DominantFaction as Clan;
//		Clan targetDominantClan = targetTribe.DominantFaction as Clan;

		sourceDominantClan.SetToUpdate ();
//		targetDominantClan.SetToUpdate ();

//		sourceTribe.AddEventMessage (new DemandClanAvoidInfluenceDemandEventMessage (sourceTribe, targetTribe, tribe, sourceTribe.CurrentLeader, sourceTribe.World.CurrentDate));
	}

	private void AvoidDemandingInfluence () {

//		LeaderAvoidsFosteringRelationship (_sourceTribe, _targetTribe, _sourceDominantClan);
	}

	private string GenerateDemandInfluenceResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_sourceTribe, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• The current leader of clan " + _targetTribe.Name.BoldText + " will receive the demand for infuence from " + _sourceTribe.CurrentLeader.Name.BoldText;
	}

	public static void LeaderDemandsInfluence_TriggerRejectDecision (Clan demandClan, Clan dominantClan, Tribe originalTribe, float chanceOfRejecting) {

		World world = originalTribe.World;

		bool rejectDemand = originalTribe.GetNextLocalRandomFloat (RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_REJECT_DEMAND) < chanceOfRejecting;

		if (originalTribe.IsUnderPlayerFocus || dominantClan.IsUnderPlayerGuidance) {

			Decision dominantClanDecision;

			if (chanceOfRejecting >= 1) {
				dominantClanDecision = new DominantClanHandlesInfluenceDemandDecision (originalTribe, demandClan, dominantClan); // Player that controls dominant clan can't reject demand
			} else {
				dominantClanDecision = new DominantClanHandlesInfluenceDemandDecision (originalTribe, demandClan, dominantClan, rejectDemand); // Give player options
			}

			if (dominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (dominantClanDecision);

			} else {

				dominantClanDecision.ExecutePreferredOption ();
			}

		} else if (rejectDemand) {

			DominantClanHandlesInfluenceDemandDecision.LeaderRejectsDemand (demandClan, dominantClan, originalTribe);

		} else {

			DominantClanHandlesInfluenceDemandDecision.LeaderAcceptsDemand (demandClan, dominantClan, originalTribe);
		}
	}

	public static void LeaderDemandsInfluence (Clan demandClan, Clan dominantClan, Tribe originalTribe, float chanceOfRejecting) {

		int rngOffset = RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_DEMANDCLAN_LEADER_DEMANDS_MODIFY_ATTRIBUTE;

//		Effect_IncreasePreference (demandClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);

		LeaderDemandsInfluence_TriggerRejectDecision (demandClan, dominantClan, originalTribe, chanceOfRejecting);
	}

	private void DemandInfluence () {

//		LeaderDemandsInfluence (_sourceTribe, _targetTribe, _sourceDominantClan, _chanceOfRejecting);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Demand more influence...", "Effects:\n" + GenerateDemandInfluenceResultEffectsString (), DemandInfluence),
			new Option ("Avoid making any demands...", "Effects:\n" + GenerateAvoidFosteringRelationshipResultEffectsString (), AvoidDemandingInfluence)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_makeAttempt)
			DemandInfluence ();
		else
			AvoidDemandingInfluence ();
	}
}
