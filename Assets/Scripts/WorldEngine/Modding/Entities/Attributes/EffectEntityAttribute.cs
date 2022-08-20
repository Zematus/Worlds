using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EffectEntityAttribute : EntityAttribute
{
    public EffectEntityAttribute(
        string id, IEntity entity, IExpression[] arguments = null, int minArguments = 0)
        : base(id, entity, arguments)
    {
        if ((minArguments > 0) && ((arguments == null) || (arguments.Length < minArguments)))
        {
            throw new System.ArgumentException(
                ToString() + ": number of arguments given less than " + minArguments);
        }
    }

    public abstract void Apply(IEffectTrigger trigger);

    protected override IExpression BuildExpression()
    {
        return new EffectEntityAttributeExpression(this);
    }
}
