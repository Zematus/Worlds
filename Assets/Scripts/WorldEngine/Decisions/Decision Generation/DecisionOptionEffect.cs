using UnityEngine;
using System.Collections.Generic;

public class DecisionOptionEffect : Description
{
    public IEffectExpression Result;

    public DecisionOptionEffect(DecisionOption option) : base(option)
    {
    }
}
