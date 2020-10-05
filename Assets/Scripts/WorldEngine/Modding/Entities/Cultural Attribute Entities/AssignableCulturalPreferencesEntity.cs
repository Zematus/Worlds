using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AssignableCulturalPreferencesEntity : CulturalPreferencesEntity
{
    public AssignableCulturalPreferencesEntity(Context c, string id) : base(c, id)
    {
    }

    protected override EntityAttribute CreatePreferenceAttribute(string attributeId)
    {
        return new AssignablePreferenceAttribute(this, attributeId);
    }
}
