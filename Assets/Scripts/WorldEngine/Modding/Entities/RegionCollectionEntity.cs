using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RegionCollectionEntity : EntityCollectionEntity<Region>
{
    public RegionCollectionEntity(
        CollectionGetterMethod<Region> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public override string GetDebugString()
    {
        return "region_collection";
    }

    protected override DelayedSetEntity<Region> ConstructEntity(
        ValueGetterMethod<Region> getterMethod, Context c, string id)
        => new RegionEntity(getterMethod, c, id);

    protected override DelayedSetEntity<Region> ConstructEntity(
        TryRequestGenMethod<Region> tryRequestGenMethod, Context c, string id)
        => new RegionEntity(tryRequestGenMethod, c, id);

    protected override DelayedSetEntityInputRequest<Region> ConstructInputRequest(
        ICollection<Region> collection, ModText text)
        => new RegionSelectionRequest(collection, text);
}
