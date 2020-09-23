using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TransferInfluenceAttribute : EffectEntityAttribute
{
    private PolityEntity _polityEntity;

    private readonly IValueExpression<float> _percentExp;

    private readonly FactionEntity _sourceFactionEntity;
    private readonly FactionEntity _targetFactionEntity;

    public TransferInfluenceAttribute(PolityEntity polityEntity, IExpression[] arguments)
        : base(PolityEntity.TransferInfluenceAttributeId, polityEntity, arguments, 3)
    {
        _polityEntity = polityEntity;

        _percentExp = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);

        IValueExpression<Entity> sourceFactionExp =
            ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);

        _sourceFactionEntity = sourceFactionExp.Value as FactionEntity;

        if (_sourceFactionEntity == null)
        {
            throw new System.Exception(
                "Input parameter 0 is not of a valid faction entity: " + sourceFactionExp.Value.GetType() +
                "\n - expression: " + sourceFactionExp.ToString() +
                "\n - value: " + sourceFactionExp.ToPartiallyEvaluatedString());
        }

        IValueExpression<Entity> targetFactionExp =
            ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[1]);

        _targetFactionEntity = targetFactionExp.Value as FactionEntity;

        if (_targetFactionEntity == null)
        {
            throw new System.Exception(
                "Input parameter 1 is not of a valid faction entity: " + targetFactionExp.Value.GetType() +
                "\n - expression: " + targetFactionExp.ToString() +
                "\n - value: " + targetFactionExp.ToPartiallyEvaluatedString());
        }
    }

    public override void Apply()
    {
        float percentValue = _percentExp.Value;

        if (!percentValue.IsInsideRange(0, 1))
        {
            throw new System.ArgumentException(
                "transfer_influence: percentage to transfer outside of range (0, 1): " +
                "\n - expression: " + ToString() +
                "\n - percentage expression: " + _percentExp.ToPartiallyEvaluatedString() +
                "\n - percentage value: " + _percentExp.ToPartiallyEvaluatedString());
        }

        Polity.TransferInfluence(_sourceFactionEntity.Faction, _targetFactionEntity.Faction, percentValue);
    }
}
