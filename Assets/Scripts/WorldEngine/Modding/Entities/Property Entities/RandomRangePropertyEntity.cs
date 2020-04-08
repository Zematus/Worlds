using System;
using UnityEngine;

public class RandomRangePropertyEntity : PropertyEntity<float>
{
    public const string MinAttributeId = "min";
    public const string MaxAttributeId = "max";

    private float _min;
    private float _max;

    public IValueExpression<float> Min;
    public IValueExpression<float> Max;

    private EntityAttribute _minAttribute;
    private EntityAttribute _maxAttribute;

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
            case MinAttributeId:
                _minAttribute =
                    _minAttribute ?? new ValueGetterEntityAttribute<float>(
                        MinAttributeId, this, GetMin);
                return _minAttribute;

            case MaxAttributeId:
                _maxAttribute =
                    _maxAttribute ?? new ValueGetterEntityAttribute<float>(
                        MaxAttributeId, this, GetMax);
                return _maxAttribute;
        }

        return base.GetAttribute(attributeId, arguments);
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

    public override float GetValue()
    {
        EvaluateIfNeeded();

        return Value;
    }

    protected override void Calculate()
    {
        _min = Min.Value;
        _max = Max.Value;

        Value = Mathf.Lerp(_min, _max, _context.GetNextRandomFloat(_idHash));
    }

    public override string GetFormattedString()
    {
        EvaluateIfNeeded();

        return "(" + _min.ToString("0.00") + " - " + _max.ToString("0.00") + ")";
    }
}
