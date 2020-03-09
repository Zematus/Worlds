using System;

public class StringValuePropertyEntity : ValuePropertyEntity
{
    private string _value;

    private IStringExpression _expression;

    private class ValueAttribute : StringEntityAttribute
    {
        private StringValuePropertyEntity _propertyEntity;

        public ValueAttribute(StringValuePropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override string Value => _propertyEntity.GetValue();
    }

    public StringValuePropertyEntity(Context context, string id, IExpression exp)
        : base(context, id)
    {
        _expression = ExpressionBuilder.ValidateStringExpression(exp);
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

    public string GetValue()
    {
        EvaluateIfNeeded();

        return _value;
    }

    protected override void Calculate()
    {
        _value = _expression.Value;
    }
}
