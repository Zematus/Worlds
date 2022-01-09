using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RegionCollectionEntity : EntityCollectionEntity<Region>
{
    public RegionCollectionEntity(
        CollectionGetterMethod<Region> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString()
    {
        return "region_collection";
    }

    protected override DelayedSetEntity<Region> ConstructEntity(
        ValueGetterMethod<Region> getterMethod, Context c, string id, IEntity parent)
        => new RegionEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntity<Region> ConstructEntity(
        TryRequestGenMethod<Region> tryRequestGenMethod, Context c, string id, IEntity parent)
        => new RegionEntity(tryRequestGenMethod, c, id, parent);

    protected override DelayedSetEntityInputRequest<Region> ConstructInputRequest(
        ICollection<Region> collection, ModText text)
        => new RegionSelectionRequest(collection, text);
}
