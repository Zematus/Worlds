using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalDiscoveriesEntity : CulturalEntryContainerEntity, ICulturalDiscoveriesEntity
{
    public CulturalDiscoveriesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalDiscoveriesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_discoveries";

    public override string GetFormattedString() => "<i>cultural discoveries</i>";

    protected override bool ContainsKey(string key) => Culture.HasDiscovery(key);

    protected override bool ValidateKey(string attributeId) => Discovery.Discoveries.ContainsKey(attributeId);
}
