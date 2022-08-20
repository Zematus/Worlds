using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class KnowledgeEntity : DelayedSetValueEntity<CulturalKnowledge,float>
{
    public const string LimitAttributeId = "limit";

    public virtual CulturalKnowledge Knowledge
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Knowledge;

    public override float Value
    {
        get
        {
            if (Knowledge != null)
            {
                return Knowledge.Value;
            }

            return 0;
        }
    }

    public KnowledgeEntity(
        ValueGetterMethod<CulturalKnowledge> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public KnowledgeEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public override string GetFormattedString()
    {
        return $"<i>{Knowledge.Name}</i>";
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case LimitAttributeId:
                throw new System.InvalidOperationException("Knowledge limit can only be set for cell groups");
        }

        return base.GetAttribute(attributeId, arguments);
    }

    public override void Set(float v)
    {
        throw new System.InvalidOperationException("Knowledge value is read-only");
    }
}
