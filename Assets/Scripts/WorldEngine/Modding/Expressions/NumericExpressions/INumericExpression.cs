using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface INumericExpression : IExpression, IModTextPart
{
    float Value { get; }
}
