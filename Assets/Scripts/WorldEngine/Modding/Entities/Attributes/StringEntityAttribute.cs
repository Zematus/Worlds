using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class StringEntityAttribute : EntityAttribute
{
    public abstract string GetValue();
}
