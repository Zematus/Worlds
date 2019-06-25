using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Discovery : CellCulturalDiscovery, ICellGroupEventGenerator
{
    public class Event : CellGroupEventGeneratorEvent
    {
        public Event(
            Discovery discovery,
            CellGroup group,
            long triggerDate,
            long eventTypeId) :
            base(discovery, group, triggerDate, eventTypeId)
        {
        }
    }

    public static Dictionary<string, Discovery> Discoveries;
    
    public string EventGeneratorId;

    public int IdHash;
    public string EventSetFlag;

    public Condition[] GainConditions = null;
    public Condition[] HoldConditions = null;

    public Effect[] GainEffects = null;
    public Effect[] LossEffects = null;

    public int EventTimeToTrigger;
    public Factor[] EventTimeToTriggerFactors = null;

    public static void ResetDiscoveries()
    {
        Discoveries = new Dictionary<string, Discovery>();
    }

    public static void LoadDiscoveriesFile(string filename)
    {
        foreach (Discovery discovery in DiscoveryLoader.Load(filename))
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
        foreach (Discovery discovery in Discoveries.Values)
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

    public bool CanBeGained(CellGroup group)
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

    public override bool CanBeHeld(CellGroup group)
    {
        foreach (Condition condition in HoldConditions)
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

        return CanBeGained(group);
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

    public CellGroupEvent GenerateAndAssignEvent(CellGroup group)
    {
        long triggerDate = CalculateTriggerDate(group);

        Event discoveryEvent = new Event(this, group, triggerDate, IdHash);

        group.World.InsertEventToHappen(discoveryEvent);

        return discoveryEvent;
    }

    public bool CanTriggerEvent(CellGroup group)
    {
        return CanBeGained(group);
    }

    public void TriggerEvent(CellGroup group)
    {
        group.Culture.AddDiscoveryToFind(this);
    }

    public string GetEventGeneratorId()
    {
        return EventGeneratorId;
    }

    public override void OnGain(CellGroup group)
    {
        base.OnGain(group);

        foreach (Effect effect in GainEffects)
        {
            effect.Apply(group);
        }
    }

    public override void OnLoss(CellGroup group)
    {
        base.OnLoss(group);

        foreach (Effect effect in LossEffects)
        {
            effect.Apply(group);
        }

        if (CanAssignEventTypeToGroup(group))
        {
            GenerateAndAssignEvent(group);
        }
    }

    public string GetEventSetFlag()
    {
        return EventSetFlag;
    }
}
