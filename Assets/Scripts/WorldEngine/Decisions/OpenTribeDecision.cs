using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class OpenTribeDecision : PolityDecision {

	public const float BaseMinIsolationPreferencePercentDecrease = 0.05f;
	public const float BaseMaxIsolationPreferencePercentDecrease = 0.15f;

	private bool _makeAttempt = true;

	private Tribe _tribe;

	public OpenTribeDecision (Tribe tribe, bool makeAttempt, long eventId) : base (tribe, eventId) {

		_tribe = tribe;

		Description = "The elders have suggested that perhaps " + tribe.GetNameAndTypeStringBold ().FirstLetterToUpper () + " has been to isolated and we " + 
			", should attempt to be more receptive to influence from our neighboors.\n\n" +
			"Should " + tribe.CurrentLeader.Name.BoldText + " attempt to allow for our tribal society to become more open?";

		_makeAttempt = makeAttempt;
	}

	private string GenerateAvoidOpeningTribeResultEffectsString () {

		return 
			"\t• The status quo for " + _tribe.GetNameAndTypeStringBold ().FirstLetterToUpper () + " is preserved";
	}

	public static void LeaderAvoidsOpeningTribe (Tribe tribe) {

//		int rngOffset = RngOffsets.FOSTER_TRIBE_RELATION_EVENT_SOURCETRIBE_LEADER_AVOIDS_ATTEMPT_MODIFY_ATTRIBUTE;

		Clan dominantClan = tribe.DominantFaction as Clan;

		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new AvoidOpeningTribeEventMessage (tribe, tribe.CurrentLeader, tribe.World.CurrentDate));
	}

	private void AvoidOpeningTribe () {

		LeaderAvoidsOpeningTribe (_tribe);
	}

	private string GenerateOpenTribeResultEffectsString () {

		return 
			"\t• " + GenerateEffectsString_DecreasePreference (_tribe, CulturalPreference.IsolationPreferenceId, BaseMinIsolationPreferencePercentDecrease, BaseMaxIsolationPreferencePercentDecrease);
	}

	public static void LeaderOpensTribe (Tribe tribe) {

		int rngOffset = RngOffsets.OPEN_TRIBE_EVENT_SOURCETRIBE_LEADER_MAKES_ATTEMPT_MODIFY_ATTRIBUTE;

		Effect_DecreasePreference (tribe, CulturalPreference.IsolationPreferenceId, BaseMinIsolationPreferencePercentDecrease, BaseMaxIsolationPreferencePercentDecrease, rngOffset++);

		Clan dominantClan = tribe.DominantFaction as Clan;

		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new OpenTribeEventMessage (tribe, tribe.CurrentLeader, tribe.World.CurrentDate));
	}

	private void OpenTribe () {

		LeaderOpensTribe (_tribe);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Promote openness...", "Effects:\n" + GenerateOpenTribeResultEffectsString (), OpenTribe),
			new Option ("Remain isolated...", "Effects:\n" + GenerateAvoidOpeningTribeResultEffectsString (), AvoidOpeningTribe)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_makeAttempt)
			OpenTribe ();
		else
			AvoidOpeningTribe ();
	}
}
