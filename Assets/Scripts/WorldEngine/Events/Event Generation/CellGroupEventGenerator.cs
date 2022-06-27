using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroupEventGenerator : EventGenerator, ICellGroupEventGenerator
{
    public readonly GroupEntity Target;

    private readonly Dictionary<string, float> _onKnowledgeLevelFallsBelow_parameterValues = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _onKnowledgeLevelRaisesAbove_parameterValues = new Dictionary<string, float>();

    private readonly Dictionary<string, HashSet<CellGroup>> _onKnowledgeLevelFallsBelow_groupsToTest = new Dictionary<string, HashSet<CellGroup>>();
    private readonly Dictionary<string, HashSet<CellGroup>> _onKnowledgeLevelRaisesAbove_groupsToTest = new Dictionary<string, HashSet<CellGroup>>();

    public bool TestOnKnowledgeLevelFallsBelow(string knowledge, CellGroup group, float scaledValue)
    {
        if (scaledValue >= _onKnowledgeLevelFallsBelow_parameterValues[knowledge])
        {
            _onKnowledgeLevelFallsBelow_groupsToTest[knowledge].Add(group);
            return false;
        }
        else if (!_onKnowledgeLevelFallsBelow_groupsToTest[knowledge].Contains(group))
        {
            return false;
        }

        _onKnowledgeLevelFallsBelow_groupsToTest[knowledge].Remove(group);

        return true;
    }

    protected override void SetTargetToUpdate()
    {
        Target.Group.SetToUpdate();
    }

    public bool TestOnKnowledgeLevelRaisesAbove(string knowledge, CellGroup group, float scaledValue)
    {
        if (scaledValue <= _onKnowledgeLevelRaisesAbove_parameterValues[knowledge])
        {
            _onKnowledgeLevelRaisesAbove_groupsToTest[knowledge].Add(group);
            return false;
        }
        else if (!_onKnowledgeLevelRaisesAbove_groupsToTest[knowledge].Contains(group))
        {
            return false;
        }

        _onKnowledgeLevelRaisesAbove_groupsToTest[knowledge].Remove(group);

        return true;
    }

    public void RemoveReferences(CellGroup group)
    {
        foreach (var groupsToTestSet in _onKnowledgeLevelFallsBelow_groupsToTest.Values)
        {
            groupsToTestSet.Remove(group);
        }

        foreach (var groupsToTestSet in _onKnowledgeLevelRaisesAbove_groupsToTest.Values)
        {
            groupsToTestSet.Remove(group);
        }
    }

    public CellGroupEventGenerator()
    {
        Target = new GroupEntity(this, TargetEntityId, null);

        // Add the target to the context's entity map
        AddEntity(Target);
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

    public override void SetToAssignOnCoreGroupProminenceValueFallsBelow(string[] valueStrs)
    {
        throw new System.InvalidOperationException(
            $"OnAssign does not support '{AssignOnCoreGroupProminenceValueFallsBelow}' for Cell Groups");
    }

    public override void SetToAssignOnKnowledgeLevelFallsBelow(string[] valueStrs)
    {
        if ((valueStrs == null) || (valueStrs.Length < 2))
        {
            throw new System.ArgumentException
                ($"invalid or no parameters for '{AssignOnKnowledgeLevelFallsBelow}'");
        }

        var knowledgeId = valueStrs[0];
        var levelStr = valueStrs[1];

        if (string.IsNullOrWhiteSpace(knowledgeId))
        {
            throw new System.ArgumentException
                ($"knowledge id for '{AssignOnKnowledgeLevelFallsBelow}' is empty");
        }

        if (!Knowledge.Knowledges.ContainsKey(knowledgeId))
        {
            throw new System.ArgumentException
                ($"knowledge id for '{AssignOnKnowledgeLevelFallsBelow}' is not recognized: {knowledgeId}");
        }

        if (string.IsNullOrWhiteSpace(levelStr))
        {
            throw new System.ArgumentException
                ($"level value for '{AssignOnKnowledgeLevelFallsBelow}' is empty");
        }

        if (!MathUtility.TryParseCultureInvariant(levelStr, out float level))
        {
            throw new System.ArgumentException
                ($"level value for '{AssignOnKnowledgeLevelFallsBelow}' is not a valid number: {level}");
        }

        _onKnowledgeLevelFallsBelow_parameterValues[knowledgeId] = level;
        _onKnowledgeLevelFallsBelow_groupsToTest[knowledgeId] = new HashSet<CellGroup>();

        if (!CellGroup.OnKnowledgeLevelFallsBelowEventGenerators.ContainsKey(knowledgeId))
        {
            CellGroup.OnKnowledgeLevelFallsBelowEventGenerators.Add(knowledgeId, new List<IWorldEventGenerator>());
        }

        CellGroup.OnKnowledgeLevelFallsBelowEventGenerators[knowledgeId].Add(this);
        CellGroup.EventGeneratorsThatNeedCleanup.Add(this);
    }

    public override void SetToAssignOnKnowledgeLevelRaisesAbove(string[] valueStrs)
    {
        if ((valueStrs == null) || (valueStrs.Length < 2))
        {
            throw new System.ArgumentException
                ($"invalid or no parameters for '{AssignOnKnowledgeLevelRaisesAbove}'");
        }

        var knowledgeId = valueStrs[0];
        var levelStr = valueStrs[1];

        if (string.IsNullOrWhiteSpace(knowledgeId))
        {
            throw new System.ArgumentException
                ($"knowledge id for '{AssignOnKnowledgeLevelRaisesAbove}' is empty");
        }

        if (!Knowledge.Knowledges.ContainsKey(knowledgeId))
        {
            throw new System.ArgumentException
                ($"knowledge id for '{AssignOnKnowledgeLevelRaisesAbove}' is not recognized: {knowledgeId}");
        }

        if (string.IsNullOrWhiteSpace(levelStr))
        {
            throw new System.ArgumentException
                ($"level value for '{AssignOnKnowledgeLevelRaisesAbove}' is empty");
        }

        if (!MathUtility.TryParseCultureInvariant(levelStr, out float level))
        {
            throw new System.ArgumentException
                ($"level value for '{AssignOnKnowledgeLevelRaisesAbove}' is not a valid number: {level}");
        }

        _onKnowledgeLevelRaisesAbove_parameterValues[knowledgeId] = level;
        _onKnowledgeLevelRaisesAbove_groupsToTest[knowledgeId] = new HashSet<CellGroup>();

        if (!CellGroup.OnKnowledgeLevelRaisesAboveEventGenerators.ContainsKey(knowledgeId))
        {
            CellGroup.OnKnowledgeLevelRaisesAboveEventGenerators.Add(knowledgeId, new List<IWorldEventGenerator>());
        }

        CellGroup.OnKnowledgeLevelRaisesAboveEventGenerators[knowledgeId].Add(this);
        CellGroup.EventGeneratorsThatNeedCleanup.Add(this);
    }

    public override void SetToAssignOnGainedDiscovery(string[] valueStrs)
    {
        if ((valueStrs == null) || (valueStrs.Length < 1))
        {
            throw new System.ArgumentException
                ($"invalid or no parameters for '{AssignOnGainedDiscovery}'");
        }

        var discoveryId = valueStrs[0];

        if (string.IsNullOrWhiteSpace(discoveryId))
        {
            throw new System.ArgumentException
                ($"discovery id for '{AssignOnGainedDiscovery}' is empty");
        }

        if (!Discovery.Discoveries.ContainsKey(discoveryId))
        {
            throw new System.ArgumentException
                ($"discovery id for '{AssignOnGainedDiscovery}' is not recognized: {discoveryId}");
        }

        if (!CellGroup.OnGainedDiscoveryEventGenerators.ContainsKey(discoveryId))
        {
            CellGroup.OnGainedDiscoveryEventGenerators.Add(discoveryId, new List<IWorldEventGenerator>());
        }

        CellGroup.OnGainedDiscoveryEventGenerators[discoveryId].Add(this);
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        CellGroupModEvent modEvent =
            new CellGroupModEvent(this, Target.Group, triggerDate);

        return modEvent;
    }

    public void SetTarget(CellGroup group)
    {
        Reset();

        Target.Set(group);
    }

    public override int GetNextRandomInt(int iterOffset, int maxValue) =>
        Target.Group.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        Target.Group.GetNextLocalRandomFloat(iterOffset);

    public override int GetBaseOffset() => Target.Group.GetHashCode();

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
            $"\tTarget Group: {Target.Group.Id}");
    }
}
