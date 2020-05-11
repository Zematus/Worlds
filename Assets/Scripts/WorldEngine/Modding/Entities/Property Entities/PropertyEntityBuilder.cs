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
        if (string.IsNullOrEmpty(p.type))
        {
            throw new ArgumentException("'type' can't be null or empty");
        }

        IReseteableEntity entity;

        switch (p.type)
        {
            case ConditionSetType:
                entity = new ConditionSetPropertyEntity(context, p);
                break;

            case RandomRangeType:
                entity = new RandomRangePropertyEntity(context, p);
                break;

            case ValueType:
                entity = BuildValuePropertyEntity(context, p);
                break;

            default:
                throw new ArgumentException("Property type not recognized: " + p.type);
        }

        return entity;
    }

    public static IReseteableEntity BuildValuePropertyEntity(
        Context context, Context.LoadedContext.LoadedProperty p)
    {
        if (string.IsNullOrEmpty(p.value))
        {
            throw new ArgumentException("'value' can't be null or empty");
        }

        IBaseValueExpression exp = ValueExpressionBuilder.BuildValueExpression(context, p.value);

        if (exp is IValueExpression<float>)
        {
            return new ValuePropertyEntity<float>(context, p.id, exp);
        }

        if (exp is IValueExpression<bool>)
        {
            return new ValuePropertyEntity<bool>(context, p.id, exp);
        }

        if (exp is IValueExpression<string>)
        {
            return new ValuePropertyEntity<string>(context, p.id, exp);
        }

        if (exp is IValueExpression<Entity>)
        {
            return new ValuePropertyEntity<Entity>(context, p.id, exp);
        }

        throw new ArgumentException("Unhandled expression type: " + exp.GetType());
    }
}
