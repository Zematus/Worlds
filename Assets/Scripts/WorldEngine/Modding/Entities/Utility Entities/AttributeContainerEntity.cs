using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class AttributeContainerEntity<T> : EntryContainerEntity<T>
{
    public AttributeContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public AttributeContainerEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected abstract EntityAttribute CreateEntryAttribute(string attributeId);

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        if (ValidateKey(attributeId))
        {
            return CreateEntryAttribute(attributeId);
        }

        return base.GetAttribute(attributeId, arguments);
    }
}
