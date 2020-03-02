using System;

public class RandomRangeProperty : ContextProperty
{
    public INumericExpression Min;
    public INumericExpression Max;

    public RandomRangeProperty(Context context, Context.LoadedProperty p)
        : base(context, p)
    {
        if (string.IsNullOrEmpty(p.min))
        {
            throw new ArgumentException("'min' can't be null or empty");
        }

        if (string.IsNullOrEmpty(p.max))
        {
            throw new ArgumentException("'max' can't be null or empty");
        }

        Min = ExpressionBuilder.ValidateNumericExpression(
            ExpressionBuilder.BuildExpression(context, p.min));
        Max = ExpressionBuilder.ValidateNumericExpression(
            ExpressionBuilder.BuildExpression(context, p.max));
    }
}
