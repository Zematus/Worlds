using System.Collections.Generic;
using UnityEngine.Profiling;

public class FactionEventGenerator : EventGenerator, IFactionEventGenerator
{
    public readonly FactionEntity Target;

    private float _onCoreGroupProminenceValueFallsBelow_parameterValue;
    private readonly HashSet<Faction> _onCoreGroupProminenceValueFallsBelow_factionsToTest = new HashSet<Faction>();

    private readonly Dictionary<string, float> _onKnowledgeLevelFallsBelow_parameterValues = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _onKnowledgeLevelRaisesAbove_parameterValues = new Dictionary<string, float>();

    private readonly Dictionary<string, HashSet<Faction>> _onKnowledgeLevelFallsBelow_factionsToTest = new Dictionary<string, HashSet<Faction>>();
    private readonly Dictionary<string, HashSet<Faction>> _onKnowledgeLevelRaisesAbove_factionsToTest = new Dictionary<string, HashSet<Faction>>();

    protected override void SetTargetToUpdate()
    {
        Target.Faction.SetToUpdate();
    }

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

    public bool TestOnKnowledgeLevelFallsBelow(string knowledge, Faction faction, float scaledValue)
    {
        if (scaledValue >= _onKnowledgeLevelFallsBelow_parameterValues[knowledge])
        {
            _onKnowledgeLevelFallsBelow_factionsToTest[knowledge].Add(faction);
            return false;
        }
        else if (!_onKnowledgeLevelFallsBelow_factionsToTest[knowledge].Contains(faction))
        {
            return false;
        }

        _onKnowledgeLevelFallsBelow_factionsToTest[knowledge].Remove(faction);

        return true;
    }

    public bool TestOnKnowledgeLevelRaisesAbove(string knowledge, Faction faction, float scaledValue)
    {
        if (scaledValue <= _onKnowledgeLevelRaisesAbove_parameterValues[knowledge])
        {
            _onKnowledgeLevelRaisesAbove_factionsToTest[knowledge].Add(faction);
            return false;
        }
        else if (!_onKnowledgeLevelRaisesAbove_factionsToTest[knowledge].Contains(faction))
        {
            return false;
        }

        _onKnowledgeLevelRaisesAbove_factionsToTest[knowledge].Remove(faction);

        return true;
    }

    public void RemoveReferences(Faction faction)
    {
        _onCoreGroupProminenceValueFallsBelow_factionsToTest.Remove(faction);

        foreach (var factionsToTestSet in _onKnowledgeLevelFallsBelow_factionsToTest.Values)
        {
            factionsToTestSet.Remove(faction);
        }

        foreach (var factionsToTestSet in _onKnowledgeLevelRaisesAbove_factionsToTest.Values)
        {
            factionsToTestSet.Remove(faction);
        }
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
        _onKnowledgeLevelFallsBelow_factionsToTest[knowledgeId] = new HashSet<Faction>();

        if (!Faction.OnKnowledgeLevelFallsBelowEventGenerators.ContainsKey(knowledgeId))
        {
            Faction.OnKnowledgeLevelFallsBelowEventGenerators.Add(knowledgeId, new List<IWorldEventGenerator>());
        }

        Faction.OnKnowledgeLevelFallsBelowEventGenerators[knowledgeId].Add(this);
        Faction.EventGeneratorsThatNeedCleanup.Add(this);
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
        _onKnowledgeLevelRaisesAbove_factionsToTest[knowledgeId] = new HashSet<Faction>();

        if (!Faction.OnKnowledgeLevelRaisesAboveEventGenerators.ContainsKey(knowledgeId))
        {
            Faction.OnKnowledgeLevelRaisesAboveEventGenerators.Add(knowledgeId, new List<IWorldEventGenerator>());
        }

        Faction.OnKnowledgeLevelRaisesAboveEventGenerators[knowledgeId].Add(this);
        Faction.EventGeneratorsThatNeedCleanup.Add(this);
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

        if (!Faction.OnGainedDiscoveryEventGenerators.ContainsKey(discoveryId))
        {
            Faction.OnGainedDiscoveryEventGenerators.Add(discoveryId, new List<IWorldEventGenerator>());
        }

        Faction.OnGainedDiscoveryEventGenerators[discoveryId].Add(this);
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
