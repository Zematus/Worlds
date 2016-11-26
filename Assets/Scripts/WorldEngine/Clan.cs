using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Clan : Faction {

	public Clan () {

	}

	public Clan (CellGroup group, Polity polity, Name name) : base (group, polity, name) {
	}

	public override void UpdateInternal () {
		
	}

	public static Name GenerateName (Polity polity) {
	
		Language language = polity.Culture.Language;

		return null;
	}
}

public class ClanSplitEvent : FactionEvent {

	public const string EventSetFlag = "ClanSplitEvent_Set";

	public ClanSplitEvent () {

	}

	public ClanSplitEvent (Faction faction, int triggerDate) : base (faction, triggerDate, ClanSplitEventId) {

		Faction.SetFlag (EventSetFlag);
	}

	public static bool CanBeAssignedTo (Faction faction) {

		if (faction.IsFlagSet (EventSetFlag))
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		return true;
	}

	public override void Trigger () {

		Tribe tribe = Polity as Tribe;

		CellGroup targetGroup = tribe.GetRandomWeightedInfluencedGroup (RngOffsets.EVENT_TRIGGER + (int)Id);

		#if DEBUG
		if (targetGroup == null) {
			throw new System.Exception ("target group is null");
		}
		#endif

		Polity.AddFaction (new Clan (targetGroup, tribe, Clan.GenerateName (Polity)));

		World.AddPolityToUpdate (Polity);
	}

	protected override void DestroyInternal ()
	{
		if (Faction != null) {
			Faction.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}
