using UnityEngine;
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

    public override bool RequiresInput => _tryRequestGenMethod != null;

    public DelayedSetEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id)
        : base(c, id)
    {
        _getterMethod = getterMethod;
    }

    public DelayedSetEntity(
        TryRequestGenMethod<T> tryRequestGenMethod, Context c, string id)
        : base(c, id)
    {
        _tryRequestGenMethod = tryRequestGenMethod;
    }

    public DelayedSetEntity(Context c, string id)
        : base(c, id)
    {
        _getterMethod = null;
    }

    public void Reset()
    {
        _setable = default;

        ResetInternal();

        _isReset = true;
    }

    public virtual void Set(T t)
    {
        _setable = t;

        ResetInternal();

        _isReset = false;
    }

    protected virtual void ResetInternal()
    {
    }

    protected virtual T Setable
    {
        set
        {
            Set(_setable);
        }
        get
        {
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
            Set(e.Setable);
        }
        else if (o is T t)
        {
            Set(t);
        }
        else
        {
            throw new System.ArgumentException("Unexpected entity value type: " +
                o.GetType() + "\nVerify that the value pased to '" + Id + "' is properly " +
                "defined when calling " + Context.DebugType + " '" + Context.Id + "'");
        }
    }

    public override bool TryGetRequest(out InputRequest request)
    {
        if ((!_isReset) || (!RequiresInput))
        {
            request = null;

            return false;
        }

        if (!_tryRequestGenMethod(out DelayedSetEntityInputRequest<T> entityRequest))
        {
            request = null;

            return false;
        }

        entityRequest.SetEntity(this);

        request = entityRequest;

        return true;
    }
}
