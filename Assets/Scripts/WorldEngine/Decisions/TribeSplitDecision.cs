using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribeSplitDecision : FactionDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinInfluencePercentChange = 0.05f;
	public const float BaseMaxInfluencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private Clan _dominantClan;
	private Clan _splitClan;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan splitClan) {

		return 
			"The pressures of distance and strained relationships has made most of the populance under clan " + splitClan.Name.BoldText + " to feel that " +
			"they are no longer part of the " + tribe.Name.BoldText + " tribe and wish for the clan to become their own tribe.\n\n";
	}

	public TribeSplitDecision (Tribe tribe, Clan splitClan, Clan dominantClan) : base (dominantClan) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_splitClan = splitClan;

		Description = GenerateDescriptionIntro (tribe, splitClan) +
			"Unfortunately, the situation is beyond control for the tribe leader, " + _dominantClan.CurrentLeader.Name.BoldText + ", to be able to do anything other than let " +
			"clan " + splitClan.Name.BoldText + " leave the tribe...";

		_cantPrevent = true;
	}

	public TribeSplitDecision (Tribe tribe, Clan splitClan, Clan dominantClan, bool preferSplit) : base (dominantClan) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_splitClan = splitClan;

		Description = GenerateDescriptionIntro (tribe, splitClan) +
			"Should the tribe leader, " + _dominantClan.CurrentLeader.Name.BoldText + ", allow clan " + splitClan.Name.BoldText + " to leave the tribe and form its own?";

		_preferSplit = preferSplit;
	}

	private string GeneratePreventSplitResultEffectsString () {

		string splitClanInfluenceChangeEffect;
		string dominantClanInfluenceChangeEffect;

		GenerateEffectsString_TransferInfluence (
			_dominantClan, _splitClan, _tribe, BaseMinInfluencePercentChange, BaseMaxInfluencePercentChange, out dominantClanInfluenceChangeEffect, out splitClanInfluenceChangeEffect);

		return 
			"\t• " + GenerateEffectsString_IncreaseRelationship (_dominantClan, _splitClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange) + "\n" + 
			"\t• " + dominantClanInfluenceChangeEffect + "\n" + 
			"\t• " + splitClanInfluenceChangeEffect;
	}

	public static void LeaderPreventsSplit_notifySplitClan (Clan splitClan, Clan dominantClan, Tribe originalTribe) {

		World world = originalTribe.World;

		if (originalTribe.IsUnderPlayerFocus || splitClan.IsUnderPlayerGuidance) {

			Decision decision = new PreventedClanTribeSplitDecision (originalTribe, splitClan, dominantClan); // Notify player that tribe leader prevented split

			if (splitClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (decision);

			} else {

				decision.ExecutePreferredOption ();
			}

		} else {

			PreventedClanTribeSplitDecision.TribeLeaderPreventedSplit (splitClan, dominantClan, originalTribe);
		}
	}

	public static void LeaderPreventsSplit (Clan splitClan, Clan dominantClan, Tribe tribe) {

		float charismaFactor = splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.TRIBE_SPLITTING_EVENT_TRIBE_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

		// Influence

		float randomFactor = dominantClan.GetNextLocalRandomFloat (rngOffset++);
		float influencePercentChange = (BaseMaxInfluencePercentChange - BaseMinInfluencePercentChange) * randomFactor + BaseMinInfluencePercentChange;
		influencePercentChange /= attributesFactor;

		Polity.TransferInfluence (dominantClan, splitClan, influencePercentChange);

		// Relationship

		randomFactor = dominantClan.GetNextLocalRandomFloat (rngOffset++);
		float relationshipPercentChange = (BaseMaxRelationshipPercentChange - BaseMinRelationshipPercentChange) * randomFactor + BaseMinRelationshipPercentChange;
		relationshipPercentChange *= attributesFactor;

		float newValue = MathUtility.IncreaseByPercent (dominantClan.GetRelationshipValue (splitClan), relationshipPercentChange);
		Faction.SetRelationship (dominantClan, splitClan, newValue);

		// Updates

		LeaderPreventsSplit_notifySplitClan (splitClan, dominantClan, tribe);
	}

	private void PreventSplit () {

		LeaderPreventsSplit (_splitClan, _dominantClan, _tribe);
	}

	private string GenerateAllowSplitResultMessage () {

		string message = "\t• Clan " + _splitClan.Name.BoldText + " will leave the " + _tribe.Name.BoldText + " tribe and form a tribe of their own";

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

		LeaderAllowsSplit (_splitClan, _dominantClan, _tribe);
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
	