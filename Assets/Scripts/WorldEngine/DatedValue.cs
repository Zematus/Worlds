
public interface IWorldDateGetter
{
    long CurrentDate { get; }
}

public delegate T ValueUpdateMethod<T>();

public class DatedValue<T>
{
    private T _value;

    private readonly ValueUpdateMethod<T> _updateMethod;
    private readonly IWorldDateGetter _dateGetter;

    private long _lastUpdateDate = -1;

    public T Value
    {
        get
        {
            if (_lastUpdateDate < _dateGetter.CurrentDate)
            {
                _value = _updateMethod();

                _lastUpdateDate = _dateGetter.CurrentDate;
            }

            return _value;
        }
    }

    // NOTE: Commented as this implicit conversion doesn't work to extract members
    //public static implicit operator T(DatedValue<T> dv)
    //{
    //    return dv.Value;
    //}

    public DatedValue(
        IWorldDateGetter dateGetter,
        ValueUpdateMethod<T> updateMethod,
        T initialValue = default)
    {
        _dateGetter = dateGetter;
        _updateMethod = updateMethod;

        _value = initialValue;
    }
}
