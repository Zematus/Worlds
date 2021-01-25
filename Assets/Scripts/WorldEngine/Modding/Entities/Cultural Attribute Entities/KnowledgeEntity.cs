﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class KnowledgeEntity : DelayedSetValueEntity<CulturalKnowledge,float>
{
    public virtual CulturalKnowledge Knowledge
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Knowledge;

    public KnowledgeEntity(
        ValueGetterMethod<CulturalKnowledge> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public KnowledgeEntity(Context c, string id) : base(c, id)
    {
    }

    public override float GetValue()
    {
        if (Knowledge != null)
        {
            return Knowledge.ScaledValue;
        }

        return 0;
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        return base.GetAttribute(attributeId, arguments);
    }

    public override string GetFormattedString()
    {
        return "<i>" + Knowledge.Name + "</i>";
    }

    public override void Set(float v)
    {
        throw new System.InvalidOperationException("Knowledge attribute is read-only");
    }
}