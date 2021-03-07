using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SetRelationshipAttribute : EffectEntityAttribute
{
    private FactionEntity _factionEntity;

    private readonly IValueExpression<IEntity> _factionExp;
    private readonly IValueExpression<float> _valueExp;

    public SetRelationshipAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.SetRelationshipAttributeId, factionEntity, arguments, 2)
    {
        _factionEntity = factionEntity;

        _factionExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
        _valueExp = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
    }

    public override void Apply()
    {
        FactionEntity targetEntity = _factionExp.Value as FactionEntity;

        if (targetEntity == null)
        {
            throw new System.ArgumentException(
                "set_relationship: invalid faction to set relationship to:" +
                "\n - expression: " + ToString() +
                "\n - faction: " + _factionExp.ToPartiallyEvaluatedString() +
                "\n - value: " + _valueExp.ToPartiallyEvaluatedString());
        }

        _factionEntity.Faction.SetRelationship(targetEntity.Faction, _valueExp.Value);
    }
}
