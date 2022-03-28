using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupCollectionEntity : EntityCollectionEntity<CellGroup>
{
    public GroupCollectionEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
    }

    public GroupCollectionEntity(
        CollectionGetterMethod<CellGroup> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString()
    {
        return "group_collection";
    }

    protected override DelayedSetEntity<CellGroup> ConstructEntity(
        ValueGetterMethod<CellGroup> getterMethod, Context c, string id, IEntity parent)
        => new GroupEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntity<CellGroup> ConstructEntity(
        TryRequestGenMethod<CellGroup> tryRequestGenMethod, Context c, string id, IEntity parent)
        => new GroupEntity(tryRequestGenMethod, c, id, parent);

    protected override DelayedSetEntity<CellGroup> ConstructEntity(
        Context c, string id, IEntity parent)
        => new GroupEntity(c, id, parent);

    protected override EntityCollectionEntity<CellGroup> ConstructEntitySubsetEntity(
        Context c, string id, IEntity parent)
        => new GroupCollectionEntity(c, id, parent);

    protected override EntityCollectionEntity<CellGroup> ConstructEntitySubsetEntity(
        CollectionGetterMethod<CellGroup> getterMethod, Context c, string id, IEntity parent)
        => new GroupCollectionEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntityInputRequest<CellGroup> ConstructInputRequest(
        ICollection<CellGroup> collection, ModText text)
        => new GroupSelectionRequest(collection, text);
}
