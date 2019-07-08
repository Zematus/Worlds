using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Discovery : CellCulturalDiscovery, ICellGroupEventGenerator
{
    public class Event : CellGroupEventGeneratorEvent
    {
        private Discovery _discovery;

        public Event(
            Discovery discovery,
            CellGroup group,
            long triggerDate,
            long eventTypeId) :
            base(discovery, group, triggerDate, eventTypeId)
        {
            _discovery = discovery;
        }

        private void TryGenerateEventMessage()
        {
            DiscoveryEventMessage eventMessage = null;

            World world = Group.World;
            TerrainCell cell = Group.Cell;

            if (!world.HasEventMessage(_discovery.UId))
            {
                eventMessage = new DiscoveryEventMessage(_discovery.Name, cell, _discovery.UId, TriggerDate);

                world.AddEventMessage(eventMessage);
            }

            if (cell.EncompassingTerritory != null)
            {
                Polity encompassingPolity = cell.EncompassingTerritory.Polity;

                if (!encompassingPolity.HasEventMessage(_discovery.UId))
                {
                    if (eventMessage == null)
                        eventMessage = new DiscoveryEventMessage(_discovery.Name, cell, _discovery.UId, TriggerDate);

                    encompassingPolity.AddEventMessage(eventMessage);
                }
            }
        }

        public override void Trigger()
        {
            Group.Culture.AddDiscoveryToFind(_discovery);

            TryGenerateEventMessage();
        }

        public override bool CanTrigger()
        {
            if (!base.CanTrigger())
                return false;

            return _discovery.CanBeGained(Group);
        }

        public override void FinalizeLoad()
        {
            base.FinalizeLoad();

            _discovery = Generator as Discovery;
        }
    }

    public static Dictionary<string, Discovery> Discoveries;

    public static int CurrentUId = 0;
    
    public string EventGeneratorId;

    public int IdHash;
    public int UId; // Do not use as seed or part of (no consistency guarantee after reload). TODO: Get rid of it if possible
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

    public static Discovery GetDiscovery(string id)
    {
        Discovery d;

        if (!Discoveries.TryGetValue(id, out d))
        {
            return null;
        }

        return d;
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

        if (GainConditions == null)
            return true;

        foreach (Condition condition in GainConditions)
        {
            if (!condition.Evaluate(group))
                return false;
        }

        return true;
    }

    public override bool CanBeHeld(CellGroup group)
    {
        if (HoldConditions == null)
            return true;

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

        if (EventTimeToTriggerFactors != null)
        {
            foreach (Factor factor in EventTimeToTriggerFactors)
            {
                dateSpan *= factor.Calculate(group);
            }
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

    public string GetEventGeneratorId()
    {
        return EventGeneratorId;
    }

    public override void OnGain(CellGroup group)
    {
        base.OnGain(group);

        if (GainEffects == null)
            return;

        foreach (Effect effect in GainEffects)
        {
            effect.Apply(group);
        }
    }

    public override void OnLoss(CellGroup group)
    {
        base.OnLoss(group);

        if (LossEffects != null)
        {
            foreach (Effect effect in LossEffects)
            {
                effect.Apply(group);
            }
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
