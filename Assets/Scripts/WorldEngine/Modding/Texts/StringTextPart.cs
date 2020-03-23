using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class StringTextPart : IModTextPart
{
    string _str;

    public StringTextPart(string str)
    {
        _str = str;
    }

    public string GetFormattedString() => _str;
}
