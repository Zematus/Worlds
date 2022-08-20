
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

    protected override EntityAttribute GetAddAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'add' attribute requires at least 1 argument");
        }

        var keyArgExp = ValidateKeyArgument(arguments[0]);

        EffectApplierMethod applierMethod = null;

        if (arguments.Length == 2)
        {
            var initialLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);

            applierMethod = () => AddKey(keyArgExp.Value, initialLevel.Value);
        }
        else
        {
            applierMethod = () => AddKey(keyArgExp.Value, 0);
        }

        return new EffectApplierEntityAttribute(AddAttributeId, this, applierMethod);
    }

    private void AddKey(string key, float initialLevel)
    {
        (Culture as CellCulture).AddKnowledgeToLearn(key, initialLevel);
        Culture.SetHolderToUpdate(warnIfUnexpected: false);
    }

    protected override void RemoveKey(string key)
    {
        (Culture as CellCulture).AddKnowledgeToLose(key);
        Culture.SetHolderToUpdate(warnIfUnexpected: false);
    }
}
