using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IBooleanExpression : IExpression, IModTextPart
{
    bool Value { get; }
}
