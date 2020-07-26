using UnityEngine;
using System.Collections.Generic;

public class DecisionOption : OptionalDescription
{
    public IValueExpression<float> Weight;

    public DecisionOptionEffect[] Effects;

    public DecisionOption(ModDecision decision) : base(decision)
    {
    }
}
