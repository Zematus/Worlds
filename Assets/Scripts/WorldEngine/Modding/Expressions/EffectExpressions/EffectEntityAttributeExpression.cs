using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EffectEntityAttributeExpression : EntityAttributeExpression, IEffectExpression
{
    private readonly EffectEntityAttribute _effectAttribute;

    public EffectEntityAttributeExpression(
        EntityAttribute attribute) : base(attribute)
    {
        Trigger = null;
        _effectAttribute = attribute as EffectEntityAttribute;
    }

    public IEffectTrigger Trigger { get; set; }

    public void Apply()
    {
        if (Trigger == null)
        {
            throw new System.Exception(
                "Effect expression trigger must be set before applying...");
        }

#if DEBUG
        if (Trigger.GetLastUseDate(this) == Manager.CurrentWorld.CurrentDate)
        {
            //Debug.LogWarning(
            //    "Trigger has already been used by this expression in this date: " +
            //    Manager.CurrentWorld.CurrentDate);
        }
#endif

        _effectAttribute.Apply(Trigger);

#if DEBUG
        // This will let us know if the trigger is being reused
        Trigger.SetLastUseDate(this, Manager.CurrentWorld.CurrentDate);
#endif

        // Reset the trigger after applying to force next caller to set it before the
        // next call to apply
        // NOTE: The reason we define trigger as property as opposed to a parameter
        // is that that way it can be set beforehand, outside of the closure where
        // 'Apply' will be called
        Trigger = null;
    }
}
