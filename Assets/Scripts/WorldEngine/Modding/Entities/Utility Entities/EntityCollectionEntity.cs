using System.Collections.Generic;

public abstract class EntityCollectionEntity<T> : CollectionEntity<T>
{
    private int _selectedEntityIndex = 0;

    private readonly List<DelayedSetEntity<T>>
        _entitiesToSet = new List<DelayedSetEntity<T>>();

    public EntityCollectionEntity(Context c, string id)
        : base(c, id)
    {
    }

    public EntityCollectionEntity(
        CollectionGetterMethod<T> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected abstract DelayedSetEntity<T> ConstructEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id);

    protected abstract DelayedSetEntity<T> ConstructEntity(
        TryRequestGenMethod<T> tryRequestGenMethod, Context c, string id);

    protected abstract DelayedSetEntityInputRequest<T> ConstructInputRequest(
        ICollection<T> collection, ModText text);

    protected override EntityAttribute GenerateRequestSelectionAttribute(IExpression[] arguments)
    {
        int index = _selectedEntityIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        if ((arguments == null) && (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                "'request_selection' is missing 1 argument");
        }

        IValueExpression<ModText> textExpression =
            ValueExpressionBuilder.ValidateValueExpression<ModText>(arguments[0]);

        DelayedSetEntity<T> entity = ConstructEntity(
            (out DelayedSetEntityInputRequest<T> request) =>
            {
                request = ConstructInputRequest(
                    Collection, textExpression.Value);
                return true;
            },
            Context,
            BuildAttributeId("selected_entity_" + index));

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    protected override EntityAttribute GenerateSelectRandomAttribute()
    {
        int index = _selectedEntityIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        DelayedSetEntity<T> entity = ConstructEntity(
            () => {
                int offset = iterOffset + Context.GetBaseOffset();
                return Collection.RandomSelect(Context.GetNextRandomInt, offset);
            },
            Context,
            BuildAttributeId("selected_entity_" + index));

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (var entity in _entitiesToSet)
        {
            entity.Reset();
        }
    }
}
