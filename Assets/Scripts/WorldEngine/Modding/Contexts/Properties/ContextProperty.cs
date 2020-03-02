using System;

public abstract class ContextProperty
{
    public const string ConditionSetType = "condition_set";
    public const string RandomRangeType = "random_range";

    /// <summary>
    /// String Id for the property
    /// </summary>
    public string Id;

    public ContextProperty(Context context, Context.LoadedProperty p)
    {
        if (string.IsNullOrEmpty(p.id))
        {
            throw new ArgumentException("'id' can't be null or empty");
        }

        Id = p.id;
    }
}
