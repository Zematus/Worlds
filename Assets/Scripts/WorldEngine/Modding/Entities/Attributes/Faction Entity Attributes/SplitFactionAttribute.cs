using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SplitFactionAttribute : EffectEntityAttribute
{
    private FactionEntity _factionEntity;

    private ModDecision _decisionToTrigger = null;

    private readonly IValueExpression<IEntity> _coreGroupArg;
    private readonly IValueExpression<float> _influencePercentToTransferArg;
    private readonly IValueExpression<float> _relationshipValueArg;
    private readonly IValueExpression<string> _newFactionTypeArg = null;

    public SplitFactionAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(factionEntity.BuildAttributeId(FactionEntity.SplitAttributeId), factionEntity, arguments, 1)
    {
        _factionEntity = factionEntity;

        _coreGroupArg =
            ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
        _influencePercentToTransferArg =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        _relationshipValueArg =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);

        if (arguments.Length > 3)
        {
            _newFactionTypeArg =
                ValueExpressionBuilder.ValidateValueExpression<string>(arguments[3]);
        }
    }

    public override void Apply()
    {
        if (!(_coreGroupArg.Value is GroupEntity groupEntity))
        {
            throw new System.Exception(
                "Seconds argument is not a group entity: " + _coreGroupArg.Value.GetType());
        }

        Faction faction = _factionEntity.Faction;

        string newFactionType;
        if (_newFactionTypeArg != null)
        {
            newFactionType = _newFactionTypeArg.Value;
        }
        else
        {
            newFactionType = faction.Type;
        }

        float influencePercentToTransfer = _influencePercentToTransferArg.Value;

        if (influencePercentToTransfer >= 0.5)
        {
            throw new System.Exception(
                "ERROR: SplitFactionAttribute.Apply - influence percent to transfer can't be equal or greater than 0.5" +
                "\n - attribute id: " + Id +
                "\n - partial influence to transfer expression: " + _influencePercentToTransferArg.ToPartiallyEvaluatedString(true) +
                "\n - world date: " + faction.World.CurrentDate +
                "\n - faction id: " + faction.Id +
                "\n - expression result: " + influencePercentToTransfer);
        }

        float influenceToTransfer = _influencePercentToTransferArg.Value * faction.Influence;

        faction.Split(
            newFactionType,
            groupEntity.Group,
            influenceToTransfer,
            _relationshipValueArg.Value);
    }
}
