
public class CulturalSkillsEntity : CulturalAttributeContainerEntity, ICulturalSkillsEntity
{
    public CulturalSkillsEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalSkillsEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_skills";

    public override string GetFormattedString() => "<i>cultural skills</i>";

    protected override EntityAttribute CreateEntryAttribute(string attributeId) => 
        new SkillAttribute(this, attributeId);

    // TODO: These should be validated somehow
    protected override bool ValidateKey(string attributeId) => true;

    protected override bool ContainsKey(string key) => Culture.HasSkill(key);
}
