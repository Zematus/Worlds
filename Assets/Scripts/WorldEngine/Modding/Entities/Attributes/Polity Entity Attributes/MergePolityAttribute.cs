using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MergePolityAttribute : EffectEntityAttribute
{
    private PolityEntity _polityEntity;

    private readonly IValueExpression<IEntity> _mergedPolityExp;

    public MergePolityAttribute(PolityEntity polityEntity, IExpression[] arguments)
        : base(PolityEntity.MergeAttributeId, polityEntity, arguments, 1)
    {
        _polityEntity = polityEntity;

        _mergedPolityExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
    }

    public override void Apply(IEffectTrigger trigger)
    {
        PolityEntity mergedPolityEntity = _mergedPolityExp.Value as PolityEntity;

        if (mergedPolityEntity == null)
        {
            throw new System.ArgumentException(
                Id + ": invalid splitting faction: " +
                "\n - expression: " + ToString() +
                "\n - merged polity: " + _mergedPolityExp.ToPartiallyEvaluatedString());
        }

        _polityEntity.Polity.MergePolity(mergedPolityEntity.Polity);
    }
}
