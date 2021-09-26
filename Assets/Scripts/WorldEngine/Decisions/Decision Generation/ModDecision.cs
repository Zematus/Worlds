using UnityEngine;
using System.Collections.Generic;

public class ModDecision : Context
{
    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

    private int _randomOffset;

    public readonly FactionEntity Target;

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

    public Context SourceContext { get; private set; }

    public IEffectTrigger Trigger { get; private set; }

    public bool DebugPlayerGuidance = false;

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

    public void Set(
        IEffectTrigger trigger,
        Context sourceContext,
        string triggerPrio,
        Faction targetFaction,
        IBaseValueExpression[] parameterValues)
    {
        var initializer =
            new ModDecisionData(
                this, trigger, sourceContext, targetFaction, parameterValues);

        targetFaction.World.AddDecisionToResolve(initializer, triggerPrio);
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

    public void InitEvaluation(ModDecisionData initializer)
    {
//#if DEBUG
//        if (Id == "clan_decide_split")
//        {
//            // This line will force the decision dialog to be displayed and
//            // evaluated by the player when debugging
//            Manager.SetGuidedFaction(initializer.TargetFaction);
//        }
//#endif

        Reset();

        Trigger = initializer.Trigger;
        SourceContext = initializer.SourceContext;

        initializer.TargetFaction.CoreGroup.SetToUpdate();

        Target.Set(initializer.TargetFaction);

        if (_parameterEntities == null) // we are expecting no parameters
        {
            return;
        }

        var parameterValues = initializer.ParameterValues;
        int valueCount = (parameterValues != null) ? parameterValues.Length : 0;

        OpenDebugOutput("Setting Decision Parameters:");

        for (int i = 0; i < _parameterEntities.Length; i++)
        {
            var entity = _parameterEntities[i];

            if (i >= valueCount)
            {
                if (!entity.HasDefaultValue)
                {
                    throw new System.Exception(
                        $"Parameter '{entity.Id}' is not set and has no default value");
                }

                AddValueDebugOutput($"Parameter '{entity.Id}'", entity.GetDefaultValue());

                entity.UseDefaultValue();

                continue;
            }

            var valueExp = parameterValues[i];

            AddExpDebugOutput($"Parameter '{entity.Id}'", valueExp);

            entity.Set(
                valueExp.ValueObject,
                valueExp.ToPartiallyEvaluatedString);
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
                        $"\n - expression: {option.Weight.ToPartiallyEvaluatedString(0)}" :
                        string.Empty;

                    throw new System.Exception(
                        $"{Id}->{option.Id}, decision option weight is less than " +
                        $"zero: {weightPartialExpression}\n - weight: {weight}");
                }
            }
            else
            {
                AddDebugOutput("  Option not allowed");
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

        AddDebugOutput("  Value rolled: " + randValue);

        // Figure out which option we should apply
        int chossenIndex = 0;
        while (optionWeights[chossenIndex] < randValue)
        {
            chossenIndex++;
        }

        DecisionOption chossenOption = Options[chossenIndex];

        AddDebugOutput("  Randomly picked option: " + chossenOption.Id);

        DecisionOptionEffect[] effects = chossenOption.Effects;

        if (effects != null)
        {
            OpenDebugOutput("Applying simulation chosen decision option effects:");

            // Apply all option effects
            foreach (DecisionOptionEffect effect in effects)
            {
                AddExpDebugOutput("Effect", effect.Result);

                effect.Result.Trigger = Trigger;
                effect.Result.Apply();
            }

            CloseDebugOutput();
        }

        CloseDebugOutput();
    }
}
