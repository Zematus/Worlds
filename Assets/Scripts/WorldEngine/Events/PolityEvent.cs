using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class PolityEvent : WorldEvent
{
    #region PolityId
    [XmlAttribute("PId")]
    public string PolityIdStr
    {
        get { return PolityId; }
        set { PolityId = value; }
    }
    [XmlIgnore]
    public Identifier PolityId;
    #endregion

    #region OriginalDominantFactionId
    [XmlAttribute("ODFId")]
    public string OriginalDominantFactionIdStr
    {
        get { return OriginalDominantFactionId; }
        set { OriginalDominantFactionId = value; }
    }
    [XmlIgnore]
    public Identifier OriginalDominantFactionId;
    #endregion

    [XmlIgnore]
    public Polity Polity;

    [XmlIgnore]
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

    public static long GenerateUniqueIdentifier(Polity polity, long triggerDate, long eventTypeId)
    {
        if (triggerDate >= World.MaxSupportedDate)
        {
            Debug.LogWarning("PolityEvent.GenerateUniqueIdentifier - 'triggerDate' is greater than " + World.MaxSupportedDate + " (triggerDate = " + triggerDate + ")");
        }

        return (triggerDate * 1000000000L) + ((polity.GetHashCode() % 1000000L) * 1000L) + eventTypeId;
    }

    public override bool IsStillValid()
    {
        if (!base.IsStillValid())
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

    public override void Reset(long newTriggerDate)
    {
        OriginalDominantFaction = Polity.DominantFaction;
        OriginalDominantFactionId = OriginalDominantFaction.Id;

        Reset(newTriggerDate, GenerateUniqueIdentifier(Polity, newTriggerDate, TypeId));
    }

    public override WorldEventData GetData()
    {
        return new PolityEventData(this);
    }
}
