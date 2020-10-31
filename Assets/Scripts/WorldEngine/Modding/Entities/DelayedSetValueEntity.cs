using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class DelayedSetValueEntity<T,S> : DelayedSetEntity<T>, IValueEntity<S>
{
    public const string ValueAttributeId = "value";

    private ValueGetterEntityAttribute<S> _valueAttribute;

    private IValueExpression<S> _valueExpression = null;

    public S Value => GetValue();

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
        ValueGetterMethod<T> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public DelayedSetValueEntity(Context c, string id)
        : base(c, id)
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
            "DelayedSetValueEntity: Unable to find attribute: " + attributeId);
    }

    public override EntityAttribute GetThisEntityAttribute(Entity parent)
    {
        _thisAttribute =
            _thisAttribute ?? new FixedValueEntityAttribute<IValueEntity<S>>(
                this, Id, parent);

        return _thisAttribute;
    }

    public override string GetDebugString()
    {
        return GetValue().ToString();
    }

    public abstract S GetValue();

    public abstract void Set(S v);
}
