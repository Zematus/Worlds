
public class ModifiableCellCulturalActivitiesEntity : ModifiableCulturalActivitiesEntity
{
    public ModifiableCellCulturalActivitiesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableCellCulturalActivitiesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected override void AddKey(string key) => (Culture as CellCulture).AddActivityToPerform(key);

    protected override void RemoveKey(string key) => (Culture as CellCulture).AddActivityToStop(key);
}
