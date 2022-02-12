using UnityEngine;
using UnityEngine.Profiling;

public class FactionEventGenerator : EventGenerator, IFactionEventGenerator
{
    public readonly FactionEntity Target;

    public float OnCoreGroupProminenceValueBelowParameterValue;

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

    public override void SetToAssignOnCoreGroupProminenceValueBelow(string valueStr)
    {
        if (string.IsNullOrWhiteSpace(valueStr))
        {
            throw new System.ArgumentException
                ($"parameter for 'core_group_prominence_value_below' is empty");
        }

        if (!MathUtility.TryParseCultureInvariant(valueStr, out float value))
        {
            throw new System.ArgumentException
                ($"parameter for 'core_group_prominence_value_below' is not a valid number: {valueStr}");
        }

        if (!value.IsInsideRange(0, 1))
        {
            throw new System.ArgumentException
                ($"parameter for 'core_group_prominence_value_below', '{valueStr}' is not a value between 0 and 1");
        }

        OnCoreGroupProminenceValueBelowParameterValue = value;

        Faction.OnCoreGroupProminenceValueBelowEventGenerators.Add(this);
    }

    protected override WorldEvent GenerateEvent(long triggerDate)
    {
        FactionModEvent modEvent = new FactionModEvent(this, Target.Faction, triggerDate);

        return modEvent;
    }

    public void SetTarget(Faction faction)
    {
        Profiler.BeginSample("FactionEventGenerator.SetTarget - Reset");

#if DEBUG
        if (Manager.CurrentWorld.CurrentDate == 181582635)
        {
            Debug.Log("Debugging FactionEventGenerator.SetTarget");
        }
#endif
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
