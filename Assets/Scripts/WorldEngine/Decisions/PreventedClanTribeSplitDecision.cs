using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PreventedClanTribeSplitDecision : FactionDecision {

	private Tribe _tribe;

	private Clan _splitClan;
	private Clan _dominantClan;

	public PreventedClanTribeSplitDecision (Tribe tribe, Clan splitClan, Clan dominantClan) : base (splitClan) {

		_tribe = tribe;

		Description = "The tribe leader, " + tribe.CurrentLeader.Name.BoldText + ", has managed to convince clan " + splitClan.Name.BoldText + 
			" from leaving the tribe by trying to mend their relationship with clan " + tribe.DominantFaction.Name.BoldText + " and recognizing their importance within the tribe.";

		_dominantClan = dominantClan;
		_splitClan = splitClan;
	}

	private string GeneratePreventedSplitResultEffectsString () {

		return 
			"\t• " + GenerateResultEffectsString_IncreaseRelationship (_dominantClan, _splitClan) + "\n" + 
			"\t• " + GenerateResultEffectsString_DecreaseInfluence (_dominantClan, _tribe) + "\n" + 
			"\t• " + GenerateResultEffectsString_IncreaseInfluence (_splitClan, _tribe);
	}

	public static void TribeLeaderPreventedSplit (Clan splitClan, Clan dominantClan, Tribe tribe) {

		splitClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new PreventTribeSplitEventMessage (tribe, splitClan, tribe.CurrentLeader, splitClan.World.CurrentDate));
	}

	private void PreventedSplit () {

		TribeLeaderPreventedSplit (_splitClan, _dominantClan, _tribe);
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
	