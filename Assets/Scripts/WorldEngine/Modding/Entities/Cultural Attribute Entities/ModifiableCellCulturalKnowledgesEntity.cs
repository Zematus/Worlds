
public class ModifiableCellCulturalKnowledgesEntity : ModifiableCulturalKnowledgesEntity
{
    public ModifiableCellCulturalKnowledgesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableCellCulturalKnowledgesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected override DelayedSetEntity<CulturalKnowledge> CreateEntity(string attributeId) =>
        new CellKnowledgeEntity(
            () => (Culture as CellCulture).GetLearnedKnowledgeOrToLearn(attributeId),
            Context,
            BuildAttributeId(attributeId),
            this);
}
