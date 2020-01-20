using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ContextBooleanExpression : BooleanExpression
{
    public const string Regex = @"^" + ModUtility.IdentifierRegexPart + @"\s*$";

    private readonly Context Context;

    public string Identifier;

    static public bool IsBooleanExpression(Context context, string identifier)
    {
        if (context.Expressions.TryGetValue(identifier, out Expression expression))
        {
            return expression is BooleanExpression;
        }

        return false;
    }

    public ContextBooleanExpression(Context context, string identifier)
    {
        if (!context.Expressions.ContainsKey(identifier))
        {
            throw new System.ArgumentException("Identifier '" + identifier +
                "' not defined within '" + context.Id + "'");
        }

        Context = context;

        Identifier = identifier;
    }

    protected override bool Evaluate()
    {
        BooleanExpression expression = ValidateExpression(Context.Expressions[Identifier]);

        return expression.GetValue();
    }

    public override string ToString()
    {
        return Identifier;
    }
}
