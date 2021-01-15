using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class DelayedSetEntity<T> : Entity
{
    private ValueGetterMethod<T> _getterMethod;

    private T _setable = default;
    protected bool _isReset = false;

    public DelayedSetEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id)
        : base(c, id)
    {
        _getterMethod = getterMethod;
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
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
