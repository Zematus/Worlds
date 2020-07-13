using UnityEngine;
using System.Collections.Generic;

public class ModDecision : Context
{
    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

    private int _randomOffset;

    private readonly FactionEntity _target;

    private Entity[] _parameterEntities;

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
        if (targetStr != FactionTargetType)
        {
            throw new System.ArgumentException("Invalid target type: " + targetStr);
        }

        IdHash = id.GetHashCode();

        _randomOffset = IdHash;

        _target = new FactionEntity(this, TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(_target);
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
        _target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Faction.GetNextLocalRandomFloat(iterOffset);

    public override int GetBaseOffset() => _target.Faction.GetHashCode();

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

    public void Set(Faction targetFaction, IBaseValueExpression[] parameters)
    {
        Reset();

        _target.Set(targetFaction);

        if (_parameterEntities == null) // we are expecting no parameters
        {
            return;
        }

        if (parameters == null)
        {
            throw new System.Exception(
                "No parameters given to decision '" + Id + "' when expected " + _parameterEntities.Length);
        }

        if (parameters.Length < _parameterEntities.Length)
        {
            throw new System.Exception(
                "Number of parameters given to decision '" + Id +
                "', " + parameters.Length + ", below minimum expected " + _parameterEntities.Length);
        }

        for (int i = 0; i < _parameterEntities.Length; i++)
        {
            _parameterEntities[i].Set(
                parameters[i].ValueObject,
                parameters[i].ToPartiallyEvaluatedString);
        }
    }

    /// <summary>
    /// This function will be called by the AI when the target faction is not controlled by a player
    /// </summary>
    private void AutoEvaluate()
    {
        float totalWeight = 0;
        float[] optionWeights = new float[Options.Length];

        // Calculate the current weights for all options
        for (int i = 0; i < Options.Length; i++)
        {
            DecisionOption option = Options[i];

            // If option is not available set its weight to 0
            float weight = 0;
            if (option.CanShow())
            {
                weight = (option.Weight != null ) ? option.Weight.Value : 1;
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

            totalWeight += weight;
            optionWeights[i] = totalWeight;
        }

        if (totalWeight <= 0)
        {
            // Something went wrong, at least one option should be
            // available everytime we evaluate a decision

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
    }

    public void Evaluate()
    {
        Faction targetFaction = _target.Faction;

        // Uncomment this line to test the decision dialog
        //Manager.SetGuidedFaction(targetFaction);

        targetFaction.CoreGroup.SetToUpdate();

        if (targetFaction.IsUnderPlayerGuidance)
        {
            targetFaction.World.AddDecisionToResolve(this);
        }
        else
        {
            AutoEvaluate();
        }
    }
}
