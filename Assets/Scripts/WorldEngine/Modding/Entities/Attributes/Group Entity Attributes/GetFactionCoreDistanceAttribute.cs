using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GetFactionCoreDistanceAttribute : ValueEntityAttribute<float>
{
    private GroupEntity _groupEntity;

    private readonly IValueExpression<IEntity> _argumentExp;

    public GetFactionCoreDistanceAttribute(GroupEntity groupEntity, IExpression[] arguments)
        : base(GroupEntity.GetFactionCoreDistanceAttributeId, groupEntity, arguments, 1)
    {
        _groupEntity = groupEntity;

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
    }

    public override float Value
    {
        get
        {
            if (_argumentExp.Value is PolityEntity pEntity)
            {
                return _groupEntity.Group.GetFactionCoreDistance(pEntity.Polity);
            }

            throw new System.Exception(
                "Input parameter is not of a valid polity entity: " + _argumentExp.Value.GetType() +
                "\n - expression: " + _argumentExp.ToString() +
                "\n - value: " + _argumentExp.ToPartiallyEvaluatedString());
        }
    }
}
