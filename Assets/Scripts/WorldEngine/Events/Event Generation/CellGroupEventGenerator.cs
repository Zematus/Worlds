using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroupEventGenerator : EventGenerator, ICellGroupEventGenerator
{
    private readonly GroupEntity _target;

    public CellGroupEventGenerator()
    {
        _target = new GroupEntity(TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(_target);
    }

    public override void Initialize()
    {
        base.Initialize();

        CellGroup.OnSpawnEventGenerators.Add(this);
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        CellGroupModEvent modEvent =
            new CellGroupModEvent(this, _target.Group, triggerDate);

        return modEvent;
    }

    public void SetTarget(CellGroup group) => _target.Set(group);

    public override float GetNextRandomInt(int iterOffset, int maxValue) =>
        _target.Group.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Group.GetNextLocalRandomFloat(iterOffset);

    public bool TryGenerateEventAndAssign(
        CellGroup group,
        WorldEvent originalEvent = null,
        bool reassign = false)
    {
        if (!reassign && group.IsFlagSet(EventSetFlag))
        {
            return false;
        }

        SetTarget(group);

        return TryGenerateEventAndAssign(group.World, originalEvent);
    }

    public bool TryReasignEvent(CellGroupModEvent modEvent)
    {
        if (!Repeteable)
        {
            return false;
        }

        return TryGenerateEventAndAssign(modEvent.Group, modEvent, true);
    }
}
