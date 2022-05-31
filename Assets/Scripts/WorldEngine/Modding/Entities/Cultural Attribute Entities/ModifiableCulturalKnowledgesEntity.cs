
public abstract class ModifiableCulturalKnowledgesEntity : ModifiableCulturalEntityAttributeContainerEntity<CulturalKnowledge>, ICulturalKnowledgesEntity
{
    public ModifiableCulturalKnowledgesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableCulturalKnowledgesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_knowledges";

    public override string GetFormattedString() => "<i>cultural knowledges</i>";

    protected override bool ContainsKey(string key) => Culture.HasKnowledge(key);

    protected override bool ValidateKey(string attributeId) => Knowledge.Knowledges.ContainsKey(attributeId);
}
