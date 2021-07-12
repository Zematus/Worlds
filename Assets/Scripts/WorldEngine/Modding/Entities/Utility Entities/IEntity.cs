using System.Collections.Generic;
using System;

public interface IEntity : IComparable<object>, IInputRequester, IFormattedStringGenerator
{
    string Id { get; }

    EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null);

    string GetDebugString();

    string BuildAttributeId(string attrId);

    Context Context { get; }

    IValueExpression<IEntity> Expression { get; }

    void Set(object o);

    void Set(object o, PartiallyEvaluatedStringConverter converter);

    EntityAttribute GetThisEntityAttribute(Entity parent);

    string ToPartiallyEvaluatedString(int depth = -1);
}
