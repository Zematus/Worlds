
public abstract class DelayedSetEntityInputRequest<T> : InputRequest
{
    protected DelayedSetEntity<T> Entity { get; set; }

    public void SetEntity(DelayedSetEntity<T> entity)
    {
        Entity = entity;
    }

    public void Set(T t)
    {
        Entity.Set(t);
    }
}
