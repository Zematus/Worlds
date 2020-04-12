using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class used to load decision mod entries from mod JSON files.
/// Class properties should match the root structure of the JSON file.
/// </summary>
[Serializable]
public class DecisionLoader
{
#pragma warning disable 0649 // Disable warning for unitialized properties...

    public LoadedDecision[] decisions;

    [Serializable]
    public class LoadedDecision : Context.LoadedContext
    {
        [Serializable]
        public class LoadedParameter
        {
            public string id;
            public string type;
        }

        [Serializable]
        public class LoadedDescription
        {
            public string id;
            public string text;
        }

        [Serializable]
        public class LoadedOptionalDescription : LoadedDescription
        {
            public string[] conditions;
        }

        [Serializable]
        public class LoadedOption : LoadedOptionalDescription
        {
            [Serializable]
            public class LoadedEffect : LoadedDescription
            {
                public string result;
            }

            public string weight;
            public LoadedEffect[] effects;
        }

        public string id;
        public string name;
        public string target;
        public LoadedParameter[] parameters;
        public LoadedOptionalDescription[] description;
        public LoadedOption[] options;
    }

#pragma warning restore 0649

    public static IEnumerable<ModDecision> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        // Load json object from file into intermediary object
        DecisionLoader loader = JsonUtility.FromJson<DecisionLoader>(jsonStr);

        for (int i = 0; i < loader.decisions.Length; i++)
        {
            ModDecision decision;
            try
            {
                decision = CreateDecision(loader.decisions[i]);
            }
            catch (Exception e)
            {
                // If there's a failure while loading a decision entry, report
                // the file from which the event came from and its index within
                // the file...
                throw new Exception(
                    "Failure loading decision #" + i + " in " + filename + ": "
                    + e.Message, e);
            }

            yield return decision;
        }
    }

    private static void InitializeDescription(
        Description segment,
        LoadedDecision.LoadedDescription d)
    {
        if (string.IsNullOrEmpty(d.id))
        {
            throw new ArgumentException("description 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.text))
        {
            throw new ArgumentException("description 'text' can't be null or empty");
        }

        segment.Id = d.id;
        segment.Text = new ModText(segment, d.text);
    }

    private static void InitializeOptionalDescription(
        OptionalDescription segment,
        LoadedDecision.LoadedOptionalDescription od)
    {
        InitializeDescription(segment, od);

        IValueExpression<bool>[] conditions = null;

        if (od.conditions != null)
        {
            // Build the condition expressions (must evaluate to bool values)
            conditions = ValueExpressionBuilder.BuildValueExpressions<bool>(segment, od.conditions);
        }

        segment.Conditions = conditions;
    }

    private static OptionalDescription CreateDescriptionSegment(
        ModDecision decision,
        LoadedDecision.LoadedOptionalDescription ds)
    {
        OptionalDescription segment = new OptionalDescription(decision);

        InitializeOptionalDescription(segment, ds);

        return segment;
    }

    private static DecisionOptionEffect CreateOptionEffect(
        DecisionOption option,
        LoadedDecision.LoadedOption.LoadedEffect oe)
    {
        DecisionOptionEffect effect = new DecisionOptionEffect(option);

        InitializeDescription(effect, oe);

        IEffectExpression result = null;
        if (oe.result != null)
        {
            result = ExpressionBuilder.ValidateEffectExpression(
                ExpressionBuilder.BuildExpression(effect, oe.result));
        }

        effect.Result = result;

        return effect;
    }

    private static DecisionOption CreateOption(
        ModDecision decision,
        LoadedDecision.LoadedOption o)
    {
        DecisionOption option = new DecisionOption(decision);

        InitializeOptionalDescription(option, o);

        IValueExpression<float> weight = null;
        if (!string.IsNullOrWhiteSpace(o.weight))
        {
            weight = ValueExpressionBuilder.BuildValueExpression<float>(option, o.weight);
        }

        DecisionOptionEffect[] effects = null;
        if (o.effects != null)
        {
            effects = new DecisionOptionEffect[o.effects.Length];

            for (int i = 0; i < o.effects.Length; i++)
            {
                try
                {
                    effects[i] = CreateOptionEffect(option, o.effects[i]);
                }
                catch (Exception e)
                {
                    // If there's a failure while loading a option effect entry,
                    // report the index within the option...
                    throw new Exception(
                        "Failure loading option effect #" + i + " in option '" + o.id + "': "
                        + e.Message, e);
                }
            }
        }

        option.Weight = weight;
        option.Effects = effects;

        return option;
    }

    private static Entity[] CreateParameterEntities(LoadedDecision.LoadedParameter[] ps)
    {
        Entity[] entities = new Entity[ps.Length];

        for (int i = 0; i < ps.Length; i++)
        {
            entities[i] = CreateParameterEntity(ps[i]);
        }

        return entities;
    }

    private static Entity CreateParameterEntity(LoadedDecision.LoadedParameter p)
    {
        switch (p.type)
        {
            case "group":
                return new GroupEntity(p.id);

            case "faction":
                return new FactionEntity(p.id);

            case "polity":
                return new PolityEntity(p.id);

            case "cell":
                return new CellEntity(p.id);

            case "agent":
                return new AgentEntity(p.id);

            case "string":
                return new ValueEntity<string>(p.id);

            case "text":
                return new ValueEntity<string>(p.id);

            case "number":
                return new ValueEntity<float>(p.id);

            case "boolean":
                return new ValueEntity<bool>(p.id);

            default:
                throw new Exception("Unhandled parameter type: " + p.type);
        }
    }

    private static ModDecision CreateDecision(LoadedDecision d)
    {
        if (string.IsNullOrEmpty(d.id))
        {
            throw new ArgumentException("decision 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.name))
        {
            throw new ArgumentException("decision 'name' can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.target))
        {
            throw new ArgumentException("decision 'target' can't be null or empty");
        }

        if (d.description == null)
        {
            throw new ArgumentException("decision 'description' list can't be empty");
        }

        Entity[] parameterEntities = null;

        if (d.parameters != null)
        {
            parameterEntities = CreateParameterEntities(d.parameters);
        }

        ModDecision decision = new ModDecision(d.id, d.target, parameterEntities);
        decision.Initialize(d);

        OptionalDescription[] segments = new OptionalDescription[d.description.Length];

        for (int i = 0; i < d.description.Length; i++)
        {
            try
            {
                segments[i] = CreateDescriptionSegment(decision, d.description[i]);
            }
            catch (Exception e)
            {
                // If there's a failure while loading a description entry,
                // report the index within the decision...
                throw new Exception(
                    "Failure loading description segment #" + i + " in decision '" + d.id + "': "
                    + e.Message, e);
            }
        }

        DecisionOption[] options = new DecisionOption[d.options.Length];

        for (int i = 0; i < d.options.Length; i++)
        {
            try
            {
                options[i] = CreateOption(decision, d.options[i]);
            }
            catch (Exception e)
            {
                // If there's a failure while loading an option entry,
                // report the index within the decision...
                throw new Exception(
                    "Failure loading option #" + i + " in decision '" + d.id + "': "
                    + e.Message, e);
            }
        }

        decision.Name = d.name;
        decision.DescriptionSegments = segments;
        decision.Options = options;

        decision.FinishInitialization();

        return decision;
    }
}
