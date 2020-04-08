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
    public class LoadedContext
    {
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

        public LoadedProperty[] properties;
    }

    /// <summary>
    /// List of entities that can be referenced by expressions using this context
    /// </summary>
    readonly private Dictionary<string, Entity> _entities =
        new Dictionary<string, Entity>();

    public void Initialize(LoadedContext c)
    {
        if (c.properties != null)
        {
            foreach (LoadedContext.LoadedProperty lp in c.properties)
            {
                AddPropertyEntity(lp);
            }
        }
    }

    private void AddPropertyEntity(LoadedContext.LoadedProperty p)
    {
        Entity entity = PropertyEntityBuilder.BuildPropertyEntity(this, p);

        _entities.Add(entity.Id, entity);
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
