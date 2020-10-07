using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalPreferencesEntity : DelayedSetEntity<Culture>
{
    public virtual Culture Culture
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Culture;

    public CulturalPreferencesEntity(Context c, string id) : base(c, id)
    {
    }

    public CulturalPreferencesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected virtual EntityAttribute CreatePreferenceAttribute(string attributeId)
    {
        return new PreferenceAttribute(this, attributeId);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        if (!PreferenceGenerator.Generators.ContainsKey(attributeId))
        {
            throw new System.ArgumentException(
                "Unrecognized cultural preference in entity attribute: " + attributeId);
        }

        return CreatePreferenceAttribute(attributeId);
    }

    public override string GetDebugString()
    {
        return "cultural_preferences";
    }

    public override string GetFormattedString()
    {
        return "<i>cultural preferences</i>";
    }
}
