using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalKnowledgesEntity : DelayedSetEntity<Culture>
{
    public virtual Culture Culture
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Culture;

    private readonly Dictionary<string, KnowledgeEntity> _knowledgeEntities =
        new Dictionary<string, KnowledgeEntity>();

    public CulturalKnowledgesEntity(Context c, string id) : base(c, id)
    {
    }

    public CulturalKnowledgesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected virtual EntityAttribute CreateKnowledgeAttribute(string attributeId)
    {
        if (!_knowledgeEntities.TryGetValue(attributeId, out KnowledgeEntity entity))
        {
            entity = new KnowledgeEntity(
                () => Culture.GetKnowledge(attributeId),
                Context,
                BuildAttributeId(attributeId));

            _knowledgeEntities[attributeId] = entity;
        }

        return entity.GetThisEntityAttribute(this);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        if (!Knowledge.Knowledges.ContainsKey(attributeId))
        {
            throw new System.ArgumentException(
                "Unrecognized cultural knowledge in entity attribute: " + attributeId);
        }

        return CreateKnowledgeAttribute(attributeId);
    }

    public override string GetDebugString()
    {
        return "cultural_knowledges";
    }

    public override string GetFormattedString()
    {
        return "<i>cultural knowledges</i>";
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (KnowledgeEntity entity in _knowledgeEntities.Values)
        {
            entity.Reset();
        }
    }
}
