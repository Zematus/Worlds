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

    public string Id { get; set; }

    public bool DebugLogEnabled
    {
        get
        {
            if (_parentContext != null)
            {
                return _parentContext.DebugLogEnabled;
            }

            return (Manager.CurrentDevMode == DevMode.Advanced) && _debugLogEnabled;
        }
    }

    protected int _currentIterOffset = 0;

    protected Context _parentContext = null;

    protected bool _debugLogEnabled = false;

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
    readonly protected Dictionary<string, Entity> _entities =
        new Dictionary<string, Entity>();

    readonly private List<IResettableEntity> _propertyEntities =
        new List<IResettableEntity>();

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

        _debugLogEnabled = c.enableDebugLog;
    }

    public void EnableDebugLog(bool state)
    {
        _debugLogEnabled = state;
    }

    private void AddPropertyEntity(LoadedContext.LoadedProperty p)
    {
        IResettableEntity propEntity = PropertyEntityBuilder.BuildPropertyEntity(this, p);
        Entity entity = propEntity as Entity;

        _entities.Add(entity.Id, entity);
        _propertyEntities.Add(propEntity);
    }

    public virtual void Reset()
    {
        foreach (IResettableEntity entity in _propertyEntities)
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
        List<string> partEvals = new List<string>();

        int depth = 0;
        string partEval = exp.ToPartiallyEvaluatedString(depth);
        partEvals.Add(partEval);

        while (true)
        {
            string nextPartEval = exp.ToPartiallyEvaluatedString(++depth);

            if (nextPartEval.Equals(partEval))
                break;

            partEval = nextPartEval;
            partEvals.Add(partEval);
        }

        string output = "";

        for (int i = partEvals.Count - 1; i >= 0; i--)
        {
            output += $"\n\t - Partial eval (depth {i}): {partEvals[i]}";
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

    public void AddValueDebugOutput<T>(
        string label, T value)
    {
        if (_parentContext != null)
        {
            _parentContext.AddValueDebugOutput(label, value);
            return;
        }

        if (DebugLogEnabled)
        {
            AddDebugOutput(
                $"\t{label} Value: {value}");
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
