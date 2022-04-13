
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

    protected abstract bool ContainsKey(string key);

    private EntityAttribute GetContainsAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'contains' attribute requires at least 1 argument");
        }

        var argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);

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
