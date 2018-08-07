using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class AcceptedClanInfluenceDemandDecision : FactionDecision {

	private Tribe _tribe;

	private Clan _demandClan;
	private Clan _dominantClan;

	public AcceptedClanInfluenceDemandDecision (Tribe tribe, Clan demandClan, Clan dominantClan, long eventId) : base (demandClan, eventId) {

		_tribe = tribe;

		Description = "The leader of clan " + dominantClan.Name.BoldText + ", " + dominantClan.CurrentLeader.Name.BoldText + ", has accepted the demand from clan " + demandClan.Name.BoldText + 
			" to gain more influence within the " + tribe.Name.BoldText + " tribe.";

		_dominantClan = dominantClan;
		_demandClan = demandClan;
	}

	private string GenerateAcceptedDemandResultEffectsString () {

		return 
			"\t• " + GenerateResultEffectsString_IncreaseInfluence (_demandClan, _tribe) + "\n" + 
			"\t• " + GenerateResultEffectsString_DecreaseInfluence (_dominantClan, _tribe) + "\n" + 
			"\t• " + GenerateResultEffectsString_IncreaseRelationship (_dominantClan, _demandClan) + "\n" + 
			"\t• " + GenerateResultEffectsString_DecreasePreference (_dominantClan, CulturalPreference.AuthorityPreferenceId);
	}

	public static void DominantClanAcceptedDemand (Clan demandClan, Clan dominantClan, Tribe tribe) {

		demandClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new AcceptedClanInlfuenceDemandEventMessage (tribe, demandClan, dominantClan, dominantClan.CurrentLeader, tribe.World.CurrentDate));
	}

	private void AcceptedDemand () {

		DominantClanAcceptedDemand (_demandClan, _dominantClan, _tribe);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Of course they would!", "Effects:\n" + GenerateAcceptedDemandResultEffectsString (), AcceptedDemand)
		};
	}

	public override void ExecutePreferredOption ()
	{
		AcceptedDemand ();
	}
}
	