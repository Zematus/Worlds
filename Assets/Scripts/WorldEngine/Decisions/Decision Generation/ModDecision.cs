using UnityEngine;
using System.Collections.Generic;

public class ModDecision : Context
{
    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

    private int _randomOffset;

    public readonly FactionEntity Target;

    private Faction _targetFaction;

    private Entity[] _parameterEntities;

    private IBaseValueExpression[] _parameterValues;

    public static Dictionary<string, ModDecision> Decisions;

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

    public ModDecision(string id, string targetStr)
    {
        DebugType = "Decision";

        if (targetStr != FactionTargetType)
        {
            throw new System.ArgumentException("Invalid target type: " + targetStr);
        }

        IdHash = id.GetHashCode();

        _randomOffset = IdHash;

        Target = new FactionEntity(this, TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(Target);
    }

    public void SetParameterEntities(Entity[] parameterEntities)
    {
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

    public override int GetNextRandomInt(int iterOffset, int maxValue) =>
        Target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        Target.Faction.GetNextLocalRandomFloat(iterOffset);

    public override int GetBaseOffset() => Target.Faction.GetHashCode();

    public void Set(Faction targetFaction, IBaseValueExpression[] parameterValues)
    {
        _targetFaction = targetFaction;
        _parameterValues = parameterValues;

        // Uncomment this line to test the decision dialog
        //Manager.SetGuidedFaction(targetFaction);

        targetFaction.CoreGroup.SetToUpdate();
        targetFaction.World.AddDecisionToResolve(this);
    }

    public override void Reset()
    {
        base.Reset();

        foreach (OptionalDescription d in DescriptionSegments)
        {
            d.Reset();
        }

        foreach (DecisionOption o in Options)
        {
            o.Reset();
        }
    }

    public void InitEvaluation()
    {
        Reset();

        Target.Set(_targetFaction);

        if (_parameterEntities == null) // we are expecting no parameters
        {
            return;
        }

        if (_parameterValues == null)
        {
            throw new System.Exception(
                "No parameters given to decision '" + Id + "' when expected " + _parameterEntities.Length);
        }

        if (_parameterValues.Length < _parameterEntities.Length)
        {
            throw new System.Exception(
                "Number of parameters given to decision '" + Id +
                "', " + _parameterValues.Length + ", below minimum expected " + _parameterEntities.Length);
        }

        OpenDebugOutput("Setting Decision Parameters:");

        for (int i = 0; i < _parameterEntities.Length; i++)
        {
            AddExpDebugOutput("Parameter '" + _parameterEntities[i].Id + "'", _parameterValues[i]);

            _parameterEntities[i].Set(
                _parameterValues[i].ValueObject,
                _parameterValues[i].ToPartiallyEvaluatedString);
        }

        CloseDebugOutput();
    }

    /// <summary>
    /// This function will be used by the AI when the target faction is not controlled by a player
    /// </summary>
    public void AutoEvaluate()
    {
        float totalWeight = 0;
        float[] optionWeights = new float[Options.Length];

        OpenDebugOutput("Auto Evaluating Decision:");

        // Calculate the current weights for all options
        for (int i = 0; i < Options.Length; i++)
        {
            DecisionOption option = Options[i];

            // Set weight to 0 for options that are meant to be used only by a human
            // player, or can't be currently shown or used
            float weight = 0;

            OpenDebugOutput("Testing option '" + option.Id + "':" +
                "\n  Allowed guide: " + option.AllowedGuide);

            if ((option.AllowedGuide != GuideType.Player) && option.CanShow())
            {
                if (option.Weight != null)
                {
                    weight = option.Weight.Value;

                    AddExpDebugOutput("Weight", option.Weight);
                }
                else
                {
                    weight = 1;

                    AddDebugOutput("  Using default weight: 1");
                }

                if (weight < 0)
                {
                    string weightPartialExpression =
                        (option.Weight != null) ?
                        ("\n - expression: " + option.Weight.ToPartiallyEvaluatedString(true)) :
                        string.Empty;

                    throw new System.Exception(
                        Id + "->" + option.Id + ", decision option weight is less than zero: " +
                        weightPartialExpression +
                        "\n - weight: " + weight);
                }
            }

            CloseDebugOutput();

            totalWeight += weight;
            optionWeights[i] = totalWeight;
        }

        AddDebugOutput("  Total options weight: " + totalWeight);

        if (totalWeight <= 0)
        {
            // Something went wrong, at least one option should be
            // available every time we evaluate a decision

            throw new System.Exception(
                Id + ", total decision option weight is equal or less than zero: " +
                "\n - total weight: " + totalWeight);
        }

        float randValue = GetNextRandomFloat(_randomOffset) * totalWeight;

        // Figure out which option we should apply
        int chossenIndex = 0;
        while (optionWeights[chossenIndex] < randValue)
        {
            chossenIndex++;
        }

        DecisionOptionEffect[] effects = Options[chossenIndex].Effects;

        if (effects != null)
        {
            // Apply all option effects
            foreach (DecisionOptionEffect effect in effects)
            {
                effect.Result.Apply();
            }
        }

        CloseDebugOutput();
    }
}
