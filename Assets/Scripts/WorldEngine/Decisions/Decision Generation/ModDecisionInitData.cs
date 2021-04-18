using UnityEngine;
using System.Collections.Generic;

public class ModDecisionData
{
    public readonly ModDecision Decision;

    public readonly Context SourceContext;

    public readonly IEffectTrigger Trigger;

    public readonly Faction TargetFaction;

    public readonly IBaseValueExpression[] ParameterValues;

    public ModDecisionData(
        ModDecision decision,
        IEffectTrigger trigger,
        Context sourceContext,
        Faction targetFaction,
        IBaseValueExpression[] parameterValues)
    {
        Decision = decision;

        Trigger = trigger;
        SourceContext = sourceContext;

        TargetFaction = targetFaction;
        ParameterValues = parameterValues;
    }
}
