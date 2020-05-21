using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RelationshipToAttribute : ValueEntityAttribute<float>
{
    private FactionEntity _factionEntity;

    private readonly IValueExpression<Entity> _argumentExp;

    public RelationshipToAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.GroupCanBeCoreAttributeId, factionEntity, arguments)
    {
        _factionEntity = factionEntity;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException("Number of arguments less than 1");
        }

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);
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
