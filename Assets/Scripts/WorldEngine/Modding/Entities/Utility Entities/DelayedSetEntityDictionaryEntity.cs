using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class DelayedSetEntityDictionaryEntity<T,S> : DelayedSetEntity<S>
{
    public const string ContainsAttributeId = "contains";

    private readonly Dictionary<string, DelayedSetEntity<T>> _entities =
        new Dictionary<string, DelayedSetEntity<T>>();

    public DelayedSetEntityDictionaryEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public DelayedSetEntityDictionaryEntity(
        ValueGetterMethod<S> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected abstract DelayedSetEntity<T> CreateEntity(string attributeId);

    protected EntityAttribute CreateEntryAttribute(string attributeId)
    {
        if (!_entities.TryGetValue(attributeId, out var entity))
        {
            _entities[attributeId] = CreateEntity(attributeId);
        }

        return entity.GetThisEntityAttribute();
    }

    protected abstract bool ValidateKeyAtributeId(string attributeId);
    protected abstract bool ContainsKey(string key);

    private EntityAttribute GetContainsAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException($"'contains' attribute requires at least 1 argument");
        }

        var argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);

        return new ValueGetterEntityAttribute<bool>(
            ContainsAttributeId, this, () => ContainsKey(argumentExp.Value));
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ContainsAttributeId:
                return GetContainsAttribute(arguments);
        }

        if (ValidateKeyAtributeId(attributeId))
        {
            return CreateEntryAttribute(attributeId);
        }

        throw new System.ArgumentException(
            $"Unable to find attribute: { attributeId }");
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (var entity in _entities.Values)
        {
            entity.Reset();
        }
    }
}
