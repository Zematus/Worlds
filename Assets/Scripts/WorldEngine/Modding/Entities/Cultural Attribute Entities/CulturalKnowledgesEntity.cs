using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalKnowledgesEntity : Entity
{
    public Culture Culture;

    protected override object _reference => Culture;

    private Dictionary<string, DelayedSetKnowledgeEntity> _knowledgeEntities =
        new Dictionary<string, DelayedSetKnowledgeEntity>();

    private bool _alreadyReset = false;

    public CulturalKnowledgesEntity(Context c, string id) : base(c, id)
    {
    }

    protected virtual EntityAttribute CreateKnowledgeAttribute(string attributeId)
    {
        if (!_knowledgeEntities.TryGetValue(attributeId, out DelayedSetKnowledgeEntity entity))
        {
            entity = new DelayedSetKnowledgeEntity(
                Culture,
                attributeId,
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

    public void Set(Culture c)
    {
        Culture = c;

        ResetInternal();

        _alreadyReset = false;
    }

    protected void ResetInternal()
    {
        if (_alreadyReset)
        {
            return;
        }

        foreach (DelayedSetKnowledgeEntity entity in _knowledgeEntities.Values)
        {
            entity.Reset();
        }

        _alreadyReset = true;
    }

    public override void Set(object o)
    {
        if (o is CulturalKnowledgesEntity e)
        {
            Set(e.Culture);
        }
        else if (o is Culture c)
        {
            Set(c);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
