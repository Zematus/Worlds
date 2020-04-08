using System;

public abstract class PropertyEntity<T> : ValueEntity<T>
{
    private bool _evaluated = false;

    protected override object _reference => this;

    protected readonly Context _context;
    protected readonly int _idHash;

    public PropertyEntity(
        Context context, Context.LoadedContext.LoadedProperty p)
        : base(p.id)
    {
        _context = context;
        _idHash = p.id.GetHashCode();
    }

    protected PropertyEntity(Context context, string id)
        : base(id)
    {
        _context = context;
        _idHash = id.GetHashCode();
    }

    public void Reset()
    {
        _evaluated = false;
    }

    protected abstract void Calculate();

    protected void EvaluateIfNeeded()
    {
        if (!_evaluated)
        {
            Calculate();
            _evaluated = true;
        }
    }

    public override void Set(T v)
    {
        throw new System.Exception("Set() should be never be called for " + this.GetType());
    }

    public override void Set(object o)
    {
        throw new System.Exception("Set() should be never be called for " + this.GetType());
    }
}
