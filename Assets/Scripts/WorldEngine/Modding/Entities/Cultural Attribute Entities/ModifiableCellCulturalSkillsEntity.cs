
public class ModifiableCellCulturalSkillsEntity : ModifiableCulturalSkillsEntity
{
    public ModifiableCellCulturalSkillsEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableCellCulturalSkillsEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected override void AddKey(string key) => (Culture as CellCulture).AddSkillToLearn(key);

    protected override void RemoveKey(string key) => (Culture as CellCulture).AddSkillToLose(key);
}
