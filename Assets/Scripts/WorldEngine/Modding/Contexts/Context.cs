using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Context objects contain data used by sets of expressions to resolve references and receive input
/// from the simulation
/// </summary>
public abstract class Context
{
    public bool DebugEnabled => Manager.DebugModeEnabled && _debug;

    protected int _currentIterOffset = 0;

    protected Context _parentContext = null;

    protected bool _debug = false;

    private string _dbgStr = null;
    private int _dbgTabCount = -1;
    private string _dbgTab;

    [Serializable]
    public class LoadedContext
    {
        public bool debug;

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

    readonly private List<IReseteableEntity> _propertyEntities =
        new List<IReseteableEntity>();

    public void Initialize(LoadedContext c)
    {
        if (c.properties != null)
        {
            foreach (LoadedContext.LoadedProperty lp in c.properties)
            {
                AddPropertyEntity(lp);
            }
        }

        _debug = c.debug;
    }

    private void AddPropertyEntity(LoadedContext.LoadedProperty p)
    {
        IReseteableEntity propEntity = PropertyEntityBuilder.BuildPropertyEntity(this, p);
        Entity entity = propEntity as Entity;

        _entities.Add(entity.Id, entity);
        _propertyEntities.Add(propEntity);
    }

    public virtual void Reset()
    {
        foreach (IReseteableEntity entity in _propertyEntities)
        {
            entity.Reset();
        }
    }

    public Context(Context parent = null)
    {
        _parentContext = parent;
    }

    public virtual int GetNextIterOffset()
    {
        if (_parentContext != null)
        {
            return _parentContext.GetNextIterOffset();
        }

        return _currentIterOffset++;
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

    public void OpenDebugOutput(string message)
    {
        if (DebugEnabled)
        {
            _dbgTabCount++;

            if (_dbgTabCount > 0)
            {
                _dbgTab += "\t";
            }
        }

        AddDebugOutput(message);
    }

    public void AddDebugOutput(string message)
    {
        if (DebugEnabled)
        {
            if (_dbgStr == null)
            {
                _dbgStr = "";
            }
            else
            {
                _dbgStr += "\n";
            }

            message = message.Replace("\n", "\n" + _dbgTab);
            _dbgStr += _dbgTab + message;
        }
    }

    public void CloseDebugOutput(string message)
    {
        AddDebugOutput(message);

        if (DebugEnabled)
        {
            _dbgTabCount--;

            if (_dbgTabCount < 0)
            {
                if (_dbgStr != null)
                {
                    UnityEngine.Debug.Log(_dbgStr);
                    _dbgStr = null;
                }
            }
            else
            {
                _dbgTab = "";
                for (int i = 0; i < _dbgTabCount; i++)
                {
                    _dbgTab += "\t";
                }
            }
        }
    }
}
