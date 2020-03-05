using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class GroupEventGenerator : EventGenerator
{
    private readonly GroupEntity _target;

    public GroupEventGenerator()
    {
        _target = new GroupEntity(TargetEntityId);

        // Add the target to the context's entity map
        Entities.Add(TargetEntityId, _target);
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        GroupModEvent modEvent = new GroupModEvent(this, _target.Group, triggerDate);

        return modEvent;
    }

    public void SetTarget(CellGroup group) => _target.Set(group);

    public override float GetNextRandomInt(int iterOffset, int maxValue) =>
        _target.Group.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Group.GetNextLocalRandomFloat(iterOffset);

    public bool TryGenerateEventAndAssign(CellGroup group)
    {
        SetTarget(group);

        return TryGenerateEventAndAssign(group.World);
    }

    public bool TryReasignEvent(GroupModEvent modEvent)
    {
        if (!Repeteable)
        {
            return false;
        }

        SetTarget(modEvent.Group);

        World world = modEvent.Group.World;

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
