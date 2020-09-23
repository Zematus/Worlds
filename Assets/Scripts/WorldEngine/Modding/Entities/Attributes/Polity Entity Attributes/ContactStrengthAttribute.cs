using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ContactStrengthAttribute : ValueEntityAttribute<float>
{
    private PolityEntity _polityEntity;

    private readonly PolityEntity _targetPolityEntity;

    public ContactStrengthAttribute(PolityEntity polityEntity, IExpression[] arguments)
        : base(PolityEntity.ContactStrengthId, polityEntity, arguments, 1)
    {
        _polityEntity = polityEntity;

        IValueExpression<Entity> argumentExp =
            ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);

        _targetPolityEntity = argumentExp.Value as PolityEntity;

        if (_targetPolityEntity == null)
        {
            throw new System.Exception(
                "Input parameter is not of a valid polity entity: " + argumentExp.Value.GetType() +
                "\n - expression: " + argumentExp.ToString() +
                "\n - value: " + argumentExp.ToPartiallyEvaluatedString());
        }
    }

    public override float Value
    {
        get
        {
            return _polityEntity.Polity.CalculateContactStrength(_targetPolityEntity.Polity);
        }
    }
}
