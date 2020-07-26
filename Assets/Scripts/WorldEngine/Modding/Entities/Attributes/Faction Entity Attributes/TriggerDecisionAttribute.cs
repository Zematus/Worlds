﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TriggerDecisionAttribute : EffectEntityAttribute
{
    private FactionEntity _factionEntity;

    private ModDecision _decisionToTrigger = null;

    private readonly IValueExpression<string> _argumentExp;

    private readonly IBaseValueExpression[] _parameterExps;

    public TriggerDecisionAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.TriggerDecisionAttributeId, factionEntity, arguments, 1)
    {
        _factionEntity = factionEntity;

        _argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);

        if (arguments.Length > 1)
        {
            _parameterExps = new IBaseValueExpression[arguments.Length - 1];

            for (int i = 0; i < _parameterExps.Length; i++)
            {
                _parameterExps[i] =
                    ValueExpressionBuilder.ValidateValueExpression(arguments[i + 1]);
            }
        }
        else
        {
            _parameterExps = null;
        }
    }

    public override void Apply()
    {
        _decisionToTrigger = ModDecision.Decisions[_argumentExp.Value];

        _decisionToTrigger.Set(_factionEntity.Faction, _parameterExps);
        _decisionToTrigger.Evaluate();
    }
}
