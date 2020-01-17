using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Expression
{
    public static Expression BuildExpression(string expressionStr)
    {
        Debug.Log("parsing: " + expressionStr);

        Match match = Regex.Match(expressionStr, ModUtility.BinaryOpStatementRegex);

        if (match.Success == true)
        {
            Debug.Log("match: " + match.Value);
            Debug.Log("statement1: " + ModUtility.Debug_CapturesToString(match.Groups["statement1"]));
            Debug.Log("binaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["binaryOp"]));
            Debug.Log("statement2: " + ModUtility.Debug_CapturesToString(match.Groups["statement2"]));

            return BuildBinaryOpExpression(match);
        }

        match = Regex.Match(expressionStr, ModUtility.UnaryOpStatementRegex);
        if (match.Success == true)
        {
            Debug.Log("match: " + match.Value);
            Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            Debug.Log("unaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOp"]));

            return BuildUnaryOpExpression(match);
        }

        match = Regex.Match(expressionStr, ModUtility.InnerStatementRegex);
        if (match.Success == true)
        {
            Debug.Log("match: " + match.Value);
            Debug.Log("innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));

            expressionStr = match.Groups["innerStatement"].Value;

            return BuildExpression(expressionStr);
        }

        match = Regex.Match(expressionStr, ModUtility.BaseStatementRegex);
        if (match.Success == true)
        {
            expressionStr = match.Groups["statement"].Value;

            return BuildBaseExpression(expressionStr);
        }

        throw new System.ArgumentException("Not a valid parseable expression: " + expressionStr);
    }

    private static Expression BuildUnaryOpExpression(Match match)
    {
        string expressionStr = match.Groups["statement"].Value;
        string unaryOp = match.Groups["unaryOp"].Value.Trim().ToUpper();

        switch (unaryOp)
        {
            case "-":
                return NegateExpression.Build(expressionStr);
            case "!":
                return null;
        }

        throw new System.ArgumentException("Unrecognized unary op: " + unaryOp);
    }

    private static Expression BuildBinaryOpExpression(Match match)
    {
        string expressionAStr = match.Groups["statement1"].Value;
        string expressionBStr = match.Groups["statement2"].Value;
        string opStr = match.Groups["opStr"].Value.Trim().ToUpper();

        switch (opStr)
        {
            case "+":
                return SumExpression.Build(expressionAStr, expressionBStr);
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
        }

        throw new System.ArgumentException("Unrecognized binary op: " + opStr);
    }

    private static Expression BuildBaseExpression(string expressionStr)
    {
        Match match = Regex.Match(expressionStr, NumberExpression.Regex);
        if (match.Success == true)
        {
            return new NumberExpression(expressionStr);
        }

        throw new System.ArgumentException("Not a recognized factor: " + expressionStr);
    }

    public static Expression[] BuildExpressions(ICollection<string> expressionStrs)
    {
        Expression[] expression = new Expression[expressionStrs.Count];

        int i = 0;
        foreach (string expressionStr in expressionStrs)
        {
            expression[i++] = BuildExpression(expressionStr);
        }

        return expression;
    }

    public abstract float Evaluate();
}
