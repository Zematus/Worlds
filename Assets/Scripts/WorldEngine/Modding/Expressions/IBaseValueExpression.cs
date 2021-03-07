﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IBaseValueExpression : IExpression, IFormattedStringGenerator
{
    object ValueObject { get; }
}
