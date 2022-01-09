using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ContactCollectionEntity : EntityCollectionEntity<PolityContact>
{
    public ContactCollectionEntity(
        CollectionGetterMethod<PolityContact> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString()
    {
        return "contact_collection";
    }

    protected override DelayedSetEntity<PolityContact> ConstructEntity(
        ValueGetterMethod<PolityContact> getterMethod, Context c, string id, IEntity parent)
        => new ContactEntity(getterMethod, c, id, parent);

    protected override DelayedSetEntity<PolityContact> ConstructEntity(
        TryRequestGenMethod<PolityContact> tryRequestGenMethod, Context c, string id, IEntity parent)
        => new ContactEntity(tryRequestGenMethod, c, id, parent);

    protected override DelayedSetEntityInputRequest<PolityContact> ConstructInputRequest(
        ICollection<PolityContact> collection, ModText text)
        => new ContactSelectionRequest(collection, text);
}
