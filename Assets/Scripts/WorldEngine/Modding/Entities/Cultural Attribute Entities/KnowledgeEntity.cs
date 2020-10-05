﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class KnowledgeEntity : Entity
{
    public const string LevelAttributeId = "level";

    public virtual CulturalKnowledge Knowledge { get; private set; }

    private ValueGetterEntityAttribute<float> _levelAttribute;

    protected override object _reference => Knowledge;

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

    public void Set(CulturalKnowledge k) => Knowledge = k;

    public override void Set(object o)
    {
        if (o is KnowledgeEntity e)
        {
            Set(e.Knowledge);
        }
        else if (o is CulturalKnowledge c)
        {
            Set(c);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
