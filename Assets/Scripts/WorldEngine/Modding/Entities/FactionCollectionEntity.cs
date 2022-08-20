using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionCollectionEntity : EntityCollectionEntity<Faction>
{
    public FactionCollectionEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
    }

    public FactionCollectionEntity(
        CollectionGetterMethod<Faction> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString()
    {
        return "faction_collection";
    }

    protected override DelayedSetEntity<Faction> ConstructEntity(
        ValueGetterMethod<Faction> getterMethod, Context c, string id, IEntity parent)
        => new FactionEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntity<Faction> ConstructEntity(
        TryRequestGenMethod<Faction> tryRequestGenMethod, Context c, string id, IEntity parent)
        => new FactionEntity(tryRequestGenMethod, c, id, parent);

    protected override DelayedSetEntity<Faction> ConstructEntity(
        Context c, string id, IEntity parent)
        => new FactionEntity(c, id, parent);

    protected override EntityCollectionEntity<Faction> ConstructEntitySubsetEntity(
        Context c, string id, IEntity parent)
        => new FactionCollectionEntity(c, id, parent);

    protected override EntityCollectionEntity<Faction> ConstructEntitySubsetEntity(
        CollectionGetterMethod<Faction> getterMethod, Context c, string id, IEntity parent)
        => new FactionCollectionEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntityInputRequest<Faction> ConstructInputRequest(
        ICollection<Faction> collection, ModText text)
        => new FactionSelectionRequest(collection, text);
}
