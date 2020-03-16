using UnityEngine;
using System.Collections.Generic;

public class ModDecision : Context
{
    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

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

    /// <summary>
    /// Conditions that decide if an event should be assigned to a target
    /// </summary>
    public IBooleanExpression[] AssignmentConditions;
    /// <summary>
    /// Conditions that decide if an event should trigger
    /// </summary>
    public IBooleanExpression[] TriggerConditions;

    /// <summary>
    /// First UId to use for decisions loaded from mods
    /// </summary>
    protected const long StartUId = 0;

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
