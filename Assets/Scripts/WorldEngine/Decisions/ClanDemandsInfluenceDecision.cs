using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanDemandsInfluenceDecision : FactionDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinInfluencePercentChange = 0.05f;
	public const float BaseMaxInfluencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _makeDemand = true;

	private float _chanceOfRejecting;

	private Clan _dominantClan;
	private Clan _demandClan;

	[System.Obsolete]
	public ClanDemandsInfluenceDecision (Tribe tribe, Clan demandClan, Clan dominantClan, bool performDemand, float chanceOfRejecting, long eventId) : base (demandClan, eventId) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_demandClan = demandClan;

		_chanceOfRejecting = chanceOfRejecting;

		Description = "Influential members of clan " + demandClan.Name.BoldText + " have suggested for clan leader, " + demandClan.CurrentLeader.Name.BoldText + 
			", to demand to have more influence within the " + tribe.Name.BoldText + " tribe from the current dominant clan, " + dominantClan.Name.BoldText + ".\n\n" +
			"Should " + demandClan.CurrentLeader.Name.BoldText + " make the demand?";

		_makeDemand = performDemand;
	}

	private string GenerateAvoidDemandingInfluenceResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_DecreasePreference (_demandClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_IncreaseRelationship (_demandClan, _dominantClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange);
	}

	public static void LeaderAvoidsDemandingInfluence (Clan demandClan, Clan dominantClan, Tribe tribe) {

		int rngOffset = RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_DEMANDCLAN_LEADER_AVOIDS_DEMAND_MODIFY_ATTRIBUTE;

		Effect_DecreasePreference (demandClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);
		Effect_IncreaseRelationship (demandClan, dominantClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange, rngOffset++);

		demandClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new DemandClanAvoidInfluenceDemandEventMessage (demandClan, dominantClan, tribe, demandClan.CurrentLeader, demandClan.World.CurrentDate));
	}

	private void AvoidDemandingInfluence () {

		LeaderAvoidsDemandingInfluence (_demandClan, _dominantClan, _tribe);
	}

	private string GenerateDemandInfluenceResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_demandClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• The current leader of clan " + _dominantClan.Name.BoldText + " will receive the demand for infuence from " + _demandClan.CurrentLeader.Name.BoldText;
	}

	public static void LeaderDemandsInfluence_TriggerRejectDecision (Clan demandClan, Clan dominantClan, Tribe originalTribe, float chanceOfRejecting, long eventId) {

		World world = originalTribe.World;

		bool acceptDemand = originalTribe.GetNextLocalRandomFloat (RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_ACCEPT_DEMAND) > chanceOfRejecting;

		if (originalTribe.IsUnderPlayerFocus || dominantClan.IsUnderPlayerGuidance) {

			Decision dominantClanDecision;

			if (chanceOfRejecting <= 0) {
				dominantClanDecision = new DominantClanHandlesInfluenceDemandDecision (originalTribe, demandClan, dominantClan, eventId); // Player that controls dominant clan can't reject demand
			} else {
				dominantClanDecision = new DominantClanHandlesInfluenceDemandDecision (originalTribe, demandClan, dominantClan, acceptDemand, eventId); // Give player options
			}

			if (dominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (dominantClanDecision);

			} else {

				dominantClanDecision.ExecutePreferredOption ();
			}

		} else if (acceptDemand) {

			DominantClanHandlesInfluenceDemandDecision.LeaderAcceptsDemand (demandClan, dominantClan, originalTribe, eventId);

		} else {

			DominantClanHandlesInfluenceDemandDecision.LeaderRejectsDemand (demandClan, dominantClan, originalTribe, eventId);
		}
	}

	public static void LeaderDemandsInfluence (Clan demandClan, Clan dominantClan, Tribe originalTribe, float chanceOfRejecting, long eventId) {

		int rngOffset = RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_DEMANDCLAN_LEADER_DEMANDS_MODIFY_ATTRIBUTE;

		Effect_IncreasePreference (demandClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);

		LeaderDemandsInfluence_TriggerRejectDecision (demandClan, dominantClan, originalTribe, chanceOfRejecting, eventId);
	}

	private void DemandInfluence () {

		LeaderDemandsInfluence (_demandClan, _dominantClan, _tribe, _chanceOfRejecting, _eventId);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Demand more influence...", "Effects:\n" + GenerateDemandInfluenceResultEffectsString (), DemandInfluence),
			new Option ("Avoid making any demands...", "Effects:\n" + GenerateAvoidDemandingInfluenceResultEffectsString (), AvoidDemandingInfluence)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_makeDemand)
			DemandInfluence ();
		else
			AvoidDemandingInfluence ();
	}
}
