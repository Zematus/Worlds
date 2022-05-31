using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModifiableCulturalDiscoveriesEntity : ModifiableCulturalEntryContainerEntity, ICulturalDiscoveriesEntity
{
    public ModifiableCulturalDiscoveriesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableCulturalDiscoveriesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_discoveries";

    public override string GetFormattedString() => "<i>cultural discoveries</i>";

    protected override bool ContainsKey(string key) => Culture.HasDiscovery(key);

    protected override bool ValidateKey(string attributeId) => Discovery.Discoveries.ContainsKey(attributeId);

    protected override void AddKey(string key)
    {
        if (Culture.HasDiscovery(key))
        {
            throw new System.Exception($"'{key}' is already present in group.");
        }

        (Culture as CellCulture).AddDiscoveryToFind(Discovery.Discoveries[key]);
    }

    protected override void RemoveKey(string key)
    {
        if (!Culture.HasDiscovery(key))
        {
            throw new System.Exception($"'{key}' is not present in group.");
        }

        (Culture as CellCulture).AddDiscoveryToLose(Discovery.Discoveries[key]);
    }
}
