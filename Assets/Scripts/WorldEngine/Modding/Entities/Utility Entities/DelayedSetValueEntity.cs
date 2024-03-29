﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class DelayedSetValueEntity<T, S> : DelayedSetEntity<T>, IValueEntity<S>
{
    public const string ValueAttributeId = "value";

    private ValueGetterEntityAttribute<S> _valueAttribute;

    private IValueExpression<S> _valueExpression = null;

    public abstract S Value { get; }

    public IValueExpression<S> ValueExpression
    {
        get
        {
            _valueExpression = _valueExpression ??
                new ValueGetterExpression<S>(Id, GetValue);

            return _valueExpression;
        }
    }

    public IBaseValueExpression BaseValueExpression => ValueExpression;

    public DelayedSetValueEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public DelayedSetValueEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueAttributeId:
                _valueAttribute =
                    _valueAttribute ?? new ValueGetterEntityAttribute<S>(
                        ValueAttributeId, this, GetValue);
                return _valueAttribute;
        }

        throw new System.ArgumentException(
            $"{Id}: Unable to find attribute: {attributeId}");
    }

    public override string GetDebugString()
    {
        return Value.ToString();
    }

    private S GetValue() => Value;

    public abstract void Set(S v);
}
