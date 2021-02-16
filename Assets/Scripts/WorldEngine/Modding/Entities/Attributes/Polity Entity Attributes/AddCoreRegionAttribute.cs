using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddCoreRegionAttribute : EffectEntityAttribute
{
    private PolityEntity _polityEntity;

    private readonly IValueExpression<IEntity> _targetRegionExp;

    public AddCoreRegionAttribute(PolityEntity polityEntity, IExpression[] arguments)
        : base(PolityEntity.AddCoreRegionAttributeId, polityEntity, arguments, 1)
    {
        _polityEntity = polityEntity;

        _targetRegionExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
    }

    public override void Apply()
    {
        RegionEntity regionEntity = _targetRegionExp.Value as RegionEntity;

        if (regionEntity == null)
        {
            throw new System.ArgumentException(
                "transfer_influence: invalid target region: " +
                "\n - expression: " + ToString() +
                "\n - target region: " + _targetRegionExp.ToPartiallyEvaluatedString());
        }

        _polityEntity.Polity.AddCoreRegion(regionEntity.Region);
    }
}
