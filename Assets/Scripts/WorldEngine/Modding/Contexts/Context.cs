using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Context objects contain data used by sets of expressions to resolve references and receive input
/// from the simulation
/// </summary>
public abstract class Context
{
    [Serializable]
    public class LoadedProperty
    {
        public string id;
        public string type;
        public string min;
        public string max;
        public string[] conditions;
    }

    /// <summary>
    /// List of expressions that can be referenced by other expressions using this context
    /// </summary>
    readonly public Dictionary<string, IExpression> Expressions =
        new Dictionary<string, IExpression>();

    /// <summary>
    /// List of entities that can be referenced by expressions using this context
    /// </summary>
    readonly public Dictionary<string, Entity> Entities =
        new Dictionary<string, Entity>();

    readonly public Dictionary<string, ContextProperty> Properties =
        new Dictionary<string, ContextProperty>();

    public static ContextProperty CreateProperty(Context context, LoadedProperty p)
    {
        if (string.IsNullOrEmpty(p.type))
        {
            throw new ArgumentException("'type' can't be null or empty");
        }

        ContextProperty property = null;

        switch (p.type)
        {
            case ContextProperty.ConditionSetType:
                property = new ConditionSetProperty(context, p);
                break;

            case ContextProperty.RandomRangeType:
                property = new RandomRangeProperty(context, p);
                break;

            default:
                throw new ArgumentException("Property type not recognized: " + p.type);
        }

        return property;
    }
}
