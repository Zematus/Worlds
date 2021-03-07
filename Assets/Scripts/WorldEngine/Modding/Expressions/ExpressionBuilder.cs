﻿using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public static class ExpressionBuilder
{
    public static IExpression BuildExpression(
        Context context, string expressionStr, bool allowInputRequesters = false)
    {
#if DEBUG
        //TestMatch(context, expressionStr);
#endif

        IExpression expression = null;

        Match match = Regex.Match(expressionStr, ModParseUtility.BinaryOpStatementRegex);
        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement1: " + ModUtility.Debug_CapturesToString(match.Groups["statement1"]));
            //Debug.Log("binaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["binaryOp"]));
            //Debug.Log("statement2: " + ModUtility.Debug_CapturesToString(match.Groups["statement2"]));

            expression = BuildBinaryOpExpression(context, match, allowInputRequesters);
        }

        if (( expression == null) &&
            ((match = Regex.Match(
                expressionStr, ModParseUtility.UnaryOpStatementRegex)).Success == true))
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            //Debug.Log("unaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOp"]));

            expression = BuildUnaryOpExpression(context, match, allowInputRequesters);
        }

        if ((expression == null) &&
            ((match = Regex.Match(
                expressionStr, ModParseUtility.AccessorOpStatementRegex)).Success == true))
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            //Debug.Log("attribute: " + ModUtility.Debug_CapturesToString(match.Groups["attribute"]));
            //Debug.Log("identifier: " + ModUtility.Debug_CapturesToString(match.Groups["identifier"]));
            //Debug.Log("arguments: " + ModUtility.Debug_CapturesToString(match.Groups["arguments"]));

            expression = BuildAccessorOpExpression(context, match, allowInputRequesters);
        }

        if ((expression == null) &&
            ((match = Regex.Match(
                expressionStr, ModParseUtility.InnerStatementRegex)).Success == true))
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));

            expressionStr = match.Groups["innerStatement"].Value.Trim();

            expression = BuildExpression(context, expressionStr, allowInputRequesters);
        }

        if ((expression == null) &&
            ((match = Regex.Match(
                expressionStr, ModParseUtility.BaseStatementRegex)).Success == true))
        {
            expression = BuildBaseExpression(context, match, allowInputRequesters);
        }

        if (expression == null)
        {
            throw new System.ArgumentException(
                "Not a valid expression: " + expressionStr);
        }

        if ((!allowInputRequesters) && expression.RequiresInput)
        {
            throw new System.ArgumentException(
                "Context doesn't allow expressions that require user input. Expression: " +
                expressionStr);
        }

        return expression;
    }

    private static IExpression BuildAccessorOpExpression(
        Context context, Match match, bool allowInputRequesters = false)
    {
        string entityStr = match.Groups["statement"].Value.Trim();

        IValueExpression<IEntity> entExpression =
            ValueExpressionBuilder.BuildValueExpression<IEntity>(
                context, entityStr, allowInputRequesters);

        string identifier = match.Groups["identifier"].Value.Trim();
        string arguments = match.Groups["arguments"].Value.Trim();

        IExpression[] argExpressions = null;
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            argExpressions = BuildFunctionArgumentExpressions(
                context, arguments, allowInputRequesters);
        }

        EntityAttribute attribute =
            entExpression.Value.GetAttribute(identifier, argExpressions);

        return attribute.GetExpression();
    }

#if DEBUG
    private static void TestMatch(Context context, string text)
    {
        bool matched = false;

        Debug.Log("- Test parsing: " + text);

        Match match = Regex.Match(text, ModParseUtility.UnaryOpStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched UnaryOpStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
        }

        match = Regex.Match(text, ModParseUtility.BinaryOpStatementRegex);

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

        match = Regex.Match(text, ModParseUtility.AccessorOpStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched AccessorOpStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
        }

        match = Regex.Match(text, ModParseUtility.BinaryOperandStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched OperandStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
            //Debug.Log("-- -- baseStatement: " + ModUtility.Debug_CapturesToString(match.Groups["baseStatement"]));
            //Debug.Log("-- -- innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));
        }

        match = Regex.Match(text, ModParseUtility.ArgumentListRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched ArgumentListRegex");
            //Debug.Log("-- -- match: " + match.Value);
            //Debug.Log("-- -- argument: " + ModUtility.Debug_CapturesToString(match.Groups["argument"]));
            //Debug.Log("-- -- otherArgs: " + ModUtility.Debug_CapturesToString(match.Groups["otherArgs"]));
        }

        match = Regex.Match(text, ModParseUtility.IdentifierStatementRegex);

        if (match.Success == true)
        {
            matched = true;
            Debug.Log("-- Matched IdentifierStatementRegex");
            //Debug.Log("-- -- match: " + match.Value);
            //Debug.Log("-- -- identifier: " + ModUtility.Debug_CapturesToString(match.Groups["argument"]));
        }

        match = Regex.Match(text, ModParseUtility.BaseStatementRegex);

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

    private static IExpression[] BuildFunctionArgumentExpressions(
        Context context, string arguments, bool allowInputRequesters = false)
    {
        List<IExpression> argExpressions = new List<IExpression>();

        Match match = Regex.Match(arguments, ModParseUtility.ArgumentListRegex);
        //
        //#if DEBUG
        //        TestMatch(context, arguments);
        //#endif

        while (match.Success == true)
        {
            string argument = match.Groups["argument"].Value.Trim();
            string otherArgs = match.Groups["otherArgs"].Value.Trim();

            //Debug.Log("- match: " + match.Value);
            //Debug.Log("- argument: " + ModParseUtility.Debug_CapturesToString(match.Groups["argument"]));
            //Debug.Log("-- unaryOpStatement: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOpStatement"]));
            //Debug.Log("-- binaryOpStatement: " + ModUtility.Debug_CapturesToString(match.Groups["binaryOpStatement"]));
            //Debug.Log("-- accessorOpStatement: " + ModUtility.Debug_CapturesToString(match.Groups["accessorOpStatement"]));
            //Debug.Log("-- functionStatement: " + ModUtility.Debug_CapturesToString(match.Groups["functionStatement"]));
            //Debug.Log("-- baseStatement: " + ModUtility.Debug_CapturesToString(match.Groups["baseStatement"]));
            //Debug.Log("-- innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));
            //Debug.Log("- otherArgs: " + ModUtility.Debug_CapturesToString(match.Groups["otherArgs"]));

            argExpressions.Add(BuildExpression(context, argument, allowInputRequesters));

            match = Regex.Match(otherArgs, ModParseUtility.ArgumentListRegex);
            
            //#if DEBUG
            //            TestMatch(context, otherArgs);
            //#endif
        }

        return argExpressions.ToArray();
    }

    private static IExpression BuildIdentifierExpression(
        Context context, Match match, bool allowInputRequesters = false)
    {
        string identifier = match.Groups["identifier"].Value.Trim();
        string arguments = match.Groups["arguments"].Value.Trim();

        IExpression[] argExpressions = null;
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            argExpressions = BuildFunctionArgumentExpressions(
                context, arguments, allowInputRequesters);
        }

        switch (identifier)
        {
            case LerpFunctionExpression.FunctionId:
                return new LerpFunctionExpression(context, argExpressions);
            case PercentFunctionExpression.FunctionId:
                return new PercentFunctionExpression(context, argExpressions);
            case SaturationFunctionExpression.FunctionId:
                return new SaturationFunctionExpression(context, argExpressions);
            case NormalizeFunctionExpression.FunctionId:
                return new NormalizeFunctionExpression(context, argExpressions);
            case RandomFunctionExpression.FunctionId:
                return new RandomFunctionExpression(context, argExpressions);
            case MaxFunctionExpression.FunctionId:
                return new MaxFunctionExpression(context, argExpressions);
            case MinFunctionExpression.FunctionId:
                return new MinFunctionExpression(context, argExpressions);
            case ClampFunctionExpression.FunctionId:
                return new ClampFunctionExpression(context, argExpressions);
        }

        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new FixedStringValueExpression(identifier);
        }

        throw new System.ArgumentException("Unrecognized function identifier: " + identifier);
    }

    private static IExpression BuildUnaryOpExpression(
        Context context, Match match, bool allowInputRequesters = false)
    {
        string unaryOp = match.Groups["unaryOp"].Value.Trim();
        string expressionStr = match.Groups["statement"].Value.Trim();

        switch (unaryOp)
        {
            case "-":
                return NegateNumberExpression.Build(
                    context, expressionStr, allowInputRequesters);
            case "!":
                return NegateBooleanValueExpression.Build(
                    context, expressionStr, allowInputRequesters);
        }

        throw new System.ArgumentException("Unrecognized unary op: " + unaryOp);
    }

    private static IExpression BuildBinaryOpExpression(
        Context context, Match match, bool allowInputRequesters = false)
    {
        string binaryOp = match.Groups["binaryOp"].Value.Trim();
        string expressionAStr = match.Groups["statement1"].Value.Trim();
        string expressionBStr = match.Groups["statement2"].Value.Trim();

        switch (binaryOp)
        {
            case "+":
                return SumExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "-":
                return SubtractExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "*":
                return MultiplyExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "/":
                return DivideExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "=":
                return ValueAssignmentExpressionBuilder.BuildValueAssignmentExpression(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "==":
                return EqualsExpressionBuilder.BuildEqualsExpression(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "!=":
                return NotEqualsExpressionBuilder.BuildNotEqualsExpression(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case ">=":
                return MoreThanOrEqualExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "<=":
                return LessThanOrEqualExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case ">":
                return MoreThanExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "<":
                return LessThanExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "&&":
                return AndExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
            case "||":
                return OrExpression.Build(
                    context, expressionAStr, expressionBStr, allowInputRequesters);
        }

        throw new System.ArgumentException("Unrecognized binary op: " + binaryOp);
    }

    private static IExpression BuildBaseExpression(
        Context context, Match match, bool allowInputRequesters = false)
    {
        string number = match.Groups["number"].Value.Trim();
        string boolean = match.Groups["boolean"].Value.Trim();
        string identifierStatement = match.Groups["identifierStatement"].Value.Trim();
        string modText = match.Groups["modText"].Value.Trim();

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
            if (context.TryGetEntity(identifierStatement, out Entity entity))
            {
                return entity.Expression;
            }

            return BuildIdentifierExpression(context, match, allowInputRequesters);
        }

        if (!string.IsNullOrWhiteSpace(modText))
        {
            return new ModTextExpression(context, match, allowInputRequesters);
        }

        throw new System.ArgumentException("Unrecognized statement: " + match.Value);
    }

    public static IEffectExpression[] BuildEffectExpressions(
        Context context, ICollection<string> expressionStrs, bool allowInputRequesters = false)
    {
        IEffectExpression[] expressions = new IEffectExpression[expressionStrs.Count];

        int i = 0;
        foreach (string expStr in expressionStrs)
        {
            expressions[i++] = ValidateEffectExpression(
                BuildExpression(context, expStr, allowInputRequesters));
        }

        return expressions;
    }

    public static IEffectExpression BuildEffectExpression(
        Context context, string expressionStr, bool allowInputRequesters = false)
    {
        return ValidateEffectExpression(
            BuildExpression(context, expressionStr, allowInputRequesters));
    }

    public static IEffectExpression ValidateEffectExpression(IExpression expression)
    {
        if (!(expression is IEffectExpression effectExpression))
        {
            throw new ArgumentException(expression + " is not a valid effect expression");
        }

        return effectExpression;
    }
}
