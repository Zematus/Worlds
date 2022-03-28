using System.Collections.Generic;

public abstract class EntityCollectionEntity<T> : CollectionEntity<T>
{
    private const string _selectedEntityPrefix = "selected_entity_";
    private const string _entitySubsetPrefix = "entity_subset_";

    private int _selectedEntityIndex = 0;
    private int _entitySubsetIndex = 0;

    private readonly List<DelayedSetEntity<T>>
        _entitiesToSet = new List<DelayedSetEntity<T>>();

    private readonly List<EntityCollectionEntity<T>>
        _entitySubsetsToSet = new List<EntityCollectionEntity<T>>();

    public EntityCollectionEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
    }

    public EntityCollectionEntity(
        CollectionGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected abstract DelayedSetEntity<T> ConstructEntity(
        Context c, string id, IEntity parent);

    protected abstract DelayedSetEntity<T> ConstructEntity(
        ValueGetterMethod<T> getterMethod, Context c, string id, IEntity parent);

    protected abstract DelayedSetEntity<T> ConstructEntity(
        TryRequestGenMethod<T> tryRequestGenMethod, Context c, string id, IEntity parent);

    protected abstract EntityCollectionEntity<T> ConstructEntitySubsetEntity(
        Context c, string id, IEntity parent);

    protected abstract EntityCollectionEntity<T> ConstructEntitySubsetEntity(
        CollectionGetterMethod<T> getterMethod, Context c, string id, IEntity parent);

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

        var textExpression =
            ValueExpressionBuilder.ValidateValueExpression<ModText>(arguments[0]);

        var entity = ConstructEntity(
            (out DelayedSetEntityInputRequest<T> request) =>
            {
                request = ConstructInputRequest(
                    Collection, textExpression.Value);
                return true;
            },
            Context,
            BuildAttributeId(_selectedEntityPrefix + index),
            this);

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute();
    }

    protected override EntityAttribute GenerateSelectRandomAttribute()
    {
        int index = _selectedEntityIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        var entity = ConstructEntity(
            () => {
                int offset = iterOffset + Context.GetBaseOffset();
                return Collection.RandomSelect(Context.GetNextRandomInt, offset);
            },
            Context,
            BuildAttributeId(_selectedEntityPrefix + index),
            this);

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute();
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (var entity in _entitiesToSet)
        {
            entity.Reset();
        }

        foreach (var entity in _entitySubsetsToSet)
        {
            entity.Reset();
        }
    }

    public override ParametricSubcontext BuildSelectAttributeSubcontext(Context parentContext, string[] paramIds)
    {
        int index = _selectedEntityIndex;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SelectAttributeId}: expected at least one parameter identifier");
        }

        var subcontext =
            new ParametricSubcontext(
                $"{SelectAttributeId}_{index}",
                parentContext);

        var entity = ConstructEntity(subcontext, paramIds[0], this);
        subcontext.AddEntity(entity);

        return subcontext;
    }

    public override ParametricSubcontext BuildGetSubsetAttributeSubcontext(Context parentContext, string[] paramIds)
    {
        int index = _entitySubsetIndex;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{GetSubsetAttributeId}: expected at least one parameter identifier");
        }

        var subcontext =
            new ParametricSubcontext(
                $"{GetSubsetAttributeId}_{index}",
                parentContext);

        var entity = ConstructEntity(subcontext, paramIds[0], this);
        subcontext.AddEntity(entity);

        return subcontext;
    }

    public override EntityAttribute GenerateSelectAttribute(ParametricSubcontext subcontext, string[] paramIds, IExpression[] arguments)
    {
        int index = _selectedEntityIndex++;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SelectAttributeId}: expected one parameter identifier");
        }

        var paramEntity = subcontext.GetEntity(paramIds[0]) as DelayedSetEntity<T>;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SelectAttributeId}: expected one condition argument");
        }

        var conditionExp = ValueExpressionBuilder.ValidateValueExpression<bool>(arguments[0]);

        var entity = ConstructEntity(
            () =>
            {
                foreach (var item in _collection)
                {
                    paramEntity.Set(item);

                    if (conditionExp.Value)
                    {
                        return item;
                    }
                }

                return default;
            },
            Context,
            BuildAttributeId(_selectedEntityPrefix + index),
            this);

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute();
    }

    public override EntityAttribute GenerateGetSubsetAttribute(ParametricSubcontext subcontext, string[] paramIds, IExpression[] arguments)
    {
        int index = _entitySubsetIndex++;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{GetSubsetAttributeId}: expected one parameter identifier");
        }

        var paramEntity = subcontext.GetEntity(paramIds[0]) as DelayedSetEntity<T>;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                $"{GetSubsetAttributeId}: expected one condition argument");
        }

        var conditionExp = ValueExpressionBuilder.ValidateValueExpression<bool>(arguments[0]);

        var entity = ConstructEntitySubsetEntity(
            () =>
            {
                var items = new HashSet<T>();

                foreach (var item in _collection)
                {
                    paramEntity.Set(item);

                    if (conditionExp.Value)
                    {
                        items.Add(item);
                    }
                }

                return items;
            },
            Context,
            BuildAttributeId(_entitySubsetPrefix + index),
            this);

        _entitySubsetsToSet.Add(entity);

        return entity.GetThisEntityAttribute();
    }
}
