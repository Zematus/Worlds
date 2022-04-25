
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

        EffectApplierMethod applierMethod = null;

        if (arguments.Length >= 3)
        {
            var limitLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
            var initialLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);

            applierMethod = () => AddKey(keyArgExp.Value, (int)initialLevel.Value, (int)limitLevel.Value);
        }
        else if (arguments.Length == 2)
        {
            var initialLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);

            applierMethod = () => AddKey(keyArgExp.Value, (int)initialLevel.Value, -1);
        }
        else
        {
            applierMethod = () => AddKey(keyArgExp.Value, 0, -1);
        }

        return new EffectApplierEntityAttribute(AddAttributeId, this, applierMethod);
    }

    protected override void RemoveKey(string key) => (Culture as CellCulture).AddKnowledgeToLose(key);

    private void AddKey(string key, float initialLevel, float limitLevel)
    {
        int initialLevelInt = (int)(initialLevel * MathUtility.FloatToIntScalingFactor);
        int limitLevelInt = (int)(limitLevel * MathUtility.FloatToIntScalingFactor);

        (Culture as CellCulture).TryAddKnowledgeToLearn(key, initialLevelInt, limitLevelInt);
    }
}
