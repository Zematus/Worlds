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

    public static PropertyEntity CreatePropertyEntity(Context context, LoadedProperty p)
    {
        if (string.IsNullOrEmpty(p.type))
        {
            throw new ArgumentException("'type' can't be null or empty");
        }

        PropertyEntity propEntity = null;

        switch (p.type)
        {
            case PropertyEntity.ConditionSetType:
                propEntity = new ConditionSetPropertyEntity(context, p);
                break;

            case PropertyEntity.RandomRangeType:
                propEntity = new RandomRangePropertyEntity(context, p);
                break;

            default:
                throw new ArgumentException("Property type not recognized: " + p.type);
        }

        return propEntity;
    }

    public abstract float GetNextRandomFloat(int iterOffset);
    public abstract float GetNextRandomInt(int iterOffset, int maxValue);
}
