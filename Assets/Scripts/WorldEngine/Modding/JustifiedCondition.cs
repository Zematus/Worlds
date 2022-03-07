using UnityEngine;
using System.Collections.Generic;

public class JustifiedCondition
{
    protected Context _context = null;

    public IValueExpression<bool> Condition;
    public ModText Info;

    public JustifiedCondition(Context context)
    {
        _context = context;
    }
}
