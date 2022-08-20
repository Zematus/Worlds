using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellCollectionEntity : EntityCollectionEntity<TerrainCell>
{
    public CellCollectionEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
    }
    public CellCollectionEntity(
        CollectionGetterMethod<TerrainCell> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString()
    {
        return "cell_collection";
    }

    protected override DelayedSetEntity<TerrainCell> ConstructEntity(
        ValueGetterMethod<TerrainCell> getterMethod, Context c, string id, IEntity parent)
        => new CellEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntity<TerrainCell> ConstructEntity(
        TryRequestGenMethod<TerrainCell> tryRequestGenMethod, Context c, string id, IEntity parent)
        => new CellEntity(tryRequestGenMethod, c, id, parent);

    protected override DelayedSetEntity<TerrainCell> ConstructEntity(
        Context c, string id, IEntity parent)
        => new CellEntity(c, id, parent);

    protected override EntityCollectionEntity<TerrainCell> ConstructEntitySubsetEntity(
        Context c, string id, IEntity parent)
        => new CellCollectionEntity(c, id, parent);

    protected override EntityCollectionEntity<TerrainCell> ConstructEntitySubsetEntity(
        CollectionGetterMethod<TerrainCell> getterMethod, Context c, string id, IEntity parent)
        => new CellCollectionEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntityInputRequest<TerrainCell> ConstructInputRequest(
        ICollection<TerrainCell> collection, ModText text)
        => new CellSelectionRequest(collection, text);
}
