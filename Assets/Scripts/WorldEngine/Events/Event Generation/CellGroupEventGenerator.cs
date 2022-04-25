using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroupEventGenerator : EventGenerator, ICellGroupEventGenerator
{
    private readonly GroupEntity _target;

    public readonly Dictionary<string, float> OnKnowledgeLevelBelowParameterValues = new Dictionary<string, float>();

    public CellGroupEventGenerator()
    {
        _target = new GroupEntity(this, TargetEntityId, null);

        // Add the target to the context's entity map
        AddEntity(_target);
    }

    public override void SetToAssignOnSpawn()
    {
        CellGroup.OnSpawnEventGenerators.Add(this);
    }

    public override void SetToAssignOnEvent()
    {
        // Normally there's nothing to do here as all events
        // can be assigned by other events by default
    }

    public override void SetToAssignOnPolityCountChange()
    {
        CellGroup.OnPolityCountChangeEventGenerators.Add(this);
    }

    public override void SetToAssignOnCoreCountChange()
    {
        CellGroup.OnCoreCountChangeEventGenerators.Add(this);
    }

    public override void SetToAssignOnStatusChange()
    {
        throw new System.InvalidOperationException(
            "OnAssign does not support 'status_change' for Cell Groups");
    }

    public override void SetToAssignOnContactChange()
    {
        throw new System.InvalidOperationException(
            "OnAssign does not support 'contact_change' for Cell Groups");
    }

    public override void SetToAssignOnCoreHighestProminenceChange()
    {
        throw new System.InvalidOperationException(
            "OnAssign does not support 'core_highest_prominence_change' for Cell Groups");
    }

    public override void SetToAssignOnRegionAccessibilityUpdate()
    {
        throw new System.InvalidOperationException(
            "OnAssign does not support 'region_accessibility_update' for Cell Groups");
    }

    public override void SetToAssignOnGuideSwitch()
    {
        throw new System.InvalidOperationException(
            "OnAssign does not support 'guide_switch' for Cell Groups");
    }

    public override void SetToAssignOnCoreGroupProminenceValueBelow(string[] valueStrs)
    {
        throw new System.InvalidOperationException(
            $"OnAssign does not support '{AssignOnCoreGroupProminenceValueBelow}' for Cell Groups");
    }

    public override void SetToAssignOnKnowledgeLevelBelow(string[] valueStrs)
    {
        if ((valueStrs == null) || (valueStrs.Length < 2))
        {
            throw new System.ArgumentException
                ($"invalid or no parameters for '{AssignOnKnowledgeLevelBelow}'");
        }

        var knowledgeId = valueStrs[0];
        var levelStr = valueStrs[1];

        if (string.IsNullOrWhiteSpace(knowledgeId))
        {
            throw new System.ArgumentException
                ($"knowledge id for '{AssignOnKnowledgeLevelBelow}' is empty");
        }

        if (!Knowledge.Knowledges.ContainsKey(knowledgeId))
        {
            throw new System.ArgumentException
                ($"knowledge id for '{AssignOnKnowledgeLevelBelow}' is not recognized: {knowledgeId}");
        }

        if (string.IsNullOrWhiteSpace(levelStr))
        {
            throw new System.ArgumentException
                ($"level value for '{AssignOnKnowledgeLevelBelow}' is empty");
        }

        if (!MathUtility.TryParseCultureInvariant(levelStr, out float level))
        {
            throw new System.ArgumentException
                ($"level value for '{AssignOnKnowledgeLevelBelow}' is not a valid number: {level}");
        }

        OnKnowledgeLevelBelowParameterValues[knowledgeId] = level;

        if (!CellGroup.OnKnowledgeLevelBelowEventGenerators.ContainsKey(knowledgeId))
        {
            CellGroup.OnKnowledgeLevelBelowEventGenerators.Add(knowledgeId, new List<IWorldEventGenerator>());
        }

        CellGroup.OnKnowledgeLevelBelowEventGenerators[knowledgeId].Add(this);
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        CellGroupModEvent modEvent =
            new CellGroupModEvent(this, _target.Group, triggerDate);

        return modEvent;
    }

    public void SetTarget(CellGroup group)
    {
        Reset();

        _target.Set(group);
    }

    public override int GetNextRandomInt(int iterOffset, int maxValue) =>
        _target.Group.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Group.GetNextLocalRandomFloat(iterOffset);

    public override int GetBaseOffset() => _target.Group.GetHashCode();

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

    protected override void AddTargetDebugOutput()
    {
        AddDebugOutput(
            $"\tTarget Group: {_target.Group.Id}");
    }
}
