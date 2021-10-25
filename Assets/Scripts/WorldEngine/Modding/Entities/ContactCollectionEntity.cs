using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ContactCollectionEntity : CollectionEntity<PolityContact>
{
    private int _selectedContactIndex = 0;

    private readonly List<ContactEntity>
        _contactEntitiesToSet = new List<ContactEntity>();

    public ContactCollectionEntity(
        CollectionGetterMethod<PolityContact> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected override EntityAttribute GenerateRequestSelectionAttribute(IExpression[] arguments)
    {
        int index = _selectedContactIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        if ((arguments == null) && (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                "'request_selection' is missing 1 argument");
        }

        IValueExpression<ModText> textExpression =
            ValueExpressionBuilder.ValidateValueExpression<ModText>(arguments[0]);

        ContactEntity entity = new ContactEntity(
            (out DelayedSetEntityInputRequest<PolityContact> request) =>
            {
                request = new ContactSelectionRequest(
                    Collection, textExpression.Value);
                return true;
            },
            Context,
            BuildAttributeId("selected_contact_" + index));

        _contactEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    protected override EntityAttribute GenerateSelectRandomAttribute()
    {
        int index = _selectedContactIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        ContactEntity entity = new ContactEntity(
            () => {
                int offset = iterOffset + Context.GetBaseOffset();
                return Collection.RandomSelect(Context.GetNextRandomInt, offset); 
            },
            Context,
            BuildAttributeId("selected_contact_" + index));

        _contactEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    public override string GetDebugString()
    {
        return "contact_collection";
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (ContactEntity entity in _contactEntitiesToSet)
        {
            entity.Reset();
        }
    }
}
