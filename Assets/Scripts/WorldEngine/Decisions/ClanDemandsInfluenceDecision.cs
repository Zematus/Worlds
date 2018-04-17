using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanDemandsInfluenceDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinInfluencePercentChange = 0.05f;
	public const float BaseMaxInfluencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _performDemand = true;

	private Clan _dominantClan;
	private Clan _demandClan;

	public ClanDemandsInfluenceDecision (Tribe tribe, Clan demandClan, Clan dominantClan, bool performDemand) : base (tribe) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_demandClan = demandClan;

		Description = "Influential members of clan " + demandClan.Name.BoldText + " have suggested for clan leader, " + demandClan.CurrentLeader.Name.BoldText + 
			", to demand to have more influence within the " + tribe.Name.BoldText + " tribe from the current dominant clan, " + dominantClan.Name.BoldText + ".\n\n" +
			"Should " + demandClan.CurrentLeader.Name.BoldText + " make the demand?";

		_performDemand = performDemand;
	}

	private string GenerateAvoidDemandingInfluenceResultEffectsString_AuthorityPreference () {

		float charismaFactor = _demandClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _demandClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinPreferencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxPreferencePercentChange / attributesFactor;

		float originalValue = _demandClan.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		float minValChange = MathUtility.DecreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _demandClan.Name.BoldText + ": authority preference (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GenerateAvoidDemandingInfluenceResultEffectsString_Relationship () {

		_dominantClan = _tribe.DominantFaction as Clan;

		float charismaFactor = _demandClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _demandClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinRelationshipPercentChange * attributesFactor;
		float maxPercentChange = BaseMaxRelationshipPercentChange * attributesFactor;

		float originalValue = _demandClan.GetRelationshipValue (_dominantClan);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _demandClan.Name.BoldText + ": relationship with clan " + _dominantClan.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GenerateAvoidDemandingInfluenceResultEffectsString () {

		return 
			"\t• " + GenerateAvoidDemandingInfluenceResultEffectsString_AuthorityPreference () + "\n" +
			"\t• " + GenerateAvoidDemandingInfluenceResultEffectsString_Relationship ();
	}

	public static void LeaderAvoidsDemandingInfluence (Clan demandClan, Clan dominantClan, Tribe tribe) {

		float charismaFactor = demandClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = demandClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.TRIBE_SPLITTING_EVENT_SPLITCLAN_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

		// Authority preference

		float randomFactor = demandClan.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		demandClan.DecreasePreferenceValue (CulturalPreference.AuthorityPreferenceId, authorityPreferencePercentChange);

		// Influence

		randomFactor = demandClan.GetNextLocalRandomFloat (rngOffset++);
		float influencePercentChange = (BaseMaxInfluencePercentChange - BaseMinInfluencePercentChange) * randomFactor + BaseMinInfluencePercentChange;
		influencePercentChange /= attributesFactor;

		Polity.TransferInfluence (demandClan, dominantClan, influencePercentChange);

		// Relationship

		randomFactor = demandClan.GetNextLocalRandomFloat (rngOffset++);
		float relationshipPercentChange = (BaseMaxRelationshipPercentChange - BaseMinRelationshipPercentChange) * randomFactor + BaseMinRelationshipPercentChange;
		relationshipPercentChange *= attributesFactor;

		float newValue = MathUtility.IncreaseByPercent (demandClan.GetRelationshipValue (dominantClan), relationshipPercentChange);
		Faction.SetRelationship (demandClan, dominantClan, newValue);

		// Updates

		demandClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new SplitClanPreventTribeSplitEventMessage (demandClan, tribe, demandClan.CurrentLeader, demandClan.World.CurrentDate));
	}

	private void AvoidDemandingInfluence () {

		LeaderAvoidsDemandingInfluence (_demandClan, _dominantClan, _tribe);
	}

	private string GenerateDemandInfluenceResultEffectsString () {

		string message = "\t• The leader of clan " + _dominantClan.Name.BoldText + " will receive the demand for infuence from " + _demandClan.CurrentLeader.Name.BoldText;

		return message;
	}

	public static void LeaderDemandsInfluence (Clan demandClan, Clan dominantClan, Tribe originalTribe, float chanceOfRejecting) {

		World world = originalTribe.World;

		bool rejectDemand = originalTribe.GetNextLocalRandomFloat (RngOffsets.CLAN_DEMANDS_INFLUENCE_REJECT_DEMAND) < chanceOfRejecting;

		if (originalTribe.IsUnderPlayerFocus || dominantClan.IsUnderPlayerGuidance) {

			Decision tribeDecision;

			if (chanceOfRejecting >= 1) {
				tribeDecision = new TribeSplitDecision (originalTribe, demandClan, dominantClan); // Player that controls dominant clan can't prevent splitting from happening
			} else {
				tribeDecision = new TribeSplitDecision (originalTribe, demandClan, dominantClan, rejectDemand); // Give player options
			}

			if (dominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (tribeDecision);

			} else {

				tribeDecision.ExecutePreferredOption ();
			}

		} else if (rejectDemand) {

			TribeSplitDecision.LeaderAllowsSplit (demandClan, dominantClan, originalTribe);

		} else {

			TribeSplitDecision.LeaderPreventsSplit (demandClan, dominantClan, originalTribe);
		}
	}

	private void DemandInfluence () {

//		LeaderDemandsInfluence (_demandClan, _dominantClan, _tribe);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Demand more influence...", "Effects:\n" + GenerateDemandInfluenceResultEffectsString (), DemandInfluence),
			new Option ("Avoid making any demands...", "Effects:\n" + GenerateAvoidDemandingInfluenceResultEffectsString (), AvoidDemandingInfluence)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_performDemand)
			DemandInfluence ();
		else
			AvoidDemandingInfluence ();
	}
}
