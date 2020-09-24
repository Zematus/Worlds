using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ContactStrengthAttribute : ValueEntityAttribute<float>
{
    private PolityEntity _polityEntity;

    private readonly IValueExpression<Entity> _argumentExp;

    private readonly PolityEntity _targetPolityEntity;

    public ContactStrengthAttribute(PolityEntity polityEntity, IExpression[] arguments)
        : base(PolityEntity.ContactStrengthId, polityEntity, arguments, 1)
    {
        _polityEntity = polityEntity;

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);
    }

    public override float Value
    {
        get
        {
            if (_argumentExp.Value is PolityEntity pEntity)
            {
                return _polityEntity.Polity.GetRelationshipValue(pEntity.Polity);
            }

            if (_argumentExp.Value is ContactEntity cEntity)
            {
                return _polityEntity.Polity.GetRelationshipValue(cEntity.Contact.Polity);
            }

            throw new System.Exception(
                "Input parameter is not of a valid polity or contact entity: " + _argumentExp.Value.GetType() +
                "\n - expression: " + _argumentExp.ToString() +
                "\n - value: " + _argumentExp.ToPartiallyEvaluatedString());
        }
    }
}
