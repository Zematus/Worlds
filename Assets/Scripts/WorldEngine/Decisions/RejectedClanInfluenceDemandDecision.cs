using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class RejectedClanInfluenceDemandDecision : FactionDecision {

	private Tribe _tribe;

	private Clan _demandClan;
	private Clan _dominantClan;

	public RejectedClanInfluenceDemandDecision (Tribe tribe, Clan demandClan, Clan dominantClan) : base (demandClan) {

		_tribe = tribe;

		Description = "The leader of clan " + dominantClan.Name.BoldText + ", " + dominantClan.CurrentLeader.Name.BoldText + ", has rejected the demand from clan " + demandClan.Name.BoldText + 
			" to gain more influence within the " + tribe.Name.BoldText + " tribe.";

		_dominantClan = dominantClan;
		_demandClan = demandClan;
	}

	private string GenerateRejectedDemandResultEffectsString () {

		return 
			"\t• " + GenerateResultEffectsString_DecreaseRelationship (_dominantClan, _demandClan) + "\n" + 
			"\t• " + GenerateResultEffectsString_IncreasePreference (_dominantClan, CulturalPreference.AuthorityPreferenceId);
	}

	public static void DominantClanRejectedDemand (Clan demandClan, Clan dominantClan, Tribe tribe) {

		demandClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new RejectedClanInlfuenceDemandEventMessage (tribe, demandClan, dominantClan, dominantClan.CurrentLeader, tribe.World.CurrentDate));
	}

	private void RejectedDemand () {

		DominantClanRejectedDemand (_demandClan, _dominantClan, _tribe);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Oh well...", "Effects:\n" + GenerateRejectedDemandResultEffectsString (), RejectedDemand)
		};
	}

	public override void ExecutePreferredOption ()
	{
		RejectedDemand ();
	}
}
	