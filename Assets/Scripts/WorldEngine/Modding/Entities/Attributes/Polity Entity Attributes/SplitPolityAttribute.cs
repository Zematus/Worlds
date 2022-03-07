using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SplitPolityAttribute : EffectEntityAttribute
{
    private PolityEntity _polityEntity;

    private readonly IValueExpression<IEntity> _splittingFactionExp;

    public SplitPolityAttribute(PolityEntity polityEntity, IExpression[] arguments)
        : base(PolityEntity.SplitAttributeId, polityEntity, arguments, 1)
    {
        _polityEntity = polityEntity;

        _splittingFactionExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
    }

    public override void Apply(IEffectTrigger trigger)
    {
        FactionEntity factionEntity = _splittingFactionExp.Value as FactionEntity;

        if (factionEntity == null)
        {
            throw new System.ArgumentException(
                "split: invalid splitting faction: " +
                "\n - expression: " + ToString() +
                "\n - splitting faction: " + _splittingFactionExp.ToPartiallyEvaluatedString());
        }

        _polityEntity.Polity.Split(Tribe.PolityTypeStr, factionEntity.Faction);
    }
}
