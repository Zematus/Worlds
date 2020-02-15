using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Context
{
    readonly public string Id;

    readonly public Dictionary<string, IExpression> Expressions = new Dictionary<string, IExpression>();
    readonly public Dictionary<string, Entity> Entities = new Dictionary<string, Entity>();

    public Context(string id)
    {
        Id = id;
    }

    public void Reset()
    {
        foreach (IExpression expression in Expressions.Values)
        {
            //expression.Reset();
        }
    }
}
