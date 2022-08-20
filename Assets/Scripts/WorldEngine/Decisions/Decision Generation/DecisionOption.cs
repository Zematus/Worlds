using UnityEngine;
using System.Collections.Generic;

public class DecisionOption : OptionalDescription
{
    public GuideType AllowedGuide = GuideType.All;

    public IValueExpression<float> Weight;

    public DecisionOptionEffect[] Effects;

    public DecisionOption(ModDecision decision) : base(decision)
    {
    }
}
