using System;

public class NumericValuePropertyEntity : ValuePropertyEntity
{
    private float _value;

    private INumericExpression _expression;

    private class ValueAttribute : NumericEntityAttribute
    {
        private NumericValuePropertyEntity _propertyEntity;

        public ValueAttribute(NumericValuePropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override float Value => _propertyEntity.GetValue();
    }

    public NumericValuePropertyEntity(Context context, string id, IExpression exp)
        : base(context, id)
    {
        _expression = ExpressionBuilder.ValidateNumericExpression(exp);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueId:
                _valueAttribute =
                    _valueAttribute ??
                    new ValueAttribute(this);
                return _valueAttribute;
        }

        throw new System.ArgumentException(Id + " property: Unable to find attribute: " + attributeId);
    }

    public float GetValue()
    {
        EvaluateIfNeeded();

        return _value;
    }

    protected override void Calculate()
    {
        _value = _expression.Value;
    }
}
