﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BooleanEntityAttribute : EntityAttribute
{
    public abstract bool GetValue();
}
