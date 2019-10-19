using UnityEngine;
using ProtoBuf;

[ProtoContract]
public abstract class PolityEvent : WorldEvent {

	[ProtoMember(1)]
	public long PolityId;

    [ProtoMember(2)]
    public long OriginalDominantFactionId;

	public Polity Polity;

	public Faction OriginalDominantFaction;

    public PolityEvent()
    {

    }

    public PolityEvent(Polity polity, PolityEventData data) : base(polity.World, data, GenerateUniqueIdentifier(polity, data.TriggerDate, data.TypeId))
    {
        Polity = polity;
        PolityId = Polity.Id;

        OriginalDominantFactionId = data.OriginalDominantFactionId;
        OriginalDominantFaction = World.GetFaction(OriginalDominantFactionId);
    }

    public PolityEvent(Polity polity, long triggerDate, long eventTypeId) : base(polity.World, triggerDate, GenerateUniqueIdentifier(polity, triggerDate, eventTypeId), eventTypeId)
    {
        Polity = polity;
        PolityId = Polity.Id;

        OriginalDominantFactionId = polity.DominantFaction.Id;
        OriginalDominantFaction = polity.DominantFaction;

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			string polityId = "Id: " + polity.Id;
        //
        //			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("PolityEvent - Polity: " + polityId, "TriggerDate: " + TriggerDate);
        //
        //			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //		}
        //		#endif
    }

    public static long GenerateUniqueIdentifier (Polity polity, long triggerDate, long eventTypeId) {

		#if DEBUG
		if (triggerDate >= World.MaxSupportedDate) {
			Debug.LogWarning ("'triggerDate' shouldn't be greater than " + World.MaxSupportedDate + " (triggerDate = " + triggerDate + ")");
		}
		#endif

		return (triggerDate * 1000000000L) + ((polity.Id % 1000000L) * 1000L) + eventTypeId;
	}

	public override bool IsStillValid () {

		if (!base.IsStillValid ())
			return false;

		if (Polity == null)
			return false;

		return Polity.StillPresent;
	}

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        Polity = World.GetPolity(PolityId);
        OriginalDominantFaction = World.GetFaction(OriginalDominantFactionId);

        if (Polity == null)
        {
            Debug.LogError("PolityEvent: Polity with Id:" + PolityId + " not found");
        }
    }

    public virtual void Reset (long newTriggerDate) {

		OriginalDominantFaction = Polity.DominantFaction;
		OriginalDominantFactionId = OriginalDominantFaction.Id;

		Reset (newTriggerDate, GenerateUniqueIdentifier (Polity, newTriggerDate, TypeId));
	}

	public override WorldEventData GetData () {

		return new PolityEventData (this);
	}
}
