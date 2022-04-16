
public class CulturalActivitiesEntity : CulturalAttributeContainerEntity, ICulturalActivitiesEntity
{
    public CulturalActivitiesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalActivitiesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_activities";

    public override string GetFormattedString() => "<i>cultural activities</i>";

    protected override EntityAttribute CreateEntryAttribute(string attributeId) => 
        new ActivityAttribute(this, attributeId);

    protected override bool ValidateKey(string attributeId) => 
        PreferenceGenerator.Generators.ContainsKey(attributeId);

    protected override bool ContainsKey(string key) => Culture.HasPreference(key);
}
