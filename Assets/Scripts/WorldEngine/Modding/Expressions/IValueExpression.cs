using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IValueExpression<T> : IBaseValueExpression
{
    T Value { get; }
}
