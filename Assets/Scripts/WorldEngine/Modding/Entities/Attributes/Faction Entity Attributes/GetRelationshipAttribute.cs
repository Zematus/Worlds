using UnityEngine;

public class GetRelationshipAttribute : ValueEntityAttribute<float>
{
    private FactionEntity _factionEntity;

    private readonly IValueExpression<IEntity> _argumentExp;

    public GetRelationshipAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.GetRelationshipAttributeId, factionEntity, arguments, 1)
    {
        _factionEntity = factionEntity;

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
    }

    public override float Value
    {
        get
        {
            if (_argumentExp.Value is FactionEntity fEntity)
            {
                if (fEntity.Faction == null)
                {
                    throw new System.Exception(
                        $"faction entity is null. {_factionEntity.Context.DebugType}: {_factionEntity.Context.Id}");
                }

                float value = _factionEntity.Faction.GetRelationshipValue(fEntity.Faction);

#if DEBUG
                if ((value <= 0) || (value >= 1))
                {
                    Debug.LogWarning($"Relationship value not between 0 and 1: {value}");
                }
#endif

                return value;
            }

            throw new System.Exception(
                $"Input parameter is not of a valid faction entity: {_argumentExp.Value.GetType()}" +
                $"\n - expression: {_argumentExp.ToString()}" +
                $"\n - value: {_argumentExp.ToPartiallyEvaluatedString()}");
        }
    }
}
