using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ContactCollectionEntity : EntityCollectionEntity<PolityContact>
{
    public ContactCollectionEntity(
        CollectionGetterMethod<PolityContact> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public override string GetDebugString()
    {
        return "contact_collection";
    }

    protected override DelayedSetEntity<PolityContact> ConstructEntity(
        ValueGetterMethod<PolityContact> getterMethod, Context c, string id)
        => new ContactEntity(getterMethod, c, id);

    protected override DelayedSetEntity<PolityContact> ConstructEntity(
        TryRequestGenMethod<PolityContact> tryRequestGenMethod, Context c, string id)
        => new ContactEntity(tryRequestGenMethod, c, id);

    protected override DelayedSetEntityInputRequest<PolityContact> ConstructInputRequest(
        ICollection<PolityContact> collection, ModText text)
        => new ContactSelectionRequest(collection, text);
}
