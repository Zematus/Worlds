using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Obsolete]
public class GroupCanBeCoreAttribute : ValueEntityAttribute<bool>
{
    private FactionEntity _factionEntity;

    private readonly IValueExpression<IEntity> _argumentExp;

    public GroupCanBeCoreAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.GroupCanBeCoreAttributeId, factionEntity, arguments, 1)
    {
        _factionEntity = factionEntity;

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
    }

    public override bool Value
    {
        get
        {
            if (_argumentExp.Value is GroupEntity gEntity)
            {
                return _factionEntity.Faction.GroupCanBeCore(gEntity.Group);
            }

            throw new System.Exception(
                "Input parameter is not of a valid group entity: " + _argumentExp.Value.GetType() +
                "\n - expression: " + _argumentExp.ToString() +
                "\n - value: " + _argumentExp.ToPartiallyEvaluatedString());
        }
    }
}
