using System.Collections.Generic;

public abstract class EntityCollectionEntity<T> : CollectionEntity<T>
{
    private const string _sumPrefix = "sum_";
    private const string _entityPrefix = "entity_";
    private const string _entitySubsetPrefix = "entity_subset_";

    private int _sumIndex = 0;
    private int _entityIndex = 0;
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
        int index = _entityIndex++;
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
            BuildAttributeId(_entityPrefix + index),
            this);

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute();
    }

    protected override EntityAttribute GenerateSelectRandomAttribute()
    {
        int index = _entityIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        var entity = ConstructEntity(
            () => {
                int offset = iterOffset + Context.GetBaseOffset();
                return Collection.RandomSelect(Context.GetNextRandomInt, offset);
            },
            Context,
            BuildAttributeId(_entityPrefix + index),
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
        int index = _entityIndex;

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

    public override ParametricSubcontext BuildSumAttributeSubcontext(Context parentContext, string[] paramIds)
    {
        int index = _sumIndex;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SumAttributeId}: expected at least one parameter identifier");
        }

        var subcontext =
            new ParametricSubcontext(
                $"{SumAttributeId}_{index}",
                parentContext);

        var entity = ConstructEntity(subcontext, paramIds[0], this);
        subcontext.AddEntity(entity);

        return subcontext;
    }

    public override ParametricSubcontext BuildSelectBestAttributeSubcontext(Context parentContext, string[] paramIds)
    {
        int index = _entityIndex;

        if ((paramIds == null) || (paramIds.Length < 2))
        {
            throw new System.ArgumentException(
                $"{SelectAttributeId}: expected at least two parameter identifiers");
        }

        var subcontext =
            new ParametricSubcontext(
                $"{SelectAttributeId}_{index}",
                parentContext);

        var entityA = ConstructEntity(subcontext, paramIds[0], this);
        var entityB = ConstructEntity(subcontext, paramIds[1], this);
        subcontext.AddEntity(entityA);
        subcontext.AddEntity(entityB);

        return subcontext;
    }

    public override ParametricSubcontext BuildSelectSubsetAttributeSubcontext(Context parentContext, string[] paramIds)
    {
        int index = _entitySubsetIndex;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SelectSubsetAttributeId}: expected at least one parameter identifier");
        }

        var subcontext =
            new ParametricSubcontext(
                $"{SelectSubsetAttributeId}_{index}",
                parentContext);

        var entity = ConstructEntity(subcontext, paramIds[0], this);
        subcontext.AddEntity(entity);

        return subcontext;
    }

    public override EntityAttribute GenerateSelectAttribute(ParametricSubcontext subcontext, string[] paramIds, IExpression[] arguments)
    {
        int index = _entityIndex++;

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
            BuildAttributeId(_entityPrefix + index),
            this);

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute();
    }

    public override EntityAttribute GenerateSumAttribute(ParametricSubcontext subcontext, string[] paramIds, IExpression[] arguments)
    {
        int index = _sumIndex++;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SumAttributeId}: expected one parameter identifier");
        }

        var paramEntity = subcontext.GetEntity(paramIds[0]) as DelayedSetEntity<T>;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SumAttributeId}: expected one expression argument");
        }

        var valExpression = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);

        var attribute = new ValueGetterEntityAttribute<float>(
            BuildAttributeId(_sumPrefix + index),
            this,
            () =>
            {
                float result = 0;
                bool first = true;

                foreach (var item in _collection)
                {
                    paramEntity.Set(item);

                    if (first)
                    {
                        result = valExpression.Value;
                        first = false;
                        continue;
                    }

                    result += valExpression.Value;
                }

                return result;
            });

        return attribute;
    }

    public override EntityAttribute GenerateSelectBestAttribute(ParametricSubcontext subcontext, string[] paramIds, IExpression[] arguments)
    {
        int index = _entityIndex++;

        if ((paramIds == null) || (paramIds.Length < 2))
        {
            throw new System.ArgumentException(
                $"{SelectBestAttributeId}: expected two parameter identifiers");
        }

        var paramEntityA = subcontext.GetEntity(paramIds[0]) as DelayedSetEntity<T>;
        var paramEntityB = subcontext.GetEntity(paramIds[1]) as DelayedSetEntity<T>;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SelectBestAttributeId}: expected one condition argument");
        }

        var conditionExp = ValueExpressionBuilder.ValidateValueExpression<bool>(arguments[0]);

        var entity = ConstructEntity(
            () =>
            {
                T bestItem = default;
                bool first = true;

                foreach (var item in _collection)
                {
                    if (first)
                    {
                        bestItem = item;
                        paramEntityA.Set(bestItem);
                        first = false;
                        continue;
                    }

                    paramEntityB.Set(item);

                    if (!conditionExp.Value)
                    {
                        bestItem = item;
                        paramEntityA.Set(bestItem);
                    }
                }

                return bestItem;
            },
            Context,
            BuildAttributeId(_entityPrefix + index),
            this);

        _entitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute();
    }

    public override EntityAttribute GenerateSelectSubsetAttribute(ParametricSubcontext subcontext, string[] paramIds, IExpression[] arguments)
    {
        int index = _entitySubsetIndex++;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SelectSubsetAttributeId}: expected one parameter identifier");
        }

        var paramEntity = subcontext.GetEntity(paramIds[0]) as DelayedSetEntity<T>;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                $"{SelectSubsetAttributeId}: expected one condition argument");
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
