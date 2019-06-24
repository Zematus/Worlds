using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DiscoveryClass : ICellGroupEventGenerator
{
    public class Event : CellGroupEventGeneratorEvent
    {
        public Event(
            DiscoveryClass discoveryClass,
            CellGroup group,
            long triggerDate,
            long eventTypeId) :
            base(discoveryClass, group, triggerDate, eventTypeId)
        {
        }
    }

    public static Dictionary<string, DiscoveryClass> Discoveries;

    public string Id;
    public string Name;

    public string EventGeneratorId;

    public int IdHash;
    public string EventSetFlag;

    public Condition[] GainConditions = null;
    public Condition[] HoldConditions = null;

    public Effect[] GainEffects = null;
    public Effect[] LossEffects = null;

    public int EventTimeToTrigger;
    public Factor[] EventTimeToTriggerFactors = null;

    public bool IsPresentAtStart = false;

    public static void ResetDiscoveries()
    {
        Discoveries = new Dictionary<string, DiscoveryClass>();
    }

    public static void LoadDiscoveriesFile(string filename)
    {
        foreach (DiscoveryClass discovery in DiscoveryLoader.Load(filename))
        {
            if (Discoveries.ContainsKey(discovery.Id))
            {
                Discoveries[discovery.Id] = discovery;
            }
            else
            {
                Discoveries.Add(discovery.Id, discovery);
            }
        }
    }

    public static void InitializeDiscoveries()
    {
        foreach (DiscoveryClass discovery in Discoveries.Values)
        {
            discovery.Initialize();
        }
    }

    public void Initialize()
    {
        string eventPrefix = Id + "_discovery_event";

        EventGeneratorId = eventPrefix + "_generator";
        EventSetFlag = eventPrefix + "_set";

        World.EventGenerators.Add(EventGeneratorId, this);
        CellGroup.OnSpawnEventGenerators.Add(this);
    }

    public bool CanGainDiscovery(CellGroup group)
    {
        if (group.Culture.HasOrWillHaveDiscovery(Id))
            return false;

        foreach (Condition condition in GainConditions)
        {
            if (!condition.Evaluate(group))
                return false;
        }

        return true;
    }

    public bool CanAssignEventTypeToGroup(CellGroup group)
    {
        if (group.IsFlagSet(EventSetFlag))
            return false;

        return CanGainDiscovery(group);
    }

    private long CalculateTriggerDate(CellGroup group)
    {
        float randomFactor = group.GetNextLocalRandomFloat(IdHash);

        float dateSpan = randomFactor * EventTimeToTrigger;

        foreach (Factor factor in EventTimeToTriggerFactors)
        {
            dateSpan *= factor.Calculate(group);
        }

        long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

        return targetDate;
    }

    public CellGroupEvent GenerateAndAddEvent(CellGroup group)
    {
        long triggerDate = CalculateTriggerDate(group);

        Event discoveryEvent = new Event(this, group, triggerDate, IdHash);

        group.SetFlag(EventSetFlag);

        group.World.InsertEventToHappen(discoveryEvent);

        return discoveryEvent;
    }

    public bool CanTriggerEvent(CellGroup group)
    {
        return CanGainDiscovery(group);
    }

    public void TriggerEvent(CellGroup group)
    {
        CellCulturalDiscovery discovery = null;

        group.Culture.AddDiscoveryToFind(discovery);

        foreach (Effect effect in GainEffects)
        {
            effect.Apply(group);
        }
    }

    public string GetEventGeneratorId()
    {
        return EventGeneratorId;
    }
}
