using System;

public class ConditionSetProperty : ContextProperty
{
    public IBooleanExpression[] Conditions;

    public ConditionSetProperty(Context context, Context.LoadedProperty p)
        : base(context, p)
    {
        if (p.conditions == null)
        {
            throw new ArgumentException("'conditions' list can't be empty");
        }

        Conditions = ExpressionBuilder.BuildBooleanExpressions(context, p.conditions);
    }
}
