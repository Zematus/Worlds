using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public abstract class PolityDecision : Decision {

	public Polity Polity;

	public PolityDecision (Polity polity) : base () {

		Polity = polity;
	}

	protected static string GenerateEffectsString_IncreasePreference (Polity polity, string preferenceId, float minPercentChange, float maxPercentChange) {

		float charismaFactor = polity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = polity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange * attributesFactor;
		float modMaxPercentChange = maxPercentChange * attributesFactor;

		Faction dominantFaction = polity.DominantFaction;

		CulturalPreference preference = dominantFaction.Culture.GetPreference (preferenceId);
		float originalValue = preference.Value;

		float minValChange = MathUtility.IncreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, modMaxPercentChange);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " " + preference.Name.ToLower () + 
			" preference (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static string GenerateEffectsString_DecreasePreference (Polity polity, string preferenceId, float minPercentChange, float maxPercentChange) {

		float charismaFactor = polity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = polity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange / attributesFactor;
		float modMaxPercentChange = maxPercentChange / attributesFactor;

		Faction dominantFaction = polity.DominantFaction;

		CulturalPreference preference = dominantFaction.Culture.GetPreference (preferenceId);
		float originalValue = preference.Value;

		float minValChange = MathUtility.DecreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, modMaxPercentChange);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " " + preference.Name.ToLower () + 
			" preference (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static string GenerateEffectsString_IncreaseRelationship (Polity polity, Polity targetPolity, float minPercentChange, float maxPercentChange) {

		float charismaFactor = polity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = polity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange * attributesFactor;
		float modMaxPercentChange = maxPercentChange * attributesFactor;

		Faction dominantFaction = polity.DominantFaction;
		Faction targetdominantFaction = targetPolity.DominantFaction;

		float originalValue = dominantFaction.GetRelationshipValue (targetdominantFaction);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, modMaxPercentChange);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " relationship with " + 
			targetdominantFaction.GetNameAndTypeWithPolityStringBold () + " (" + originalValue.ToString ("0.00") + 
			") increases to: " + minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static string GenerateEffectsString_DecreaseRelationship (Polity polity, Polity targetPolity, float minPercentChange, float maxPercentChange) {

		float charismaFactor = polity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = polity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float modMinPercentChange = minPercentChange / attributesFactor;
		float modMaxPercentChange = maxPercentChange / attributesFactor;

		Faction dominantFaction = polity.DominantFaction;
		Faction targetdominantFaction = targetPolity.DominantFaction;

		float originalValue = dominantFaction.GetRelationshipValue (targetdominantFaction);

		float minValChange = MathUtility.DecreaseByPercent (originalValue, modMinPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, modMaxPercentChange);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " relationship with " + 
			targetdominantFaction.GetNameAndTypeWithPolityStringBold () + " (" + originalValue.ToString ("0.00") + 
			") decreases to: " + minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	protected static string GenerateResultEffectsString_IncreaseRelationship (Polity polity, Polity targetPolity) {

		Faction dominantFaction = polity.DominantFaction;
		Faction targetdominantFaction = targetPolity.DominantFaction;

		float value = dominantFaction.GetRelationshipValue (targetdominantFaction);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " relationship with " + 
			targetdominantFaction.GetNameAndTypeWithPolityStringBold () + " increases to: " + value.ToString ("0.00");
	}

	protected static string GenerateResultEffectsString_DecreaseRelationship (Polity polity, Polity targetPolity) {

		Faction dominantFaction = polity.DominantFaction;
		Faction targetdominantFaction = targetPolity.DominantFaction;

		float value = dominantFaction.GetRelationshipValue (targetdominantFaction);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " relationship with " + 
			targetdominantFaction.GetNameAndTypeWithPolityStringBold () + " decreases to: " + value.ToString ("0.00");
	}

	protected static string GenerateResultEffectsString_IncreasePreference (Polity polity, string preferenceId) {

		Faction dominantFaction = polity.DominantFaction;

		CulturalPreference preference = dominantFaction.Culture.GetPreference (preferenceId);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " preference increases to: " + 
			preference.Value.ToString ("0.00");
	}

	protected static string GenerateResultEffectsString_DecreasePreference (Polity polity, string preferenceId) {

		Faction dominantFaction = polity.DominantFaction;

		CulturalPreference preference = dominantFaction.Culture.GetPreference (preferenceId);

		return polity.GetNameAndTypeStringBold ().FirstLetterToUpper () + ": " + dominantFaction.GetNameAndTypeStringBold ().AddPossApos () + " preference decreases to: " + 
			preference.Value.ToString ("0.00");
	}

	protected static void Effect_IncreaseRelationship (Polity sourcePolity, Polity targetPolity, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = sourcePolity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = sourcePolity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = sourcePolity.GetNextLocalRandomFloat (rngOffset);
		float relationshipPercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		relationshipPercentChange *= attributesFactor;

		Faction sourceDominantFaction = sourcePolity.DominantFaction;
		Faction targetdominantFaction = targetPolity.DominantFaction;

		float newValue = MathUtility.IncreaseByPercent (sourceDominantFaction.GetRelationshipValue (targetdominantFaction), relationshipPercentChange);
		Faction.SetRelationship (sourceDominantFaction, targetdominantFaction, newValue);
	}

	protected static void Effect_DecreaseRelationship (Polity sourcePolity, Polity targetPolity, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = sourcePolity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = sourcePolity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = sourcePolity.GetNextLocalRandomFloat (rngOffset);
		float relationshipPercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		relationshipPercentChange /= attributesFactor;

		Faction sourceDominantFaction = sourcePolity.DominantFaction;
		Faction targetdominantFaction = targetPolity.DominantFaction;

		float newValue = MathUtility.DecreaseByPercent (sourceDominantFaction.GetRelationshipValue (targetdominantFaction), relationshipPercentChange);
		Faction.SetRelationship (sourceDominantFaction, targetdominantFaction, newValue);
	}

	public static void Effect_IncreasePreference (Polity polity, string preferenceId, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = polity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = polity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = polity.GetNextLocalRandomFloat (rngOffset++);
		float preferencePercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		preferencePercentChange *= attributesFactor;

		Faction dominantFaction = polity.DominantFaction;

		dominantFaction.IncreasePreferenceValue (preferenceId, preferencePercentChange);
	}

	public static void Effect_DecreasePreference (Polity polity, string preferenceId, float minPercentChange, float maxPercentChange, int rngOffset) {

		float charismaFactor = polity.CurrentLeader.Charisma / 10f;
		float wisdomFactor = polity.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float randomFactor = polity.GetNextLocalRandomFloat (rngOffset++);
		float preferencePercentChange = (maxPercentChange - minPercentChange) * randomFactor + minPercentChange;
		preferencePercentChange /= attributesFactor;

		Faction dominantFaction = polity.DominantFaction;

		dominantFaction.DecreasePreferenceValue (preferenceId, preferencePercentChange);
	}
}
