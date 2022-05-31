
public abstract class ModifiableAttributeContainerEntity<T> : ModifiableEntryContainerEntity<T>
{
    public ModifiableAttributeContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableAttributeContainerEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected abstract EntityAttribute CreateEntryAttribute(string attributeId);

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        if (ValidateKey(attributeId))
        {
            return CreateEntryAttribute(attributeId);
        }

        return base.GetAttribute(attributeId, arguments);
    }
}
