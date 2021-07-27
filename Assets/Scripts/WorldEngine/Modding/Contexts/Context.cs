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

    protected bool DebugLogEnabled
    {
        get
        {
            if (_parentContext != null)
            {
                return _parentContext.DebugLogEnabled;
            }

            return (Manager.CurrentDevMode == DevMode.Advanced) && _enableDebugLog;
        }
    }

    protected int _currentIterOffset = 0;

    protected Context _parentContext = null;

    protected bool _enableDebugLog = false;

    private string _dbgStr = null;
    private int _dbgTabCount = -1;
    private string _dbgTab;

    public string DebugType { get; protected set; }

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
        public bool enableDebugLog;
    }

    /// <summary>
    /// List of entities that can be referenced by expressions using this context
    /// </summary>
    readonly private Dictionary<string, Entity> _entities =
        new Dictionary<string, Entity>();

    readonly private List<IReseteableEntity> _propertyEntities =
        new List<IReseteableEntity>();

    protected Context()
    {
        DebugType = "Context";
    }

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

        _enableDebugLog = c.enableDebugLog;
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
        if (_parentContext != null)
        {
            _parentContext.OpenDebugOutput(message);
            return;
        }

        if (DebugLogEnabled)
        {
            _dbgTabCount++;

            if (_dbgTabCount > 0)
            {
                _dbgTab += "\t";
            }

            AddDebugOutput(message);
        }
    }

    private string ExplodedPartiallyEvaluatedExpression(IExpression exp)
    {
        string output = "";

        int depth = 0;
        string partEval = exp.ToPartiallyEvaluatedString(depth);
        output += $"\n\t - Partial eval (depth {depth}): {partEval}";

        while (true)
        {
            string nextPartEval = exp.ToPartiallyEvaluatedString(++depth);

            if (nextPartEval.Equals(partEval))
                break;

            output += $"\n\t - Partial eval (depth {depth}): {nextPartEval}";
            partEval = nextPartEval;
        }

        return output;
    }

    public void AddExpDebugOutput(
        string label, IExpression exp)
    {
        if (_parentContext != null)
        {
            _parentContext.AddExpDebugOutput(label, exp);
            return;
        }

        if (DebugLogEnabled)
        {
            if (exp != null)
            {
                AddDebugOutput(
                    $"\t{label}: {exp}" +
                    $"{ExplodedPartiallyEvaluatedExpression(exp)}");
            }
            else
            {
                AddDebugOutput($"\t{label} is null");
            }
        }
    }

    public void AddExpDebugOutput<T>(
        string label, IValueExpression<T> exp)
    {
        if (_parentContext != null)
        {
            _parentContext.AddExpDebugOutput(label, exp);
            return;
        }

        if (DebugLogEnabled)
        {
            if (exp != null)
            {
                AddDebugOutput(
                    $"\t{label}: {exp}" +
                    $"{ExplodedPartiallyEvaluatedExpression(exp)}" +
                    $"\n\t - Value: {exp.Value}");
            }
            else
            {
                AddDebugOutput($"\t{label} is null");
            }
        }
    }

    public void AddExpDebugOutput(
        string label, IBaseValueExpression exp)
    {
        if (_parentContext != null)
        {
            _parentContext.AddExpDebugOutput(label, exp);
            return;
        }

        if (DebugLogEnabled)
        {
            if (exp != null)
            {
                AddDebugOutput(
                    $"\t{label}: {exp}" +
                    $"{ExplodedPartiallyEvaluatedExpression(exp)}" +
                    $"\n\t - Value: {exp.ValueObject}");
            }
            else
            {
                AddDebugOutput($"\t{label} is null");
            }
        }
    }

    public void AddDebugOutput(string message)
    {
        if (_parentContext != null)
        {
            _parentContext.AddDebugOutput(message);
            return;
        }

        if (string.IsNullOrEmpty(message))
            return;

        if (DebugLogEnabled)
        {
            string idString = "[" + Id + "] ";

            if (_dbgStr == null)
            {
                _dbgStr = DebugType + " Mod Debug " + idString + ":\n\t";
            }
            else
            {
                _dbgStr += "\n\t";
            }

            message = message.Replace("\n", "\n\t" + _dbgTab);
            _dbgStr += _dbgTab + message;
        }
    }

    public void CloseDebugOutput(string message = null)
    {
        if (_parentContext != null)
        {
            _parentContext.CloseDebugOutput(message);
            return;
        }

        if (DebugLogEnabled)
        {
            AddDebugOutput(message);

            _dbgTabCount--;

            if (_dbgTabCount < 0)
            {
                if (_dbgStr != null)
                {
                    Debug.Log(_dbgStr);
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
