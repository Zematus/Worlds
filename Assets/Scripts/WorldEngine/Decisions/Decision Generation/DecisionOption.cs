using UnityEngine;
using System.Collections.Generic;

public class DecisionOption : OptionalDescription
{
    public INumericExpression Weight;

    public DecisionOptionEffect[] Effects;

    public DecisionOption(ModDecision decision) : base(decision)
    {
    }
}
