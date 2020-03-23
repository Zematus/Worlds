using System.Collections.Generic;
using System;

public abstract class Entity : IComparable<object>
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

    public abstract string GetFormattedString();

    public override string ToString()
    {
        return GetFormattedString();
    }

    public override bool Equals(object obj)
    {
        return obj is Entity entity &&
               EqualityComparer<object>.Default.Equals(_reference, entity._reference);
    }

    public override int GetHashCode()
    {
        return -417141133 + EqualityComparer<object>.Default.GetHashCode(_reference);
    }

    public int CompareTo(object other)
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !left.Equals(right);
    }
}
