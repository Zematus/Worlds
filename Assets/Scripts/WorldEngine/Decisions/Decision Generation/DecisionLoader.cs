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
            public string defaultValue;
        }

        [Serializable]
        public class LoadedDescription : Context.LoadedContext
        {
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
            public string allowedGuide;
        }

        public string name;
        public string target;
        public LoadedParameter[] parameters;
        public LoadedOptionalDescription[] description;
        public LoadedOption[] options;
        public bool debugPlayerGuidance;
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
        segment.Initialize(ds);

        InitializeOptionalDescription(segment, ds);

        return segment;
    }

    private static DecisionOptionEffect CreateOptionEffect(
        DecisionOption option,
        LoadedDecision.LoadedOption.LoadedEffect oe)
    {
        DecisionOptionEffect effect = new DecisionOptionEffect(option);
        effect.Initialize(oe);

        InitializeDescription(effect, oe);

        IEffectExpression result = null;
        if (oe.result != null)
        {
            result = ExpressionBuilder.BuildEffectExpression(
                effect, oe.result, option.AllowedGuide == GuideType.Player);
        }

        effect.Result = result;

        return effect;
    }

    private static DecisionOption CreateOption(
        ModDecision decision,
        LoadedDecision.LoadedOption o)
    {
        DecisionOption option = new DecisionOption(decision);
        option.Initialize(o);

        InitializeOptionalDescription(option, o);

        IValueExpression<float> weight = null;
        if (!string.IsNullOrWhiteSpace(o.weight))
        {
            weight = ValueExpressionBuilder.BuildValueExpression<float>(option, o.weight);
        }

        if (!string.IsNullOrEmpty(o.allowedGuide))
        {
            switch (o.allowedGuide)
            {
                case Context.Guide_All:
                    option.AllowedGuide = GuideType.All;
                    break;
                case Context.Guide_Player:
                    option.AllowedGuide = GuideType.Player;
                    break;
                case Context.Guide_Simulation:
                    option.AllowedGuide = GuideType.Simulation;
                    break;
                default:
                    throw new System.ArgumentException(
                        "Not a valid allowed guide type: " + o.allowedGuide);
            }
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

    private static Entity[] CreateParameterEntities(Context c, LoadedDecision.LoadedParameter[] ps)
    {
        Entity[] entities = new Entity[ps.Length];

        for (int i = 0; i < ps.Length; i++)
        {
            entities[i] = CreateParameterEntity(c, ps[i]);
        }

        return entities;
    }

    private static Entity CreateParameterEntity(Context c, LoadedDecision.LoadedParameter p)
    {
        string dvExceptionMsg = $"'defaultValue' not supported for type: {p.type}";

        switch (p.type)
        {
            case "group":
                if (p.defaultValue != null) 
                    throw new Exception(dvExceptionMsg);

                return new GroupEntity(c, p.id, null);

            case "group_collection":
                if (p.defaultValue != null)
                    throw new Exception(dvExceptionMsg);

                return new GroupCollectionEntity(c, p.id, null);

            case "faction":
                if (p.defaultValue != null)
                    throw new Exception(dvExceptionMsg);

                return new FactionEntity(c, p.id, null);

            case "polity":
                if (p.defaultValue != null)
                    throw new Exception(dvExceptionMsg);

                return new PolityEntity(c, p.id, null);

            case "cell":
                if (p.defaultValue != null)
                    throw new Exception(dvExceptionMsg);

                return new CellEntity(c, p.id, null);

            case "agent":
                if (p.defaultValue != null)
                    throw new Exception(dvExceptionMsg);

                return new AgentEntity(c, p.id, null);

            case "region":
                if (p.defaultValue != null)
                    throw new Exception(dvExceptionMsg);

                return new RegionEntity(c, p.id, null);

            case "string":
                if (p.defaultValue != null)
                {
                    return new ValueEntity<string>(c, p.id, null, p.defaultValue);
                }

                return new ValueEntity<string>(c, p.id, null);

            case "text":
                if (p.defaultValue != null)
                {
                    return new ValueEntity<string>(c, p.id, null, p.defaultValue);
                }

                return new ValueEntity<string>(c, p.id, null);

            case "number":
                if (p.defaultValue != null)
                {
                    if (!MathUtility.TryParseCultureInvariant(p.defaultValue, out float defaultValue))
                    {
                        throw new Exception($"Unable to parse 'defaultValue' as a valid number: {p.defaultValue}");
                    }

                    return new ValueEntity<float>(c, p.id, null, defaultValue);
                }

                return new ValueEntity<float>(c, p.id, null);

            case "boolean":
                if (p.defaultValue != null)
                {
                    if (!bool.TryParse(p.defaultValue, out bool defaultValue))
                    {
                        throw new Exception($"Unable to parse 'defaultValue' as a valid boolean value: {p.defaultValue}");
                    }

                    return new ValueEntity<bool>(c, p.id, null, defaultValue);
                }

                return new ValueEntity<bool>(c, p.id, null);

            default:
                throw new Exception($"Unhandled parameter type: {p.type}");
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

        ModDecision decision = new ModDecision(d.id, d.target);

        Entity[] parameterEntities = null;

        if (d.parameters != null)
        {
            parameterEntities = CreateParameterEntities(decision, d.parameters);
        }

        decision.SetParameterEntities(parameterEntities);

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
        decision.DebugPlayerGuidance = d.debugPlayerGuidance;

        return decision;
    }
}
