using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Base interface for all mod expressions
/// </summary>
public interface IExpression
{
    string ToPartiallyEvaluatedString(bool evaluate = true);
}
