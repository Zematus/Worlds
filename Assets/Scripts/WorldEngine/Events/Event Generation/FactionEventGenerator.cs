using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionEventGenerator : EventGenerator
{
    private readonly FactionEntity _target;

    public FactionEventGenerator()
    {
        _target = new FactionEntity(TargetEntityId);

        // Add the target to the context's entity map
        Entities.Add(TargetEntityId, _target);
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        FactionModEvent modEvent = new FactionModEvent(this, _target.Faction, triggerDate);

        return modEvent;
    }

    public void SetTarget(Faction faction) => _target.Set(faction);

    public override float GetNextRandomInt(int iterOffset, int maxValue) =>
        _target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Faction.GetNextLocalRandomFloat(iterOffset);

    public bool TryGenerateEventAndAssign(Faction faction)
    {
        SetTarget(faction);

        return TryGenerateEventAndAssign(faction.World);
    }

    public bool TryReasignEvent(FactionModEvent modEvent)
    {
        if (!Repeteable)
        {
            return false;
        }

        SetTarget(modEvent.Faction);

        World world = modEvent.Faction.World;

        if (!CanAssignEventToTarget())
        {
            return false;
        }

        long triggerDate = CalculateEventTriggerDate(world);

        if (triggerDate < 0)
        {
            // Do not generate an event. CalculateTriggerDate() should have logged a reason why
            return false;
        }

        modEvent.Reset(triggerDate);

        world.InsertEventToHappen(modEvent);

        return true;
    }
}
