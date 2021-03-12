using System;
using System.Collections.Generic;
using UnityEngine;

public enum GuideType
{
    Player,
    Simulation,
    All
}

/// <summary>
/// Context objects contain data used by sets of expressions to resolve references and receive input
/// from the simulation
/// </summary>
public abstract class Context : IDebugLogger
{
    public const string Guide_Simulation = "simulation";
    public const string Guide_Player = "player";
    public const string Guide_All = "all";

    public string Id;

    public bool DebugEnabled => (Manager.CurrentDevMode != DevMode.None) && _debug;

    protected int _currentIterOffset = 0;

    protected Context _parentContext = null;

    protected bool _debug = false;

    private string _dbgStr = null;
    private int _dbgTabCount = -1;
    private string _dbgTab;

    [Serializable]
    public class LoadedContext
    {
        public string id;

        [Serializable]
        public class LoadedProperty
        {
            public string id;
            public string value;
        }

        public LoadedProperty[] properties;
        public bool debug;
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
        if (string.IsNullOrEmpty(c.id))
        {
            throw new ArgumentException("context 'id' can't be null or empty");
        }

        Id = c.id;

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
    public abstract int GetNextRandomInt(int iterOffset, int maxValue);
    public abstract int GetBaseOffset();

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

    public void OpenDebugOutput(string message = null)
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
        if (string.IsNullOrEmpty(message))
            return;

        if (DebugEnabled)
        {
            string idString = "[" + Id + "] ";
            string idTabStr = new string(' ', idString.Length);

            if (_dbgStr == null)
            {
                _dbgStr = idString;
            }
            else
            {
                _dbgStr += "\n" + idTabStr;
            }

            message = message.Replace("\n", "\n" + idTabStr + _dbgTab);
            _dbgStr += _dbgTab + message;
        }
    }

    public void CloseDebugOutput(string message = null)
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
