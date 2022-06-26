
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
            () => (Culture as CellCulture).GetLearnedKnowledgeOrToLearn(attributeId, addNonPresent: true),
            Context,
            BuildAttributeId(attributeId),
            this);

    protected override EntityAttribute GetAddAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 2)
        {
            throw new System.ArgumentException($"'add' attribute requires at least 2 arguments");
        }

        var keyArgExp = ValidateKeyArgument(arguments[0]);

        EffectApplierMethod applierMethod = null;

        if (arguments.Length >= 3)
        {
            var limitLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
            var initialLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);

            applierMethod = () => AddKey(keyArgExp.Value, (int)initialLevel.Value, (int)limitLevel.Value);
        }
        else
        {
            var limitLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);

            applierMethod = () => AddKey(keyArgExp.Value, 0, (int)limitLevel.Value);
        }

        return new EffectApplierEntityAttribute(AddAttributeId, this, applierMethod);
    }

    protected override EntityAttribute GetRemoveAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 2)
        {
            throw new System.ArgumentException($"'remove' attribute requires at least 2 arguments");
        }

        var keyArgExp = ValidateKeyArgument(arguments[0]);

        EffectApplierMethod applierMethod = null;

        var limitLevel = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);

        applierMethod = () => RemoveKey(keyArgExp.Value, (int)limitLevel.Value);

        return new EffectApplierEntityAttribute(AddAttributeId, this, applierMethod);
    }

    private void AddKey(string key, float initialLevel, float limitLevel)
    {
        int initialLevelInt = (int)(initialLevel * MathUtility.FloatToIntScalingFactor);
        int limitLevelInt = (int)(limitLevel * MathUtility.FloatToIntScalingFactor);

        (Culture as CellCulture).AddKnowledgeToLearn(key, initialLevelInt, limitLevelInt);
        Culture.SetHolderToUpdate(warnIfUnexpected: false);
    }

    private void RemoveKey(string key, float limitLevel)
    {
        int limitLevelInt = (int)(limitLevel * MathUtility.FloatToIntScalingFactor);

        (Culture as CellCulture).AddKnowledgeToLose(key, limitLevelInt, true);
        Culture.SetHolderToUpdate(warnIfUnexpected: false);
    }
}
