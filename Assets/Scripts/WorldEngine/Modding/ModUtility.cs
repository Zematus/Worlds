using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public static class ModUtility
{
    public const string IdentifierRegexPart = @"[a-zA-Z_][a-zA-Z0-9_]*";
    public const string NumberRegexPart = @"-?\d+(?:\.\d+)?";
    public const string BooleanRegexPart = @"(true|false)";

    public const string OperatorRegexPart = @"\s*(?<opStr>[\!\+\-\*\/])\s*";
    public const string AccessorRegexPart = @"\.";

    public const string BaseStatementRegexPart =
        @"[^\s\(\)]+";

    public const string InnerStatementRegexPart =
        @"(?:(?:" + 
            @"(?<open>\()" + 
        @"|" + 
            @"(?<innerStatement-open>\))" + 
        @")[^\(\)]*?)+" +
        @"(?(open)(?!))";

    public const string OperandStatementRegexPart =
        @"(?:" +
            BaseStatementRegexPart +
        @")|(?:" +
            InnerStatementRegexPart +
        @")";

    public const string ArgumentsRegexPart =
        @"(?:(?:" +
            @"(?<open>\()" +
        @"|" +
            @"(?<arguments-open>\))" +
        @")[^\(\)]*?)+" +
        @"(?(open)(?!))";

    public const string ArgumentListRegex =
        @"^\s*" +
        @"(?<argument>" + ArgumentRegexPart + @")\s*" +
        @"(?:," +
            @"(?<otherArgs>" +
                @"(?:\s*" + ArgumentRegexPart + @"\s*)" +
                @"(?:,\s*" + ArgumentRegexPart + @"\s*)*" +
            @")" +
        @")?" +
        @"$";

    public const string ArgumentRegexPart =
        @"(?:" +
            FunctionStatementRegexPart +
        @")|(?:" +
            UnaryOpStatementRegexPart +
        @")|(?:" +
            BinaryOpStatementRegexPart +
        @")|(?:" +
            AccessorOpStatementRegexPart +
        @")|(?:" +
            OperandStatementRegexPart +
        @")";

    public const string FunctionStatementRegexPart =
        @"(?<funcName>" + IdentifierRegexPart + @")\s*" +
        ArgumentsRegexPart;

    public const string FunctionStatementRegex =
        @"^\s*" + FunctionStatementRegexPart + @"\s*$";

    public const string UnaryOpStatementRegexPart =
        @"(?<unaryOp>" + OperatorRegexPart + @")" +
        @"(?<statement>" + OperandStatementRegexPart + @")";

    public const string UnaryOpStatementRegex =
        @"^\s*" + UnaryOpStatementRegexPart + @"\s*$";

    public const string BinaryOpStatementRegexPart =
        @"(?<statement1>" + OperandStatementRegexPart + @")\s*" +
        @"(?<binaryOp>" + OperatorRegexPart + @")\s*" +
        @"(?<statement2>" +
            @"(?:" + OperandStatementRegexPart + @")" +
            @"(?:\s*" +
                @"(?:" + OperatorRegexPart + @")\s*" +
                @"(?:" + OperandStatementRegexPart + @")" +
            @")*" +
        @")";

    public const string BinaryOpStatementRegex =
        @"^\s*" + BinaryOpStatementRegexPart + @"\s*$";

    public const string AccessorOpStatementRegexPart =
        @"(?<statement>" +
            @"(?:" + OperandStatementRegexPart + @")" +
            @"(?:" +
                AccessorRegexPart +
                IdentifierRegexPart +
            @")*" +
        @")" +
        AccessorRegexPart +
        @"(?<attribute>" + IdentifierRegexPart + @")";

    public const string AccessorOpStatementRegex =
        @"^\s*" + AccessorOpStatementRegexPart + @"\s*$";
    
    public const string BaseStatementRegex = @"^\s*(?<statement>" + BaseStatementRegexPart + @")\s*$";
    public const string OperandStatementRegex = @"^\s*(?<statement>" + OperandStatementRegexPart + @")\s*$";
    public const string InnerStatementRegex = @"^\s*(?<statement>" + InnerStatementRegexPart + @")\s*$";
    public const string IdentifierRegex = @"^" + IdentifierRegexPart + @"\s*$";

#if DEBUG
    public static string Debug_CapturesToString(Group group)
    {
        Capture[] captures = new Capture[group.Captures.Count];
        group.Captures.CopyTo(captures, 0);

        return string.Join("; ", captures.Select(c => c.Value));
    }
#endif
}
