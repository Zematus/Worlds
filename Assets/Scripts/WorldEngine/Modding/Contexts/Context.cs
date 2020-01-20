using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Context
{
    readonly public string Id;

    readonly public Dictionary<string, Expression> Expressions = new Dictionary<string, Expression>();
    readonly public Dictionary<string, Entity> Entities = new Dictionary<string, Entity>();

    public Context(string id)
    {
        Id = id;
    }

    public void ResetExpressionCaches()
    {
        foreach (Expression expression in Expressions.Values)
        {
            expression.ResetCache();
        }
    }
}
