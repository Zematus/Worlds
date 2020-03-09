using System;

public abstract class ValuePropertyEntity : PropertyEntity
{
    public const string ValueId = "value";

    protected EntityAttribute _valueAttribute;

    protected ValuePropertyEntity(Context context, string id)
        : base(context, id)
    {
    }

    public static ValuePropertyEntity BuildValuePropertyEntity(
        Context context, Context.LoadedProperty p)
    {
        if (string.IsNullOrEmpty(p.value))
        {
            throw new ArgumentException("'value' can't be null or empty");
        }

        IExpression exp = ExpressionBuilder.BuildExpression(context, p.value);

        if (exp is INumericExpression)
        {
            return new NumericValuePropertyEntity(context, p.id, exp);
        }

        if (exp is IBooleanExpression)
        {
            return new BooleanValuePropertyEntity(context, p.id, exp);
        }

        if (exp is IStringExpression)
        {
            return new StringValuePropertyEntity(context, p.id, exp);
        }

        if (exp is IEntityExpression)
        {
            return new EntityValuePropertyEntity(context, p.id, exp);
        }

        throw new ArgumentException("Unhandled expression type: " + exp.GetType());
    }
}
