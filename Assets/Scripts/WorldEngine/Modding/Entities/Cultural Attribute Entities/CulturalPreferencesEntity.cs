using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalPreferencesEntity : CulturalAttributeContainerEntity
{
    public CulturalPreferencesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalPreferencesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_preferences";

    public override string GetFormattedString() => "<i>cultural preferences</i>";

    protected override EntityAttribute CreateEntryAttribute(string attributeId) => 
        new PreferenceAttribute(this, attributeId);

    protected override bool ValidateKey(string attributeId) => 
        PreferenceGenerator.Generators.ContainsKey(attributeId);

    protected override bool ContainsKey(string key) => Culture.HasPreference(key);
}
