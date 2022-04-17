
public abstract class EntryModifiableContainerEntity<T> : EntryContainerEntity<T>
{
    public const string AddAttributeId = "add";
    public const string RemoveAttributeId = "remove";

    public EntryModifiableContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public EntryModifiableContainerEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected virtual void AddKey(string key)
    { }

    protected virtual void RemoveKey(string key)
    { }

    protected virtual EntityAttribute GetAddAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'add' attribute requires at least 1 argument");
        }

        var argumentExp = ValidateKeyArgument(arguments[0]);

        return new EffectApplierEntityAttribute(
            AddAttributeId, this, () => AddKey(argumentExp.Value));
    }

    protected virtual EntityAttribute GetRemoveAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'remove' attribute requires at least 1 argument");
        }

        var argumentExp = ValidateKeyArgument(arguments[0]);

        return new EffectApplierEntityAttribute(
            RemoveAttributeId, this, () => RemoveKey(argumentExp.Value));
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case AddAttributeId:
                return GetAddAttribute(arguments);

            case RemoveAttributeId:
                return GetRemoveAttribute(arguments);
        }

        return base.GetAttribute(attributeId, arguments);
    }
}
