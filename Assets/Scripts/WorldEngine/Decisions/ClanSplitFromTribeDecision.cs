using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanSplitFromTribeDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinProminencePercentChange = 0.05f;
	public const float BaseMaxProminencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private Clan _triggerClan;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan triggerClan) {
		
//		Agent clanLeader = triggerClan.CurrentLeader;

		return 
			"The pressures of distance and strained relationships has made most of the populance under Clan " + triggerClan.Name.BoldText + " to feel that " +
			"they are no longer part of the " + tribe.Name.BoldText + " Tribe and wish for the Clan to become their own tribe.";
	}

	public ClanSplitFromTribeDecision (Tribe tribe, Clan triggerClan) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, triggerClan) +
			"Unfortunately, the pressure is too high for the clan leader, " + triggerClan.CurrentLeader.Name.BoldText + ", to do anything other than to acquiesce " +
			"to demands of " + triggerClan.CurrentLeader.PossessiveNoun + " people...";

		_cantPrevent = true;

		_triggerClan = triggerClan;
	}

	public ClanSplitFromTribeDecision (Tribe tribe, Clan triggerClan, bool preferSplit) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, triggerClan) +
			"Should the clan leader, " + triggerClan.CurrentLeader.Name.BoldText + ", follow the wish of " + triggerClan.CurrentLeader.PossessiveNoun + " people " +
			"and try to create a tribe of their own?";

		_preferSplit = preferSplit;

		_triggerClan = triggerClan;
	}

	private string GeneratePreventSplitResultEffectsString_AuthorityPreference () {

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinPreferencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxPreferencePercentChange / attributesFactor;

		float originalValue = _triggerClan.GetAuthorityPreferenceValue ();

		float minValChange = MathUtility.DecreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _triggerClan.Name.BoldText + ": authority preference (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString_CohesivenessPreference () {

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinPreferencePercentChange * attributesFactor;
		float maxPercentChange = BaseMaxPreferencePercentChange * attributesFactor;

		float originalValue = _triggerClan.GetCohesivenessPreferenceValue ();

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _triggerClan.Name.BoldText + ": cohesiveness preference (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private void GeneratePreventSplitResultEffectsString_Prominence (out string effectTriggerClan, out string effectDominantClan) {

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinProminencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxProminencePercentChange / attributesFactor;

		float oldProminenceValue = _triggerClan.Prominence;

		float minValChange = oldProminenceValue * (1f - minPercentChange);
		float maxValChange = oldProminenceValue * (1f - maxPercentChange);

		Clan dominantClan = _tribe.DominantFaction as Clan;

		float oldDominantProminenceValue = dominantClan.Prominence;

		float minValChangeDominant = oldDominantProminenceValue + oldProminenceValue - maxValChange;
		float maxValChangeDominant = oldDominantProminenceValue + oldProminenceValue - minValChange;

		effectTriggerClan = "Clan " + _triggerClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldProminenceValue.ToString ("0.00") + ") decreases to: " + minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");

		effectDominantClan = "Clan " + dominantClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldDominantProminenceValue.ToString ("0.00") + ") increases to: " + minValChangeDominant.ToString ("0.00") + " - " + maxValChangeDominant.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString_Relationship () {

		Clan dominantClan = _tribe.DominantFaction as Clan;

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinRelationshipPercentChange * attributesFactor;
		float maxPercentChange = BaseMaxRelationshipPercentChange * attributesFactor;

		float originalValue = _triggerClan.GetRelationshipValue (dominantClan);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _triggerClan.Name.BoldText + ": relationship with Clan " + dominantClan.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString () {

		string triggerClanProminenceChangeEffect;
		string dominantClanProminenceChangeEffect;

		GeneratePreventSplitResultEffectsString_Prominence (out triggerClanProminenceChangeEffect, out dominantClanProminenceChangeEffect);

		return 
			"\t• " + GeneratePreventSplitResultEffectsString_AuthorityPreference () + "\n" + 
			"\t• " + GeneratePreventSplitResultEffectsString_CohesivenessPreference () + "\n" + 
			"\t• " + GeneratePreventSplitResultEffectsString_Relationship () + "\n" + 
			"\t• " + triggerClanProminenceChangeEffect + "\n" + 
			"\t• " + dominantClanProminenceChangeEffect;
	}

	public static void LeaderPreventsSplit (Clan splitClan, Tribe tribe) {

		Clan dominantClan = splitClan.Polity.DominantFaction as Clan;

		float charismaFactor = splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.TRIBE_SPLITTING_EVENT_SPLITCLAN_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

		// Authority preference

		float randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		splitClan.DecreaseAuthorityPreferenceValue (authorityPreferencePercentChange);

		// Cohesiveness preference

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float cohesivenessPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		cohesivenessPreferencePercentChange *= attributesFactor;

		splitClan.IncreaseCohesivenessPreferenceValue (cohesivenessPreferencePercentChange);

		// Prominence

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float prominencePercentChange = (BaseMaxProminencePercentChange - BaseMinProminencePercentChange) * randomFactor + BaseMinProminencePercentChange;
		prominencePercentChange /= attributesFactor;

		Polity.TransferProminence (splitClan, dominantClan, prominencePercentChange);

		// Relationship

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float relationshipPercentChange = (BaseMaxRelationshipPercentChange - BaseMinRelationshipPercentChange) * randomFactor + BaseMinRelationshipPercentChange;
		relationshipPercentChange *= attributesFactor;

		float newValue = MathUtility.IncreaseByPercent (splitClan.GetRelationshipValue (dominantClan), relationshipPercentChange);
		Faction.SetRelationship (splitClan, dominantClan, newValue);

		// Updates

		splitClan.World.AddFactionToUpdate (splitClan);
		splitClan.World.AddFactionToUpdate (dominantClan);

		splitClan.World.AddPolityToUpdate (tribe);

		tribe.AddEventMessage (new SplitClanPreventTribeSplitEventMessage (splitClan, tribe, splitClan.CurrentLeader, splitClan.World.CurrentDate));
	}

	private void PreventSplit () {

		LeaderPreventsSplit (_triggerClan, _tribe);
	}

	private string GenerateAllowSplitResultMessage () {

		string message = "\n\t• Clan " + _triggerClan.Name.BoldText + " will attempt to leave the " + _tribe.Name.BoldText + " Tribe and form a tribe of their own";

		return message;
	}

	public static void LeaderAllowsSplit (Clan splitClan, Tribe originalTribe) {

		Tribe newTribe = new Tribe (splitClan, originalTribe);
		newTribe.Initialize ();

		splitClan.World.AddPolity (newTribe);
		splitClan.World.AddPolityToUpdate (newTribe);
		splitClan.World.AddPolityToUpdate (originalTribe);

		originalTribe.AddEventMessage (new TribeSplitEventMessage (splitClan, originalTribe, newTribe, splitClan.World.CurrentDate));
	}

	private void AllowSplit () {

		LeaderAllowsSplit (_triggerClan, _tribe);
	}

	public override Option[] GetOptions () {

		if (_cantPrevent) {

			return new Option[] {
				new Option ("Oh well...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			};
		}

		return new Option[] {
			new Option ("Allow clan to form a new tribe...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			new Option ("Prevent clan from leaving tribe...", "Effects:\n" + GeneratePreventSplitResultEffectsString (), PreventSplit)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_preferSplit)
			AllowSplit ();
		else
			PreventSplit ();
	}
}
	