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

    public const string ArgumentListRegexPart =
        @"^\s*" +
        @"(?<argument>" + OperandStatementRegexPart + @")\s*" +
        @"(?:," +
            @"(?<otherArgs>" +
                @"(?:\s*" + OperandStatementRegexPart + @"\s*)" +
                @"(?:,\s*" + OperandStatementRegexPart + @"\s*)*" +
            @")" +
        @")?" +
        @"$";

    public const string FunctionStatementRegex =
        @"^\s*" +
        @"(?<funcName>" + IdentifierRegexPart + @")\s*" +
        ArgumentsRegexPart + @"\s*" +
        @"$";

    public const string UnaryOpStatementRegex =
        @"^\s*" +
        @"(?<unaryOp>" + OperatorRegexPart + @")" +
        @"(?<statement>" + OperandStatementRegexPart + @")\s*" +
        @"$";

    public const string BinaryOpStatementRegex =
        @"^\s*" +
        @"(?<statement1>" + OperandStatementRegexPart + @")\s*" +
        @"(?<binaryOp>" + OperatorRegexPart + @")\s*" +
        @"(?<statement2>" +
            @"(?:" + OperandStatementRegexPart + @")" +
            @"(?:\s*" +
                @"(?:" + OperatorRegexPart + @")\s*" +
                @"(?:" + OperandStatementRegexPart + @")" +
            @")*" +
        @")\s*" +
        @"$";

    public const string AccessorOpStatementRegex =
        @"^\s*" +
        @"(?<statement>" +
            @"(?:" + OperandStatementRegexPart + @")" +
            @"(?:" +
                AccessorRegexPart +
                IdentifierRegexPart +
            @")*" +
        @")" +
        AccessorRegexPart +
        @"(?<attribute>" + IdentifierRegexPart + @")\s*" +
        @"$";
    
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
