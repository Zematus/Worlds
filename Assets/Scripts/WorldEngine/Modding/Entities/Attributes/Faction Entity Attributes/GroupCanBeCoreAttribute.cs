using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupCanBeCoreAttribute : ValueEntityAttribute<bool>
{
    private FactionEntity _factionEntity;

    private readonly IValueExpression<Entity> _argumentExp;

    public GroupCanBeCoreAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.GroupCanBeCoreAttributeId, factionEntity, arguments)
    {
        _factionEntity = factionEntity;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException("Number of arguments less than 1");
        }

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);
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
                "Input parameter is not of a valid group entity: " + _argumentExp.Value.GetType());
        }
    }
}
