using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Discovery : ICellGroupEventGenerator
{
    public class Event : CellGroupEventGeneratorEvent
    {
        private Discovery _discovery;

        public Event()
        {
        }

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
                eventMessage = new DiscoveryEventMessage(_discovery, cell, _discovery.UId, TriggerDate);

                world.AddEventMessage(eventMessage);
            }

            if (cell.EncompassingTerritory != null)
            {
                Polity encompassingPolity = cell.EncompassingTerritory.Polity;

                if (!encompassingPolity.HasEventMessage(_discovery.UId))
                {
                    if (eventMessage == null)
                        eventMessage = new DiscoveryEventMessage(_discovery, cell, _discovery.UId, TriggerDate);

                    encompassingPolity.AddEventMessage(eventMessage);
                }
            }
        }

        public override void Trigger()
        {
            Group.Culture.AddDiscoveryToFind(_discovery);

            Group.World.AddGroupToUpdate(Group);

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
    
    public string Id;
    public string Name;

    public string EventGeneratorId;

    public int IdHash;
    public int UId; // Do not use as seed or part of (no consistency guarantee after reload). TODO: Get rid of it if possible
    public string EventSetFlag;

    public Condition[] GainConditions = null;
    public Condition[] HoldConditions = null;

    public Effect[] GainEffects = null;
    public Effect[] LossEffects = null;

    public long EventTimeToTrigger;
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

        InitializeOnConditions(GainConditions);
    }

    private void InitializeOnConditions(Condition[] conditions)
    {
        foreach (Condition c in conditions)
        {
            if ((c.ConditionType & ConditionType.Knowledge) == ConditionType.Knowledge)
            {
                InitializeOnKnowledgeCondition(c);
            }
        }
    }

    private void InitializeOnKnowledgeCondition(Condition c)
    {
        string knowledgeIds = c.GetPropertyValue(Condition.Property_KnowledgeId);

        if (knowledgeIds == null)
        {
            throw new System.Exception("Discovery: Knowledge condition doesn't reference any Knowledge Ids: " + c.ToString());
        }

        string[] knowledgeIdArray = c.GetPropertyValue(Condition.Property_KnowledgeId).Split(',');

        foreach (string kId in knowledgeIdArray)
        {
            Knowledge knowledge = Knowledge.GetKnowledge(kId);

            if (knowledge == null)
            {
                throw new System.Exception("Discovery: Unable to find knowledge with Id: " + kId);
            }

            knowledge.OnUpdateEventGenerators.Add(this);
        }
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

    public bool CanBeHeld(CellGroup group)
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
                float factorValue = factor.Calculate(group);

                dateSpan *= Mathf.Clamp01(factorValue);
            }
        }

        long targetDate = group.World.CurrentDate + (long)dateSpan + 1;

        if ((targetDate <= group.World.CurrentDate) || (targetDate > World.MaxSupportedDate))
        {
            // log details about invalid date
            Debug.LogWarning("Discovery+Event.CalculateTriggerDate - targetDate (" + targetDate + 
                ") less than or equal to World.CurrentDate (" + group.World.CurrentDate +
                "), randomFactor: " + randomFactor + 
                ", EventTimeToTrigger: " + EventTimeToTrigger +
                ", dateSpan: " + dateSpan);

            return long.MinValue;
        }

        return targetDate;
    }

    public CellGroupEvent GenerateAndAssignEvent(CellGroup group)
    {
        long triggerDate = CalculateTriggerDate(group);

        if (triggerDate < 0)
        {
            // Do not generate an event. CalculateTriggerDate() should have logged a reason why
            return null;
        }

        Event discoveryEvent = new Event(this, group, triggerDate, IdHash);

        group.World.InsertEventToHappen(discoveryEvent);

        return discoveryEvent;
    }

    public string GetEventGeneratorId()
    {
        return EventGeneratorId;
    }

    public void OnGain(CellGroup group)
    {
        if (GainEffects == null)
            return;

        foreach (Effect effect in GainEffects)
        {
            if (effect.IsDeferred())
            {
                effect.Defer(group);
                continue;
            }

            effect.Apply(group);
        }
    }

    public void OnLoss(CellGroup group)
    {
        if (LossEffects != null)
        {
            foreach (Effect effect in LossEffects)
            {
                if (effect.IsDeferred())
                {
                    effect.Defer(group);
                    continue;
                }

                effect.Apply(group);
            }
        }
    }

    public void RetryAssignAfterLoss(CellGroup group)
    {
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
