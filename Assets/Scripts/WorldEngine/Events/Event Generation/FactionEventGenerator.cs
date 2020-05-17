using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionEventGenerator : EventGenerator, IFactionEventGenerator
{
    private readonly FactionEntity _target;

    public FactionEventGenerator()
    {
        _target = new FactionEntity(TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(_target);
    }

    public override void SetToAssignOnSpawn()
    {
        Faction.OnSpawnEventGenerators.Add(this);
    }

    public override void SetToAssignOnEvent()
    {
        // Normally there's nothing to do here as all events
        // can be assigned by other events by default
    }

    public override void SetToAssignOnStatusChange()
    {
        Faction.OnStatusChangeEventGenerators.Add(this);
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        FactionModEvent modEvent = new FactionModEvent(this, _target.Faction, triggerDate);

        return modEvent;
    }

    public void SetTarget(Faction faction)
    {
        Reset();

        _target.Set(faction);
    }

    public override float GetNextRandomInt(int iterOffset, int maxValue) =>
        _target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Faction.GetNextLocalRandomFloat(iterOffset);

    public bool TryGenerateEventAndAssign(
        Faction faction,
        WorldEvent originalEvent = null,
        bool reassigning = false)
    {
        if (!reassigning && faction.IsFlagSet(EventSetFlag))
        {
            return false;
        }

        SetTarget(faction);

        return TryGenerateEventAndAssign(faction.World, originalEvent);
    }

    public bool TryReasignEvent(FactionModEvent modEvent)
    {
        if (!Repeteable)
        {
            return false;
        }

        return TryGenerateEventAndAssign(modEvent.Faction, modEvent, true);
    }
}
