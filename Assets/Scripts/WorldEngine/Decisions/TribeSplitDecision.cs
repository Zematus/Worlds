using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribeSplitDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinProminencePercentChange = 0.05f;
	public const float BaseMaxProminencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private Clan _dominantClan;
	private Clan _triggerClan;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan triggerClan) {

		return 
			"The pressures of distance and strained relationships has made most of the populance under Clan " + triggerClan.Name.BoldText + " to feel that " +
			"they are no longer part of the " + tribe.Name.BoldText + " Tribe and wish for the Clan to become their own tribe.";
	}

	public TribeSplitDecision (Tribe tribe, Clan triggerClan) : base (tribe) {

		_tribe = tribe;

		_dominantClan = tribe.DominantFaction as Clan;
		_triggerClan = triggerClan;

		Description = GenerateDescriptionIntro (tribe, triggerClan) +
			"Unfortunately, the pressure is too high for the tribe leader, " + _dominantClan.CurrentLeader.Name.BoldText + ", to do anything other than let " +
			"Clan " + triggerClan.Name.BoldText + " form it's own tribe";

		_cantPrevent = true;
	}

	public TribeSplitDecision (Tribe tribe, Clan triggerClan, bool preferSplit) : base (tribe) {

		_tribe = tribe;

		_dominantClan = tribe.DominantFaction as Clan;
		_triggerClan = triggerClan;

		Description = GenerateDescriptionIntro (tribe, triggerClan) +
			"Should the tribe leader, " + _dominantClan.CurrentLeader.Name.BoldText + ", allow Clan " + triggerClan.Name.BoldText + " to form a tribe of it's own?";

		_preferSplit = preferSplit;
	}

	private string GeneratePreventSplitResultEffectsString_AuthorityPreference () {

		float charismaFactor = _dominantClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _dominantClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinPreferencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxPreferencePercentChange / attributesFactor;

		float originalValue = _dominantClan.GetAuthorityPreferenceValue ();

		float minValChange = MathUtility.DecreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _dominantClan.Name.BoldText + ": authority preference (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

//	private string GeneratePreventSplitResultEffectsString_CohesivenessPreference () {
//
//		float charismaFactor = _dominantClan.CurrentLeader.Charisma / 10f;
//		float wisdomFactor = _dominantClan.CurrentLeader.Wisdom / 15f;
//
//		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
//		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);
//
//		float minPercentChange = BaseMinPreferencePercentChange * attributesFactor;
//		float maxPercentChange = BaseMaxPreferencePercentChange * attributesFactor;
//
//		float originalValue = _dominantClan.GetCohesivenessPreferenceValue ();
//
//		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
//		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);
//
//		return "Clan " + _dominantClan.Name.BoldText + ": cohesiveness preference (" + originalValue.ToString ("0.00") + ") increases to: " + 
//			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
//	}

	private void GeneratePreventSplitResultEffectsString_Prominence (out string effectTriggerClan, out string effectDominantClan) {

		float charismaFactor = _dominantClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _dominantClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinProminencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxProminencePercentChange / attributesFactor;

		float oldProminenceValue = _dominantClan.Prominence;

		float minValChange = oldProminenceValue * (1f - minPercentChange);
		float maxValChange = oldProminenceValue * (1f - maxPercentChange);

		float oldTriggerClanProminenceValue = _triggerClan.Prominence;

		float minValChangeTriggerClan = oldTriggerClanProminenceValue + oldProminenceValue - maxValChange;
		float maxValChangeTriggerClan = oldTriggerClanProminenceValue + oldProminenceValue - minValChange;

		effectDominantClan = "Clan " + _dominantClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldProminenceValue.ToString ("0.00") + ") decreases to: " + minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");

		effectTriggerClan = "Clan " + _triggerClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldTriggerClanProminenceValue.ToString ("0.00") + ") increases to: " + minValChangeTriggerClan.ToString ("0.00") + " - " + maxValChangeTriggerClan.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString_Relationship () {

		float charismaFactor = _dominantClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _dominantClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinRelationshipPercentChange * attributesFactor;
		float maxPercentChange = BaseMaxRelationshipPercentChange * attributesFactor;

		float originalValue = _dominantClan.GetRelationshipValue (_triggerClan);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _dominantClan.Name.BoldText + ": relationship with Clan " + _triggerClan.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString () {

		string triggerClanProminenceChangeEffect;
		string dominantClanProminenceChangeEffect;

		GeneratePreventSplitResultEffectsString_Prominence (out triggerClanProminenceChangeEffect, out dominantClanProminenceChangeEffect);

		return 
			"\t• " + GeneratePreventSplitResultEffectsString_AuthorityPreference () + "\n" + 
//			"\t• " + GeneratePreventSplitResultEffectsString_CohesivenessPreference () + "\n" + 
			"\t• " + GeneratePreventSplitResultEffectsString_Relationship () + "\n" + 
			"\t• " + dominantClanProminenceChangeEffect + "\n" + 
			"\t• " + triggerClanProminenceChangeEffect;
	}

	public static void LeaderPreventsSplit (Clan splitClan, Tribe tribe) {

		Clan dominantClan = tribe.DominantFaction as Clan;

		float charismaFactor = splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.TRIBE_SPLITTING_EVENT_TRIBE_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

		// Authority preference

		float randomFactor = dominantClan.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		dominantClan.DecreaseAuthorityPreferenceValue (authorityPreferencePercentChange);

//		// Cohesiveness preference
//
//		randomFactor = dominantClan.GetNextLocalRandomFloat (rngOffset++);
//		float cohesivenessPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
//		cohesivenessPreferencePercentChange *= attributesFactor;
//
//		dominantClan.IncreaseCohesivenessPreferenceValue (cohesivenessPreferencePercentChange);

		// Prominence

		randomFactor = dominantClan.GetNextLocalRandomFloat (rngOffset++);
		float prominencePercentChange = (BaseMaxProminencePercentChange - BaseMinProminencePercentChange) * randomFactor + BaseMinProminencePercentChange;
		prominencePercentChange /= attributesFactor;

		Polity.TransferProminence (dominantClan, splitClan, prominencePercentChange);

		// Relationship

		randomFactor = dominantClan.GetNextLocalRandomFloat (rngOffset++);
		float relationshipPercentChange = (BaseMaxRelationshipPercentChange - BaseMinRelationshipPercentChange) * randomFactor + BaseMinRelationshipPercentChange;
		relationshipPercentChange *= attributesFactor;

		float newValue = MathUtility.IncreaseByPercent (dominantClan.GetRelationshipValue (splitClan), relationshipPercentChange);
		Faction.SetRelationship (dominantClan, splitClan, newValue);

		// Updates

		tribe.World.AddFactionToUpdate (splitClan);
		tribe.World.AddFactionToUpdate (dominantClan);

		tribe.World.AddPolityToUpdate (tribe);

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
	