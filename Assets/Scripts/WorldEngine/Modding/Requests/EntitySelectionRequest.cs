using System.Collections.Generic;

public abstract class EntitySelectionRequest<T> : DelayedSetEntityInputRequest<T>
{
    public ICollection<T> Collection { get; private set; }

    public EntitySelectionRequest(ICollection<T> collection)
    {
        Collection = collection;
    }
}
