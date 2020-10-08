using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AssignableCulturalPreferencesEntity : CulturalPreferencesEntity
{
    public AssignableCulturalPreferencesEntity(Context c, string id) : base(c, id)
    {
    }

    public AssignableCulturalPreferencesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected override EntityAttribute CreatePreferenceAttribute(string attributeId)
    {
        return new AssignablePreferenceAttribute(this, attributeId);
    }
}
