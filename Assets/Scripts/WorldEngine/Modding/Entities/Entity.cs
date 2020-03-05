using System.Collections.Generic;
using System;

public abstract class Entity
{
    public string Id;

    protected abstract object _reference { get; }

    public Entity(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("'id' can't be null or empty");
        }

        Id = id;
    }

    protected string BuildInternalEntityId(string entityId)
    {
        return Id + "." + entityId;
    }

    public abstract EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null);

    public override bool Equals(object obj)
    {
        return obj is Entity entity &&
               EqualityComparer<object>.Default.Equals(_reference, entity._reference);
    }

    public override int GetHashCode()
    {
        return -417141133 + EqualityComparer<object>.Default.GetHashCode(_reference);
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return EqualityComparer<Entity>.Default.Equals(left, right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}
