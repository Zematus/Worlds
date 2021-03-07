using System.Collections.Generic;

public delegate ICollection<T> CollectionGetterMethod<T>();

public abstract class CollectionEntity<T> : Entity
{
    public const string CountAttributeId = "count";
    public const string RequestSelectionAttributeId = "request_selection";
    public const string SelectRandomAttributeId = "select_random";

    private CollectionGetterMethod<T> _getterMethod = null;

    private ValueGetterEntityAttribute<float> _countAttribute;

    protected override object _reference => Collection;

    protected ICollection<T> _collection = null;
    protected bool _isReset = false;

    public CollectionEntity(
        CollectionGetterMethod<T> getterMethod, Context c, string id)
        : base(c, id)
    {
        _getterMethod = getterMethod;
    }

    public void Reset()
    {
        _collection = null;

        ResetInternal();

        _isReset = true;
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
            Set(e.Collection);
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
