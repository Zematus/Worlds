using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TransferInfluenceAttribute : EffectEntityAttribute
{
    private readonly PolityEntity _polityEntity;

    private readonly IValueExpression<IEntity> _sourceFactionExp;
    private readonly IValueExpression<IEntity> _targetFactionExp;
    private readonly IValueExpression<float> _percentExp;

    public TransferInfluenceAttribute(PolityEntity polityEntity, IExpression[] arguments)
        : base(PolityEntity.TransferInfluenceAttributeId, polityEntity, arguments, 3)
    {
        _polityEntity = polityEntity;

        _sourceFactionExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
        _targetFactionExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[1]);
        _percentExp = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
    }

    public override void Apply(IEffectTrigger trigger)
    {
        FactionEntity sourceEntity = _sourceFactionExp.Value as FactionEntity;

        if (sourceEntity == null)
        {
            throw new System.ArgumentException(
                "transfer_influence: invalid source faction to set relationship to: " +
                "\n - expression: " + ToString() +
                "\n - source faction: " + _sourceFactionExp.ToPartiallyEvaluatedString() +
                "\n - target faction: " + _targetFactionExp.ToPartiallyEvaluatedString() +
                "\n - percentage expression: " + _percentExp.ToPartiallyEvaluatedString());
        }

        FactionEntity targetEntity = _targetFactionExp.Value as FactionEntity;

        if (targetEntity == null)
        {
            throw new System.ArgumentException(
                "transfer_influence: invalid target faction to set relationship to: " +
                "\n - expression: " + ToString() +
                "\n - source faction: " + _sourceFactionExp.ToPartiallyEvaluatedString() +
                "\n - target faction: " + _targetFactionExp.ToPartiallyEvaluatedString() +
                "\n - percentage expression: " + _percentExp.ToPartiallyEvaluatedString());
        }

        float percentValue = _percentExp.Value;

        if (!percentValue.IsInsideRange(0, 1))
        {
            throw new System.ArgumentException(
                "transfer_influence: percentage to transfer outside of range (0, 1): " +
                "\n - expression: " + ToString() +
                "\n - source faction: " + _sourceFactionExp.ToPartiallyEvaluatedString() +
                "\n - target faction: " + _targetFactionExp.ToPartiallyEvaluatedString() +
                "\n - percentage expression: " + _percentExp.ToPartiallyEvaluatedString() +
                "\n - percentage value: " + _percentExp.ToPartiallyEvaluatedString());
        }

        Polity.TransferInfluence(sourceEntity.Faction, targetEntity.Faction, percentValue);
    }
}
