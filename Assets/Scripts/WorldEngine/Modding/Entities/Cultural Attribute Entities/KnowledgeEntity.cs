using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class KnowledgeEntity : DelayedSetEntity<CulturalKnowledge>, IValueEntity<float>
{
    public const string ValueAttributeId = "value";

    public virtual CulturalKnowledge Knowledge
    {
        get => Setable;
        private set => Setable = value;
    }

    private ValueGetterEntityAttribute<float> _valueAttribute;

    private IValueExpression<float> _valueExpression = null;

    protected override object _reference => Knowledge;

    public float Value => GetValue();

    public IValueExpression<float> ValueExpression
    {
        get
        {
            _valueExpression = _valueExpression ??
                new ValueGetterExpression<float>(Id, GetValue, ToPartiallyEvaluatedString);

            return _valueExpression;
        }
    }

    public IBaseValueExpression BaseValueExpression => ValueExpression;

    public override IValueExpression<IEntity> Expression
    {
        get
        {
            _expression = _expression ?? new ValueEntityExpression<float>(this);

            return _expression;
        }
    }

    public KnowledgeEntity(
        ValueGetterMethod<CulturalKnowledge> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public KnowledgeEntity(Context c, string id) : base(c, id)
    {
    }

    public float GetValue()
    {
        if (Knowledge != null)
        {
            return Knowledge.ScaledValue;
        }

        return 0;
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueAttributeId:
                _valueAttribute =
                    _valueAttribute ?? new ValueGetterEntityAttribute<float>(
                        ValueAttributeId, this, GetValue);
                return _valueAttribute;
        }

        throw new System.ArgumentException("Knowledge: Unable to find attribute: " + attributeId);
    }

    public override string GetDebugString()
    {
        return GetValue().ToString();
    }

    public override string GetFormattedString()
    {
        return "<i>" + Knowledge.Name + "</i>";
    }

    public void Set(float v)
    {
        throw new System.InvalidOperationException("Knowledge attribute is read-only");
    }
}
