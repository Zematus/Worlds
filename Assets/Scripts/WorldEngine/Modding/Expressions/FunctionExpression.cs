﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public abstract class FunctionExpression : IExpression
{
    public readonly string Id;

    protected Context _context;

    private IExpression[] _arguments;

    public FunctionExpression(Context context, string id, int minArguments, IExpression[] arguments)
    {
        _context = context;

        if ((arguments == null) || (arguments.Length < minArguments))
        {
            throw new System.ArgumentException(
                context.Id + " - " +
                id + ": number of arguments given less than " + minArguments);
        }

        Id = id;
        _arguments = arguments;
    }

    public override string ToString()
    {
        string parameters = string.Join<IExpression>(", ", _arguments);

        return Id + "(" + parameters + ")";
    }

    public string ToPartiallyEvaluatedString(bool evaluate)
    {
        string parameters =
            string.Join(", ", _arguments.Select(e => e.ToPartiallyEvaluatedString(evaluate)));

        return Id + "(" + parameters + ")";
    }
}
