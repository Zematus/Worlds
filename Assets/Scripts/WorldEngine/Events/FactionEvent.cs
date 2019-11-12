using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class FactionEvent : WorldEvent {

	[XmlAttribute("FactId")]
	public long FactionId;

	[XmlAttribute("OPolId")]
	public long OriginalPolityId;

//	[XmlAttribute("CPolId")]
//	public long CurrentPolityId;

	[XmlIgnore]
	public Faction Faction;

	[XmlIgnore]
	public Polity OriginalPolity;

	public FactionEvent () {

	}

	public FactionEvent (Faction faction, FactionEventData data) : base (faction.World, data, GenerateUniqueIdentifier (faction, data.TriggerDate, data.TypeId)) {

		Faction = faction;
		FactionId = Faction.Id;

		OriginalPolityId = data.OriginalPolityId;
		OriginalPolity = World.GetPolity(OriginalPolityId);
    }

    public FactionEvent (Faction faction, long triggerDate, long eventTypeId) : base (faction.World, triggerDate, GenerateUniqueIdentifier (faction, triggerDate, eventTypeId), eventTypeId) {

		Faction = faction;
		FactionId = Faction.Id;

		OriginalPolity = faction.Polity;
		OriginalPolityId = OriginalPolity.Id;

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			string factionId = "Id: " + faction.Id;
        //
        //			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("FactionEvent - Faction: " + factionId, "TriggerDate: " + TriggerDate);
        //
        //			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //		}
        //		#endif

//#if DEBUG
//        if ((Id == 160349336613603015L) || (Id == 160349354613603010L))
//        {
//            Debug.LogWarning("Generating Event Id: " + Id + ". Faction Id: " + faction.Id +
//                ", current date: " + World.CurrentDate + ", loaded world: " + Manager.Debug_IsLoadedWorld);
//        }
//#endif
    }

    public static long GenerateUniqueIdentifier(Faction faction, long triggerDate, long eventTypeId)
    {
        if (triggerDate > World.MaxSupportedDate)
        {
            Debug.LogWarning("FactionEvent.GenerateUniqueIdentifier - 'triggerDate' is greater than " + World.MaxSupportedDate + " (triggerDate = " + triggerDate + ")");
        }

        return (triggerDate * 1000000000L) + ((faction.Id % 1000000L) * 1000L) + eventTypeId;
    }

    public override bool IsStillValid () {

		if (!base.IsStillValid ())
			return false;

		if (Faction == null)
			return false;

		if (!Faction.StillPresent)
			return false;

		Polity polity = World.GetPolity (Faction.PolityId);

		if (polity == null) {

			Debug.LogError ("FactionEvent: Polity with Id:" + Faction.PolityId + " not found");
		}

		return true;
	}

	public override void Synchronize ()
	{
//		CurrentPolityId = Faction.PolityId;

		base.Synchronize ();
	}

	public override void FinalizeLoad ()
    {
		base.FinalizeLoad ();

		Faction = World.GetFaction(FactionId);
		OriginalPolity = World.GetPolity(OriginalPolityId);

		if (Faction == null)
        {
			Debug.LogError("FactionEvent: Faction with Id:" + FactionId + " not found");
		}
	}

	public virtual void Reset (long newTriggerDate)
    {
        OriginalPolity = Faction.Polity;
		OriginalPolityId = OriginalPolity.Id;

		Reset (newTriggerDate, GenerateUniqueIdentifier (Faction, newTriggerDate, TypeId));
    }

    public override WorldEventData GetData () {

		return new FactionEventData (this);
	}
}
