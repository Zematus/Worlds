using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueEntity<T> : Entity, IValueEntity<T>
{
    public const string ValueAttributeId = "value";

    protected ValueGetterEntityAttribute<T> _valueAttribute;

    protected PartiallyEvaluatedStringConverter _partialEvalStringConverter { private get; set; }

    public virtual T Value { get; protected set; }

    protected override object _reference => Value;

    private IValueExpression<T> _valGetterExpression = null;

    public ValueEntity(Context c, string id) : base(c, id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueAttributeId:
                _valueAttribute =
                    _valueAttribute ?? new ValueGetterEntityAttribute<T>(
                        ValueAttributeId, this, GetValue, _partialEvalStringConverter);
                return _valueAttribute;
        }

        throw new System.ArgumentException("ValueEntity: Unable to find attribute: " + attributeId);
    }

    private T GetValue() => Value;

    public virtual void Set(T v) => Value = v;

    public override void Set(object o)
    {
        if (o is T v)
        {
            Value = v;
        }
        else
        {
            throw new System.Exception("Input reference is not of type " + typeof(T));
        }
    }

    public override void Set(
        object o,
        PartiallyEvaluatedStringConverter converter)
    {
        _partialEvalStringConverter = converter;

        Set(o);
    }

    public override string GetDebugString() => Value.ToString();

    public override string GetFormattedString() => Value.ToString().ToBoldFormat();

    public IValueExpression<T> ValueExpression
    {
        get
        {
            _valGetterExpression = _valGetterExpression ??
                new ValueGetterExpression<T>(
                    Id, GetValue, _partialEvalStringConverter);

            return _valGetterExpression;
        }
    }

    public override IValueExpression<IEntity> Expression
    {
        get
        {
            _expression = _expression ?? new ValueEntityExpression<T>(this);

            return _expression;
        }
    }

    public IBaseValueExpression BaseValueExpression => ValueExpression;
}
