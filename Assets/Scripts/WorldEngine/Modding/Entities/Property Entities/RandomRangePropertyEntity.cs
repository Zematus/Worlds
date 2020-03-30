using System;
using UnityEngine;

public class RandomRangePropertyEntity : PropertyEntity
{
    public const string ValueId = "value";
    public const string MinId = "min";
    public const string MaxId = "max";

    private float _min;
    private float _max;
    private float _value;

    public IValueExpression<float> Min;
    public IValueExpression<float> Max;

    private EntityAttribute _minAttribute;
    private EntityAttribute _maxAttribute;
    private EntityAttribute _valueAttribute;

    public RandomRangePropertyEntity(
        Context context, Context.LoadedContext.LoadedProperty p)
        : base(context, p)
    {
        if (string.IsNullOrEmpty(p.min))
        {
            throw new ArgumentException("'min' can't be null or empty");
        }

        if (string.IsNullOrEmpty(p.max))
        {
            throw new ArgumentException("'max' can't be null or empty");
        }

        Min = ValueExpressionBuilder.BuildValueExpression<float>(context, p.min);
        Max = ValueExpressionBuilder.BuildValueExpression<float>(context, p.max);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueId:
                _valueAttribute =
                    _valueAttribute ?? new ValueGetterEntityAttribute<float>(
                        ValueId, this, GetValue);
                return _valueAttribute;

            case MinId:
                _minAttribute =
                    _minAttribute ?? new ValueGetterEntityAttribute<float>(
                        MinId, this, GetMin);
                return _minAttribute;

            case MaxId:
                _maxAttribute =
                    _maxAttribute ?? new ValueGetterEntityAttribute<float>(
                        MaxId, this, GetMax);
                return _maxAttribute;
        }

        throw new System.ArgumentException(Id + " property: Unable to find attribute: " + attributeId);
    }

    public float GetMin()
    {
        EvaluateIfNeeded();

        return _min;
    }

    public float GetMax()
    {
        EvaluateIfNeeded();

        return _max;
    }

    public float GetValue()
    {
        EvaluateIfNeeded();

        return _value;
    }

    protected override void Calculate()
    {
        _min = Min.Value;
        _max = Max.Value;

        _value = Mathf.Lerp(_min, _max, _context.GetNextRandomFloat(_idHash));
    }

    public override string GetFormattedString()
    {
        EvaluateIfNeeded();

        return "(" + _min.ToString("0.00") + " - " + _max.ToString("0.00") + ")";
    }
}
