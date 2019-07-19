using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ModUtility
{
    public const string FirstAndLastSingleQuoteRegex = @"(?:^\s*\'\s*)|(?:\s*\'\s*$)";
    public const string SeparatorSingleQuoteRegex = @"\s*(?:(?:\'\s*,\s*\'))\s*";

    public const string IdentifierRegexPart = @"[a-zA-Z_][a-zA-Z0-9_]*";
    public const string AttributeRegexPart = @"[a-zA-Z_][a-zA-Z0-9_:]*";
    public const string NumberRegexPart = @"-?\d+(?:\.\d+)?";

    public const string OperatorRegexPart = @"\[\w+\]";
    
    public const string BaseStatementRegexPart = 
        @"[^\[\]\(\)]+";
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

    public const string UnaryOpStatementRegex =
        @"^\s*" +
        @"(?<unaryOp>" + OperatorRegexPart + @")\s*" +
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
    
    public const string BaseStatementRegex = @"^\s*(?<statement>" + BaseStatementRegexPart + @")\s*$";
    public const string OperandStatementRegex = @"^\s*(?<statement>" + OperandStatementRegexPart + @")\s*$";
    public const string InnerStatementRegex = @"^\s*(?<statement>" + InnerStatementRegexPart + @")\s*$";

#if DEBUG
    public static string Debug_CapturesToString(Group group)
    {
        string cString = "";

        foreach (Capture c in group.Captures)
        {
            cString += c.Value + "; ";
        }

        return cString;
    }
#endif
}
