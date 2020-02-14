using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Expression
{
    public static Expression BuildExpression(Context context, string expressionStr)
    {
#if DEBUG
        //TestMatch(context, expressionStr);
#endif

        Match match = Regex.Match(expressionStr, ModUtility.BinaryOpStatementRegex);
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

        match = Regex.Match(expressionStr, ModUtility.AccessorOpStatementRegex);

        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            //Debug.Log("attribute: " + ModUtility.Debug_CapturesToString(match.Groups["attribute"]));
            //Debug.Log("identifier: " + ModUtility.Debug_CapturesToString(match.Groups["identifier"]));
            //Debug.Log("arguments: " + ModUtility.Debug_CapturesToString(match.Groups["arguments"]));

            return BuildAccessorOpExpression(context, match);
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
            return BuildBaseExpression(context, match);
        }

        throw new System.ArgumentException("Not a valid parseable expression: " + expressionStr);
    }

    private static Expression BuildAccessorOpExpression(Context context, Match match)
    {
        string entityStr = match.Groups["statement"].Value;
        string attributeStr = match.Groups["attribute"].Value;

        Expression expression = BuildExpression(context, entityStr);

        if (!(expression is EntityExpression entExpression))
        {
            throw new System.ArgumentException("Not a valid entity expression: " + expression);
        }

        Match identifierMatch = Regex.Match(attributeStr, ModUtility.IdentifierStatementRegex);

        string identifier = match.Groups["identifier"].Value;
        string arguments = match.Groups["arguments"].Value;

        Expression[] argExpressions = null;
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            argExpressions = BuildFunctionArgumentExpressions(context, arguments);
        }

        EntityAttribute attribute = entExpression.GetEntity().GetAttribute(identifier, argExpressions);

        if (attribute is BooleanEntityAttribute)
        {
            return new BooleanEntityAttributeExpression(attribute, entityStr, identifier, arguments);
        }

        if (attribute is EntityEntityAttribute)
        {
            return new EntityEntityAttributeExpression(attribute, entityStr, identifier, arguments);
        }

        if (attribute is NumericEntityAttribute)
        {
            return new NumericEntityAttributeExpression(attribute, entityStr, identifier, arguments);
        }

        if (attribute is StringEntityAttribute)
        {
            return new StringEntityAttributeExpression(attribute, entityStr, identifier, arguments);
        }

        throw new System.ArgumentException("Unrecognized attribute type: " + attribute.GetType());
    }

#if DEBUG
    private static void TestMatch(Context context, string text)
    {
        bool matched = false;

        Debug.Log("- Test parsing: " + text);

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

        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new FixedStringValueExpression(identifier);
        }

        throw new System.ArgumentException("Unrecognized function identifier: " + identifier);
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
                return MultiplyExpression.Build(context, expressionAStr, expressionBStr);
            case "/":
                return null;
            case "=":
                return null;
            case "==":
                return EqualsExpression.Build(context, expressionAStr, expressionBStr);
            case ">=":
                return null;
            case "<=":
                return null;
            case ">":
                return MoreThanExpression.Build(context, expressionAStr, expressionBStr);
            case "<":
                return LessThanExpression.Build(context, expressionAStr, expressionBStr);
            case "&&":
                return null;
            case "||":
                return null;
        }

        throw new System.ArgumentException("Unrecognized binary op: " + binaryOp);
    }

    private static Expression BuildBaseExpression(Context context, Match match)
    {
        string number = match.Groups["number"].Value;
        string boolean = match.Groups["boolean"].Value;
        string identifierStatement = match.Groups["identifierStatement"].Value;

        if (!string.IsNullOrWhiteSpace(number))
        {
            return new FixedNumberExpression(number);
        }

        if (!string.IsNullOrWhiteSpace(boolean))
        {
            return new FixedBooleanValueExpression(boolean);
        }

        if (!string.IsNullOrWhiteSpace(identifierStatement))
        {
            if (context.Entities.TryGetValue(identifierStatement, out Entity entity))
            {
                return new FixedEntityExpression(entity);
            }

            if (context.Expressions.TryGetValue(identifierStatement, out Expression expression))
            {
                return expression;
            }

            return BuildIdentifierExpression(context, match);
        }

        throw new System.ArgumentException("Unrecognized statement: " + match.Value);
    }

    public static IStringExpression ValidateStringExpression(Expression expression)
    {
        if (!(expression is IStringExpression strExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid string expression");
        }

        return strExpression;
    }
}
