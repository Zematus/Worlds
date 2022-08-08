
public abstract class ModifiableCulturalEntityAttributeContainerEntity<T> : 
    ModifiableDelayedSetEntityAttributeContainerEntity<T, Culture>, ICulturalEntryContainerEntity
{
    public virtual Culture Culture
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Culture;

    public ModifiableCulturalEntityAttributeContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableCulturalEntityAttributeContainerEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }
}
