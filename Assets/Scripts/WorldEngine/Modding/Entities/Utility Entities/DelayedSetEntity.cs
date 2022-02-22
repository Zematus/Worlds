﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate bool TryRequestGenMethod<T>(
    out DelayedSetEntityInputRequest<T> request);

public abstract class DelayedSetEntity<T> : Entity
{
    public bool IsReset => _isReset;

    private readonly ValueGetterMethod<T> _getterMethod;

    private readonly TryRequestGenMethod<T> _tryRequestGenMethod = null;

    private T _setable = default;
    protected bool _isReset = false;

    private T _requestResult = default;
    private bool _requestSatisfied = false;

    protected override bool RequiresInputIgnoreParent => _tryRequestGenMethod != null;

    private bool _needsToSatisfyRequest => _isReset && (!_requestSatisfied);

#if DEBUG
    private static int _debugIdCounter = 0;
    private int _debugId = 0;
#endif

    public DelayedSetEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
        _getterMethod = getterMethod;

#if DEBUG
        _debugId = _debugIdCounter++;
#endif
    }

    public DelayedSetEntity(
        TryRequestGenMethod<T> tryRequestGenMethod, Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
        _tryRequestGenMethod = tryRequestGenMethod;
        _getterMethod = RequestResultGetter;

#if DEBUG
        _debugId = _debugIdCounter++;
#endif
    }

    public DelayedSetEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
        _getterMethod = null;

#if DEBUG
        _debugId = _debugIdCounter++;
#endif
    }

    public T RequestResultGetter()
    {
        return _requestResult;
    }

    public void Reset()
    {
        _setable = default;

        ResetInternal();

#if DEBUG
        if ((Manager.CurrentWorld.CurrentDate == 181582635) &&
            (Id == "target.polity.contacts.selected_entity_0.polity.factions_collection_0.selected_entity_0"))
        {
            Debug.Log("Debugging DelayedSetEntity.Reset");
        }
#endif
        _isReset = true;
    }

    public virtual void Set(T t, IEntity parent)
    {
        Parent = parent;

        Set(t);
    }

    public virtual void Set(T t)
    {
#if DEBUG
        if ((Manager.CurrentWorld.CurrentDate == 181582635) &&
            (Id == "target.polity.contacts.selected_entity_0.polity.factions_collection_0.selected_entity_0"))
        {
            Debug.Log("Debugging DelayedSetEntity.Set");
        }
#endif
        _setable = t;

        ResetInternal();

        _isReset = false;
        _requestSatisfied = false;
    }

    public void SetRequestResult(T t)
    {
        _requestResult = t;
        _requestSatisfied = true;
    }

    protected virtual void ResetInternal()
    {
    }

    protected virtual T Setable
    {
        set => Set(_setable);
        get
        {
#if DEBUG
            if ((Manager.CurrentWorld.CurrentDate == 181582635) &&
                (Id == "target.polity.contacts.selected_entity_0.polity.factions_collection_0.selected_entity_0"))
            {
                Debug.Log("Debugging DelayedSetEntity.Setable.get");
            }
#endif
            if (_isReset && (_getterMethod != null))
            {
                Set(_getterMethod());
            }

            return _setable;
        }
    }

    public override void Set(object o)
    {
        if (o is DelayedSetEntity<T> e)
        {
            Set(e.Setable, e.Parent);
        }
        else if (o is T t)
        {
            Set(t);
        }
        else
        {
            throw new System.ArgumentException($"Unexpected entity value type: {o.GetType()}, expected type: {typeof(T)}" +
                $"\nVerify that the value passed to '{Id}' is properly defined when calling {Context.DebugType} '{Context.Id}'");
        }
    }

    public override bool TryGetRequest(out InputRequest request)
    {
        request = null;

        if (Parent?.TryGetRequest(out request) ?? false)
        {
            return true;
        }

        if ((!RequiresInputIgnoreParent) ||
            (!_needsToSatisfyRequest) ||
            (!_tryRequestGenMethod(out DelayedSetEntityInputRequest<T> entityRequest)))
        {
            return false;
        }

        entityRequest.SetEntity(this);

        request = entityRequest;

        return true;
    }
}
