using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate void EffectApplierMethod();

public class EffectApplierEntityAttribute : EffectEntityAttribute
{
    private readonly EffectApplierMethod _applierMethod;

    private readonly PartiallyEvaluatedStringConverter _partialEvalStringConverter;

    public EffectApplierEntityAttribute(
        string id,
        Entity entity,
        EffectApplierMethod applierMethod,
        IExpression[] arguments,
        PartiallyEvaluatedStringConverter converter = null)
        : base(id, entity, arguments)
    {
        _applierMethod = applierMethod;
        _partialEvalStringConverter = converter;
    }

    public override void Apply(IEffectTrigger trigger)
    {
        _applierMethod();
    }

    public override string ToPartiallyEvaluatedString(int depth = -1)
    {
        return _partialEvalStringConverter?.Invoke(depth) ?? base.ToPartiallyEvaluatedString(depth);
    }
}
