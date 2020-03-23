using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IValueExpression<T> : IExpression, IModTextPart
{
    T Value { get; }
}
