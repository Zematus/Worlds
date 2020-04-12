using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BaseValueEntity : Entity
{
    public BaseValueEntity(string id) : base(id)
    {
    }

    public abstract IBaseValueExpression BaseValueExpression
    {
        get;
    }
}
