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

    public INumericExpression Min;
    public INumericExpression Max;

    private EntityAttribute _minAttribute;
    private EntityAttribute _maxAttribute;
    private EntityAttribute _valueAttribute;

    private Context _context;
    private int _idHash;

    private class RandomRangeMinAttribute : NumericEntityAttribute
    {
        private RandomRangePropertyEntity _propertyEntity;

        public RandomRangeMinAttribute(RandomRangePropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override float Value => _propertyEntity.GetMin();
    }

    private class RandomRangeMaxAttribute : NumericEntityAttribute
    {
        private RandomRangePropertyEntity _propertyEntity;

        public RandomRangeMaxAttribute(RandomRangePropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override float Value => _propertyEntity.GetMax();
    }

    private class RandomRangeValueAttribute : NumericEntityAttribute
    {
        private RandomRangePropertyEntity _propertyEntity;

        public RandomRangeValueAttribute(RandomRangePropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override float Value => _propertyEntity.GetValue();
    }

    public RandomRangePropertyEntity(Context context, Context.LoadedProperty p)
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

        Min = ExpressionBuilder.ValidateNumericExpression(
            ExpressionBuilder.BuildExpression(context, p.min));
        Max = ExpressionBuilder.ValidateNumericExpression(
            ExpressionBuilder.BuildExpression(context, p.max));

        _context = context;

        _idHash = p.id.GetHashCode();
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueId:
                _valueAttribute =
                    _valueAttribute ??
                    new RandomRangeValueAttribute(this);
                return _valueAttribute;

            case MinId:
                _minAttribute =
                    _minAttribute ??
                    new RandomRangeMinAttribute(this);
                return _minAttribute;

            case MaxId:
                _maxAttribute =
                    _maxAttribute ??
                    new RandomRangeMaxAttribute(this);
                return _maxAttribute;
        }

        throw new System.ArgumentException("Cell: Unable to find attribute: " + attributeId);
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
}
