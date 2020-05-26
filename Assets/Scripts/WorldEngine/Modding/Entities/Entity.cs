using System.Collections.Generic;
using System;

public abstract class Entity : IComparable<object>
{
    public string Id;

    protected abstract object _reference { get; }

    protected IValueExpression<Entity> _expression = null;

    private EntityAttribute _thisAttribute;

    public Entity(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("'id' can't be null or empty");
        }

        Id = id;
    }

    public string BuildAttributeId(string attrId)
    {
        return Id + "." + attrId;
    }

    public abstract EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null);

    public abstract string GetFormattedString();

    public abstract string GetDebugString();

    public override string ToString()
    {
        return GetDebugString();
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

    public virtual IValueExpression<Entity> Expression
    {
        get
        {
            _expression = _expression ?? new EntityExpression(this);

            return _expression;
        }
    }

    public abstract void Set(object o);

    public virtual void Set(
        object o,
        PartiallyEvaluatedStringConverter converter)
    {
        Set(o);
    }

    public EntityAttribute GetThisEntityAttribute(Entity parent)
    {
        _thisAttribute =
            _thisAttribute ?? new FixedValueEntityAttribute<Entity>(
                this, Id, parent);

        return _thisAttribute;
    }

    public virtual string ToPartiallyEvaluatedString(bool evaluate)
    {
        return Id;
    }
}
