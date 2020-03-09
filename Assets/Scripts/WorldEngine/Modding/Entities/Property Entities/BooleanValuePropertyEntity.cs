using System;

public class BooleanValuePropertyEntity : ValuePropertyEntity
{
    private bool _value;

    private IBooleanExpression _expression;

    private class ValueAttribute : BooleanEntityAttribute
    {
        private BooleanValuePropertyEntity _propertyEntity;

        public ValueAttribute(BooleanValuePropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override bool Value => _propertyEntity.GetValue();
    }

    public BooleanValuePropertyEntity(Context context, string id, IExpression exp)
        : base(context, id)
    {
        _expression = ExpressionBuilder.ValidateBooleanExpression(exp);
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

    public bool GetValue()
    {
        EvaluateIfNeeded();

        return _value;
    }

    protected override void Calculate()
    {
        _value = _expression.Value;
    }
}
