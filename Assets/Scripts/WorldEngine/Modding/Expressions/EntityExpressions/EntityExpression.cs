using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EntityExpression : IValueExpression<IEntity>
{
    public EntityExpression(IEntity entity)
    {
        Value = entity;
    }

    public override string ToString() => Value.Id;

    public string GetFormattedString() => Value.GetFormattedString();

    public IEntity Value { get; }

    public object ValueObject => Value;

    public bool RequiresInput => Value.RequiresInput;

    public string ToPartiallyEvaluatedString(int depth = -1) =>
        Value.ToPartiallyEvaluatedString(depth);

    public bool TryGetRequest(out InputRequest request) =>
        Value.TryGetRequest(out request);
}
