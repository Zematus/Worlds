﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum ConditionType
{
    None = 0x00,
    TerrainCell = 0x01,
    Group = 0x02,
    Knowledge = 0x04,
}

public abstract class Condition
{
    public const string Property_KnowledgeId = "KnowledgeId";

    public ConditionType ConditionType = ConditionType.None;

    public static Condition BuildCondition(string conditionStr)
    {
        //Debug.Log("parsing: " + conditionStr);

        Match match = Regex.Match(conditionStr, ModUtility.BinaryOpStatementRegex);

        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement1: " + ModUtility.Debug_CapturesToString(match.Groups["statement1"]));
            //Debug.Log("binaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["binaryOp"]));
            //Debug.Log("statement2: " + ModUtility.Debug_CapturesToString(match.Groups["statement2"]));

            return BuildBinaryOpCondition(match);
        }

        match = Regex.Match(conditionStr, ModUtility.UnaryOpStatementRegex);
        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            //Debug.Log("unaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOp"]));

            return BuildUnaryOpCondition(match);
        }

        match = Regex.Match(conditionStr, ModUtility.InnerStatementRegex);
        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));

            conditionStr = match.Groups["innerStatement"].Value;

            return BuildCondition(conditionStr);
        }

        match = Regex.Match(conditionStr, ModUtility.BaseStatementRegex);
        if (match.Success == true)
        {
            conditionStr = match.Groups["statement"].Value;

            return BuildBaseCondition(conditionStr);
        }

        throw new System.ArgumentException("Not a valid parseable condition: " + conditionStr);
    }

    private static Condition BuildBinaryOpCondition(Match match)
    {
        string conditionAStr = match.Groups["statement1"].Value;
        string conditionBStr = match.Groups["statement2"].Value;
        string binaryOpStr = match.Groups["binaryOp"].Value.Trim().ToUpper();

        switch (binaryOpStr)
        {
            case "[OR]":
                return new OrCondition(conditionAStr, conditionBStr);
            case "[AND]":
                return new AndCondition(conditionAStr, conditionBStr);
        }

        throw new System.ArgumentException("Unrecognized binary op: " + binaryOpStr);
    }

    private static Condition BuildUnaryOpCondition(Match match)
    {
        string conditionStr = match.Groups["statement"].Value;
        string unaryOp = match.Groups["unaryOp"].Value.Trim().ToUpper();

        switch (unaryOp)
        {
            case "[NOT]":
                return new NotCondition(conditionStr);
            case "[ANY_N_CELL]":
                return new AnyNCellCondition(conditionStr);
            case "[ANY_N_GROUP]":
                return new AnyNGroupCondition(conditionStr);
            case "[THIS_OR_ANY_N_CELL]":
                return new ThisOrAnyNCellCondition(conditionStr);
            case "[THIS_OR_ANY_N_GROUP]":
                return new ThisOrAnyNGroupCondition(conditionStr);
            case "[ALL_N_CELLS]":
                return new AllNCellsCondition(conditionStr);
            case "[ALL_N_GROUPS]":
                return new AllNGroupsCondition(conditionStr);
            case "[THIS_AND_ALL_N_CELLS]":
                return new ThisAndAllNCellsCondition(conditionStr);
            case "[THIS_AND_ALL_N_GROUPS]":
                return new ThisAndAllNGroupsCondition(conditionStr);
        }

        throw new System.ArgumentException("Unrecognized unary op: " + unaryOp);
    }

    private static Condition BuildBaseCondition(string conditionStr)
    {
        Match match = Regex.Match(conditionStr, GroupHasKnowledgeCondition.Regex);
        if (match.Success == true)
        {
            return new GroupHasKnowledgeCondition(match);
        }

        match = Regex.Match(conditionStr, GroupPopulationCondition.Regex);
        if (match.Success == true)
        {
            return new GroupPopulationCondition(match);
        }

        match = Regex.Match(conditionStr, CellHasSeaCondition.Regex);
        if (match.Success == true)
        {
            return new CellHasSeaCondition(match);
        }

        match = Regex.Match(conditionStr, CellAccessibilityCondition.Regex);
        if (match.Success == true)
        {
            return new CellAccessibilityCondition(match);
        }

        match = Regex.Match(conditionStr, CellArabilityCondition.Regex);
        if (match.Success == true)
        {
            return new CellArabilityCondition(match);
        }

        throw new System.ArgumentException("Not a recognized condition: " + conditionStr);
    }

    public static Condition[] BuildConditions(ICollection<string> conditionStrs)
    {
        Condition[] conditions = new Condition[conditionStrs.Count];

        int i = 0;
        foreach (string conditionStr in conditionStrs)
        {
            conditions[i++] = BuildCondition(conditionStr);
        }

        return conditions;
    }

    public abstract bool Evaluate(CellGroup group);
    public abstract bool Evaluate(TerrainCell cell);

    public virtual string GetPropertyValue(string propertyId)
    {
        return null;
    }
}