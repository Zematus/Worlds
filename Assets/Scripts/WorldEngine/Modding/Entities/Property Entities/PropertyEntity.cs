using System;
using UnityEngine;

public class PropertyEntity<T> : ValueEntity<T>, IReseteableEntity
{
    private bool _evaluated = false;

    private IValueExpression<T> _valExpression = null;

    protected override object _reference => this;

    protected readonly string _id;
    protected readonly int _idHash;

    public override bool RequiresInput => _valExpression.RequiresInput;

    public override T Value
    {
        get
        {
            if (!_evaluated)
            {
                _value = _valExpression.Value;
                _evaluated = true;
            }

            return _value;
        }
        protected set
        {
            throw new System.Exception("Value should be never be set for " + this.GetType());
        }
    }

    private T _value = default;

    public PropertyEntity(Context c, string id, IExpression exp) : base(c, id)
    {
        _id = id;
        _idHash = id.GetHashCode();

        PartialEvalStringConverter = ToPartiallyEvaluatedString;

        _valExpression = ValueExpressionBuilder.ValidateValueExpression<T>(exp);

    }

    public void Reset()
    {
        _evaluated = false;
    }

    public override string ToPartiallyEvaluatedString(int depth = -1) =>
        _valExpression.ToPartiallyEvaluatedString(depth);

    public override bool TryGetRequest(out InputRequest request) =>
        _valExpression.TryGetRequest(out request);

    public override void Set(T v)
    {
        throw new System.Exception("Set() should be never be called for " + this.GetType());
    }

    public override void Set(object o)
    {
        throw new System.Exception("Set() should be never be called for " + this.GetType());
    }
}
