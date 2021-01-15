using System.Collections.Generic;

public delegate ICollection<T> CollectionGetterMethod<T>();

public abstract class CollectionEntity<T> : Entity
{
    private CollectionGetterMethod<T> _getterMethod;

    protected override object _reference => _collection;

    protected ICollection<T> _collection = null;
    protected bool _isReset = false;

    public CollectionEntity(
        CollectionGetterMethod<T> getterMethod, Context c, string id)
        : base(c, id)
    {
        _getterMethod = getterMethod;
    }

    public CollectionEntity(Context c, string id)
        : base(c, id)
    {
        _getterMethod = null;
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
