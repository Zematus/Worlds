
public abstract class EntryContainerEntity<T> : DelayedSetEntity<T>
{
    public const string ContainsAttributeId = "contains";

    public EntryContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public EntryContainerEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected virtual bool ValidateKey(string attributeId)
    {
        return true;
    }

    protected abstract bool ContainsKey(string key);

    protected IValueExpression<string> ValidateKeyArgument(IExpression argument)
    {
        var argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(argument);

        if (argumentExp is FixedValueExpression<string>)
        {
            if (!ValidateKey(argumentExp.Value))
            {
                throw new System.ArgumentException(
                    $"Not a valid entry: { argumentExp.Value }");
            }
        }

        return argumentExp;
    }

    private EntityAttribute GetContainsAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'contains' attribute requires at least 1 argument");
        }

        var argumentExp = ValidateKeyArgument(arguments[0]);

        return new ValueGetterEntityAttribute<bool>(
            ContainsAttributeId, this, () => ContainsKey(argumentExp.Value));
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ContainsAttributeId:
                return GetContainsAttribute(arguments);
        }

        throw new System.ArgumentException(
            $"Unable to find attribute: { attributeId }");
    }
}
