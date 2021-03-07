using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GetRelationshipAttribute : ValueEntityAttribute<float>
{
    private FactionEntity _factionEntity;

    private readonly IValueExpression<IEntity> _argumentExp;

    public GetRelationshipAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.GetRelationshipAttributeId, factionEntity, arguments, 1)
    {
        _factionEntity = factionEntity;

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
    }

    public override float Value
    {
        get
        {
            if (_argumentExp.Value is FactionEntity fEntity)
            {
                return _factionEntity.Faction.GetRelationshipValue(fEntity.Faction);
            }

            throw new System.Exception(
                "Input parameter is not of a valid faction entity: " + _argumentExp.Value.GetType() +
                "\n - expression: " + _argumentExp.ToString() +
                "\n - value: " + _argumentExp.ToPartiallyEvaluatedString());
        }
    }
}
