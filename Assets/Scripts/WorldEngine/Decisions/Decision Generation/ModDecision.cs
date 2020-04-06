using UnityEngine;
using System.Collections.Generic;

public class ModDecision : Context
{
    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

    private readonly FactionEntity _target;

    private readonly Entity[] _parameterEntities;

    public static Dictionary<string, ModDecision> Decisions;

    /// <summary>
    /// String Id for this decision
    /// </summary>
    public string Id;
    /// <summary>
    /// Name to use in the UI for this decision
    /// </summary>
    public string Name;

    /// <summary>
    /// Hash to use for RNGs that use this decision
    /// </summary>
    public int IdHash;

    public OptionalDescription[] DescriptionSegments;
    public DecisionOption[] Options;

    public ModDecision(string targetStr, Entity[] parameterEntities)
    {
        if (targetStr != FactionTargetType)
        {
            throw new System.ArgumentException("Invalid target type: " + targetStr);
        }

        _target = new FactionEntity(TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(_target);

        _parameterEntities = parameterEntities;

        if (parameterEntities != null)
        {
            // Add the parameters to the context's entity map
            foreach (Entity p in parameterEntities)
            {
                AddEntity(p);
            }
        }
    }

    public static void ResetDecisions()
    {
        Decisions = new Dictionary<string, ModDecision>();
    }

    public static void LoadDecisionFile(string filename)
    {
        foreach (ModDecision decision in DecisionLoader.Load(filename))
        {
            if (Decisions.ContainsKey(decision.Id))
            {
                Decisions[decision.Id] = decision;
            }
            else
            {
                Decisions.Add(decision.Id, decision);
            }
        }
    }

    public override float GetNextRandomInt(int iterOffset, int maxValue) =>
        _target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Faction.GetNextLocalRandomFloat(iterOffset);

    public void Set(Faction targetFaction, object[] parameters)
    {
        _target.Set(targetFaction);

        if (parameters.Length < _parameterEntities.Length)
        {
            throw new System.Exception(
                "Number of parameters given to decision '" + Id +
                "', " + parameters.Length + ", below minimum expected: " + _parameterEntities.Length);
        }

        for (int i = 0; i < _parameterEntities.Length; i++)
        {
            _parameterEntities[i].Set(parameters[i]);
        }
    }

    public void Evaluate()
    {
    }
}
