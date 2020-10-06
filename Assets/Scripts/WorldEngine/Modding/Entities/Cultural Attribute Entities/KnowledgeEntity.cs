using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class KnowledgeEntity : DelayedSetEntity<CulturalKnowledge>
{
    public const string LevelAttributeId = "level";

    public virtual CulturalKnowledge Knowledge
    {
        get => Setable;
        private set => Setable = value;
    }

    private ValueGetterEntityAttribute<float> _levelAttribute;

    protected override object _reference => Knowledge;

    public KnowledgeEntity(
        ValueGetterMethod<CulturalKnowledge> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public KnowledgeEntity(Context c, string id) : base(c, id)
    {
    }

    private float GetProgressLevel()
    {
        if (Knowledge != null)
        {
            return Knowledge.ProgressLevel;
        }

        return 0;
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case LevelAttributeId:
                _levelAttribute =
                    _levelAttribute ?? new ValueGetterEntityAttribute<float>(
                        LevelAttributeId, this, GetProgressLevel);
                return _levelAttribute;
        }

        throw new System.ArgumentException("Knowledge: Unable to find attribute: " + attributeId);
    }

    public override string GetDebugString()
    {
        return "knowledge:" + Knowledge.Id;
    }

    public override string GetFormattedString()
    {
        return "<i>" + Knowledge.Name + "</i>";
    }
}
