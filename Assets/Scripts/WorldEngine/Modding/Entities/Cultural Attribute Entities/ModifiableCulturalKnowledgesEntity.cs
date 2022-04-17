
public class ModifiableCulturalKnowledgesEntity : CulturalEntityAttributeModifiableContainerEntity<CulturalKnowledge>, ICulturalKnowledgesEntity
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

    protected override DelayedSetEntity<CulturalKnowledge> CreateEntity(string attributeId) =>
        new KnowledgeEntity(
            () => Culture.GetKnowledge(attributeId),
            Context,
            BuildAttributeId(attributeId),
            this);

    protected override bool ValidateKey(string attributeId) => Knowledge.Knowledges.ContainsKey(attributeId);

    protected override EntityAttribute GetAddAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'add' attribute requires at least 1 argument");
        }

        var keyArgExp = ValidateKeyArgument(arguments[0]);

        if (arguments.Length >= 2)
        {
            var initialLevelExp = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);

            return new EffectApplierEntityAttribute(
                AddAttributeId, this, () => AddKey(keyArgExp.Value, initialLevelExp.Value));
        }
        else
        {
            return new EffectApplierEntityAttribute(
                AddAttributeId, this, () => AddKey(keyArgExp.Value, 1));
        }
    }

    protected override void RemoveKey(string key)
    {
        throw new System.NotImplementedException();
    }

    private void AddKey(string key, float initialLevel)
    {
        throw new System.NotImplementedException();
    }
}
