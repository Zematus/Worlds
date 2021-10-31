using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupCollectionEntity : EntityCollectionEntity<CellGroup>
{
    public GroupCollectionEntity(Context c, string id)
        : base(c, id)
    {
    }

    public GroupCollectionEntity(
        CollectionGetterMethod<CellGroup> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public override string GetDebugString()
    {
        return "group_collection";
    }

    protected override DelayedSetEntity<CellGroup> ConstructEntity(
        ValueGetterMethod<CellGroup> getterMethod, Context c, string id)
        => new GroupEntity(getterMethod, c, id);

    protected override DelayedSetEntity<CellGroup> ConstructEntity(
        TryRequestGenMethod<CellGroup> tryRequestGenMethod, Context c, string id)
        => new GroupEntity(tryRequestGenMethod, c, id);

    protected override DelayedSetEntityInputRequest<CellGroup> ConstructInputRequest(
        ICollection<CellGroup> collection, ModText text)
        => new GroupSelectionRequest(collection, text);
}
