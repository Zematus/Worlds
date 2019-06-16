using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ModUtility
{
    public const string FirstAndLastSingleQuoteRegex = @"(?:^\s*\'\s*)|(?:\s*\'\s*$)";
    public const string SeparatorSingleQuoteRegex = @"\s*(?:(?:\'\s*,\s*\'))\s*";

    public const string IdentifierRegexPart = @"[a-zA-Z_][a-zA-Z0-9_]*";
    public const string NumberRegexPart = @"\d+(?:\.\d+)?";

    public const string OperandRegexPart = @"\[\w+\]\s*";
    public const string BaseStatementRegexPart = @"(?<operand>" + OperandRegexPart + @")?(?<statement>[^\(\)]+)";
    public const string BaseStatementRegex = @"^\s*" + BaseStatementRegexPart + @"\s*$";
    public const string InnerStatementRegexPart = @"(?:(?<open>(?<operand>" + OperandRegexPart + @")?\()[^\(\)]*)+(?:(?<statement-open>\))[^\(\)]*)+(?(open)(?!))";
    public const string InnerStatementRegex = @"^\s*" + InnerStatementRegexPart + @"\s*$";
    public const string MixedStatementRegexPart = @"(?:" + InnerStatementRegexPart + "|" + BaseStatementRegexPart + ")";
    public const string MixedStatementRegex = @"^\s*" + MixedStatementRegexPart + @"\s*$";

    public const string NotStatementRegex = @"^\s*\[NOT\]\s*" + MixedStatementRegexPart + @"\s*$";
    public const string OrStatementRegex = @"^\s*" + MixedStatementRegexPart + @"\s*\[OR\]\s*(?<statement2>.+)\s*$";

    public const string InvStatementRegex = @"^\s*\[INV\]\s*" + MixedStatementRegexPart + @"\s*$";
    public const string SqStatementRegex = @"^\s*\[SQ\]\s*" + MixedStatementRegexPart + @"\s*$";
}
