
public abstract class CulturalEntityAttributeContainerEntity<T> : DelayedSetEntityAttributeContainerEntity<T, Culture>, ICulturalEntryContainerEntity
{
    public virtual Culture Culture
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Culture;

    public CulturalEntityAttributeContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalEntityAttributeContainerEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }
}
