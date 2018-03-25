using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PreventedClanTribeSplitDecision : PolityDecision {

	private Tribe _tribe;

	private Clan _splitClan;
	private Clan _dominantClan;

	public PreventedClanTribeSplitDecision (Tribe tribe, Clan splitClan) : base (tribe) {

		_tribe = tribe;

		Description = "The tribe leader, " + tribe.CurrentLeader.Name.BoldText + ", has managed to convince clan " + splitClan.Name.BoldText + 
			" from leaving the tribe by trying to mend their relationship with clan " + tribe.DominantFaction.Name.BoldText + " and recognizing their importance within the tribe.";

		_dominantClan = tribe.DominantFaction as Clan;
		_splitClan = splitClan;
	}

	private void GeneratePreventedSplitResultEffectsString_Prominence (out string effectSplitClan, out string effectDominantClan) {

		effectDominantClan = "Clan " + _dominantClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" tribe decreases to: " + _dominantClan.Prominence.ToString ("0.00");

		effectSplitClan = "Clan " + _splitClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" tribe increases to: " + _splitClan.Prominence.ToString ("0.00");
	}

	private string GeneratePreventedSplitResultEffectsString_Relationship () {

		float value = _dominantClan.GetRelationshipValue (_splitClan);

		return "Clan " + _dominantClan.Name.BoldText + ": relationship with clan " + _splitClan.Name.BoldText + " increases to: " + 
			value.ToString ("0.00");
	}

	private string GeneratePreventedSplitResultEffectsString () {

		string splitClanProminenceChangeEffect;
		string dominantClanProminenceChangeEffect;

		GeneratePreventedSplitResultEffectsString_Prominence (out splitClanProminenceChangeEffect, out dominantClanProminenceChangeEffect);

		return 
			"\t• " + GeneratePreventedSplitResultEffectsString_Relationship () + "\n" + 
			"\t• " + dominantClanProminenceChangeEffect + "\n" + 
			"\t• " + splitClanProminenceChangeEffect;
	}

	public static void TribeLeaderPreventedSplit (Clan splitClan, Tribe tribe) {

		tribe.World.AddFactionToUpdate (splitClan);
		tribe.World.AddFactionToUpdate (tribe.DominantFaction);

		tribe.World.AddPolityToUpdate (tribe);

		tribe.AddEventMessage (new PreventTribeSplitEventMessage (tribe, splitClan, splitClan.CurrentLeader, splitClan.World.CurrentDate));
	}

	private void PreventedSplit () {

		TribeLeaderPreventedSplit (_splitClan, _tribe);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Oh well...", "Effects:\n" + GeneratePreventedSplitResultEffectsString (), PreventedSplit)
		};
	}

	public override void ExecutePreferredOption ()
	{
		PreventedSplit ();
	}
}
	