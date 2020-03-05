using System;

public abstract class PropertyEntity : Entity
{
    public const string ConditionSetType = "condition_set";
    public const string RandomRangeType = "random_range";

    private bool _evaluated = false;

    protected override object _reference => this;

    public PropertyEntity(Context context, Context.LoadedProperty p)
        : base(p.id)
    {
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
}
