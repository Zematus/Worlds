using System;

public class EntityValuePropertyEntity : ValuePropertyEntity
{
    private Entity _entity;

    private IEntityExpression _expression;

    private class ValueAttribute : EntityEntityAttribute
    {
        private EntityValuePropertyEntity _propertyEntity;

        public ValueAttribute(EntityValuePropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override Entity AttributeEntity => _propertyEntity.GetAttributeEntity();
    }

    public EntityValuePropertyEntity(Context context, string id, IExpression exp)
        : base(context, id)
    {
        _expression = ExpressionBuilder.ValidateEntityExpression(exp);
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

    public Entity GetAttributeEntity()
    {
        EvaluateIfNeeded();

        return _entity;
    }

    protected override void Calculate()
    {
        _entity = _expression.Entity;
    }
}
