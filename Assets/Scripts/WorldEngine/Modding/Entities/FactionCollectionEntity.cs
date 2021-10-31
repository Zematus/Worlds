using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionCollectionEntity : EntityCollectionEntity<Faction>
{
    public FactionCollectionEntity(Context c, string id)
        : base(c, id)
    {
    }

    public FactionCollectionEntity(
        CollectionGetterMethod<Faction> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public override string GetDebugString()
    {
        return "faction_collection";
    }

    protected override DelayedSetEntity<Faction> ConstructEntity(
        ValueGetterMethod<Faction> getterMethod, Context c, string id)
        => new FactionEntity(getterMethod, c, id);

    protected override DelayedSetEntity<Faction> ConstructEntity(
        TryRequestGenMethod<Faction> tryRequestGenMethod, Context c, string id)
        => new FactionEntity(tryRequestGenMethod, c, id);

    protected override DelayedSetEntityInputRequest<Faction> ConstructInputRequest(
        ICollection<Faction> collection, ModText text)
        => new FactionSelectionRequest(collection, text);
}
