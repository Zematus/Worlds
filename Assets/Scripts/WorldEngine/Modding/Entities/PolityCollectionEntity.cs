using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityCollectionEntity : EntityCollectionEntity<Polity>
{
    public PolityCollectionEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
    }

    public PolityCollectionEntity(
        CollectionGetterMethod<Polity> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString()
    {
        return "polity_collection";
    }

    protected override DelayedSetEntity<Polity> ConstructEntity(
        ValueGetterMethod<Polity> getterMethod, Context c, string id, IEntity parent)
        => new PolityEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntity<Polity> ConstructEntity(
        TryRequestGenMethod<Polity> tryRequestGenMethod, Context c, string id, IEntity parent)
        => new PolityEntity(tryRequestGenMethod, c, id, parent);

    protected override DelayedSetEntity<Polity> ConstructEntity(
        Context c, string id, IEntity parent)
        => new PolityEntity(c, id, parent);

    protected override EntityCollectionEntity<Polity> ConstructEntitySubsetEntity(
        Context c, string id, IEntity parent)
        => new PolityCollectionEntity(c, id, parent);

    protected override EntityCollectionEntity<Polity> ConstructEntitySubsetEntity(
        CollectionGetterMethod<Polity> getterMethod, Context c, string id, IEntity parent)
        => new PolityCollectionEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntityInputRequest<Polity> ConstructInputRequest(
        ICollection<Polity> collection, ModText text)
        => new PolitySelectionRequest(collection, text);
}
