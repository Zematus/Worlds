using UnityEngine.Profiling;

public class FactionEventGenerator : EventGenerator, IFactionEventGenerator
{
    public readonly FactionEntity Target;

    public FactionEventGenerator()
    {
        Target = new FactionEntity(this, TargetEntityId);

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

    public override void SetToAssignOnPolityContactChange()
    {
        Polity.OnPolityContactChangeEventGenerators.Add(this);
    }

    public override void SetToAssignOnCoreHighestProminenceChange()
    {
        CellGroup.OnCoreHighestProminenceChangeEventGenerators.Add(this);
    }

    public override void SetToAssignOnRegionAccessibilityUpdate()
    {
        Polity.OnRegionAccessibilityUpdateEventGenerators.Add(this);
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
