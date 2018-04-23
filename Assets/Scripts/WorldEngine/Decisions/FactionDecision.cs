using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public abstract class FactionDecision : Decision {

	public Faction Faction;

	public FactionDecision (Faction faction) : base () {

		Faction = faction;
	}

	protected static string GenerateEffectsString_IncreasePreference (Faction faction, string preferenceId, float minPercentChange, float maxPercentChange) {

		float charismaFactor = faction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = faction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange * attributesFactor;
		float modMaxPercentChange = maxPercentChange * attributesFactor;

		CulturalPreference preference = faction.Culture.GetPreference (preferenceId);
		float originalValue = preference.Value;

		float minValChange = MathUtility.IncreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, modMaxPercentChange);

		return faction.Type + " " + faction.Name.BoldText + ": " + preference.Name.ToLower () + " preference (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static string GenerateEffectsString_DecreasePreference (Faction faction, string preferenceId, float minPercentChange, float maxPercentChange) {

		float charismaFactor = faction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = faction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange / attributesFactor;
		float modMaxPercentChange = maxPercentChange / attributesFactor;

		CulturalPreference preference = faction.Culture.GetPreference (preferenceId);
		float originalValue = preference.Value;

		float minValChange = MathUtility.DecreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, modMaxPercentChange);

		return faction.Type + " " + faction.Name.BoldText + ": " + preference.Name.ToLower () + " preference (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static string GenerateEffectsString_IncreaseRelationship (Faction faction, Faction targetFaction, float minPercentChange, float maxPercentChange) {

		float charismaFactor = faction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = faction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange * attributesFactor;
		float modMaxPercentChange = maxPercentChange * attributesFactor;

		float originalValue = faction.GetRelationshipValue (targetFaction);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, modMaxPercentChange);

		return faction.Type + " " + faction.Name.BoldText + ": relationship with " + targetFaction.Type + " " + targetFaction.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static string GenerateEffectsString_DecreaseRelationship (Faction faction, Faction targetFaction, float minPercentChange, float maxPercentChange) {

		float charismaFactor = faction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = faction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange / attributesFactor;
		float modMaxPercentChange = maxPercentChange / attributesFactor;

		float originalValue = faction.GetRelationshipValue (targetFaction);

		float minValChange = MathUtility.DecreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, modMaxPercentChange);

		return faction.Type + " " + faction.Name.BoldText + ": relationship with " + targetFaction.Type + " " + targetFaction.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static void GenerateEffectsString_TransferInfluence (
		Faction sourceFaction, Faction targetFaction, Polity polity, float minPercentChange, float maxPercentChange, out string effectStringSourceFaction, out string effectStringTargetFaction) {

		float charismaFactor = sourceFaction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = sourceFaction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange / attributesFactor;
		float modMaxPercentChange = maxPercentChange / attributesFactor;

		float oldSourceInfluenceValue = sourceFaction.Influence;

		float minSourceValChange = oldSourceInfluenceValue * (1f - modMinPercentChange);
		float maxSourceValChange = oldSourceInfluenceValue * (1f - modMaxPercentChange);

		float oldTargetInfluenceValue = targetFaction.Influence;

		float minTargetValChange = oldTargetInfluenceValue + oldSourceInfluenceValue - minSourceValChange;
		float maxTargetValChange = oldTargetInfluenceValue + oldSourceInfluenceValue - maxSourceValChange;

		effectStringSourceFaction = sourceFaction.Type + " " + sourceFaction.Name.BoldText + ": influence within the " + polity.Name.BoldText + 
			" " + polity.Type.ToLower () + " (" + oldSourceInfluenceValue.ToString ("P") + ") decreases to: " + minSourceValChange.ToString ("P") + " - " + maxSourceValChange.ToString ("P");

		effectStringTargetFaction = targetFaction.Type + " " + targetFaction.Name.BoldText + ": influence within the " + polity.Name.BoldText + 
			" " + polity.Type.ToLower () + " (" + oldTargetInfluenceValue.ToString ("P") + ") increases to: " + minTargetValChange.ToString ("P") + " - " + maxTargetValChange.ToString ("P");
	}

	protected static string GenerateResultEffectsString_IncreaseInfluence (Faction faction, Polity polity) {

		return faction.Type + " " + faction.Name.BoldText + ": influence within the " + polity.Name.BoldText + 
			" " + polity.Type.ToLower () + " increases to: " + faction.Influence.ToString ("P");
	}

	protected static string GenerateResultEffectsString_DecreaseInfluence (Faction faction, Polity polity) {

		return faction.Type + " " + faction.Name.BoldText + ": influence within the " + polity.Name.BoldText + 
			" " + polity.Type.ToLower () + " decreases to: " + faction.Influence.ToString ("P");
	}

	protected static string GenerateResultEffectsString_IncreaseRelationship (Faction faction, Faction targetFaction) {

		float value = faction.GetRelationshipValue (targetFaction);

		return faction.Type + " " + faction.Name.BoldText + ": relationship with " + targetFaction.Type + " " + targetFaction.Name.BoldText + " increases to: " + 
			value.ToString ("0.00");
	}

	protected static string GenerateResultEffectsString_DecreaseRelationship (Faction faction, Faction targetFaction) {

		float value = faction.GetRelationshipValue (targetFaction);

		return faction.Type + " " + faction.Name.BoldText + ": relationship with " + targetFaction.Type + " " + targetFaction.Name.BoldText + " decreases to: " + 
			value.ToString ("0.00");
	}

	protected static string GenerateResultEffectsString_IncreasePreference (Faction faction, string preferenceId) {

		CulturalPreference preference = faction.Culture.GetPreference (preferenceId);

		return faction.Type + " " + faction.Name.BoldText + ": " + preference.Name.ToLower () + " preference increases to: " + 
			preference.Value.ToString ("0.00");
	}

	protected static string GenerateResultEffectsString_DecreasePreference (Faction faction, string preferenceId) {

		CulturalPreference preference = faction.Culture.GetPreference (preferenceId);

		return faction.Type + " " + faction.Name.BoldText + ": " + preference.Name.ToLower () + " preference decreases to: " + 
			preference.Value.ToString ("0.00");
	}

	protected static void Effect_TransferInfluence (Faction sourceFaction, Faction targetFaction, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = sourceFaction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = sourceFaction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = sourceFaction.GetNextLocalRandomFloat (rngOffset);
		float influencePercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		influencePercentChange /= attributesFactor;

		Polity.TransferInfluence (sourceFaction, targetFaction, influencePercentChange);
	}

	protected static void Effect_IncreaseRelationship (Faction sourceFaction, Faction targetFaction, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = sourceFaction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = sourceFaction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = sourceFaction.GetNextLocalRandomFloat (rngOffset);
		float relationshipPercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		relationshipPercentChange *= attributesFactor;

		float newValue = MathUtility.IncreaseByPercent (sourceFaction.GetRelationshipValue (targetFaction), relationshipPercentChange);
		Faction.SetRelationship (sourceFaction, targetFaction, newValue);
	}

	protected static void Effect_DecreaseRelationship (Faction sourceFaction, Faction targetFaction, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = sourceFaction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = sourceFaction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = sourceFaction.GetNextLocalRandomFloat (rngOffset);
		float relationshipPercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		relationshipPercentChange /= attributesFactor;

		float newValue = MathUtility.DecreaseByPercent (sourceFaction.GetRelationshipValue (targetFaction), relationshipPercentChange);
		Faction.SetRelationship (sourceFaction, targetFaction, newValue);
	}

	public static void Effect_IncreasePreference (Faction faction, string preferenceId, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = faction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = faction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = faction.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		authorityPreferencePercentChange *= attributesFactor;

		faction.IncreasePreferenceValue (preferenceId, authorityPreferencePercentChange);
	}

	public static void Effect_DecreasePreference (Faction faction, string preferenceId, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = faction.CurrentLeader.Charisma / 10f;
		float wisdomFactor = faction.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = faction.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		faction.DecreasePreferenceValue (preferenceId, authorityPreferencePercentChange);
	}
}
