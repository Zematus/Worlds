using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface INumericExpression : IExpression
{
    float Value { get; }
}
