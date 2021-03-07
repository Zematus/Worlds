using System;

public static class PropertyEntityBuilder
{
    public const string ConditionSetType = "condition_set";
    public const string RandomRangeType = "random_range";
    public const string ValueType = "value";

    public static IReseteableEntity BuildPropertyEntity(
        Context context,
        Context.LoadedContext.LoadedProperty p)
    {
        if (string.IsNullOrEmpty(p.value))
        {
            throw new ArgumentException("'value' can't be null or empty");
        }

        IBaseValueExpression exp =
            ValueExpressionBuilder.BuildValueExpression(context, p.value, true);

        if (exp is IValueExpression<float>)
        {
            return new PropertyEntity<float>(context, p.id, exp);
        }

        if (exp is IValueExpression<bool>)
        {
            return new PropertyEntity<bool>(context, p.id, exp);
        }

        if (exp is IValueExpression<string>)
        {
            return new PropertyEntity<string>(context, p.id, exp);
        }

        if (exp is IValueExpression<IEntity>)
        {
            return new PropertyEntity<IEntity>(context, p.id, exp);
        }

        if (exp is IValueExpression<ModText>)
        {
            return new PropertyEntity<ModText>(context, p.id, exp);
        }

        throw new ArgumentException("Unhandled expression type: " + exp.GetType());
    }
}
