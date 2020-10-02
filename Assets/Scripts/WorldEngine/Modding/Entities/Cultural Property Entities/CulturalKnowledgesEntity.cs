using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalKnowledgesEntity : Entity
{
    public Culture Culture;

    protected override object _reference => Culture;

    public class KnowledgeAttribute : ValueEntityAttribute<float>
    {
        private CulturalKnowledgesEntity _knowledgesEntity;
        private string _knowledgeId;

        public KnowledgeAttribute(
            CulturalKnowledgesEntity knowledgesEntity,
            string knowledgeId,
            IExpression[] arguments)
            : base(knowledgeId, knowledgesEntity, arguments)
        {
            _knowledgesEntity = knowledgesEntity;
            _knowledgeId = knowledgeId;
        }

        public override float Value
        {
            get => GetValue();
        }

        private float GetValue()
        {
            CulturalKnowledge knowledge =
                _knowledgesEntity.Culture.GetKnowledge(_knowledgeId);

            if (knowledge == null)
            {
                return 0;
            }

            return knowledge.ProgressLevel;
        }
    }

    public CulturalPreferencesEntity(Context c, string id) : base(c, id)
    {
    }

    protected virtual EntityAttribute CreatePreferenceAttribute(
        string attributeId, IExpression[] arguments = null)
    {
        return new PreferenceAttribute(this, attributeId, arguments);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        if (!PreferenceGenerator.Generators.ContainsKey(attributeId))
        {
            throw new System.ArgumentException(
                "Unrecognized cultural preference in entity attribute: " + attributeId);
        }

        return CreatePreferenceAttribute(attributeId, arguments);
    }

    public override string GetDebugString()
    {
        return "cultural_preferences";
    }

    public override string GetFormattedString()
    {
        return "<i>cultural preferences</i>";
    }

    public void Set(Culture c) => Culture = c;

    public override void Set(object o)
    {
        if (o is CulturalPreferencesEntity e)
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
