
public abstract class ModifiableCulturalSkillsEntity : ModifiableCulturalAttributeContainerEntity, ICulturalSkillsEntity
{
    public ModifiableCulturalSkillsEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableCulturalSkillsEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_skills";

    public override string GetFormattedString() => "<i>cultural skills</i>";

    protected override EntityAttribute CreateEntryAttribute(string attributeId) => 
        new SkillAttribute(this, attributeId);

    protected override bool ValidateKey(string attributeId) => CulturalSkill.ValidSkillIds.Contains(attributeId);

    protected override bool ContainsKey(string key) => Culture.HasSkill(key);
}
