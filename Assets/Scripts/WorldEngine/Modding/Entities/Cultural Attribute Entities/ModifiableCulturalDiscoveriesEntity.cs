using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModifiableCulturalDiscoveriesEntity : CulturalEntryModifiableContainerEntity, ICulturalDiscoveriesEntity
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

    protected override bool AddKey(string key)
    {
        throw new System.NotImplementedException();
    }

    protected override bool ContainsKey(string key) => Culture.HasDiscovery(key);

    protected override bool RemoveKey(string key)
    {
        throw new System.NotImplementedException();
    }
}
