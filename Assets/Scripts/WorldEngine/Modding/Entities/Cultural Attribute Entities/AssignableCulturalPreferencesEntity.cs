using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AssignableCulturalPreferencesEntity : CulturalPreferencesEntity
{
    public AssignableCulturalPreferencesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public AssignableCulturalPreferencesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected override EntityAttribute CreatePreferenceAttribute(string attributeId)
    {
        return new AssignablePreferenceAttribute(this, attributeId);
    }
}
