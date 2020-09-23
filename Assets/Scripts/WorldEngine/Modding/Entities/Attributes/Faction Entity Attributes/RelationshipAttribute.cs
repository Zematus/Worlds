using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RelationshipAttribute : ValueEntityAttribute<float>
{
    private FactionEntity _factionEntity;

    private readonly FactionEntity _targetFactionEntity;

    public RelationshipAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.RelationshipAttributeId, factionEntity, arguments, 1)
    {
        _factionEntity = factionEntity;

        IValueExpression<Entity> argumentExp =
            ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);

        _targetFactionEntity = argumentExp.Value as FactionEntity;

        if (_targetFactionEntity == null)
        {
            throw new System.Exception(
                "Input parameter is not of a valid faction entity: " + argumentExp.Value.GetType() +
                "\n - expression: " + argumentExp.ToString() +
                "\n - value: " + argumentExp.ToPartiallyEvaluatedString());
        }
    }

    public override float Value
    {
        get
        {
            return _factionEntity.Faction.GetRelationshipValue(_targetFactionEntity.Faction);
        }
    }
}
