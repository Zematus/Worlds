using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IAssignableValueExpression<T> : IValueExpression<T>
{
    new T Value { get; set; }
}
