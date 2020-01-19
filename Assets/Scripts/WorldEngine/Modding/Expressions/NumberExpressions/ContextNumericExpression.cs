using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ContextNumericExpression : NumericExpression
{
    public const string Regex = @"^" + ModUtility.IdentifierRegexPart + @"\s*$";

    private readonly Context Context;

    public string Identifier;

    static public bool IsNumericExpression(Context context, string identifier)
    {
        if (context.Expressions.TryGetValue(identifier, out Expression expression))
        {
            return expression is NumericExpression;
        }

        return false;
    }

    public ContextNumericExpression(Context context, string identifier)
    {
        if (!context.Expressions.ContainsKey(identifier))
        {
            throw new System.ArgumentException("Identifier '" + identifier +
                "' not defined within '" + context.Id + "'");
        }

        Context = context;

        Identifier = identifier;
    }

    public override float Evaluate()
    {
        NumericExpression expression = ValidateExpression(Context.Expressions[Identifier]);

        return expression.Evaluate();
    }

    public override string ToString()
    {
        return Identifier;
    }
}
