using System.Collections.Generic;

public delegate ICollection<T> CollectionGetterMethod<T>();

public abstract class CollectionEntity<T> : Entity
{
    public const string CountAttributeId = "count";
    public const string RequestSelectionAttributeId = "request_selection";
    public const string SelectRandomAttributeId = "select_random";
    public const string SelectAttributeId = "select";
    public const string GetSubsetAttributeId = "get_subset";

    private CollectionGetterMethod<T> _getterMethod = null;

    private ValueGetterEntityAttribute<float> _countAttribute;

    protected override object _reference => Collection;

    protected ICollection<T> _collection = null;
    protected bool _isReset = false;

    public CollectionEntity(Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
    }

    public CollectionEntity(
        CollectionGetterMethod<T> getterMethod, Context c, string id, IEntity parent)
        : base(c, id, parent)
    {
        _getterMethod = getterMethod;
    }

    public void Reset()
    {
        _collection = null;

        ResetInternal();

        _isReset = true;
    }

    public virtual void Set(ICollection<T> c, IEntity parent)
    {
        Parent = parent;

        Set(c);
    }

    public virtual void Set(ICollection<T> c)
    {
        _collection = c;

        ResetInternal();

        _isReset = false;
    }

    protected virtual void ResetInternal()
    {
    }

    protected abstract EntityAttribute GenerateRequestSelectionAttribute(IExpression[] arguments);

    protected abstract EntityAttribute GenerateSelectRandomAttribute();

    private float GetCount() => Collection.Count;

    public abstract ParametricSubcontext BuildSelectAttributeSubcontext(
        Context parentContext,
        string[] paramIds);

    public abstract ParametricSubcontext BuildGetSubsetAttributeSubcontext(
        Context parentContext,
        string[] paramIds);

    public override ParametricSubcontext BuildParametricSubcontext(
        Context parentContext,
        string attributeId,
        string[] paramIds)
    {
        switch (attributeId)
        {
            case SelectAttributeId:
                return BuildSelectAttributeSubcontext(parentContext, paramIds);

            case GetSubsetAttributeId:
                return BuildGetSubsetAttributeSubcontext(parentContext, paramIds);
        }

        return base.BuildParametricSubcontext(parentContext, attributeId, paramIds);
    }

    public abstract EntityAttribute GenerateSelectAttribute(
        ParametricSubcontext subcontext,
        string[] paramIds,
        IExpression[] arguments);

    public abstract EntityAttribute GenerateGetSubsetAttribute(
        ParametricSubcontext subcontext,
        string[] paramIds,
        IExpression[] arguments);

    public override EntityAttribute GetParametricAttribute(
        string attributeId,
        ParametricSubcontext subcontext,
        string[] paramIds,
        IExpression[] arguments)
    {
        switch (attributeId)
        {
            case SelectAttributeId:
                return GenerateSelectAttribute(subcontext, paramIds, arguments);

            case GetSubsetAttributeId:
                return GenerateGetSubsetAttribute(subcontext, paramIds, arguments);
        }

        return base.GetParametricAttribute(attributeId, subcontext, paramIds, arguments);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case RequestSelectionAttributeId:
                return GenerateRequestSelectionAttribute(arguments);

            case SelectRandomAttributeId:
                return GenerateSelectRandomAttribute();

            case CountAttributeId:
                _countAttribute =
                    _countAttribute ?? new ValueGetterEntityAttribute<float>(
                        CountAttributeId, this, GetCount);
                return _countAttribute;
        }

        throw new System.ArgumentException(Id + ": Unable to find attribute: " + attributeId);
    }

    protected virtual ICollection<T> Collection
    {
        get
        {
            if (_isReset && (_getterMethod != null))
            {
                Set(_getterMethod());
            }

            return _collection;
        }
    }

    public override void Set(object o)
    {
        if (o is CollectionEntity<T> e)
        {
            Set(e.Collection, e.Parent);
        }
        else if (o is ICollection<T> c)
        {
            Set(c);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }

    public override string GetFormattedString()
    {
        throw new System.InvalidOperationException();
    }
}
