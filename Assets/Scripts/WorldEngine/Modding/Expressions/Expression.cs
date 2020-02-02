using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Expression
{
    public static Expression BuildExpression(Context context, string expressionStr)
    {
#if DEBUG
        TestMatch(context, expressionStr);
#endif

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
        string attributeStr = match.Groups["attribute"].Value;

        Expression expression = BuildExpression(context, expressionStr);

        if (!(expression is EntityExpression entExpression))
        {
            throw new System.ArgumentException("Not a valid entity expression: " + expression);
        }

        Match identifierMatch = Regex.Match(attributeStr, ModUtility.IdentifierStatementRegex);

        string identifierStr = match.Groups["identifier"].Value;

        EntityAttribute attribute = entExpression.GetEntity().GetAttribute(identifierStr);

        if (attribute is BooleanEntityAttribute)
        {
            return new BooleanEntityAttributeExpression(attribute, expressionStr, identifierStr);
        }

        if (attribute is EntityEntityAttribute)
        {
            return new EntityEntityAttributeExpression(attribute, expressionStr, identifierStr);
        }

        throw new System.ArgumentException("Unrecognized attribute type: " + attribute.GetType());
    }

#if DEBUG
    private static void TestMatch(Context context, string text)
    {
        bool matched = false;

        Debug.Log("- Test parsing: " + text);

        //Match match = Regex.Match(text, ModUtility.FunctionStatementRegex);

        //if (match.Success == true)
        //{
        //    matched = true;
        //    Debug.Log("Matched FunctionStatementRegex");
        //}

        Match match = Regex.Match(text, ModUtility.UnaryOpStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched UnaryOpStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
        }

        match = Regex.Match(text, ModUtility.BinaryOpStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched BinaryOpStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
            //Debug.Log("-- -- statement1: " + ModUtility.Debug_CapturesToString(match.Groups["statement1"]));
            //Debug.Log("-- -- binaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["binaryOp"]));
            //Debug.Log("-- -- statement2: " + ModUtility.Debug_CapturesToString(match.Groups["statement2"]));
            //Debug.Log("-- -- operand2: " + ModUtility.Debug_CapturesToString(match.Groups["operand2"]));
            //Debug.Log("-- -- restOp: " + ModUtility.Debug_CapturesToString(match.Groups["restOp"]));
        }

        match = Regex.Match(text, ModUtility.AccessorOpStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched AccessorOpStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
        }

        match = Regex.Match(text, ModUtility.OperandStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched OperandStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
            //Debug.Log("-- -- baseStatement: " + ModUtility.Debug_CapturesToString(match.Groups["baseStatement"]));
            //Debug.Log("-- -- innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));
        }

        match = Regex.Match(text, ModUtility.ArgumentListRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched ArgumentListRegex");
            //Debug.Log("-- -- match: " + match.Value);
            //Debug.Log("-- -- argument: " + ModUtility.Debug_CapturesToString(match.Groups["argument"]));
            //Debug.Log("-- -- otherArgs: " + ModUtility.Debug_CapturesToString(match.Groups["otherArgs"]));
        }

        match = Regex.Match(text, ModUtility.IdentifierStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched IdentifierStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
            //Debug.Log("-- -- identifier: " + ModUtility.Debug_CapturesToString(match.Groups["argument"]));
        }

        match = Regex.Match(text, ModUtility.BaseStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched BaseStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
        }

        if (!matched)
        {
            Debug.Log("-- Test match failed");
        }
    }
#endif

    private static Expression[] BuildFunctionArgumentExpressions(Context context, string arguments)
    {
        List<Expression> argExpressions = new List<Expression>();

        Match match = Regex.Match(arguments, ModUtility.ArgumentListRegex);
//
//#if DEBUG
//        TestMatch(context, arguments);
//#endif

        while (match.Success == true)
        {
            string argument = match.Groups["argument"].Value;
            string otherArgs = match.Groups["otherArgs"].Value;

            //Debug.Log("- match: " + match.Value);
            //Debug.Log("- argument: " + ModUtility.Debug_CapturesToString(match.Groups["argument"]));
            //Debug.Log("-- unaryOpStatement: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOpStatement"]));
            //Debug.Log("-- binaryOpStatement: " + ModUtility.Debug_CapturesToString(match.Groups["binaryOpStatement"]));
            //Debug.Log("-- accessorOpStatement: " + ModUtility.Debug_CapturesToString(match.Groups["accessorOpStatement"]));
            //Debug.Log("-- functionStatement: " + ModUtility.Debug_CapturesToString(match.Groups["functionStatement"]));
            //Debug.Log("-- baseStatement: " + ModUtility.Debug_CapturesToString(match.Groups["baseStatement"]));
            //Debug.Log("-- innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));
            //Debug.Log("- otherArgs: " + ModUtility.Debug_CapturesToString(match.Groups["otherArgs"]));

            argExpressions.Add(BuildExpression(context, argument));

            match = Regex.Match(otherArgs, ModUtility.ArgumentListRegex);
//
//#if DEBUG
//            TestMatch(context, otherArgs);
//#endif
        }

        return argExpressions.ToArray();
    }

    private static Expression BuildIdentifierExpression(Context context, Match match)
    {
        string identifier = match.Groups["identifier"].Value.Trim();
        string arguments = match.Groups["arguments"].Value;

        Expression[] argExpressions = null;
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            argExpressions = BuildFunctionArgumentExpressions(context, arguments);
        }

        switch (identifier)
        {
            case "lerp":
                return new LerpFunctionExpression(argExpressions);
        }

        //throw new System.ArgumentException("Unrecognized function: " + identifier);

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
        string binaryOp = match.Groups["binaryOp"].Value.Trim();
        string expressionAStr = match.Groups["statement1"].Value;
        string expressionBStr = match.Groups["statement2"].Value;

        switch (binaryOp)
        {
            case "+":
                return SumExpression.Build(context, expressionAStr, expressionBStr);
            case "-":
                return SubstractExpression.Build(context, expressionAStr, expressionBStr);
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

        throw new System.ArgumentException("Unrecognized binary op: " + binaryOp);
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

        match = Regex.Match(expressionStr, ModUtility.IdentifierStatementRegex);
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

        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("funcName: " + ModUtility.Debug_CapturesToString(match.Groups["funcName"]));
            //Debug.Log("arguments: " + ModUtility.Debug_CapturesToString(match.Groups["arguments"]));

            return BuildIdentifierExpression(context, match);
        }

        throw new System.ArgumentException("Not a recognized expression: " + expressionStr);
    }

    public abstract void Reset();
}
