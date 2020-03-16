using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IStringExpression : IExpression, IModTextPart
{
    string Value { get; }
}
