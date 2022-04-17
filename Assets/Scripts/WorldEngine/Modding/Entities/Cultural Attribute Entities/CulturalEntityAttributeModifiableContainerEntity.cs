
public abstract class CulturalEntityAttributeModifiableContainerEntity<T> : 
    DelayedSetEntityAttributeModifiableContainerEntity<T, Culture>, ICulturalEntryContainerEntity
{
    public virtual Culture Culture
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Culture;

    public CulturalEntityAttributeModifiableContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalEntityAttributeModifiableContainerEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }
}
