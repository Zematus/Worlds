using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DomClanRejectsDemandInfluenceDecision : FactionDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinInfluencePercentChange = 0.05f;
	public const float BaseMaxInfluencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _acceptDemand = true;

	private Clan _dominantClan;
	private Clan _demandClan;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan demandClan, Clan dominantClan) {

		return 
			demandClan.CurrentLeader.Name.BoldText + ", leader of clan " + demandClan.Name.BoldText + ", has demanded greater influence over the " + tribe.Name.BoldText + 
			" tribe at the expense of clan " + dominantClan.Name.BoldText + ".\n\n";
	}

	public DomClanRejectsDemandInfluenceDecision (Tribe tribe, Clan demandClan, Clan dominantClan) : base (dominantClan) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_demandClan = demandClan;

		Description = GenerateDescriptionIntro (tribe, demandClan, dominantClan) +
			"Unfortunately, the situation is beyond control for the tribe leader, " + dominantClan.CurrentLeader.Name.BoldText + ", to be able to do anything other than accept " +
			"clan " + demandClan.Name.BoldText + "'s demands...";

		_cantPrevent = true;
	}

	public DomClanRejectsDemandInfluenceDecision (Tribe tribe, Clan demandClan, Clan dominantClan, bool acceptDemand) : base (dominantClan) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_demandClan = demandClan;

		Description = GenerateDescriptionIntro (tribe, demandClan, dominantClan) +
			"Should the leader of clan " + dominantClan.Name.BoldText + ", " + _dominantClan.CurrentLeader.Name.BoldText + ", accept the demans from clan " + demandClan.Name.BoldText + "?";

		_acceptDemand = acceptDemand;
	}

	private string GenerateRejectDemandsResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_dominantClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_DecreaseRelationship (_dominantClan, _demandClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange);
	}

	public static void LeaderRejectsDemand_notifyDemandClan (Clan demandClan, Clan dominantClan, Tribe originalTribe) {

		World world = originalTribe.World;

		if (originalTribe.IsUnderPlayerFocus || demandClan.IsUnderPlayerGuidance) {

			Decision decision = new RejectedClanInfluenceDemandDecision (originalTribe, demandClan, dominantClan); // Notify player that tribe leader rejected demand

			if (demandClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (decision);

			} else {

				decision.ExecutePreferredOption ();
			}

		} else {

			RejectedClanInfluenceDemandDecision.DominantClanRejectsDemand (demandClan, dominantClan, originalTribe);
		}
	}

	public static void LeaderRejectsDemand (Clan demandClan, Clan dominantClan, Tribe tribe) {

		int rngOffset = RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_DOMINANTCLAN_LEADER_REJECTS_DEMAND_MODIFY_ATTRIBUTE;

		Effect_IncreasePreference (dominantClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);
		Effect_DecreaseRelationship (dominantClan, demandClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange, rngOffset++);

		LeaderRejectsDemand_notifyDemandClan (demandClan, dominantClan, tribe);
	}

	private void PreventSplit () {

		LeaderRejectsDemand (_demandClan, _dominantClan, _tribe);
	}

	private void GenerateAcceptDemandsResultEffectsString_Influence (out string effectSplitClan, out string effectDominantClan) {

		float charismaFactor = _dominantClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _dominantClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinInfluencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxInfluencePercentChange / attributesFactor;

		float oldInfluenceValue = _dominantClan.Influence;

		float minValChange = oldInfluenceValue * (1f - minPercentChange);
		float maxValChange = oldInfluenceValue * (1f - maxPercentChange);

		float oldSplitClanInfluenceValue = _demandClan.Influence;

		float minValChangeSplitClan = oldSplitClanInfluenceValue + oldInfluenceValue - minValChange;
		float maxValChangeSplitClan = oldSplitClanInfluenceValue + oldInfluenceValue - maxValChange;

		effectDominantClan = "Clan " + _dominantClan.Name.BoldText + ": influence within the " + _tribe.Name.BoldText + 
			" tribe (" + oldInfluenceValue.ToString ("P") + ") decreases to: " + minValChange.ToString ("P") + " - " + maxValChange.ToString ("P");

		effectSplitClan = "Clan " + _demandClan.Name.BoldText + ": influence within the " + _tribe.Name.BoldText + 
			" tribe (" + oldSplitClanInfluenceValue.ToString ("P") + ") increases to: " + minValChangeSplitClan.ToString ("P") + " - " + maxValChangeSplitClan.ToString ("P");
	}

	private string GenerateAllowSplitResultMessage () {

		string message = "\t• Clan " + _demandClan.Name.BoldText + " will leave the " + _tribe.Name.BoldText + " tribe and form a tribe of their own";

		return message;
	}

	public static void LeaderAllowsSplit (Clan splitClan, Clan dominantClan, Tribe originalTribe) {

		Tribe newTribe = new Tribe (splitClan, originalTribe);
		newTribe.Initialize ();

		splitClan.World.AddPolity (newTribe);

		splitClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		originalTribe.AddEventMessage (new TribeSplitEventMessage (splitClan, originalTribe, newTribe, splitClan.World.CurrentDate));
	}

	private void AllowSplit () {

		LeaderAllowsSplit (_demandClan, _dominantClan, _tribe);
	}

	public override Option[] GetOptions () {

		if (_cantPrevent) {

			return new Option[] {
				new Option ("Oh well...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			};
		}

		return new Option[] {
			new Option ("Allow clan to form a new tribe...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			new Option ("Prevent clan from leaving tribe...", "Effects:\n" + GenerateRejectDemandsResultEffectsString (), PreventSplit)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_acceptDemand)
			AllowSplit ();
		else
			PreventSplit ();
	}
}
	