using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Expression
{
    public static Expression BuildExpression(Context context, string expressionStr)
    {
        //Debug.Log("parsing: " + expressionStr);

        Match match = Regex.Match(expressionStr, ModUtility.AccessorOpStatementRegex);

        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            //Debug.Log("attribute: " + ModUtility.Debug_CapturesToString(match.Groups["attribute"]));

            return BuildAccessorOpExpression(context, match);
        }

        match = Regex.Match(expressionStr, ModUtility.BinaryOpStatementRegex);

        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement1: " + ModUtility.Debug_CapturesToString(match.Groups["statement1"]));
            //Debug.Log("binaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["binaryOp"]));
            //Debug.Log("statement2: " + ModUtility.Debug_CapturesToString(match.Groups["statement2"]));

            return BuildBinaryOpExpression(context, match);
        }

        match = Regex.Match(expressionStr, ModUtility.UnaryOpStatementRegex);
        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            //Debug.Log("unaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOp"]));

            return BuildUnaryOpExpression(context, match);
        }

        match = Regex.Match(expressionStr, ModUtility.FunctionStatementRegex);
        if (match.Success == true)
        {
            Debug.Log("match: " + match.Value);
            Debug.Log("funcName: " + ModUtility.Debug_CapturesToString(match.Groups["funcName"]));
            Debug.Log("arguments: " + ModUtility.Debug_CapturesToString(match.Groups["arguments"]));

            return BuildFunctionExpression(context, match);
        }

        match = Regex.Match(expressionStr, ModUtility.InnerStatementRegex);
        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));

            expressionStr = match.Groups["innerStatement"].Value;

            return BuildExpression(context, expressionStr);
        }

        match = Regex.Match(expressionStr, ModUtility.BaseStatementRegex);
        if (match.Success == true)
        {
            expressionStr = match.Groups["statement"].Value;

            return BuildBaseExpression(context, expressionStr);
        }

        throw new System.ArgumentException("Not a valid parseable expression: " + expressionStr);
    }

    private static Expression BuildAccessorOpExpression(Context context, Match match)
    {
        string expressionStr = match.Groups["statement"].Value;
        string attributeId = match.Groups["attribute"].Value;

        Expression expression = BuildExpression(context, expressionStr);

        if (!(expression is EntityExpression entExpression))
        {
            throw new System.ArgumentException("Not a valid entity expression: " + expression);
        }

        EntityAttribute attribute = entExpression.GetEntity().GetAttribute(attributeId);

        if (attribute is BooleanEntityAttribute)
        {
            return new BooleanEntityAttributeExpression(attribute, expressionStr, attributeId);
        }

        if (attribute is EntityEntityAttribute)
        {
            return new EntityEntityAttributeExpression(attribute, expressionStr, attributeId);
        }

        throw new System.ArgumentException("Unrecognized attribute type: " + attribute.GetType());
    }

    private static Expression[] BuildFunctionArgumentExpressions(Context context, string arguments)
    {
        List<Expression> argExpressions = new List<Expression>();

        Match match = Regex.Match(arguments, ModUtility.ArgumentListRegex);

        while (match.Success == true)
        {
            string argument = match.Groups["argument"].Value;
            string otherArgs = match.Groups["otherArgs"].Value;

            Debug.Log("match: " + match.Value);
            Debug.Log("argument: " + ModUtility.Debug_CapturesToString(match.Groups["argument"]));
            Debug.Log("otherArgs: " + ModUtility.Debug_CapturesToString(match.Groups["otherArgs"]));

            argExpressions.Add(BuildExpression(context, argument));

            match = Regex.Match(otherArgs, ModUtility.ArgumentListRegex);
        }

        return argExpressions.ToArray();
    }

    private static Expression BuildFunctionExpression(Context context, Match match)
    {
        string funcName = match.Groups["funcName"].Value.Trim();
        string arguments = match.Groups["arguments"].Value;

        Expression[] argExpressions = BuildFunctionArgumentExpressions(context, arguments);

        switch (funcName)
        {
            case "lerp":
                return null;
        }

        //throw new System.ArgumentException("Unrecognized function: " + funcName);

        return null;
    }

    private static Expression BuildUnaryOpExpression(Context context, Match match)
    {
        string unaryOp = match.Groups["unaryOp"].Value.Trim();
        string expressionStr = match.Groups["statement"].Value;

        switch (unaryOp)
        {
            case "-":
                return NegateNumberExpression.Build(context, expressionStr);
            case "!":
                return NegateBooleanValueExpression.Build(context, expressionStr);
        }

        throw new System.ArgumentException("Unrecognized unary op: " + unaryOp);
    }

    private static Expression BuildBinaryOpExpression(Context context, Match match)
    {
        string opStr = match.Groups["opStr"].Value.Trim();
        string expressionAStr = match.Groups["statement1"].Value;
        string expressionBStr = match.Groups["statement2"].Value;

        switch (opStr)
        {
            case "+":
                return SumExpression.Build(context, expressionAStr, expressionBStr);
            case "-":
                return null;
            case "*":
                return null;
            case "/":
                return null;
            case "=":
                return null;
            case "==":
                return null;
            case ">=":
                return null;
            case "<=":
                return null;
            case ">":
                return null;
            case "<":
                return null;
            case "&&":
                return null;
            case "||":
                return null;
        }

        throw new System.ArgumentException("Unrecognized binary op: " + opStr);
    }

    private static Expression BuildBaseExpression(Context context, string expressionStr)
    {
        Match match = Regex.Match(expressionStr, FixedNumberExpression.Regex);
        if (match.Success == true)
        {
            return new FixedNumberExpression(expressionStr);
        }

        match = Regex.Match(expressionStr, FixedBooleanValueExpression.Regex);
        if (match.Success == true)
        {
            return new FixedBooleanValueExpression(expressionStr);
        }

        match = Regex.Match(expressionStr, ModUtility.IdentifierRegex);
        if (match.Success == true)
        {
            if (context.Entities.TryGetValue(expressionStr, out Entity entity))
            {
                return new FixedEntityExpression(entity);
            }

            if (context.Expressions.TryGetValue(expressionStr, out Expression expression))
            {
                return expression;
            }
        }

        throw new System.ArgumentException("Not a recognized expression: " + expressionStr);
    }

    public abstract void Reset();
}
