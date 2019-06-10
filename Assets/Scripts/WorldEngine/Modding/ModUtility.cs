using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ModUtility
{
    public const string FirstAndLastSingleQuoteRegex = @"(?:^\s*\'\s*)|(?:\s*\'\s*$)";
    public const string SeparatorSingleQuoteRegex = @"\s*(?:(?:\'\s*,\s*\'))\s*";

    public const string InnerStatementRegex = @"(?:^\s*\'\s*)|(?:\s*\'\s*$)";
}
