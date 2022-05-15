using System.Collections.Generic;
using UnityEngine.Profiling;

public class FactionEventGenerator : EventGenerator, IFactionEventGenerator
{
    public readonly FactionEntity Target;

    private float _onCoreGroupProminenceValueFallsBelow_parameterValue;
    private readonly HashSet<Faction> _onCoreGroupProminenceValueFallsBelow_factionsToTest = new HashSet<Faction>();

    public bool TestOnCoreGroupProminenceValueFallsBelow(Faction faction, float prominenceValue)
    {
        if (prominenceValue >= _onCoreGroupProminenceValueFallsBelow_parameterValue)
        {
            _onCoreGroupProminenceValueFallsBelow_factionsToTest.Add(faction);
            return false;
        }
        else if (!_onCoreGroupProminenceValueFallsBelow_factionsToTest.Contains(faction))
        {
            return false;
        }

        _onCoreGroupProminenceValueFallsBelow_factionsToTest.Remove(faction);

        return true;
    }

    public void RemoveReferences(Faction faction)
    {
        _onCoreGroupProminenceValueFallsBelow_factionsToTest.Remove(faction);
    }

    public FactionEventGenerator()
    {
        Target = new FactionEntity(this, TargetEntityId, null);

        // Add the target to the context's entity map
        AddEntity(Target);
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

    public override void SetToAssignOnContactChange()
    {
        Polity.OnContactChangeEventGenerators.Add(this);
    }

    public override void SetToAssignOnCoreHighestProminenceChange()
    {
        CellGroup.OnCoreHighestProminenceChangeEventGenerators.Add(this);
    }

    public override void SetToAssignOnRegionAccessibilityUpdate()
    {
        Polity.OnRegionAccessibilityUpdateEventGenerators.Add(this);
    }

    public override void SetToAssignOnGuideSwitch()
    {
        Faction.OnGuideSwitchEventGenerators.Add(this);
    }

    public override void SetToAssignOnPolityCountChange()
    {
        throw new System.InvalidOperationException(
            "OnAssign does not support 'polity_count_change' for Faction");
    }

    public override void SetToAssignOnCoreCountChange()
    {
        throw new System.InvalidOperationException(
            "OnAssign does not support 'core_count_change' for Factions");
    }

    public override void SetToAssignOnCoreGroupProminenceValueFallsBelow(string[] valueStrs)
    {
        if ((valueStrs == null) || (valueStrs.Length < 1))
        {
            throw new System.ArgumentException
                ($"parameter for '{AssignOnCoreGroupProminenceValueFallsBelow}' is empty");
        }

        var valueStr = valueStrs[0];

        if (string.IsNullOrWhiteSpace(valueStr))
        {
            throw new System.ArgumentException
                ($"parameter for '{AssignOnCoreGroupProminenceValueFallsBelow}' is empty");
        }

        if (!MathUtility.TryParseCultureInvariant(valueStr, out float value))
        {
            throw new System.ArgumentException
                ($"parameter for '{AssignOnCoreGroupProminenceValueFallsBelow}' is not a valid number: {valueStr}");
        }

        if (!value.IsInsideRange(0, 1))
        {
            throw new System.ArgumentException
                ($"parameter for '{AssignOnCoreGroupProminenceValueFallsBelow}', '{valueStr}' is not a value between 0 and 1");
        }

        _onCoreGroupProminenceValueFallsBelow_parameterValue = value;

        Faction.OnCoreGroupProminenceValueFallsBelowEventGenerators.Add(this);
        Faction.EventGeneratorsThatNeedCleanup.Add(this);
    }

    public override void SetToAssignOnKnowledgeLevelFallsBelow(string[] valueStrs)
    {
        throw new System.InvalidOperationException(
            $"OnAssign does not support '{AssignOnKnowledgeLevelFallsBelow}' for Factions");
    }

    public override void SetToAssignOnKnowledgeLevelRaisesAbove(string[] valueStrs)
    {
        throw new System.InvalidOperationException(
            $"OnAssign does not support '{AssignOnKnowledgeLevelRaisesAbove}' for Factions");
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        FactionModEvent modEvent = new FactionModEvent(this, Target.Faction, triggerDate);

        return modEvent;
    }

    public void SetTarget(Faction faction)
    {
        Profiler.BeginSample("FactionEventGenerator.SetTarget - Reset");

        Reset();

        Profiler.EndSample(); // "FactionEventGenerator.SetTarget - Reset"

        Profiler.BeginSample("FactionEventGenerator.SetTarget - Target.Set");

        Target.Set(faction);

        Profiler.EndSample(); // "FactionEventGenerator.SetTarget - Target.Set"
    }

    public override int GetNextRandomInt(int iterOffset, int maxValue) =>
        Target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        Target.Faction.GetNextLocalRandomFloat(iterOffset);

    public override int GetBaseOffset() => Target.Faction.GetHashCode();

    public bool TryGenerateEventAndAssign(
        Faction faction,
        WorldEvent originalEvent = null,
        bool reassigning = false)
    {
        if (faction.BeingRemoved)
            return false;

        if (!reassigning && faction.IsFlagSet(EventSetFlag))
            return false;

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

    protected override void AddTargetDebugOutput()
    {
        AddDebugOutput(
            $"\tTarget Faction: {Target.Faction.Name}");
    }
}
