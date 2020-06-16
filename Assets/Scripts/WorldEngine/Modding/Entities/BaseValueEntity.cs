using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BaseValueEntity : Entity
{
    public BaseValueEntity(Context c, string id) : base(c, id)
    {
    }

    public abstract IBaseValueExpression BaseValueExpression
    {
        get;
    }
}
