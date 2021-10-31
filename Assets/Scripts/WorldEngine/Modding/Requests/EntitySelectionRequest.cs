using System.Collections.Generic;

public abstract class EntitySelectionRequest<T> : DelayedSetEntityInputRequest<T>, IEntitySelectionRequest
{
    public ModText Text { get; private set; }

    public ICollection<T> Collection { get; private set; }

    public EntitySelectionRequest(ICollection<T> collection, ModText text)
    {
        Collection = collection;
        Text = text;
    }
}
