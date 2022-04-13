using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntryModifiableContainerEntity<T> : EntryContainerEntity<T>
{
    public const string AddAttributeId = "add";
    public const string RemoveAttributeId = "remove";

    public EntryModifiableContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public EntryModifiableContainerEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected abstract bool AddKey(string key);
    protected abstract bool RemoveKey(string key);

    private EntityAttribute GetAddAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'add' attribute requires at least 1 argument");
        }

        var argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);

        return new ValueGetterEntityAttribute<bool>(
            AddAttributeId, this, () => AddKey(argumentExp.Value));
    }

    private EntityAttribute GetRemoveAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'remove' attribute requires at least 1 argument");
        }

        var argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);

        return new ValueGetterEntityAttribute<bool>(
            RemoveAttributeId, this, () => RemoveKey(argumentExp.Value));
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case AddAttributeId:
                return GetAddAttribute(arguments);

            case RemoveAttributeId:
                return GetRemoveAttribute(arguments);
        }

        return base.GetAttribute(attributeId, arguments);
    }
}
