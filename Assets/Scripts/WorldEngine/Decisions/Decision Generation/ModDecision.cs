using UnityEngine;
using System.Collections.Generic;

public class ModDecision : Context
{
    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

    private readonly FactionEntity _target;

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

    public ModDecision(string targetStr)
    {
        if (targetStr != FactionTargetType)
        {
            throw new System.ArgumentException("Invalid target type: " + targetStr);
        }

        _target = new FactionEntity(TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(_target);
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

    public override float GetNextRandomFloat(int iterOffset)
    {
        throw new System.NotImplementedException();
    }

    public override float GetNextRandomInt(int iterOffset, int maxValue)
    {
        throw new System.NotImplementedException();
    }
}
