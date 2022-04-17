using System.Collections.Generic;

public abstract class DelayedSetEntityAttributeModifiableContainerEntity<T,S> : AttributeModifiableContainerEntity<S>
{
    private readonly Dictionary<string, DelayedSetEntity<T>> _entities =
        new Dictionary<string, DelayedSetEntity<T>>();

    public DelayedSetEntityAttributeModifiableContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public DelayedSetEntityAttributeModifiableContainerEntity(
        ValueGetterMethod<S> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected abstract DelayedSetEntity<T> CreateEntity(string attributeId);

    protected override EntityAttribute CreateEntryAttribute(string attributeId)
    {
        if (!_entities.TryGetValue(attributeId, out var entity))
        {
            entity = CreateEntity(attributeId);

            _entities[attributeId] = entity;
        }

        return entity.GetThisEntityAttribute();
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
