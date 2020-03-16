using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Context objects contain data used by sets of expressions to resolve references and receive input
/// from the simulation
/// </summary>
public abstract class Context
{
    protected Context _parentContext = null;

    [Serializable]
    public class LoadedProperty
    {
        public string id;
        public string type;
        public string min;
        public string max;
        public string[] conditions;
        public string value;
    }

    /// <summary>
    /// List of entities that can be referenced by expressions using this context
    /// </summary>
    readonly private Dictionary<string, Entity> _entities =
        new Dictionary<string, Entity>();

    public PropertyEntity AddPropertyEntity(LoadedProperty p)
    {
        if (string.IsNullOrEmpty(p.type))
        {
            throw new ArgumentException("'type' can't be null or empty");
        }

        PropertyEntity entity;

        switch (p.type)
        {
            case PropertyEntity.ConditionSetType:
                entity = new ConditionSetPropertyEntity(this, p);
                break;

            case PropertyEntity.RandomRangeType:
                entity = new RandomRangePropertyEntity(this, p);
                break;

            case PropertyEntity.ValueType:
                entity = ValuePropertyEntity.BuildValuePropertyEntity(this, p);
                break;

            default:
                throw new ArgumentException("Property type not recognized: " + p.type);
        }

        _entities.Add(entity.Id, entity);

        return entity;
    }

    public Context(Context parent = null)
    {
        _parentContext = parent;
    }

    public abstract float GetNextRandomFloat(int iterOffset);
    public abstract float GetNextRandomInt(int iterOffset, int maxValue);

    public void AddEntity(Entity entity)
    {
        _entities.Add(entity.Id, entity);
    }

    public bool TryGetEntity(string id, out Entity e)
    {
        if (_entities.TryGetValue(id, out e))
        {
            return true;
        }

        if (_parentContext == null)
        {
            return false;
        }

        return _parentContext.TryGetEntity(id, out e);
    }
}
